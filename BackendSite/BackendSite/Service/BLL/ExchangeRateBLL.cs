using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class ExchangeUSDRateBLL
    {
        private readonly MySqlClient _mainDbClient;
       
        public ExchangeUSDRateBLL(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);        
        }

        public GetExchangeUSDRateResponse GetExchangeUSDRate(int CurrencyId)
        {
            return _mainDbClient.QueryFirst<GetExchangeUSDRateResponse>("Admin_CurrencyRate_Get", new { CurrencyId });
        }

        public bool SetExchangeUSDRate(SetExchangeUSDRateRequest request)
        {
            return _mainDbClient.Execute("Admin_CurrencyRate_Set", request) > 0;
        }
    }
}
