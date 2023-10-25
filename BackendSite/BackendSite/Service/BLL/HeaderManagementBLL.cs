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
        
        public int UpdateSort(int id,int sort, string updater)
        {
            return _headerManagementService.UpdateSort(id, sort ,updater);
        }
        
        public async Task<bool> UploadHeaderPictureAsync(int id ,HeaderPictureEnum picType, IFormFile file)
        {
            if (!_aWSS3Helper.CheckIsImage(file))
            {
                return false;
            }

            var ext = Path.GetExtension(file.FileName);
            var path = $"/HeaderPicture/{id}_{picType.ToString()}{ext}";
            var key = _folder + path;
            var success = await _aWSS3Helper.UploadFileAsync(file, _bucketName, key);
            
            if (success)
            {
                return _headerManagementService.UploadHeaderPicture(id, picType, key) > 0;
            }
            return false;
        }
        
        public HeaderPicture GetHeaderPicture(int id)
        {
            return _headerManagementService.GetHeaderPicture(id);
        }
        
        public IEnumerable<HeaderLobbyCategory> GetLobbyCategory()
        {
            return _headerManagementService.GetLobbyCategory();
        }
        
    }
}
