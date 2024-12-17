using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

namespace BackendSite.Service.DAL
{
    public class LocalBankService
    {
        private readonly IConfiguration configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public LocalBankService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
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

            return _reportDbClient.Query<OfflineBankAccount>("Admin_OfflineBankAccount_Get_V3", p);
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
            return _mainDbClient.Query<BankInfoPay>("Common_BankInfoPayForPayout_Get_V5", new { _SiteId = siteId });
        }

        public IEnumerable<BankInfoPay> GetEWalletPayList(int siteId)
        {
            return _reportDbClient.Query<BankInfoPay>("Common_EWalletPayForPayout_Get_V2", new { _SiteId = siteId });
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
