using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
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
            return conn.Execute("Admin_RebatePeriod_Set", new { SiteId = siteId, PeriodType = periodType, PeriodDay = periodDay, RedeemHours = redeemHours, LastUpdateUser = lastUpdateUser }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool DelRebatePeriod(int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RebatePeriod_Del", new { SiteId = siteId}, commandType: CommandType.StoredProcedure) > 0;
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
            return conn.Execute("Admin_RebatePayoutLimit_Set", new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId, MaxPayout = maxPayout, MinPayout = minPayout, LastUpdateUser = lastUpdateUser }, commandType: CommandType.StoredProcedure) >0;
        }
        public bool SetRebatePayoutLimitBatch(int siteId, int currencyId, string levelPayout, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RebatePayoutLimitBatch_Set", new { SiteId = siteId, CurrencyId = currencyId, LevelPayout = levelPayout, LastUpdateUser = lastUpdateUser }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool DelRebatePayoutLimit(int siteId, int currencyId, int categoryId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RebatePayoutLimit_Del", new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool SetRebateCategory(int siteId, int currencyId, int categoryId, int level, decimal percentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RebateCategory_Set", new { siteId, currencyId, categoryId, level, percentage, lastUpdateUser }, commandType: CommandType.StoredProcedure) >0;
        }
        public bool SetRebateCategoryBatch(int siteId, int CurrencyId, int categoryId, string levelPercentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RebateCategoryBatch_Set", new { siteId, CurrencyId, categoryId, levelPercentage, lastUpdateUser }, commandType: CommandType.StoredProcedure) >0;
        }
        public bool DelRebateCategory(int siteId, int currencyId, int categoryId, int level)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RebateCategory_Del", new { siteId, currencyId, categoryId, level }, commandType: CommandType.StoredProcedure) >0;
        }
        public bool SetRebateGameType(int siteId, int currencyId, int gameType, int level, decimal percentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RebateGameType_Set", new { siteId, currencyId, gameType, level, percentage, lastUpdateUser }, commandType: CommandType.StoredProcedure) >0;
        }
        public bool SetRebateGameTypeBatch(int siteId, int currencyId, int gameType, string levelPercentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RebateGameTypeBatch_Set", new { siteId, currencyId, gameType, levelPercentage, lastUpdateUser }, commandType: CommandType.StoredProcedure)>0;
        }
        public bool DelRebateGameType(int siteId, int currencyId, int gameType, int level)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RebateGameType_Del", new { siteId, currencyId, gameType, level}, commandType: CommandType.StoredProcedure) >0;
        }
       
    }
}
