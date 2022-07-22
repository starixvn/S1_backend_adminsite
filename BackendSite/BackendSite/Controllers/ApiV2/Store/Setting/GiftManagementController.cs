using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Store.GiftManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using GetListRequest = BackendSite.Service.Model.Store.GiftManagement.GetListRequest;

namespace BackendSite.Controllers.ApiV2.Store.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [ApiController]
    [Route("apiV2/[controller]/[action]")]
    public class GiftManagementController : ControllerBase
    {
        private readonly GiftManagementBLL _giftManagementBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;

        public GiftManagementController(IServiceProvider provider)
        {
            _giftManagementBll = provider.GetRequiredService<GiftManagementBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
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
        public Task<ApiResult<RedemptionLimit>> GetRedemptionLimitAsync(GetRedemptionLimitRequest request)
        {
            var result = _giftManagementBll.GetRedemptionLimitAsync(request);

            return result;
        }
        
        [HttpPost]
        public Task<ApiResult<IEnumerable<GiftInfo>>> GetAsync(GetGiftRequest req)
        {
            var result = _giftManagementBll.GetGiftAsync(req);

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
        public async Task<ApiResult> UpdateAsync(UpdateGiftRequest request)
        {
            var user = User.Identity.Name;
            var before = await _giftManagementBll.GetGiftAsync(new GetGiftRequest
            {
                GiftId = request.GiftId
            });
            var result = await _giftManagementBll.UpdateGiftAsync(request, user);
            
            if (result.ErrorCode == (int) ErrorCodeEnum.Success)
            {
                var b = before.Data.First(x => x.Lang == "en");
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = user,
                    SiteId = b.SiteId,
                    CurrencyId = b.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.GiftManagement,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        Name = b.GiftName,
                        Point = b.Point,
                        Status = b.Status.ToString()
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        Point = request.Point,
                        Status = request.Status.ToString()
                    })
                });
            }
            
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
        public async Task<ApiResult> SetRedemptionLimitAsync(SetRedemptionLimitRequest request)
        {
            var user = User.Identity.Name;
            var before = await _giftManagementBll.GetRedemptionLimitAsync(
                new GetRedemptionLimitRequest
                {
                    SiteId = request.SiteId
                });
            
            var result = await _giftManagementBll.SetRedemptionLimitAsync(request, user);
            if (result.ErrorCode == (int) ErrorCodeEnum.Success)
            {
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = user,
                    SiteId = request.SiteId,
                    CurrencyId = 0,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.RedemptionLimitSetting,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        Limit = before.Data.Limit,
                        Status = before.Data.Status.ToString()
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        Limit = request.Limit,
                        Status = request.Status.ToString()
                    })
                });
            }

            return result;
        }

        [HttpPost]
        public async Task<ApiResult> UploadAsync([FromForm] UploadImageRequest request)
        {
            var result = await _giftManagementBll.UploadPictureAsync(request.File);

            return new ApiResult<UploadPictureResult>
            {
                Data = new UploadPictureResult(result),
                ErrorCode = (int) (string.IsNullOrEmpty(result) ? ErrorCodeEnum.Fail : ErrorCodeEnum.Success)
            };
        }
        
        [HttpPost]
        public async Task<ApiResult> SetSortAsync(SetSortRequest request)
        {
            var user = User.Identity.Name;
            var result = await _giftManagementBll.SetGiftSortAsync(request, user);

            return result;
        }
    }
}
