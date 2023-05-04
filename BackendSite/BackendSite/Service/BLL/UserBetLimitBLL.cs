using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.BetLimit;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BackendSite.Service.BLL
{
    public class UserBetLimitBLL
    {
        private readonly CustInfoBLL _custInfoBll;
        private readonly UserBetLimitService _userBetLimitService;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly string _apiUrl;
        private readonly CourierAdapter _courierAdapter;
        private readonly ILogger<UserBetLimitBLL> _logger;

        public UserBetLimitBLL(CustInfoBLL custInfoBll, UserBetLimitService userBetLimitService, AdjustmentLogReportService adjustmentLogReportService, IConfiguration configuration, CourierAdapter courierAdapter, ILogger<UserBetLimitBLL> logger)
        {
            _userBetLimitService = userBetLimitService;
            _adjustmentLogReportService = adjustmentLogReportService;
            _apiUrl = configuration.GetValue<string>("TpService");
            _courierAdapter = courierAdapter;
            _custInfoBll = custInfoBll;
            _logger = logger;
        }

        public async Task<IEnumerable<TpCategoryBetLimit>> GetTpCategoryList()
        {
            var tpCategories = await _userBetLimitService.GetTpCategoryList();

            return tpCategories.Where(a => a.CategoryId == (int)TpCategoryEnum.LiveCasino)
                .Select(category => new TpCategoryBetLimit() { CategoryId = category.CategoryId, CategoryName = category.CategoryName }).ToList();
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

            var betSettings = (List<BetLimitSetting>)ParseSetting(req["after"].betSetting);
            foreach (var item in betSettings)
            {
                if (item.GameTypeId == 77) 
                {
                    var tpId = await _userBetLimitService.GetTpIdByGameType(item.GameTypeId);
                    var betLimitApi = $"{_apiUrl}SetBetLimit";
                    var betLimit = new SetBetLimit
                    {
                        TpId = tpId,
                        CustId = custId,
                        BetSetting = JsonConvert.SerializeObject(item)
                    };
                    var apiResult = await _courierAdapter.MyPostAsync<ApiResult, SetBetLimit>(betLimitApi, betLimit);
                    if (apiResult.ErrorCode != 0)
                        return -1;
                }
            }

            var result = await _userBetLimitService.SetBetLimit(siteId, custId, JsonConvert.SerializeObject(betSettings), updateOperator);
            if (result > 0) 
            {
                var after = new CustBetLimit()
                {
                    UserName = userName,
                    BetSetting = betSettings
                };
                var logParam = new CreateAdminAdjustmentInfoLogInput
                {
                    SiteId = siteId,
                    CurrencyId = currencyId,
                    MenuEnum = AdminAdjustmentLogMenuEnum.BetLimit,
                    ActionType = result == 1 ? ActionType.Create : ActionType.Update,
                    AfterDetailInfo = after.ToLog(),
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

        public async Task<int> Delete(ParamsWithLog<DeleteCustBetLimit> input)
        {
            return await _userBetLimitService.Delete(input);
        }

        public async Task<IEnumerable<GameTypeBetLimit>> GetTpGameTypeListbyCategoryAsync(int categoryId)
        {
            var tpGameType = await _userBetLimitService.GetTpGameTypeListbyCategoryAsync(categoryId);
            var gameTypeWithBetLimit = tpGameType
                .Select(a => new GameTypeBetLimit()
                {
                    TypeId = a.Type,
                    TypeName = a.TypeName
                });
            return gameTypeWithBetLimit;
        }

        public async Task<IEnumerable<SiteTpListIfo>> GetTpListByCategoryAsync(int categoryId)
        {
            var tpGameType = await _userBetLimitService.GetTpListByCategoryAsync(categoryId);

            return tpGameType;
        }

        public async Task<IEnumerable<TpBetLimitGroup>> GetTpBetLimitGroupAsync(int categoryId, int currencyId)
        {
            var tpGameType = await _userBetLimitService.GetTpGameTypeListbyCategoryAsync(categoryId);
            var listTpLimitGroup = new List<TpBetLimitGroup>();
            foreach (var type in tpGameType)
            {
                var betLimit = BetLimitFactory.Create(_userBetLimitService.ReportDbClient, (TpGameTypeEnum)type.Type);
                if (betLimit == null) continue;
                var group = await betLimit.GetLimitGroup(currencyId);
                var tpBetLimitGroup = new TpBetLimitGroup()
                {
                    TypeId = type.Type,
                    TypeName = type.TypeName,
                    SelectedValue = group.DefaultGroup,
                    LimitGroup = JsonConvert.DeserializeObject<List<LimitGroup>>(group.LimitGroup)
                };
                listTpLimitGroup.Add(tpBetLimitGroup);
            }
            return listTpLimitGroup;
        }

        public async Task<CustBetLimitByCustIdV2> GetByCustIdV2(int custId)
        {
            var betLimit = await _userBetLimitService.UserBetLimitGetByCustId(custId);
            var betSettings = await ParseSetting(betLimit.BetSetting, betLimit.CurrencyId);

            var resp = new CustBetLimitByCustIdV2()
            {
                custId = betLimit.CustId,
                UserName = betLimit.UserName,
                CurrencyId = betLimit.CurrencyId,
                BetSetting = betSettings
            };
            return resp;
        }

        public async Task<IEnumerable<TpBetLimitGroupV2>> GetTpBetLimitGroupV2Async(int currencyId)
        {
            var listTpLimitGroup = new List<TpBetLimitGroupV2>();
            var tpCategories = await _userBetLimitService.GetTpCategoryList();
            var tpCategoryBetLimitList = tpCategories.Where(a => a.CategoryId == (int)TpCategoryEnum.LiveCasino)
                .Select(category => new TpCategoryBetLimit() { CategoryId = category.CategoryId, CategoryName = category.CategoryName }).ToList();

            tpCategoryBetLimitList.ForEach(async x =>
            {
                var tpGameType = await _userBetLimitService.GetTpGameTypeListbyCategoryAsync(x.CategoryId);
                foreach (var type in tpGameType)
                {
                    var betLimit = BetLimitFactory.Create(_userBetLimitService.ReportDbClient, (TpGameTypeEnum)type.Type);
                    if (betLimit == null) continue;
                    var group = await betLimit.GetLimitGroup(currencyId);
                    var tpBetLimitGroup = new TpBetLimitGroupV2()
                    {
                        CategoryId = x.CategoryId,
                        CategoryName = x.CategoryName,
                        TypeId = type.Type,
                        TypeName = type.TypeName,
                        SelectedValue = group.DefaultGroup,
                        LimitGroup = JsonConvert.DeserializeObject<List<LimitGroup>>(group.LimitGroup)
                    };
                    listTpLimitGroup.Add(tpBetLimitGroup);
                }
            });

            return listTpLimitGroup;
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

        private async Task<List<CustBetLimitSetting>> ParseSetting(string betLimit, int currencyId)
        {
            var betSettings = new List<CustBetLimitSetting>();
            var setting = JsonConvert.DeserializeObject<dynamic>(betLimit);

            foreach (var item in setting)
            {
                var tpCategory = (int)item["TpCategory"];
                if (tpCategory == (int)TpCategoryEnum.Sports)
                {
                    var sports = new CustSportsSetting
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
                    var gameType = (int)item["GameTypeId"];
                    var liveCasisnoGroup = await _userBetLimitService.GetLimitGroup(gameType, currencyId);
                    var liveCasino = new CustLiveCasinoSetting
                    {
                        Checked = (bool)item["Checked"],
                        TpCategory = item["TpCategory"],
                        TpCategoryName = item["TpCategoryName"],
                        GameTypeId = item["GameTypeId"],
                        GameTypeName = item["GameTypeName"],
                        LimitGroup = item["LimitGroup"],
                        LimitRange = item["LimitRange"],
                        LimitGroupList = JsonConvert.DeserializeObject<List<LimitGroupList>>(liveCasisnoGroup.LimitGroup)
                    };
                    betSettings.Add(liveCasino);
                }
            }
            return betSettings;
        }
    }
}