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
        
        /// <summary>
        /// API V2 使用，取代 GetAllBankAccountOff、GetBankAccountOff
        /// </summary>
        public IEnumerable<OfflineBankAccount> GetOfflineBankAccount(int siteId = 0, int currencyId = 0)
        {
            var p = new DynamicParameters(new
            {
                _SiteId = siteId,
                _CurrencyId = currencyId
            });
            
            var a = _reportDbClient.Query<OfflineBankAccount>("Admin_OfflineBankAccount_Get", p);

            return a;
        }
        
        public IEnumerable<BankCodeModel> GetBankCodeList(int siteId)
        {
            var p = new DynamicParameters(new
            {
                _SiteId = siteId,
            });
            
            var a = _reportDbClient.Query<BankCodeModel>("Admin_BankInfoSys_Get_V3", p);

            return a;
        }
        
        public IEnumerable<BankInfoPay> GetBankInfoPayList(int siteId)
        {
            return _mainDbClient.Query<BankInfoPay>("Common_BankInfoPayForPayout_Get_V3", new { _SiteId = siteId });
        }
        
        public IEnumerable<EWalletPay> GetEWalletPayList()
        {
            return _reportDbClient.Query<EWalletPay>("Common_EWalletPayForPayout_Get");
        }

        public IEnumerable<BankInfoPay> GetCryptoPayList(int siteId)
        {
            return _mainDbClient.Query<BankInfoPay>("Common_CryptoPayForPayout_Get_V2", new { _SiteId = siteId });
        }

        public IEnumerable<BankInfoOff> GetBankAccountOffByCurrency(int currencyId)
        {
            return _reportDbClient.Query<BankInfoOff>("Admin_BankAccountOffByCurrency_Get", new DynamicParameters(new { CurrencyId = currencyId }));
        }
    }
}
