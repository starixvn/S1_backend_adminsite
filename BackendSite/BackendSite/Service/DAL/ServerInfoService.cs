using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class ServerInfoService
    {
        private readonly IConfiguration configuration;

        public ServerInfoService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public ServerInfo GetModel()
        {
            int siteId = 1;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<ServerInfo>("ServerInfo_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public ServerInfo GetModelWithSiteId(int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<ServerInfo>("ServerInfo_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<ServerInfo> GetSiteList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<ServerInfo>("Admin_ServerInfoList_Get", commandType: CommandType.StoredProcedure);
        }
        public int AddAdminOperateLog(AddAdminOperateLog addAdminOperateLog)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Log_LogAdminOperateInfo_Ins", addAdminOperateLog, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<GetAdminOperateLog> GetAdminOperateLog(int? SiteId, int? CustId, string Menu, string Operator, long StartDate, long EndDate, int PageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new { SiteId, CustId, Menu, Operator, StartDate, EndDate, PageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var adminOperateLogList = conn.Query<GetAdminOperateLog>("Log_LogAdminOperateInfo_Get", p, commandType: CommandType.StoredProcedure);
                pageNumber = p.Get<int?>("PageNumber") ?? 0;
                totalPages = p.Get<int?>("TotalPages") ?? 0;
                rowCount = p.Get<int?>("RowCount") ?? 0;
                return adminOperateLogList;
            }
        }
    }
}
