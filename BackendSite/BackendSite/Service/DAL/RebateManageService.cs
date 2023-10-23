using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using MySql.Data.MySqlClient;

using Newtonsoft.Json;

using static BackendSite.Controllers.ApiV2.Marketing.Bonus.RebateExcludeGameController;

namespace BackendSite.Service.DAL
{
    public class RebateManageService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly WinLostReportService _winLostReportService;
        private readonly CustInfoService _custInfoService;

        public RebateManageService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _winLostReportService = provider.GetRequiredService<WinLostReportService>();
            _custInfoService = provider.GetRequiredService<CustInfoService>();
        }

        private async Task<bool> AddNewOprateLog(int siteId, int currencyId, object beofreData, object afterData,
            AdminAdjustmentLogMenuEnum adminAdjustmentLogMenu, ActionType actionType, string updater, MySqlTransaction trans)
        {
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", siteId);
            logParam.Add("_CurrencyId", currencyId);
            logParam.Add("_Menu", (int)adminAdjustmentLogMenu);
            logParam.Add("_ActionType", (int)actionType);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", beofreData.ToLog());
            logParam.Add("_AfterDetailInfo", afterData.ToLog());
            logParam.Add("_Operator", updater);

            var logSuccess = await _mainDbClient.ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;

            if (logSuccess)
            {
                return true;
            }
            return false;
        }

        public IEnumerable<TPCategory> GetTPCategoryList()
        {
            return _reportDbClient.Query<TPCategory>("Admin_TPCategory_Get");
        }

        public IEnumerable<RebatePeriod> GetRebatePeriod(int siteId)
        {
            var p = new DynamicParameters(new { _SiteId = siteId });

            return _reportDbClient.Query<RebatePeriod>("Admin_RebatePeriod_Get_V2", p);
        }
        public async Task<bool> SetRebatePeriodAsync(int siteId, IEnumerable<int> currencyList, int periodType, int? periodDay, int redeemHours, string lastUpdateUser)
        {
            var result = true;

            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();
            var beforeData = GetRebatePeriod(siteId);
            foreach (var currencyId in currencyList)
            {
                var asignBeforeData = beforeData.FirstOrDefault(x => x.CurrencyId == currencyId);
                var afterData = new { SiteId = siteId, CurrencyId = currencyId, PeriodType = periodType, PeriodDay = periodDay, RedeemHours = redeemHours };
                if ((asignBeforeData?.PeriodType == periodType) && (asignBeforeData?.PeriodDay == periodDay) && (asignBeforeData?.RedeemHours == redeemHours)) continue;

                var parameters = new { SiteId = siteId, CurrencyId = currencyId, PeriodType = periodType, PeriodDay = periodDay, RedeemHours = redeemHours, LastUpdateUser = lastUpdateUser };

                if (_mainDbClient.Execute("Admin_RebatePeriod_Set_V2", trans, new DynamicParameters(parameters)) == 0)
                    result = false;

                if (!await AddNewOprateLog(siteId, currencyId, asignBeforeData, afterData, AdminAdjustmentLogMenuEnum.VipRebatePeriod,
                    (asignBeforeData == null ? ActionType.Create : ActionType.Update), lastUpdateUser, trans))
                    result = false;
            }

            if (result)
            {
                await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                return true;
            }
            else
            {
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }
        }

        public IEnumerable<RebateLevelData> GetRebateDataByCategory(int siteId, int categoryId, int currencyId)
        {
            return _reportDbClient.Query<RebateLevelData>("Admin_RebateCategory_Get", new DynamicParameters(new { SiteId = siteId, CategoryId = categoryId, CurrencyId = currencyId }));
        }
        public IEnumerable<RebateLevelData> GetRebateDataByGameType(int siteId, int gameType, int currencyId)
        {
            return _reportDbClient.Query<RebateLevelData>("Admin_RebateGameType_Get", new DynamicParameters(new { SiteId = siteId, GameType = gameType, CurrencyId = currencyId }));
        }

        public IEnumerable<RebateGameTypeDetail> GetRebateGameTypeList(int siteId, int currencyId, int gametype)
        {
            return _reportDbClient.Query<RebateGameTypeDetail>("Admin_RebateGameType_Get", new DynamicParameters(new { SiteId = siteId, GameType = gametype, CurrencyId = currencyId }));
        }
        public IEnumerable<RebatePayoutLimitData> GetRebatePayoutLimit(int siteId, int currencyId, int? categoryId)
        {
            return _reportDbClient.Query<RebatePayoutLimitData>("Admin_RebatePayoutLimit_Get", new DynamicParameters(new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId }));
        }
        public async Task<bool> SetRebatePayoutLimitAsync(int siteId, int currencyId, int categoryId, decimal maxPayout, decimal minPayout, string lastUpdateUser)
        {
            var result = true;

            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();
            var categoryData = GetTPCategoryList().FirstOrDefault(x => x.CategoryId == categoryId) ?? null;
            var asignBeforeData = GetRebatePayoutLimit(siteId, currencyId, categoryId).FirstOrDefault() ?? null;

            var beforeData = new { CategoryId = categoryId, CategoryName = categoryData?.CategoryName, MaxPayout = asignBeforeData?.MaxPayout, MinPayout = asignBeforeData?.MinPayout };
            var afterData = new { CategoryId = categoryId, CategoryName = categoryData?.CategoryName, MaxPayout = maxPayout, MinPayout = minPayout };
            if ((asignBeforeData?.MaxPayout == maxPayout) && (asignBeforeData?.MinPayout == minPayout)) return true;

            var parameters = new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId, MaxPayout = maxPayout, MinPayout = minPayout, LastUpdateUser = lastUpdateUser };
            if (_mainDbClient.Execute("Admin_RebatePayoutLimit_Set", trans, new DynamicParameters(parameters)) == 0)
                result = false;

            if (!await AddNewOprateLog(siteId, currencyId, beforeData, afterData, AdminAdjustmentLogMenuEnum.VipRebatePayoutLimit,
                (asignBeforeData == null ? ActionType.Create : ActionType.Update), lastUpdateUser, trans))
                result = false;
            if (result)
            {
                await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                return true;
            }
            else
            {
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }
        }
        public async Task<bool> SetRebateCategoryAsync(int siteId, int currencyId, int categoryId, int level, decimal percentage, string lastUpdateUser)
        {
            var result = true;

            var categoryData = GetTPCategoryList().FirstOrDefault(x => x.CategoryId == categoryId) ?? null;
            var asignBeforeData = GetRebateDataByCategory(siteId, categoryId, currencyId).FirstOrDefault(x => x.Level == level) ?? null;
            var vipLevelData = _custInfoService.GetAllSiteVipLevelList().FirstOrDefault(x => x.SiteId == siteId && x.Level == level) ?? null;

            var beforeData = new { CategoryId = categoryId, CategoryName = categoryData?.CategoryName, LevelName = vipLevelData?.LevelName, Percentage = asignBeforeData?.Percentage };
            var afterData = new { CategoryId = categoryId, CategoryName = categoryData?.CategoryName, LevelName = vipLevelData?.LevelName, Percentage = percentage };
            if (asignBeforeData?.Percentage == percentage) return true;

            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();

            var parameters = new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId, Level = level, Percentage = percentage, LastUpdateUser = lastUpdateUser };
            if (_mainDbClient.Execute("Admin_RebateCategory_Set", trans, new DynamicParameters(parameters)) == 0)
                result = false;

            if (!await AddNewOprateLog(siteId, currencyId, beforeData, afterData, AdminAdjustmentLogMenuEnum.VipRebateCategory,
                (asignBeforeData == null ? ActionType.Create : ActionType.Update), lastUpdateUser, trans))
                result = false;

            if (result)
            {
                await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                return true;
            }
            else
            {
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }
        }

        public async Task<bool> SetRebateGameTypeAsync(int siteId, int currencyId, int gameType, int level, decimal percentage, string lastUpdateUser, MySqlTransaction trans)
        {
            var result = true;
            List<int> siteList = new() { siteId };
            var gameTypeData = _winLostReportService.GetAllGameType(JsonConvert.SerializeObject(siteList), currencyId).FirstOrDefault(x => x.Type == gameType) ?? null;
            var asignBeforeData = GetRebateGameTypeList(siteId, currencyId, gameType).FirstOrDefault(x => x.Level == level) ?? null;
            var vipLevelData = _custInfoService.GetAllSiteVipLevelList().FirstOrDefault(x => x.SiteId == siteId && x.Level == level) ?? null;

            var beforeData = new { GameType = gameType, GameTypeName = gameTypeData?.TypeName, LevelName = vipLevelData?.LevelName, Percentage = asignBeforeData?.Percentage };
            var afterData = new { GameType = gameType, GameTypeName = gameTypeData?.TypeName, LevelName = vipLevelData?.LevelName, Percentage = percentage };
            if (asignBeforeData?.Percentage == percentage) return true;
            var parameters = new { SiteId = siteId, CurrencyId = currencyId, GameType = gameType, Level = level, Percentage = percentage, LastUpdateUser = lastUpdateUser };
            if (_mainDbClient.Execute("Admin_RebateGameType_Set", trans, new DynamicParameters(parameters)) == 0)
                result = false;

            if (!await AddNewOprateLog(siteId, currencyId, beforeData, afterData, AdminAdjustmentLogMenuEnum.VipRebateGameType,
                (asignBeforeData == null ? ActionType.Create : ActionType.Update), lastUpdateUser, trans))
                result = false;

            if (result)
                return true;
            else
                return false;
        }

        public async Task<IEnumerable<RebateExcludeGame>> GetRebateExcludeGameByCurrencyIdAsync(int siteId, int currencyId)
        {
            return await _reportDbClient.QueryAsync<RebateExcludeGame>("Admin_RebateExcludeGame_Get", new DynamicParameters(new { _SiteId = siteId, _CurrencyId = currencyId }));
        }

        public async Task<bool> SetRebateExcludeGameAsync(int siteId, int currencyId, IEnumerable<ExcludeGame> excludeGames, string updateOperator)
        {
            var result = true;

            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();

            var parameters = new { _SiteId = siteId, _CurrencyId = currencyId, _Infos = JsonConvert.SerializeObject(excludeGames), _LastUpdateUser = updateOperator };
            if (_mainDbClient.Execute("Admin_RebateExcludeGame_Set", trans, new DynamicParameters(parameters)) == 0)
                result = false;

            if (result)
            {
                await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                return true;
            }
            else
            {
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }
        }
    }
}
