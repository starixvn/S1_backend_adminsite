using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.BetLimit;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;

namespace BackendSite.Service.BLL
{
    public class UserBetLimitBLL
    {
        private readonly CustInfoBLL _custInfoBll;
        private readonly UserBetLimitService _userBetLimitService;
        private readonly ThirdPartyService _thirdPartyService;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly string _apiUrl;
        private readonly CourierAdapter _courierAdapter;
        private readonly ILogger<UserBetLimitBLL> _logger;

        public UserBetLimitBLL(CustInfoBLL custInfoBll, UserBetLimitService userBetLimitService, AdjustmentLogReportService adjustmentLogReportService, IConfiguration configuration, CourierAdapter courierAdapter, ILogger<UserBetLimitBLL> logger, ThirdPartyService thirdPartyService)
        {
            _userBetLimitService = userBetLimitService;
            _adjustmentLogReportService = adjustmentLogReportService;
            _apiUrl = configuration.GetValue<string>("TpService");
            _courierAdapter = courierAdapter;
            _custInfoBll = custInfoBll;
            _logger = logger;
            _thirdPartyService = thirdPartyService;
        }

        public async Task<IEnumerable<TpCategoryBetLimit>> GetTpCategoryList()
        {
            var tpCategories = await _userBetLimitService.GetTpCategoryList();

            return tpCategories.Where(a => a.CategoryId == (int)TpCategoryEnum.LiveCasino)
                .Select(category => new TpCategoryBetLimit() { CategoryId = category.CategoryId, CategoryName = category.CategoryName }).ToList();
        }
        public async Task<IEnumerable<BetLimitOption>> GetSettingList(int currencyId)
        {
            var dg = await _userBetLimitService.GetLimitGroup(10, currencyId);
            var dgGroup = JsonConvert.DeserializeObject<List<LimitGroup>>(dg.LimitGroup);
            return new List<BetLimitOption>()
            {
                new BetLimitOption() { CategoryId = (int)TpCategoryEnum.LiveCasino, CategoryName = "Live Casino", GameType = 10, GameTypeName = "DG L.Casino", IsGroup = true, LimitGroup = dgGroup },
                new BetLimitOption() { CategoryId = (int)TpCategoryEnum.Sports, CategoryName = "Sports", GameType = 77, GameTypeName = "SBO Sports", IsGroup = false, LimitInput = new LimitInput() { Min = 5, Max = 100000, MaxPerMatch = 340000 }}
            };
        }
        public async Task<int> SetUserBetLimit(dynamic req, string updateOperator)
        {
            var userName = (string)req["after"].userName;
            var siteId = (int)req["siteId"];

            var customer = await _custInfoBll.GetCustId(null, userName, siteId);
            if (customer == null)
            {
                throw new AdminException(ErrorCodeEnum.NoData, "User Not Found");
            }
            var custId = customer.CustId;
            var currencyId = customer.CurrencyId;
            var custBetLimit = await _userBetLimitService.UserBetLimitGetByCustId(custId);
            var setting = new List<CustBetLimitSetting>();
            if (custBetLimit != null)
                setting = JsonConvert.DeserializeObject<List<CustBetLimitSetting>>(custBetLimit.BetSetting);

            var betSettings = (List<BetLimitSetting>)ParseSetting(req["after"].betSetting);
            foreach (var item in betSettings)
            {
                if (item.GameTypeId == 77) 
                {
                    var before = setting.FirstOrDefault(a => a.GameTypeId == item.GameTypeId);
                    var after = betSettings.FirstOrDefault(a => a.GameTypeId == item.GameTypeId);
                    var betLimit = await ParseSetting(custId, currencyId, before, (SportsSetting)after);
                    var apiResult = await SetBetLimit(betLimit);
                    if (apiResult.ErrorCode != 0)
                        return -1;
                }
            }

            var result = await _userBetLimitService.SetBetLimit(siteId, custId, JsonConvert.SerializeObject(betSettings), updateOperator);
            if (result > 0) 
            {
                var logParam = new CreateAdminAdjustmentInfoLogInput
                {
                    SiteId = siteId,
                    CurrencyId = currencyId,
                    MenuEnum = AdminAdjustmentLogMenuEnum.BetLimit,
                    ActionType = result == 1 ? ActionType.Create : ActionType.Update,
                    BeforeDetailInfo = custBetLimit.ToLog(),
                    AfterDetailInfo = new CustBetLimit()
                    {
                        UserName = userName,
                        BetSetting = betSettings
                    }.ToLog(),
                    Operator = updateOperator
                };

                if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
                {
                    _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
                }
            }
            return result;
        }

        public Task<DataWithPage<UserBetLimit>> GetUserBetLimit(int siteId, IEnumerable<int> currencyIds, string userName, DateTime startTime, DateTime endTime, int pageSize, int pageNumber)
        {
            return _userBetLimitService.GetUserBetLimit(siteId, currencyIds, userName, startTime, endTime, pageSize, pageNumber);
        }
        public async Task<CustBetLimitByCustIdV2> GetByCustIdV2(int custId)
        {
            var betLimit = await _userBetLimitService.UserBetLimitGetByCustId(custId);
            var betSettings = ParseSetting(betLimit.BetSetting);

            var resp = new CustBetLimitByCustIdV2()
            {
                custId = betLimit.CustId,
                UserName = betLimit.UserName,
                CurrencyId = betLimit.CurrencyId,
                BetSetting = betSettings
            };
            return resp;
        }

