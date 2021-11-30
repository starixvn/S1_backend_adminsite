using System;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class ServerInfoService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public ServerInfoService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public ServerInfo GetModel()
        {
            var siteId = 1;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<ServerInfo>("ServerInfo_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public ServerInfo GetModelWithSiteId(int siteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<ServerInfo>("ServerInfo_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<ServerInfo> GetSiteList()
        {
            return _mainDbClient.Query<ServerInfo>("Admin_ServerInfoList_Get");
        }
        public int AddAdminOperateLog(AddAdminOperateLog addAdminOperateLog)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Log_LogAdminOperateInfo_Ins", addAdminOperateLog, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<GetAdminOperateLog> GetAdminOperateLog(int custId,
            string operater,
            string menu,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId, dbType: DbType.Int32);
            p.Add("_Operator", operater, dbType: DbType.String);
            p.Add("_Menu", menu, dbType: DbType.String);
            p.Add("_StartDate", startDate.DateTime, dbType: DbType.DateTime);
            p.Add("_EndDate", endDate.DateTime, dbType: DbType.DateTime);
            p.Add("_StartIndex", startIndex,dbType: DbType.Int32);
            p.Add("_PageSize", pageSize,dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var adminOperateLogList = conn.Query<GetAdminOperateLog>("Log_LogAdminOperateInfo_Get_V3", p, commandType: CommandType.StoredProcedure);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return adminOperateLogList;
        }
    }
}
