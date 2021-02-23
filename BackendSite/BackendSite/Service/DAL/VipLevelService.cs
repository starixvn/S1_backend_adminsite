using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

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
    }
}
