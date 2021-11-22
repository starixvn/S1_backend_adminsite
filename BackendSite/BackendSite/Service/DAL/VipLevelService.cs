using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using BackendSite.Service.Model.CustVipLevel;

namespace BackendSite.Service.DAL
{
    public class VipLevelService
    {
        private readonly IConfiguration _configuration;

        public VipLevelService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<VipLevelSetting> GetVipLevelSetting(int SiteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var vipLevelSetting = conn.Query<VipLevelSetting>("Admin_VipLevel_Get", new { SiteId },
                commandType: CommandType.StoredProcedure);
            return vipLevelSetting;
        }

        public VipLevelInfo GetVipLevelInfo(int CustId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var vipLevelInfo = conn.QueryFirstOrDefault<VipLevelInfo>("Admin_CustVipLevel_Get_V3", new { CustId },
                commandType: CommandType.StoredProcedure);
            return vipLevelInfo;
        }

        public IEnumerable<VipLevelLog> GetVipLevelLog(int CustId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var GetVipLevelLog = conn.Query<VipLevelLog>("Admin_CustVipLevelLog_Get_V3", new { CustId },
                commandType: CommandType.StoredProcedure);
            return GetVipLevelLog;
        }

        public int SetVipLevel(int SiteId, int CustId, string UserName, int VipLevel, string Memo, string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_CustVipLevel_Set",
                new { SiteId, CustId, UserName, VipLevel, Memo, Operator }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public int SetVipLevelManually(int SiteId, int CustId, string UserName, int Manually, string Memo,
            string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_CustVipLevelManually_Set",
                new { SiteId, CustId, UserName, Manually, Memo, Operator }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public IEnumerable<VipSettingModel> GetVipSetting(int vipType)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("_Status", vipType);
            return conn.Query<VipSettingModel>("Admin_VipSetting_Get", props,
                commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<VipUpgradeReviewDataModel> GetVipUpgradeReviewList(int siteId,
            string calculateMonth,
            int status,
            string userName,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount)
        {
            var props = new DynamicParameters();
            using var conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            props.Add("_SiteId", siteId);
            props.Add("_CalculateMonth", calculateMonth);
            props.Add("_Status", status);
            props.Add("_UserName", userName);
            props.Add("_PageSize", pageSize);
            props.Add("_StartIndex", startIndex);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = conn.Query<VipUpgradeReviewDataModel>("Admin_VipUpgradeReview_Get", props,
                commandType: CommandType.StoredProcedure);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public bool UpdateVipUpgradeReviewFlow(string custIds,
            string yearMonth,
            int newStatus,
            string updater)
        {
            var props = new DynamicParameters();
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));

            props.Add("_CustIds", custIds);
            props.Add("_CalculateMonth", yearMonth);
            props.Add("_Status", newStatus);
            props.Add("_LastUpdateUser", updater);

            conn.Execute("Admin_VipUpgradeReviewV2_Set", props, commandType: CommandType.StoredProcedure);

            return true;
        }

        public IEnumerable<VipDowngradeReviewDataModel> GetVipDowngradeReviewList(int siteId,
            string calculateYearMonth,
            int status,
            string userName,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CalculateYearMonth", calculateYearMonth);
            props.Add("_Status", status);
            props.Add("_UserName", userName);
            props.Add("_PageSize", pageSize);
            props.Add("_StartIndex", startIndex);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<VipDowngradeReviewDataModel>("Admin_VipDowngradeReview_Get", props,
                commandType: CommandType.StoredProcedure);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public bool UpdateVipDowngradeReviewFlow(string custIdList,
            string calculateYearMonth,
            int newStatus,
            string updater)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));

            var props = new DynamicParameters();
            props.Add("_CustIdList", custIdList);
            props.Add("_CalculateYearMonth", calculateYearMonth);
            props.Add("_Status", newStatus);
            props.Add("_LastUpdateUser", updater);

            conn.Execute("Admin_VipDowngradeReview_Set", props, commandType: CommandType.StoredProcedure);

            return true;
        }

        #region Private Methods

        private int CheckRowForUpdateVipDowngradeReview(string custIdList,
            string calculateYearMonth,
            int newStatus,
            MySqlConnection conn,
            MySqlTransaction trans)
        {
            var props = new DynamicParameters();
            props.Add("_CustIdList", custIdList);
            props.Add("_CalculateYearMonth", calculateYearMonth);
            props.Add("_Status", newStatus);

            return conn.ExecuteScalar<int>("Admin_VipDowngradeReviewForSet_Get", props, trans,
                commandType: CommandType.StoredProcedure);
        }

        private bool UpdateVipDowngradeReview(string custIdList,
            string calculateYearMonth,
            int newStatus,
            string updater,
            int expectedCount,
            MySqlConnection conn,
            MySqlTransaction trans)
        {
            var props = new DynamicParameters();
            props.Add("_CustIdList", custIdList);
            props.Add("_CalculateYearMonth", calculateYearMonth);
            props.Add("_Status", newStatus);
            props.Add("_LastUpdateUser", updater);

            var affectCount = conn.Execute("Admin_VipDowngradeReview_Set", props, trans,
                commandType: CommandType.StoredProcedure);


            return expectedCount == affectCount;
        }

        #endregion
    }
}