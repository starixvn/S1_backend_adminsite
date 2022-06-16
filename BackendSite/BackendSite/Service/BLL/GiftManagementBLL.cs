using System;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using Microsoft.Extensions.Configuration;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Store.GiftManagement;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.BLL
{
    public class GiftManagementBLL
    {
        private readonly CourierAdapter _courierAdapter;
        private readonly string _storeSiteUrl;

        public GiftManagementBLL(IServiceProvider provider)
        {
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            _storeSiteUrl = configuration.GetValue<string>("StoreService");
        }

        public Task<ApiResult<object>> GetGiftListAsync(GetListRequest req)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/GetList";
            var result = _courierAdapter.MyPostAsync<object, GetListRequest>(url, req);

            return result;
        }

        public Task<ApiResult<object>> GetGiftTagsAsync(GetTagsRequest req)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/GetTags";
            var result = _courierAdapter.MyPostAsync<object, GetTagsRequest>(url, req);

            return result;
        }

        public Task<ApiResult<object>> GetGiftCategoriesAsync(GetCategoriesRequest req)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/GetCategories";
            var result = _courierAdapter.MyPostAsync<object, GetCategoriesRequest>(url, req);

            return result;
        }

        public Task<ApiResult<object>> GetRedemptionLimitAsync(GetRedemptionLimitRequest req)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/GetRedemptionLimit";
            var result = _courierAdapter.MyPostAsync<object, GetRedemptionLimitRequest>(url, req);

            return result;
        }
        
        public Task<ApiResult<object>> GetGiftAsync(GetGiftRequest req)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/Get";
            var result = _courierAdapter.MyPostAsync<object, GetGiftRequest>(url, req);

            return result;
        }

        public async Task<ApiResult> CreateGiftAsync(CreateGiftRequest req, string userName)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/Create";
            var data = new CreateGiftParams(req, userName);
            var result = await _courierAdapter.MyPostAsync<object, CreateGiftParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> UpdateGiftAsync(UpdateGiftRequest req, string userName)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/Update";
            var data = new UpdateGiftParams(req, userName);
            var result = await _courierAdapter.MyPostAsync<object, UpdateGiftParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> CreateCategoryAsync(CreateCategoryRequest req)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/CreateCategory";
            var result = await _courierAdapter.MyPostAsync<object, CreateCategoryRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> UpdateCategoryAsync(UpdateCategoryRequest req)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/UpdateCategory";
            var result = await _courierAdapter.MyPostAsync<object, UpdateCategoryRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> CreateTagAsync(CreateTagRequest req)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/CreateTag";
            var result = await _courierAdapter.MyPostAsync<object, CreateTagRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> UpdateTagAsync(UpdateTagRequest req)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/UpdateTag";
            var result = await _courierAdapter.MyPostAsync<object, UpdateTagRequest>(url, req);

            return new ApiResult(result.ErrorCode, result.Message);
        }

        public async Task<ApiResult> SetRedemptionLimitAsync(SetRedemptionLimitRequest req, string userName)
        {
            var url = _storeSiteUrl + "/api/GiftManagement/SetRedemptionLimit";
            var data = new SetRedemptionLimitParams(req, userName);
            var result = await _courierAdapter.MyPostAsync<object, SetRedemptionLimitParams>(url, data);

            return new ApiResult(result.ErrorCode, result.Message);
        }
    }
}