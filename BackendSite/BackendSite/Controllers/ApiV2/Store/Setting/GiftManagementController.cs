using System;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Store.GiftManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Store.Setting
{
    [Authorize]
    [ApiController]
    [Route("apiV2/[controller]/[action]")]
    public class GiftManagementController : ControllerBase
    {
        private readonly GiftManagementBLL _giftManagementBll;

        public GiftManagementController(IServiceProvider provider)
        {
            _giftManagementBll = provider.GetRequiredService<GiftManagementBLL>();
        }

        [HttpPost]
        public Task<ApiResult<object>> GetListAsync(GetListRequest req)
        {
            var result = _giftManagementBll.GetGiftListAsync(req);

            return result;
        }

        [HttpPost]
        public Task<ApiResult<object>> GetTagsAsync(GetTagsRequest request)
        {
            var result = _giftManagementBll.GetGiftTagsAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult<object>> GetCategoriesAsync(GetCategoriesRequest request)
        {
            var result = _giftManagementBll.GetGiftCategoriesAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult<object>> GetRedemptionLimitAsync(GetRedemptionLimitRequest request)
        {
            var result = _giftManagementBll.GetRedemptionLimitAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult> CreateAsync(CreateGiftRequest request)
        {
            var user = User.Identity.Name;
            var result = _giftManagementBll.CreateGiftAsync(request, user);

            return result;
        }

        [HttpPost]
        public Task<ApiResult> UpdateAsync(UpdateGiftRequest request)
        {
            var user = User.Identity.Name;
            var result = _giftManagementBll.UpdateGiftAsync(request, user);

            return result;
        }

        [HttpPost]
        public Task<ApiResult> CreateCategoryAsync(CreateCategoryRequest request)
        {
            var result = _giftManagementBll.CreateCategoryAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult> UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            var result = _giftManagementBll.UpdateCategoryAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult> CreateTagAsync(CreateTagRequest request)
        {
            var result = _giftManagementBll.CreateTagAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult> UpdateTagAsync(UpdateTagRequest request)
        {
            var result = _giftManagementBll.UpdateTagAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult> SetRedemptionLimitAsync(SetRedemptionLimitRequest request)
        {
            var user = User.Identity.Name;
            var result = _giftManagementBll.SetRedemptionLimitAsync(request, user);

            return result;
        }
    }
}