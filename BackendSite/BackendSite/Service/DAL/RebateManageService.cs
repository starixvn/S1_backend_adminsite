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
    public class RebateManageService
    {
        private readonly IConfiguration configuration;
        public RebateManageService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IEnumerable<RebateActionTime> GetRebateActionTime(int siteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebateActionTime_Get";
                return conn.Query<RebateActionTime>(spName, new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }
        public void SetRebateActionTime(int siteId, int periodType, int? periodDay, int redeemHours, string lastUpdateUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebateActionTime_Set";
                conn.Execute(spName, new { siteId, periodType, periodDay, redeemHours, lastUpdateUser}, commandType: CommandType.StoredProcedure);
            }
        }
        public void DelRebateActionTime(int siteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebateActionTime_Del";
                conn.Execute(spName, new { siteId }, commandType: CommandType.StoredProcedure);
            }
        }
        public void SetRebatePayoutLimit(int siteId, int currencyId, int groupID, int rollOver, decimal maxPayout, decimal minPayout, string lastUpdateUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebatePayoutLimit_Set";
                conn.Execute(spName, new { siteId, currencyId, groupID, rollOver, maxPayout, minPayout, lastUpdateUser}, commandType: CommandType.StoredProcedure);
            }
        }
        public void SetRebatePayoutLimitbatch(int siteId, int currencyId, string groupPayout, string lastUpdateUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebatePayoutLimitbatch_Set";
                conn.Execute(spName, new { siteId, currencyId, groupPayout, lastUpdateUser}, commandType: CommandType.StoredProcedure);
            }
        }
        public void DelRebatePayoutLimit(int siteId, int groupID, int currencyId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebatePayoutLimit_Del";
                conn.Execute(spName, new { siteId, groupID, currencyId }, commandType: CommandType.StoredProcedure);
            }
        }
        public IEnumerable<RebatePayoutLimit> GetRebatePayoutLimit(int siteId, int currencyId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebatePayoutLimit_Get";
                return conn.Query<RebatePayoutLimit>(spName, new { SiteId = siteId , CurrencyId  = currencyId }, commandType: CommandType.StoredProcedure);
            }
        }
        public void InsLevelGroup(int siteId, string groupName, int levelBase)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_LevelGroup_Ins";
                conn.Execute(spName, new { siteId, groupName, levelBase}, commandType: CommandType.StoredProcedure);
            }
        }
        public void InsLevelGroupbatch(int siteId, string groupBaseList)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_LevelGroupBatch_Ins";
                conn.Execute(spName, new { siteId, groupBaseList }, commandType: CommandType.StoredProcedure);
            }
        }
        public void UpdateLevelGroup(int groupID, int siteId, string groupName, int levelBase)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_LevelGroup_Set";
                conn.Execute(spName, new { groupID, siteId, groupName, levelBase}, commandType: CommandType.StoredProcedure);
            }
        }
        public void DelLevelGroup(int groupID)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_LevelGroup_Del";
                conn.Execute(spName, new {groupID}, commandType: CommandType.StoredProcedure);
            }
        }
        public IEnumerable<LevelGroup> GetLevelGroup(int siteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_LevelGroup_Get";
                return conn.Query<LevelGroup>(spName, new {siteId}, commandType: CommandType.StoredProcedure);
            }
        }
        public void SetRebateCategory(int siteId, int categoryId, int groupID ,decimal percentage, string lastUpdateUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebateCategory_Set";
                conn.Execute(spName, new { siteId, categoryId, groupID, percentage, lastUpdateUser }, commandType: CommandType.StoredProcedure);
            }
        }
        public void SetRebateCategoryBatch(int siteId, int categoryId, string groupPercentage, string lastUpdateUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebateCategoryBatch_Set";
                conn.Execute(spName, new { siteId, categoryId, groupPercentage, lastUpdateUser }, commandType: CommandType.StoredProcedure);
            }
        }
        public void DelRebateCategory(int siteId, int categoryId, int groupID)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebateCategory_Del";
                conn.Execute(spName, new { siteId, categoryId, groupID}, commandType: CommandType.StoredProcedure);
            }
        }
        public IEnumerable<RebateGroup> GetRebateCategory(int siteId, int categoryId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebateCategory_Get";
                return conn.Query<RebateGroup>(spName, new { siteId, categoryId}, commandType: CommandType.StoredProcedure);
            }
        }

        public void SetRebateGameType(int siteId, int gameType, int groupID, decimal percentage, string lastUpdateUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebateGameType_Set";
                conn.Execute(spName, new { siteId, gameType, groupID, percentage, lastUpdateUser }, commandType: CommandType.StoredProcedure);
            }
        }
        public void SetRebateGameTypeBatch(int siteId, int gameType, string groupPercentage, string lastUpdateUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebateGameTypeBatch_Set";
                conn.Execute(spName, new { siteId, gameType, groupPercentage, lastUpdateUser }, commandType: CommandType.StoredProcedure);
            }
        }
        public void DelRebateGameType(int siteId, int gameType, int groupID)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebateGameType_Del";
                conn.Execute(spName, new { siteId, gameType, groupID }, commandType: CommandType.StoredProcedure);
            }
        }
        public IEnumerable<RebateGroup> GetRebateGameType(int siteId, int gameType)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string spName = "Admin_RebateGameType_Get";
                return conn.Query<RebateGroup>(spName, new { siteId, gameType }, commandType: CommandType.StoredProcedure);
            }
        }

    }
   

}
