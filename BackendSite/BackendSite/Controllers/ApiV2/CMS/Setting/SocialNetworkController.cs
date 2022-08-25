using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class SocialNetworkController : ControllerBase
    {
        private readonly SocialNetworkBLL _socialNetworkBll;

        public SocialNetworkController(SocialNetworkBLL socialNetworkBll)
        {
            this._socialNetworkBll = socialNetworkBll;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 123, 1 })]
        public async Task<ApiResult<Dictionary<string, List<SocialDataListResponse>>>> GetDataList(SocialDataListRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, List<SocialDataListResponse>>>();
            try
            {
                var responseData = await _socialNetworkBll.GetSocialDataList(request);
                apiResult.Data = responseData;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 123, 3 })]
        public async Task<ApiResult> SetSort(SetSocialSortRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                await _socialNetworkBll.SetSocialSort(request, User.Identity.Name);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 123, 3 })]
        public async Task<ApiResult> SetData(SetSocialDataRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                await _socialNetworkBll.SetSocialData(request, User.Identity.Name);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
    }
}
