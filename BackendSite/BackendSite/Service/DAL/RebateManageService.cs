using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class RebateManageService
    {
        private readonly IConfiguration configuration;

        public RebateManageService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        //public IEnumerable<VipLevel> GetVipLevel(int siteId)
        //{
        //    using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
        //    return conn.Query<VipLevel>("Admin_VipLevel_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
        //}
        private void SetRebateSettingHistory(AdjustmentInfoLogParam adjustmentInfoLogParam) 
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));  
            conn.Execute("Admin_AdjustmentInfoLog_Set", adjustmentInfoLogParam, commandType: CommandType.StoredProcedure);

        }
        public IEnumerable<TPCategory> GetTPCategoryList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<TPCategory>("Admin_TPCategory_Get", commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<TpGameType> GetTpGameTypeListbyCategory(int categoryId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<TpGameType>("Admin_TpGameTypebyCategory_Get", new { CategoryId = categoryId }, commandType: CommandType.StoredProcedure);
        }
        public int GetRebatePeriod(int siteId, out int periodType, out int periodDay, out int redeemHours)
        {
            var p = new DynamicParameters(new { SiteId = siteId});
            p.Add("PeriodType", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("PeriodDay", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RedeemHours", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_RebatePeriod_Get", p, commandType: CommandType.StoredProcedure);
            periodType = p.Get<int?>("PeriodType") ?? 0;
            periodDay = p.Get<int?>("PeriodDay") ?? 0;
            redeemHours = p.Get<int?>("RedeemHours") ?? 0;
            return result;
        }
        public bool SetRebatePeriod(int siteId, int periodType, int? periodDay, int redeemHours, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { SiteId = siteId, PeriodType = periodType, PeriodDay = periodDay, RedeemHours = redeemHours, LastUpdateUser = lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebatePeriod",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebatePeriod_Set", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool DelRebatePeriod(int siteId, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));

            var Parameters = new { SiteId = siteId };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebatePeriod",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebatePeriod_Del", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }
        public IEnumerable<RebateLevelData> GetRebateDataByCategory(int siteId, int categoryId, int currencyId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<RebateLevelData>("Admin_RebateCategory_Get", new { SiteId = siteId, CategoryId= categoryId, CurrencyId  = currencyId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<RebateLevelData> GetRebateDataByGameType(int siteId, int gameType, int currencyId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<RebateLevelData>("Admin_RebateGameType_Get", new { SiteId = siteId, GameType = gameType, CurrencyId = currencyId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<RebatePayoutLimitData> GetRebatePayoutLimit(int siteId, int currencyId,int? categoryId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<RebatePayoutLimitData>("Admin_RebatePayoutLimit_Get", new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId }, commandType: CommandType.StoredProcedure);
        }
        public bool SetRebatePayoutLimit(int siteId, int currencyId, int categoryId, decimal maxPayout , decimal minPayout, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId, MaxPayout = maxPayout, MinPayout = minPayout, LastUpdateUser = lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebatePayoutLimit",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebatePayoutLimit_Set", Parameters, commandType: CommandType.StoredProcedure) >0;
        }
        public bool SetRebatePayoutLimitBatch(int siteId, int currencyId, string levelPayout, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { SiteId = siteId, CurrencyId = currencyId, LevelPayout = levelPayout, LastUpdateUser = lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebatePayoutLimitBatch",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebatePayoutLimitBatch_Set", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool DelRebatePayoutLimit(int siteId, int currencyId, int categoryId, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebatePayoutLimit",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebatePayoutLimit_Del", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool SetRebateCategory(int siteId, int currencyId, int categoryId, int level, decimal percentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { siteId, currencyId, categoryId, level, percentage, lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebateCategory",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebateCategory_Set", Parameters, commandType: CommandType.StoredProcedure) >0;
        }
        public bool SetRebateCategoryBatch(int siteId, int CurrencyId, int categoryId, string levelPercentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { siteId, CurrencyId, categoryId, levelPercentage, lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebateCategoryBatch",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebateCategoryBatch_Set", Parameters, commandType: CommandType.StoredProcedure) >0;
        }
        public bool DelRebateCategory(int siteId, int currencyId, int categoryId, int level, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { siteId, currencyId, categoryId, level };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebateCategory",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebateCategory_Del", Parameters, commandType: CommandType.StoredProcedure) >0;
        }
        public bool SetRebateGameType(int siteId, int currencyId, int gameType, int level, decimal percentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { siteId, currencyId, gameType, level, percentage, lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebateGameType",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebateGameType_Set", Parameters, commandType: CommandType.StoredProcedure) >0;
        }
        public bool SetRebateGameTypeBatch(int siteId, int currencyId, int gameType, string levelPercentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { siteId, currencyId, gameType, levelPercentage, lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebateGameTypeBatch",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebateGameTypeBatch_Set", Parameters, commandType: CommandType.StoredProcedure)>0;
        }
        public bool DelRebateGameType(int siteId, int currencyId, int gameType, int level, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var Parameters = new { siteId, currencyId, gameType, level };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebateGameType",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam);
            return conn.Execute("Admin_RebateGameType_Del", Parameters, commandType: CommandType.StoredProcedure) >0;
        }
       
    }
}
