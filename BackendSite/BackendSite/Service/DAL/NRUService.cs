using BackendSite.Service.Model;
using Dapper;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class NRUService
    {
        private readonly IConfiguration _configuration;

        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public NRUService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        
        public List<MpaMarketingNRUInfo> GetMpaMarketingNRUInfo(
            int siteId,
            string currencyIds,
            DateTime startDate,
            DateTime endDate,
            int baseCurrencyId)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            
            var result = _mainDbClient.Query<MpaMarketingNRUInfo>("Admin_MarketingNRUList_Get", props);

            return result.ToList();
        }
        
        public List<MpaMarketingNRUBonus> GetMpaMarketingNRUBonusInfo(
            int siteId,
            string currencyIds,
            DateTime startDate,
            DateTime endDate,
            int baseCurrencyId)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            
            var result = _reportDbClient.Query<MpaMarketingNRUBonus>("Admin_MarketingNRUBonusList_Get", props);

            return result.ToList();
        }
        
        public List<MpaMarketingNRUBonusDetail> GetMpaMarketingNRUBonusDetailInfo(
            int siteId,
            string currencyIds,
            DateTime startDate,
            DateTime endDate,
            int baseCurrencyId)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            
            var result = _reportDbClient.Query<MpaMarketingNRUBonusDetail>("Admin_MarketingNRUBonusDetailList_Get", props);

            return result.ToList();
        }
    }
}
