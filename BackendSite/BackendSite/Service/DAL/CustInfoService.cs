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
    public class CustInfoService
    {
        private readonly IConfiguration configuration;

        public CustInfoService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IEnumerable<dynamic> GetCustInfoList(string userName, string transId, int siteId, int pageSize, ref int pageNumber, out int totalPages)
        {
            totalPages = 0;
            var p = new DynamicParameters(new { UserName = userName, TransId = transId, SiteId = siteId, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.Query<dynamic>("CustInfoSearch_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            return result;
        }

        public dynamic GetCustInfo(int custId, int siteId)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.QueryFirstOrDefault<dynamic>("CustInfoDetailSearch_Get", new { CustId = custId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
            return result;
        }
    }
}
