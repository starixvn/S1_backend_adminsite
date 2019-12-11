using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class PendingFundService
    {
        private readonly IConfiguration configuration;

        public PendingFundService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<PendingFund> PendingFundList(int siteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<PendingFund>("TmpPendingFund_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }
    }

}