        public async Task<int> Delete(ParamsWithLog<DeleteCustBetLimit> input)
        {
            var betLimit = await _userBetLimitService.UserBetLimitGetByCustId(input.Before.CustId);
            if (betLimit == null)
                return -1;

            var betSettings = JsonConvert.DeserializeObject<List<CustBetLimitSetting>>(betLimit.BetSetting);
            var sbo = betSettings.FirstOrDefault(a => a.GameTypeId == (int)TpGameTypeEnum.SBO_Sports);
            if(sbo != null)
                await ResetBetLimit(betLimit.CustId, sbo.GameTypeId, betLimit.CurrencyId);

            var result = await _userBetLimitService.Delete(input);
            if (result > 0)
            {
                var logParam = new CreateAdminAdjustmentInfoLogInput
                {
                    SiteId = input.SiteId,
                    CurrencyId = input.CurrencyId,
                    MenuEnum = AdminAdjustmentLogMenuEnum.BetLimit,
                    ActionType = ActionType.Delete,
                    BeforeDetailInfo = input.Before.ToLog(),
                    Operator = input.Operator
                };

                if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
                {
                    _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
                }
            }
            return result;
        }

        private async Task ResetBetLimit(int custId, int gameType, int currencyId) 
        {
            var defaultBetLimit = await _userBetLimitService.GetLimitGroup(gameType, currencyId);

            var tpId = await _userBetLimitService.GetTpIdByGameType(gameType);
            var betLimit = new SetBetLimit
            {
                TpId = tpId,
                CustId = custId,
                BetSetting = defaultBetLimit.LimitGroup
            };
            var apiResult = await SetBetLimit(betLimit);
            if (apiResult.ErrorCode != 0)
                throw new AdminException(ErrorCodeEnum.SystemException);
        }

        private async Task<ApiResult> SetBetLimit(SetBetLimit request) 
        {
            return await _courierAdapter.MyPostAsync<ApiResult, SetBetLimit>($"{_apiUrl}SetBetLimit", request);
        }

        private async Task<SetBetLimit> ParseSetting(int custId, int currencyId, CustBetLimitSetting before, SportsSetting after) 
        {
            if (after == null)
            {
                if (before == null) return null;

                var defaultBetLimit = await _userBetLimitService.GetLimitGroup(before.GameTypeId, currencyId);

                var tpId = await _userBetLimitService.GetTpIdByGameType(before.GameTypeId);
                var betLimit = new SetBetLimit
                {
                    TpId = tpId,
                    CustId = custId,
                    BetSetting = defaultBetLimit.LimitGroup
                };

                return betLimit;
            }
            else 
            {
                var tpId = await _userBetLimitService.GetTpIdByGameType(after.GameTypeId);
                var betLimit = new SetBetLimit
                {
                    TpId = tpId,
                    CustId = custId,
                    BetSetting = JsonConvert.SerializeObject(new
                    {
                        after.Min,
                        after.Max,
                        after.MaxPerMatch
                    })
                };
                return betLimit;
            }
        }

        private List<BetLimitSetting> ParseSetting(dynamic betSetting)
        {
            var betSettings = new List<BetLimitSetting>();
            foreach (var item in betSetting)
            {
                var tpCategory = (int)item.tpCategory;
                if (tpCategory == (int)TpCategoryEnum.Sports)
                {
                    var sports = new SportsSetting
                    {
                        Checked = (bool)item["checked"],
                        TpCategory = item["tpCategory"],
                        TpCategoryName = item["tpCategoryName"],
                        GameTypeId = item["gameTypeId"],
                        GameTypeName = item["gameTypeName"],
                        Min = item["min"],
                        Max = item["max"],
                        MaxPerMatch = item["maxPerMatch"]
                    };
                    if (sports.Min > sports.Max || sports.Max > sports.MaxPerMatch)
                        throw new AdminException("parameter error");
                    betSettings.Add(sports);
                }
                else if (tpCategory == (int)TpCategoryEnum.LiveCasino)
                {
                    
                    var liveCasino = new LiveCasinoSetting
                    {
                        Checked = (bool)item["checked"],
                        TpCategory = item["tpCategory"],
                        TpCategoryName = item["tpCategoryName"],
                        GameTypeId = item["gameTypeId"],
                        GameTypeName = item["gameTypeName"],
                        LimitGroup = item["limitGroup"],
                        LimitRange = item["limitRange"]
                    };
                    betSettings.Add(liveCasino);
                }
            }
            return betSettings;
        }

        private List<CustBetLimitSetting> ParseSetting(string betLimit)
        {
            var betSettings = new List<CustBetLimitSetting>();
            var setting = JsonConvert.DeserializeObject<dynamic>(betLimit);

            foreach (var item in setting)
            {
                var tpCategory = (int)item["TpCategory"];
                if (tpCategory == (int)TpCategoryEnum.Sports)
                {
                    var sports = new CustBetLimitSetting
                    {
                        Checked = item["Checked"],
                        TpCategory = item["TpCategory"],
                        TpCategoryName = item["TpCategoryName"],
                        GameTypeId = item["GameTypeId"],
                        GameTypeName = item["GameTypeName"],
                        Min = item["Min"],
                        Max = item["Max"],
                        MaxPerMatch = item["MaxPerMatch"]
                    };
                    if (sports.Min > sports.Max || sports.Max > sports.MaxPerMatch)
                        throw new AdminException("parameter error");
                    betSettings.Add(sports);
                }
                else if (tpCategory == (int)TpCategoryEnum.LiveCasino)
                {
                    var liveCasino = new CustBetLimitSetting
                    {
                        Checked = (bool)item["Checked"],
                        TpCategory = item["TpCategory"],
                        TpCategoryName = item["TpCategoryName"],
                        GameTypeId = item["GameTypeId"],
                        GameTypeName = item["GameTypeName"],
                        LimitGroup = item["LimitGroup"],
                        LimitRange = item["LimitRange"]
                    };
                    betSettings.Add(liveCasino);
                }
            }
            return betSettings;
        }
    }
}