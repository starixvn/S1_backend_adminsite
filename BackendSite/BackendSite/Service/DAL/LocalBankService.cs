using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using MySql.Data.MySqlClient;
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
            return _reportDbClient.Query<BankInfoOff>("Admin_BankAccountOff_Get");
        }

        public IEnumerable<BankInfoOff> GetBankAccountOff(int siteId)
        {
            return _reportDbClient.Query<BankInfoOff>("BankAccountOff_Get", new { SiteId = siteId });
        }

        public IEnumerable<BankInfoPay> GetBankInfoPayList()
        {
            return _reportDbClient.Query<BankInfoPay>("Common_BankInfoPayForPayout_Get");
        }
        
        public IEnumerable<EWalletPay> GetEWalletPayList()
        {
            return _reportDbClient.Query<EWalletPay>("Common_EWalletPayForPayout_Get");
        }

        public IEnumerable<BankInfoPay> GetCryptoPayList()
        {
            return _reportDbClient.Query<BankInfoPay>("Common_CryptoPayForPayout_Get");
        }

        public IEnumerable<BankInfoOff> GetBankAccountOffByCurrency(int currencyId)
        {
            return _reportDbClient.Query<BankInfoOff>("Admin_BankAccountOffByCurrency_Get", new DynamicParameters(new { CurrencyId = currencyId }));
        }
    }
}
