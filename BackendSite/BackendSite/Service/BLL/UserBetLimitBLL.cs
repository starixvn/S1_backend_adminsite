using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.BetLimit;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
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
        private readonly ILogger<UserBetLimitBLL> _logger;

        public UserBetLimitBLL(CustInfoBLL custInfoBll, UserBetLimitService userBetLimitService, AdjustmentLogReportService adjustmentLogReportService, ILogger<UserBetLimitBLL> logger)
        {
            _userBetLimitService = userBetLimitService;
            _adjustmentLogReportService = adjustmentLogReportService;
            _logger = logger;
            _custInfoBll = custInfoBll;
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

            var customer = await _custInfoBll.GetCustId(null, userName, req.SiteId);
            if (customer == null)
            {
                throw new AdminException(ErrorCodeEnum.NoData, "User Not Found");
            }
            var custId = customer.CustId;
            var siteId = customer.SiteId;
            var currencyId = customer.CurrencyId;

            var betSetting = new List<BetLimitSetting>();
            foreach (var item in req["after"].betSetting)
            {
                var tpCategory = (int)item.tpCategory;
                if (tpCategory == 1)
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
                    betSetting.Add(sports);
                }
                else if (tpCategory == 3) 
                {
                    var liveCasino = new LiveCasinoSetting
                    {
                        Checked = (bool)item["checked"],
                        TpCategory = item["tpCategory"],
                        TpCategoryName = item["tpCategoryName"],
                        GameTypeId = item["gameTypeId"],
                        GameTypeName = item["gameTypeName"],
                        LimitGroup = item["limitGroup"],
                        LimitRange = item["limitRange"],
                    };
                    betSetting.Add(liveCasino);
                }
            }

            var result = await _userBetLimitService.SetBetLimit(siteId, custId, JsonConvert.SerializeObject(betSetting), updateOperator);
            if (result > 0) 
            {
                var after = new CustBetLimit()
                {
                    UserName = userName,
                    BetSetting = betSetting
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

        public IEnumerable<UserBetLimit> GetUserBetLimit(IEnumerable<int> siteIds, IEnumerable<int> currencyIds, string userName, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            return _userBetLimitService.GetUserBetLimit(siteIds, currencyIds, userName, startDate, endDate, pageSize, pageNumber, out totalPages, out rowCount);
        }

        public async Task<int> Delete(ParamsWithLog<DeleteCustBetLimit> input)
        {
            return await _userBetLimitService.Delete(input);
        }

        public async Task<int> UpdateStatus(int custId, int status, string updateOperator)
        {
            return await _userBetLimitService.UpdateStatus(custId, status, updateOperator);
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

        public async Task<CustBetLimitByCustId> GetByCustId(int custId)
        {
            var custBetSetting = await _userBetLimitService.UserBetLimitGetByCustId(custId);
            var betSetting = JsonConvert.DeserializeObject<List<BetSetting>>(custBetSetting.BetSetting);

            var tpCategories = await _userBetLimitService.GetTpCategoryList();
            var listCategoryBetLimit = new List<TpCategoryBetLimit>();
            foreach (var category in tpCategories.Where(a => a.CategoryId == (int)TpCategoryEnum.LiveCasino))
            {
                var tpGameType = await _userBetLimitService.GetTpGameTypeListbyCategoryAsync(category.CategoryId);
                var limitGroups = new List<TpBetLimitGroup>();
                var liveCasinoSetting = betSetting.FirstOrDefault(a => a.TpCategory == category.CategoryId);
                foreach (var gameType in tpGameType)
                {
                    var betLimit = BetLimitFactory.Create(_userBetLimitService.ReportDbClient, (TpGameTypeEnum)gameType.Type);
                    if (liveCasinoSetting == null || betLimit == null) continue;
                    var group = await betLimit.GetLimitGroup(custBetSetting.CurrencyId, liveCasinoSetting.TpBetSetting);
                    var tpGroup = new TpBetLimitGroup()
                    {
                        TypeId = gameType.Type,
                        TypeName = gameType.TypeName,
                        Checked = group.Checked,
                        SelectedValue = group.DefaultGroup,
                        LimitGroup = JsonConvert.DeserializeObject<List<LimitGroup>>(group.LimitGroup)
                    };
                    limitGroups.Add(tpGroup);
                }



                var categoryBetLimit = new TpCategoryBetLimit()
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    Checked = true,
                    GameTypeBetLimit = limitGroups
                };
                listCategoryBetLimit.Add(categoryBetLimit);
            }
            var resp = new CustBetLimitByCustId()
            {
                SiteId = custBetSetting.SiteId,
                SiteName = custBetSetting.SiteName,
                custId = custBetSetting.CustId,
                UserName = custBetSetting.UserName,
                CurrencyId = custBetSetting.CurrencyId,
                BetSetting = listCategoryBetLimit
            };
            return resp;
        }

        public async Task<IEnumerable<TpBetLimitGroup>> GetTpBetLimitGroupAsync(int currencyId)
        {
            var listTpLimitGroup = new List<TpBetLimitGroup>();
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
                    var tpBetLimitGroup = new TpBetLimitGroup()
                    {
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

        public async Task<CustBetLimitByCustIdV2> GetByCustIdV2(int custId)
        {
            var custBetSetting = await _userBetLimitService.UserBetLimitGetByCustId(custId);
            var betSetting = JsonConvert.DeserializeObject<List<BetSetting>>(custBetSetting.BetSetting);

            var tpCategories = await _userBetLimitService.GetTpCategoryList();
            var limitGroups = new List<TpBetLimitGroupV2>();
            foreach (var category in tpCategories.Where(a => a.CategoryId == (int)TpCategoryEnum.LiveCasino))
            {
                var tpGameType = await _userBetLimitService.GetTpGameTypeListbyCategoryAsync(category.CategoryId);
                var liveCasinoSetting = betSetting.FirstOrDefault(a => a.TpCategory == category.CategoryId);
                foreach (var gameType in tpGameType)
                {
                    var betLimit = BetLimitFactory.Create(_userBetLimitService.ReportDbClient, (TpGameTypeEnum)gameType.Type);
                    if (liveCasinoSetting == null || betLimit == null) continue;
                    var group = await betLimit.GetLimitGroup(custBetSetting.CurrencyId, liveCasinoSetting.TpBetSetting);
                    var tpGroup = new TpBetLimitGroupV2()
                    {
                        CategoryId = category.CategoryId,
                        CategoryName = category.CategoryName,
                        TypeId = gameType.Type,
                        TypeName = gameType.TypeName,
                        Checked = group.Checked,
                        SelectedValue = group.DefaultGroup,
                        LimitGroup = JsonConvert.DeserializeObject<List<LimitGroup>>(group.LimitGroup)
                    };
                    limitGroups.Add(tpGroup);
                }
            }
            var resp = new CustBetLimitByCustIdV2()
            {
                SiteId = custBetSetting.SiteId,
                SiteName = custBetSetting.SiteName,
                custId = custBetSetting.CustId,
                UserName = custBetSetting.UserName,
                CurrencyId = custBetSetting.CurrencyId,
                GameTypeBetLimit = limitGroups
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
    }
}