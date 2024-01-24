using BackendSite.Service.DAL;
using BackendSite.Service.Model.Report;

namespace BackendSite.Service.BLL
{
    public class HeaderManagementBLL
    {
        private readonly HeaderManagementService _headerManagementService;
        private readonly ILogger<HeaderManagementBLL> _logger;


        public HeaderManagementBLL(IServiceProvider provider)
        {
            _headerManagementService = provider.GetRequiredService<HeaderManagementService>();
            _logger = provider.GetRequiredService<ILogger<HeaderManagementBLL>>();
        }
        
        public IEnumerable<HeaderManagementReport> GetList(GetListReq req)
        {
            return _headerManagementService.GetList(req);
        }
        
        public int UpdateHeaderSetting(UpdateHeaderSettingReq req , string updater)
        {
            return _headerManagementService.UpdateHeaderSetting(req, updater);
        }
        
        public int UpdateHeaderSettingByTpGameId(UpdateHeaderSettingByTpGameIdReq req , string updater)
        {
            return _headerManagementService.UpdateHeaderSettingByTpGameId(req, updater);
        }
        
        public int UpdateSort(int id,int sort, string updater)
        {
            return _headerManagementService.UpdateSort(id, sort ,updater);
        }
        
        public IEnumerable<HeaderLobbyCategory> GetLobbyCategory()
        {
            return _headerManagementService.GetLobbyCategory();
        }
        
    }
}
