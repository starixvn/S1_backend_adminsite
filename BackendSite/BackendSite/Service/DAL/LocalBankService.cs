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

        public IEnumerable<BankInfoOff> GetAllBankAccountOff()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<BankInfoOff>("Admin_BankAccountOff_Get", commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<BankInfoOff> GetBankAccountOff(int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<BankInfoOff>("BankAccountOff_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<BankInfoPay> GetBankInfoPayList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<BankInfoPay>("Common_BankInfoPayForPayout_Get", commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<BankInfoPay> GetCryptoPayList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<BankInfoPay>("Common_CryptoPayForPayout_Get", commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<BankInfoOff> GetBankAccountOffByCurrency(int currencyId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<BankInfoOff>("Admin_BankAccountOffByCurrency_Get", new { CurrencyId = currencyId }, commandType: CommandType.StoredProcedure);
        }
    }
}
