using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class UserBetLimitBLL
    {
        private readonly CustInfoBLL _custInfoBll;
        private readonly UserBetLimitService _userBetLimitService;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly string _tpServiceApiUrl;
        private readonly CourierAdapter _courierAdapter;
        private readonly ILogger<UserBetLimitBLL> _logger;
        private readonly List<int> CallApi = new() { (int)TpGameTypeEnum.SBO_Sports };

        public UserBetLimitBLL(CustInfoBLL custInfoBll, 
            UserBetLimitService userBetLimitService,
            AdjustmentLogReportService adjustmentLogReportService,
            CourierAdapter courierAdapter,
            ConfigBLL configBll,
            ILogger<UserBetLimitBLL> logger)
        {
            _userBetLimitService = userBetLimitService;
            _adjustmentLogReportService = adjustmentLogReportService;
            _tpServiceApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").TpService;
            _courierAdapter = courierAdapter;
            _custInfoBll = custInfoBll;
            _logger = logger;
        }
        public async Task<IEnumerable<BetLimitOption>> GetSettingList(int siteId, int currencyId)
        {
            var listSetting = new List<BetLimitOption>();
            var dg = await _userBetLimitService.GetLimitGroup((int)TpGameTypeEnum.DG_LiveCasino, currencyId);
            if (dg != null)
            {
                var dgGroup = JsonConvert.DeserializeObject<List<LimitGroup>>(dg.LimitGroup);
                var dgOption = new BetLimitOption() { CategoryId = (int)TpCategoryEnum.LiveCasino, CategoryName = "Live Casino", GameType = (int)TpGameTypeEnum.DG_LiveCasino, GameTypeName = "DG L.Casino", IsGroup = true, LimitGroup = dgGroup, DefaultGroup = dg.DefaultGroup };
                listSetting.Add(dgOption);
            }

            var sbo = await _userBetLimitService.GetLimitGroup((int)TpGameTypeEnum.SBO_Sports, currencyId);
            if (sbo != null)
            {
                var sboGroup = JsonConvert.DeserializeObject<LimitInput>(sbo.LimitGroup);
                var sboOption = new BetLimitOption() { CategoryId = (int)TpCategoryEnum.Sports, CategoryName = "Sports", GameType = (int)TpGameTypeEnum.SBO_Sports, GameTypeName = "SBO Sports", IsGroup = false, LimitInput = new LimitInput() { Min = sboGroup.Min, Max = sboGroup.Max, MaxPerMatch = sboGroup.MaxPerMatch } };
                listSetting.Add(sboOption);
            }
            return listSetting;
        }
        public async Task<int> SetUserBetLimit(dynamic req, string updateOperator)
        {
            try
            {
                var userName = (string)req["after"].userName;
                var siteId = (int)req["siteId"];

                var customer = await _custInfoBll.GetCustId(null, userName, siteId);
                if (customer == null)
                {
                    throw new AdminException(ErrorCodeEnum.CustNotFound, "User Not Found");
                }
                var custId = customer.CustId;
                var currencyId = customer.CurrencyId;
                var custBetLimit = await _userBetLimitService.UserBetLimitGetByCustId(custId);

                var setting = new List<BetLimitSetting>();
                if (custBetLimit != null)
                    setting = ToObject(custBetLimit.BetSetting);

                List<BetLimitSetting> betSettings = await ToObject(currencyId, req["after"].betSetting);

                foreach (var item in betSettings)
                {
                    if (CallApi.Contains(item.GameTypeId))
                    {
                        var before = setting.FirstOrDefault(a => a.GameTypeId == item.GameTypeId) as SportsSetting;
                        var after = betSettings.FirstOrDefault(a => a.GameTypeId == item.GameTypeId) as SportsSetting;
                        var betLimit = await Compare(custId, before, after);
                        if (betLimit != null)
                        {
                            var apiResult = await SetBetLimit(betLimit);
                            if (apiResult.ErrorCode != 0)
                                return -1;
                        }
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
                        AfterDetailInfo = new AfterBetLimit()
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
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return 0;
            }
        }

        public Task<DataWithPage<UserBetLimit>> GetUserBetLimit(int siteId, IEnumerable<int> currencyIds, string userName, DateTime startTime, DateTime endTime, int pageSize, int pageNumber)
        {
            return _userBetLimitService.GetUserBetLimit(siteId, currencyIds, userName, startTime, endTime, pageSize, pageNumber);
        }
        public async Task<CustBetLimitByCustIdV2> GetByCustIdV2(int custId)
        {
            var betLimit = await _userBetLimitService.UserBetLimitGetByCustId(custId);
            if (betLimit == null) return null;

            var betSettings = ToObject(betLimit.BetSetting);

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

            var betSettings = ToObject(betLimit.BetSetting);
            foreach (var item in betSettings)
            {
                if (CallApi.Contains(item.GameTypeId))
                {
                    await ResetBetLimit(betLimit.CustId, item.GameTypeId, betLimit.CurrencyId);
                }
            }

            var result = await _userBetLimitService.Delete(input);
            if (result > 0)
            {
                var logParam = new CreateAdminAdjustmentInfoLogInput
                {
                    SiteId = input.SiteId,
                    CurrencyId = input.CurrencyId,
                    MenuEnum = AdminAdjustmentLogMenuEnum.BetLimit,
                    ActionType = ActionType.Delete,
                    BeforeDetailInfo = betLimit.ToLog(),
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
            return await _courierAdapter.MyPostAsync<ApiResult, SetBetLimit>($"{_tpServiceApiUrl}SetBetLimit", request);
        }

        private async Task<SetBetLimit> Compare(int custId, SportsSetting before, SportsSetting after)
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
        private async Task<List<BetLimitSetting>> ToObject(int currencyId, dynamic betSetting)
        {
            var betSettings = new List<BetLimitSetting>();
            foreach (var item in betSetting)
            {
                var tpCategory = (int)item["tpCategory"];
                var enable = (bool)item["checked"];
                var gameTypeId = (int)item["gameTypeId"];
                if (tpCategory == (int)TpCategoryEnum.Sports)
                {
                    TpBetLimitData spoDefault = await _userBetLimitService.GetLimitGroup(gameTypeId, currencyId);
                    var input = JsonConvert.DeserializeObject<LimitInput>(spoDefault.LimitGroup);
                    var sports = new SportsSetting
                    {
                        Checked = enable,
                        TpCategory = item["tpCategory"],
                        TpCategoryName = item["tpCategoryName"],
                        GameTypeId = item["gameTypeId"],
                        GameTypeName = item["gameTypeName"],
                        Min = enable ? item["min"] : input.Min,
                        Max = enable ? item["max"] : input.Max,
                        MaxPerMatch = enable ? item["maxPerMatch"] : input.MaxPerMatch
                    };
                    if (sports.Min > sports.Max || sports.Max > sports.MaxPerMatch)
                        throw new AdminException(ErrorCodeEnum.InvalidBetLimit);
                    betSettings.Add(sports);
                }
                else if (tpCategory == (int)TpCategoryEnum.LiveCasino)
                {
                    TpBetLimitData dgDefault = await _userBetLimitService.GetLimitGroup(gameTypeId, currencyId);
                    var liveCasino = new LiveCasinoSetting
                    {
                        Checked = enable,
                        TpCategory = item["tpCategory"],
                        TpCategoryName = item["tpCategoryName"],
                        GameTypeId = item["gameTypeId"],
                        GameTypeName = item["gameTypeName"],
                        LimitGroup = enable ? item["limitGroup"] : dgDefault.DefaultGroup,
                        LimitRange = item["limitRange"]
                    };
                    betSettings.Add(liveCasino);
                }
            }
            return betSettings;
        }

        private List<BetLimitSetting> ToObject(string betLimit)
        {
            var betSettings = new List<BetLimitSetting>();
            var setting = JsonConvert.DeserializeObject<dynamic>(betLimit);

            foreach (var item in setting)
            {
                var tpCategory = (int)item["TpCategory"];
                if (tpCategory == (int)TpCategoryEnum.Sports)
                {
                    var sports = new SportsSetting
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
                        throw new AdminException(ErrorCodeEnum.InvalidBetLimit);
                    betSettings.Add(sports);
                }
                else if (tpCategory == (int)TpCategoryEnum.LiveCasino)
                {
                    var liveCasino = new LiveCasinoSetting
                    {
                        Checked = item["Checked"],
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