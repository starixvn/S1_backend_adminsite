using BackendSite.Service.Model;
using Dapper;
using Newtonsoft.Json;
using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class BettingMonitorService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public BettingMonitorService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        
        public IEnumerable<BettingMonitorModel> GetList(
            int siteId, 
            IEnumerable<int> currencyIds, 
            IEnumerable<int> tpIds, 
            IEnumerable<int> categoryIds, 
            IEnumerable<int> gameTypes, 
            int startIndex, int pageSize,
            out int totalPages, out int rowCount)
        {

            var props = new DynamicParameters();

            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", JsonConvert.SerializeObject(currencyIds));
            props.Add("_TpIds", JsonConvert.SerializeObject(tpIds));
            props.Add("_CategoryIds", JsonConvert.SerializeObject(categoryIds));
            props.Add("_GameTypes", JsonConvert.SerializeObject(gameTypes));
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<BettingMonitorModel>("Admin_BettingMonitorSetting_Get", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }
        
        public bool SetSetting(int siteId, int currencyId, int tpId, int gameType, int frequency, int status, string user)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyId", currencyId);
            props.Add("_TpId", tpId);
            props.Add("_GameType", gameType);
            props.Add("_Frequency", frequency);
            props.Add("_Status", status);
            props.Add("_Operator", user);
      
            var result = _mainDbClient.Execute("Admin_BettingMonitorSetting_Set", props) > 0;
            
            return result;
        }
        
        public bool DelSetting(int siteId, int currencyId, int tpId, int gameType)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyId", currencyId);
            props.Add("_TpId", tpId);
            props.Add("_GameType", gameType);

            var result = _mainDbClient.Execute("Admin_BettingMonitorSetting_Del", props) > 0;
            
            return result;
        }
    }
}
