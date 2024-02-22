using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;
using Dapper;

namespace BackendSite.Service.DAL
{
    public class HeaderManagementService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public HeaderManagementService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        
        public IEnumerable<HeaderManagementReport> GetList(GetListReq req)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", req.SiteId);
            props.Add("_CurrencyId", req.CurrencyId);
            props.Add("_LobbyCategoryId", req.LobbyCategoryId);
            var result = _reportDbClient.Query<HeaderManagementReport>("Admin_HeaderSetting_Get", props);

            return result;
        }
        
        public int UpdateHeaderSetting(UpdateHeaderSettingReq req, string updater)
        {
            var props = new DynamicParameters();
            props.Add("_Id", req.Id);
            props.Add("_CurrencyGameName", string.IsNullOrEmpty(req.CurrencyGameName) ? "" : req.CurrencyGameName);
            props.Add("_Memo", req.Memo);
            props.Add("_Updater", updater);
            
            var result = _mainDbClient.Execute("Admin_HeaderSetting_Set", props);
            
            return result;
        }
        
        public int UpdateHeaderSettingByTpGameId(UpdateHeaderSettingByTpGameIdReq req, string updater)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", req.SiteId);
            props.Add("_TpGameIdNo", req.TpGameIdNo);
            props.Add("_Status", req.Status);
            props.Add("_IconType", req.IconType);
            props.Add("_IconStartTime", req.IconStartTime.DateTime);
            props.Add("_IconEndTime", req.IconEndTime.DateTime);
            props.Add("_Updater", updater);
            var result = _mainDbClient.Execute("Admin_HeaderSettingByTpGameId_Set", props);
            
            return result;
        }
        
        public int UpdateHeaderSettingEnvironment(UpdateHeaderHeaderSettingEnvironmentReq req, int status, string updater)
        {
            var props = new DynamicParameters();
            props.Add("_Status", status);
            props.Add("_SiteId", req.SiteId);
            props.Add("_TpGameIdNo", req.TpGameIdNo);
            props.Add("_Updater", updater);
            var result = _mainDbClient.Execute("Admin_HeaderSettingEnvironment_Set", props);
            
            return result;
        }
        
        public int UpdateSort(int id, int sort, string updater)
        {
            var props = new DynamicParameters();
            props.Add("_Id", id);
            props.Add("_Sort", sort);
            props.Add("_Updater", updater);
            var result = _mainDbClient.Execute("Admin_HeaderSettingSort_Set", props);
            
            return result;
        }
        
        public IEnumerable<HeaderLobbyCategory> GetLobbyCategory()
        {
            var result = _reportDbClient.Query<HeaderLobbyCategory>("Admin_HeaderLobbyCategory_Get");

            return result;
        }
    }
}
