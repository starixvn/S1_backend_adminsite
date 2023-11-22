using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;

namespace BackendSite.Service.BLL
{
    public class HeaderManagementBLL
    {
        private readonly HeaderManagementService _headerManagementService;
        private readonly AWSS3Helper _aWSS3Helper;
        private readonly ILogger<HeaderManagementBLL> _logger;
        private readonly string _folder;
        private readonly string _bucketName;

        public HeaderManagementBLL(IServiceProvider provider)
        {
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _headerManagementService = provider.GetRequiredService<HeaderManagementService>();
            _aWSS3Helper = provider.GetRequiredService<AWSS3Helper>();
            _logger = provider.GetRequiredService<ILogger<HeaderManagementBLL>>();
            _bucketName = configBll.GetConfigValue("AWSBackOfficeBucketName");
            _folder = configBll.GetConfigValue("AWSHeaderFolderName");
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
