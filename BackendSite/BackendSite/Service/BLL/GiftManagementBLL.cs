using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Option;
using BackendSite.Service.Model.Store.GiftManagement;

namespace BackendSite.Service.BLL
{
    public class GiftManagementBLL
    {
        private readonly CourierAdapter _courierAdapter;
        private readonly string _storeSiteUrl;
        private readonly AWSS3Helper _awsS3Helper;
        private readonly string _bucketName;
        private readonly string _folder;

        public GiftManagementBLL(IServiceProvider provider)
        {
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _storeSiteUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").StoreService;
            _bucketName = configBll.GetConfigValue("AWSBannerBucketName");
            _folder = configBll.GetConfigValue("AWSStoreFolderName");
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _awsS3Helper = provider.GetRequiredService<AWSS3Helper>();
        }

        public Task<ApiResult<object>> GetGiftListAsync(GetListRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/GetList";
            var result = _courierAdapter.MyPostAsync<object, GetListRequest>(url, req);

            return result;
        }

        public Task<ApiResult<object>> GetGiftTagsAsync(GetTagsRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/GetTags";
            var result = _courierAdapter.MyPostAsync<object, GetTagsRequest>(url, req);

            return result;
        }

        public Task<ApiResult<object>> GetGiftCategoriesAsync(GetCategoriesRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/GetCategories";
            var result = _courierAdapter.MyPostAsync<object, GetCategoriesRequest>(url, req);

            return result;
        }

        public Task<ApiResult<RedemptionLimit>> GetRedemptionLimitAsync(GetRedemptionLimitRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/GetRedemptionLimit";
            var result = _courierAdapter.MyPostAsync<RedemptionLimit, GetRedemptionLimitRequest>(url, req);

            return result;
        }

        public Task<ApiResult<IEnumerable<GiftInfo>>> GetGiftAsync(GetGiftRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/Get";
            var result = _courierAdapter.MyPostAsync<IEnumerable<GiftInfo>, GetGiftRequest>(url, req);

            return result;
        }

        public async Task<ApiResult> CreateGiftAsync(CreateGiftRequest req, string userName)
        {
            var url = _storeSiteUrl + "GiftManagement/Create";
            var data = new CreateGiftParams(req, userName);
            var result = await _courierAdapter.MyPostAsync<object, CreateGiftParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> UpdateGiftAsync(UpdateGiftRequest req, string userName)
        {
            var url = _storeSiteUrl + "GiftManagement/Update";
            var data = new UpdateGiftParams(req, userName);
            var result = await _courierAdapter.MyPostAsync<object, UpdateGiftParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> CreateCategoryAsync(CreateCategoryRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/CreateCategory";
            var result = await _courierAdapter.MyPostAsync<object, CreateCategoryRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> UpdateCategoryAsync(UpdateCategoryRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/UpdateCategory";
            var result = await _courierAdapter.MyPostAsync<object, UpdateCategoryRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> CreateTagAsync(CreateTagRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/CreateTag";
            var result = await _courierAdapter.MyPostAsync<object, CreateTagRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> UpdateTagAsync(UpdateTagRequest req)
        {
            var url = _storeSiteUrl + "GiftManagement/UpdateTag";
            var result = await _courierAdapter.MyPostAsync<object, UpdateTagRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> SetRedemptionLimitAsync(SetRedemptionLimitRequest req, string userName)
        {
            var url = _storeSiteUrl + "GiftManagement/SetRedemptionLimit";
            var data = new SetRedemptionLimitParams(req, userName);
            var result = await _courierAdapter.MyPostAsync<object, SetRedemptionLimitParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<string> UploadPictureAsync(IFormFile file)
        {
            if (!CheckIsImage(file))
            {
                return string.Empty;
            }

            var ext = Path.GetExtension(file.FileName);
            var path = $"/store/gift/{DateTime.Now:yyyyMMddHHmmssfff}{ext}";
            var key = _folder + path;
            var success = await _awsS3Helper.UploadFileAsync(file, _bucketName, key);
            var result = success ? path : string.Empty;

            return result;
        }

        public async Task<ApiResult> SetGiftSortAsync(SetSortRequest req, string userName)
        {
            var index = 1;
            var sortInfos = req.SortedIdList.Select(x => new SortItem
            {
                Sort = index++,
                GiftId = x
            });

            var param = new SetSortParams(sortInfos, userName);
            var url = _storeSiteUrl + "GiftManagement/SetSort";
            var result = await _courierAdapter.MyPostAsync<object, SetSortParams>(url, param);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        private bool CheckIsImage(IFormFile file)
            => file.ContentType.StartsWith("image/");
    }
}