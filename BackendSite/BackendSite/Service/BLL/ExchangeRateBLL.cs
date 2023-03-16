using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

namespace BackendSite.Service.BLL
{
    public class ExchangeUSDRateBLL
    {
        private readonly MySqlClient _mainDbClient;
       
        public ExchangeUSDRateBLL(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);        
        }

        public IEnumerable<CurrencyUSDRate> GetExchangeUSDRate()
        {
            return _mainDbClient.Query<CurrencyUSDRate>("Admin_CurrencyRate_Get");
        }

        public bool SetExchangeUSDRate(SetExchangeUSDRateRequest request, string userName)
        {
            var p = new DynamicParameters(request);
            p.Add("LastUpdateUser", userName);
            return _mainDbClient.Execute("Admin_CurrencyRate_Set", p) > 0;
        }
    }
}
