using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;
using Dapper;

namespace BackendSite.Service.DAL
{
    public class MpaMarketingDashboardService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public MpaMarketingDashboardService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public PerformanceHighlightData GetPerformanceHighlightData(
            int siteId,
            string currencyIds,
            DateTime startDate,
            DateTime endDate)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            
            var result = _reportDbClient.QueryFirst<PerformanceHighlightData>("Admin_MarketingPerformanceHighlight_Get", props);

            return result;
        }

        public IEnumerable<UserActivitiesData> GetActiveUsersData(
            int siteId,
            string currencyIds,
            DateTime startDate,
            DateTime endDate)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            
            var result = _reportDbClient.Query<UserActivitiesData>("Admin_MarketingActiveUsers_Get", props);

            return result;
        }
        
        public IEnumerable<RegistrationDetailsResult> GetRegistrationDetails(
            int siteId,
            string currencyIds,
            DateTime startDate,
            DateTime endDate)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            
            var result = _reportDbClient.Query<RegistrationDetailsResult>("Admin_MarketingNRUExport_Get", props);

            return result;
        }
        
        public IEnumerable<RevenueAvgData> GetRevenueAvgData(
            int siteId,
            string currencyIds,
            string tpIds,
            DateTime startDate,
            DateTime endDate,
            int baseCurrencyId)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_TpIds", tpIds);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            
            var result = _reportDbClient.Query<RevenueAvgData>("Admin_MarketingRevenueAvg_Get", props);

            return result;
        }
        
        public IEnumerable<UserActivitiesData> GetNRUData(
            int siteId,
            string currencyIds,
            DateTime startDate,
            DateTime endDate)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            
            var result = _reportDbClient.Query<UserActivitiesData>("Admin_MarketingNRU_Get", props);

            return result;
        }
    }
}