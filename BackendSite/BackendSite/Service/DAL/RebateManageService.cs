using System;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using BackendSite.Extensions;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class RebateManageService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly WinLostReportService _winLostReportService;

        public RebateManageService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _winLostReportService = provider.GetRequiredService<WinLostReportService>();
        }
        //public IEnumerable<VipLevel> GetVipLevel(int siteId)
        //{
        //    using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
        //    return conn.Query<VipLevel>("Admin_VipLevel_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
        //}
        private void SetRebateSettingHistory(AdjustmentInfoLogParam adjustmentInfoLogParam)
        {
            _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", new DynamicParameters(adjustmentInfoLogParam));
        }

        private async Task<bool> AddNewOprateLog(int siteId, int currencyId, object beofreData, object afterData,
            AdminAdjustmentLogMenuEnum adminAdjustmentLogMenu, ActionType actionType, string updater, MySqlConnection conn, MySqlTransaction trans)
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

            var logSuccess = await conn.ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

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
        public IEnumerable<TpGameType> GetTpGameTypeListByCategory(int categoryId)
        {
            return _reportDbClient.Query<TpGameType>("Admin_TpGameTypebyCategory_Get", new DynamicParameters(new { CategoryId = categoryId }));
        }
        public IEnumerable<RebatePeriod> GetRebatePeriod(int siteId)
        {
            var p = new DynamicParameters(new { _SiteId = siteId });

            return _reportDbClient.Query<RebatePeriod>("Admin_RebatePeriod_Get_V2", p);
        }
        public async Task<bool> SetRebatePeriodAsync(int siteId, IEnumerable<int> currencyList, int periodType, int? periodDay, int redeemHours, string lastUpdateUser)
        {
            var result = true;

            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var beforeData = GetRebatePeriod(siteId);
            foreach (var currencyId in currencyList)
            {
                var asignBeforeData = beforeData.Where(x => x.CurrencyId == currencyId).FirstOrDefault() ?? null;
                var afterData = new { SiteId = siteId, CurrencyId = currencyId, PeriodType = periodType, PeriodDay = periodDay, RedeemHours = redeemHours };
                if (beforeData.ToLog() == afterData.ToLog()) return true;
                
                var parameters = new { SiteId = siteId, CurrencyId = currencyId, PeriodType = periodType, PeriodDay = periodDay, RedeemHours = redeemHours, LastUpdateUser = lastUpdateUser };

                if (conn.Execute("Admin_RebatePeriod_Set_V2", new DynamicParameters(parameters), commandType: CommandType.StoredProcedure) == 0)
                    result = false;

                if (!await AddNewOprateLog(siteId, currencyId, asignBeforeData, afterData, AdminAdjustmentLogMenuEnum.VipRebatePeriod,
                    (asignBeforeData == null ? ActionType.Create : ActionType.Update), lastUpdateUser, conn, trans))
                    result = false;
            }

            if (result)
            {
                trans.Commit();
                conn.Close();
                return true;
            }
            else
            {
                trans.Rollback();
                conn.Close();
                return false;
            }
        }
        public bool DelRebatePeriod(int siteId, string lastUpdateUser)
        {
            var parameters = new { SiteId = siteId };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebatePeriod",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return _mainDbClient.Execute("Admin_RebatePeriod_Del", new DynamicParameters(parameters)) > 0;

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

            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var categoryData = GetTPCategoryList().FirstOrDefault(x => x.CategoryId == categoryId) ?? null;
            var asignBeforeData = GetRebatePayoutLimit(siteId, currencyId, categoryId).FirstOrDefault() ?? null;
            var beforeData = new { CategoryId = categoryId, CategoryName = categoryData?.CategoryName, MaxPayout = asignBeforeData?.MaxPayout, MinPayout = asignBeforeData?.MinPayout };
            var afterData = new { CategoryId = categoryId, CategoryName = categoryData?.CategoryName, MaxPayout = maxPayout, MinPayout = minPayout };
            if (beforeData.ToLog() == afterData.ToLog()) return true;

            var parameters = new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId, MaxPayout = maxPayout, MinPayout = minPayout, LastUpdateUser = lastUpdateUser };
            if (conn.Execute("Admin_RebatePayoutLimit_Set", new DynamicParameters(parameters), commandType: CommandType.StoredProcedure) == 0)
                result = false;

            if (!await AddNewOprateLog(siteId, currencyId, beforeData, afterData, AdminAdjustmentLogMenuEnum.VipRebatePayoutLimit,
                (asignBeforeData == null ? ActionType.Create : ActionType.Update), lastUpdateUser, conn, trans))
                result = false;
            if (result)
            {
                trans.Commit();
                conn.Close();
                return true;
            }
            else
            {
                trans.Rollback();
                conn.Close();
                return false;
            }
        }
        public bool SetRebatePayoutLimitBatch(int siteId, int currencyId, string levelPayout, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var parameters = new { SiteId = siteId, CurrencyId = currencyId, LevelPayout = levelPayout, LastUpdateUser = lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebatePayoutLimitBatch",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return _mainDbClient.Execute("Admin_RebatePayoutLimitBatch_Set", new DynamicParameters(parameters)) > 0;
        }
        public bool DelRebatePayoutLimit(int siteId, int currencyId, int categoryId, string lastUpdateUser)
        {
            var parameters = new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebatePayoutLimit",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return _mainDbClient.Execute("Admin_RebatePayoutLimit_Del", new DynamicParameters(parameters)) > 0;
        }

        public async Task<bool> SetRebateCategoryAsync(int siteId, int currencyId, int categoryId, int level, decimal percentage, string lastUpdateUser)
        {
            var result = true;

            var categoryData = GetTPCategoryList().FirstOrDefault(x => x.CategoryId == categoryId) ?? null;
            var asignBeforeData = GetRebateDataByCategory(siteId, categoryId, currencyId).FirstOrDefault(x => x.Level == level) ?? null;

            var beforeData = new { CategoryId = categoryId, CategoryName = categoryData?.CategoryName, Level = asignBeforeData?.Level, Percentage = asignBeforeData?.Percentage };
            var afterData = new { CategoryId = categoryId, CategoryName = categoryData?.CategoryName, Level = level, Percentage = percentage };
            if (beforeData.ToLog() == afterData.ToLog()) return true;

            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var parameters = new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId, Level = level, Percentage = percentage, LastUpdateUser = lastUpdateUser };
            if (conn.Execute("Admin_RebateCategory_Set", new DynamicParameters(parameters), commandType: CommandType.StoredProcedure) == 0)
                result = false;

            if (!await AddNewOprateLog(siteId, currencyId, beforeData, afterData, AdminAdjustmentLogMenuEnum.VipRebateCategory,
                (asignBeforeData == null ? ActionType.Create : ActionType.Update), lastUpdateUser, conn, trans))
                result = false;

            if (result)
            {
                trans.Commit();
                conn.Close();
                return true;
            }
            else
            {
                trans.Rollback();
                conn.Close();
                return false;
            }
        }

        public bool SetRebateCategoryBatch(int siteId, int CurrencyId, int categoryId, string levelPercentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { siteId, CurrencyId, categoryId, levelPercentage, lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebateCategoryBatch",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebateCategoryBatch_Set", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }

        public bool DelRebateCategory(int siteId, int currencyId, int categoryId, int level, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { siteId, currencyId, categoryId, level };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebateCategory",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebateCategory_Del", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }

        public async Task<bool> SetRebateGameTypeAsync(int siteId, int currencyId, int gameType, int level, decimal percentage, string lastUpdateUser, MySqlConnection conn, MySqlTransaction trans)
        {
            var result = true;

            var gameTypeData = _winLostReportService.GetAllGameType().FirstOrDefault(x => x.Type == gameType) ?? null;
            var asignBeforeData = GetRebateGameTypeList(siteId, currencyId, gameType).FirstOrDefault(x => x.Level == level) ?? null;

            var beforeData = new { GameType = gameType, GameTypeName = gameTypeData?.TypeName, Level = asignBeforeData?.Level, Percentage = asignBeforeData?.Percentage };
            var afterData = new { GameType = gameType, GameTypeName = gameTypeData?.TypeName, Level = level, Percentage = percentage };
            if (beforeData.ToLog() == afterData.ToLog()) return true;
            var parameters = new { SiteId = siteId, CurrencyId = currencyId, GameType = gameType, Level = level, Percentage = percentage, LastUpdateUser = lastUpdateUser };
            if (conn.Execute("Admin_RebateGameType_Set", new DynamicParameters(parameters), commandType: CommandType.StoredProcedure) == 0)
                result = false;

            if (!await AddNewOprateLog(siteId, currencyId, beforeData, afterData, AdminAdjustmentLogMenuEnum.VipRebateCategory,
                (asignBeforeData == null ? ActionType.Create : ActionType.Update), lastUpdateUser, conn, trans))
                result = false;

            if (result)
                return true;
            else
                return false;
        }

        public bool SetRebateGameTypeBatch(int siteId, int currencyId, int gameType, string levelPercentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { siteId, currencyId, gameType, levelPercentage, lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebateGameTypeBatch",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebateGameTypeBatch_Set", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }

        public bool DelRebateGameType(int siteId, int currencyId, int gameType, int level, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { siteId, currencyId, gameType, level };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebateGameType",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebateGameType_Del", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }


    }
}
