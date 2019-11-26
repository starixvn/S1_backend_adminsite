using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class DepositService
    {
        private readonly IConfiguration configuration;

        public DepositService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<DepositRequest> GetDepositRequestList(int siteId)
        {
            //int siteId = 1;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<DepositRequest>("DepositRequest_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public DepositRequest GetDepositRequest(string transId, int siteId)
        {
            //int siteId = 1;
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);

            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<DepositRequest>("DepositRequestTransID_Get", new { DepositId = depositId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
