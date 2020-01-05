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
    public class LocalBankService
    {
        private readonly IConfiguration configuration;

        public LocalBankService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<BankInfoOff> GetBankAccountOff(int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<BankInfoOff>("BankAccountOff_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<BankInfoPay> GetBankInfoPayList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<BankInfoPay>("BankInfoPay_Get", commandType: CommandType.StoredProcedure);
        }
    }
}
