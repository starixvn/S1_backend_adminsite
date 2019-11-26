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
    public class DepositRequestService
    {
        private readonly IConfiguration configuration;

        public DepositRequestService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<DepositRequest> GetDepositRequest()
        {
            //int siteId = 1;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<DepositRequest>("DepositRequest_Get", commandType: CommandType.StoredProcedure);
            }
        }

        public DepositRequest GetDepositRequest(string transId)
        {
            //int siteId = 1;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<DepositRequest>("DepositRequestTransID_Get", new { TransId = transId }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
