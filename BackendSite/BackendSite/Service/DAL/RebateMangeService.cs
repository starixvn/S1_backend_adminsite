using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class RebateMangeService
    {
        private readonly IConfiguration configuration;

        public RebateMangeService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IEnumerable<VipLevel> GetVipLevel(int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<VipLevel>("Admin_VipLevel_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
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
        public int SetRebateCategory(int siteId, int currencyId, int categoryId, int level, decimal percentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RebateCategory_Set", new { siteId, currencyId, categoryId, level, percentage, lastUpdateUser }, commandType: CommandType.StoredProcedure);
        }
        public int SetRebateCategoryBatch(int siteId, int categoryId, string levelPercentage, string lastUpdateUser)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RebateCategoryBatch_Set", new { siteId, categoryId, levelPercentage, lastUpdateUser }, commandType: CommandType.StoredProcedure);
        }
    }
}
