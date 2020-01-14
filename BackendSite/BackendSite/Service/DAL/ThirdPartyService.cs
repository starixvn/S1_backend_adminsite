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
    public class ThirdPartyService
    {
        private readonly IConfiguration configuration;

        public ThirdPartyService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IEnumerable<TpWallet> GetTpWallet(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<TpWallet>("TpWallet_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }
    }
}
