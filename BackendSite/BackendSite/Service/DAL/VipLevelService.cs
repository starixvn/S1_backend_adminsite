using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class VipLevelService
    {
        private readonly IConfiguration configuration;
        public VipLevelService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IEnumerable<VipLevelSetting> GetVipLevelSetting(int SiteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var vipLevelSetting = conn.Query<VipLevelSetting>("Admin_VipLevel_Get", new { SiteId }, commandType: CommandType.StoredProcedure);
            return vipLevelSetting;
        }
        public VipLevelInfo GetVipLevelInfo(int CustId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var vipLevelInfo = conn.QueryFirstOrDefault<VipLevelInfo>("Admin_CustVipLevel_Get", new { CustId }, commandType: CommandType.StoredProcedure);
            return vipLevelInfo;
        }
        public IEnumerable<VipLevelLog> GetVipLevelLog(int CustId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var GetVipLevelLog = conn.Query<VipLevelLog>("Admin_CustVipLevelLog_Get", new { CustId }, commandType: CommandType.StoredProcedure);
            return GetVipLevelLog;
        }
        public int SetVipLevel(int SiteId, int CustId, string UserName, int VipLevel, string Memo, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_CustVipLevel_Set", new { SiteId, CustId, UserName, VipLevel, Memo, Operator }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public int SetVipLevelManually(int SiteId, int CustId, string UserName, int Manually, string Memo, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_CustVipLevelManually_Set", new { SiteId, CustId, UserName, Manually, Memo, Operator }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public VipUpgradeReviewData GetVipUpgradeReview(string YearMonth, int siteid, int PageNumber, int PageSize, int Status, string UserName)
        {
            VipUpgradeReviewData SearchData = new VipUpgradeReviewData();
            DynamicParameters props = new DynamicParameters();
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            props.Add("CalculateMonth", YearMonth);
            props.Add("SiteId", siteid);
            props.Add("UserName", string.IsNullOrWhiteSpace(UserName) ? "" : UserName);
            props.Add("Status", Status);
            props.Add("UpgradeTime", null);
            props.Add("PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", PageSize);
            var result = conn.Query<VipUpgradeList>("Service_VipUpgradeReview_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.VipUpgradeList = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return SearchData;
        }
        public int UpdateVipUpgradeReview(string CustIds, string YearMonth)
        {
            DynamicParameters props = new DynamicParameters();
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));

            props.Add("CustIds", CustIds);
            props.Add("CalculateMonth", YearMonth);
            props.Add("Status", (int)VipStatus.Approved);

            return conn.Execute("Admin_VipUpgradeReview_Set", props, commandType: CommandType.StoredProcedure);
        }

        public List<VipUpgradeList> GetVipUpgradedata(int SiteId, string YearMonth, int Status, string UserName)
        {
            DynamicParameters props = new DynamicParameters();
            props.Add("CalculateMonth", YearMonth);
            props.Add("SiteId", SiteId);
            props.Add("UserName", string.IsNullOrWhiteSpace(UserName) ? "" : UserName);
            props.Add("Status", Status);
            props.Add("UpgradeTime", null);
            props.Add("PageSize", -1);
            props.Add("PageNumber", 0, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<VipUpgradeList>("Service_VipUpgradeReview_Get", props, commandType: CommandType.StoredProcedure).ToList();
        }
    }
}
