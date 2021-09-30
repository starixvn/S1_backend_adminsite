using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class LocalBankService
    {
        private readonly IConfiguration configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public LocalBankService(IConfiguration configuration, ILogger<MySqlClient> logger)
        {
            this.configuration = configuration;
             _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
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
            return _reportDbClient.Query<BankInfoOff>("Admin_BankAccountOffByCurrency_Get", new DynamicParameters(new { CurrencyId = currencyId }));
        }
    }
}
