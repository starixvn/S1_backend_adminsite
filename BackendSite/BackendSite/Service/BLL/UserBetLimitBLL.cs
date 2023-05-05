using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.BetLimit;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

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

                    var apiResult = await _courierAdapter.MyPostAsync<ApiResult, SetBetLimit>($"{_apiUrl}SetBetLimit", betLimit);
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

        private async Task ResetBetLimit(int custId, int gameType, int currencyId) 
        {
            var defaultBetLimit = await _userBetLimitService.GetLimitGroup(gameType, currencyId);

            var tpId = await _userBetLimitService.GetTpIdByGameType(gameType);
            var betLimitApi = $"{_apiUrl}SetBetLimit";
            var betLimit = new SetBetLimit
            {
                TpId = tpId,
                CustId = custId,
                BetSetting = defaultBetLimit.LimitGroup
            };
            var apiResult = await _courierAdapter.MyPostAsync<ApiResult, SetBetLimit>(betLimitApi, betLimit);
            if (apiResult.ErrorCode != 0)
                throw new AdminException("failed");
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

        private async Task<List<CustBetLimitSetting>> ParseSetting(string betLimit, int currencyId)
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
                    var gameType = (int)item["GameTypeId"];
                    var liveCasisnoGroup = await _userBetLimitService.GetLimitGroup(gameType, currencyId);
                    var liveCasino = new CustBetLimitSetting
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
