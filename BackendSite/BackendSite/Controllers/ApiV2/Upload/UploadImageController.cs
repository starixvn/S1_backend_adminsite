using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.Model.Common.UploadImage;
using BackendSite.Service.Model.enums;
using BackendSite.Attributes;

namespace BackendSite.Controllers.ApiV2.Upload
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class UploadImageController : ControllerBase
    {
        private readonly IUploadImageBLL _uploadImageBll;

        public UploadImageController(IServiceProvider provider)
        {
            _uploadImageBll = provider.GetRequiredService<IUploadImageBLL>();
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 55, 3 })]
        public async Task<ApiResult<UploadImageResponse>> BannerManagementAsync([FromForm] UploadImageRequest uploadImageRequest)
        {
            return await HandleUploadActionAsync(uploadImageRequest, UploadImageFunctionEnum.BannerManagement);
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 45, 3 })]
        public async Task<ApiResult<UploadImageResponse>> BonusManagementAsync([FromForm] UploadImageRequest uploadImageRequest)
        {
            return await HandleUploadActionAsync(uploadImageRequest, UploadImageFunctionEnum.BonusManagement);
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 54, 3 })]
        public async Task<ApiResult<UploadImageResponse>> NewsManagementAsync([FromForm] UploadImageRequest uploadImageRequest)
        {
            return await HandleUploadActionAsync(uploadImageRequest, UploadImageFunctionEnum.NewsManagement);
        }

        #region private method

        private async Task<ApiResult<UploadImageResponse>> HandleUploadActionAsync(UploadImageRequest uploadImageRequest,
            UploadImageFunctionEnum functionEnum)
        {
            try
            {
                var responseResult = await _uploadImageBll.UploadImageAsync(uploadImageRequest, functionEnum);
                return HandleResponse(responseResult);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        private ApiResult<UploadImageResponse> HandleResponse(UploadResultDto responseResult)
        {
            var apiResult = new ApiResult<UploadImageResponse>
            {
                Data = new UploadImageResponse(responseResult.ImagePath),
                ErrorCode = responseResult.Success ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.SystemException,
                Message = responseResult.Message
            };
            return apiResult;
        }

        private ApiResult<UploadImageResponse> HandleException(Exception ex)
        {
            var apiResult = new ApiResult<UploadImageResponse>
            {
                ErrorCode = (int)ErrorCodeEnum.SystemException,
                Message = ex.Message
            };
            return apiResult;
        }

        #endregion
    }
}
