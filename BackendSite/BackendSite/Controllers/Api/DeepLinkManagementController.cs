using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DeepLinkManagementController
    {
        private readonly DeepLinkBLL _deepLinkBLL;
        private readonly AuthorizeBLL _authorizeBLL;
        private readonly SelLangBLL _selLangBll;
        public DeepLinkManagementController(IServiceProvider provider)
        {
            _deepLinkBLL = provider.GetRequiredService<DeepLinkBLL>();
            _authorizeBLL = provider.GetRequiredService<AuthorizeBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/DeepLinkManagement" })]
        public ApiResult<IEnumerable<DeepLink>> GetDeepLinkList()
        {
            var apiResult = new ApiResult<IEnumerable<DeepLink>>
            {
                Data = _deepLinkBLL.GetDeepLinkList(),
                ErrorCode = 0
            };

            return apiResult;
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/DeepLinkManagement" })]
        public ApiResult<IEnumerable<GetDeepLinkRep>> GetDeepLinkTable(GetDeepLinkReq req)
        {
            var apiResult = new ApiResult<IEnumerable<GetDeepLinkRep>>
            {
                Data = _deepLinkBLL.GetDeepLinkTable(req.SiteId, req.DeepLinkId, 1, 100, out _, out _),
                ErrorCode = 0
            };

            return apiResult;
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/DeepLinkManagement" })]
        public async Task<ApiResult> SetDeepLink(SetDeepLinkReq req)
        {
            var apiResult = new ApiResult();
            try
            {
                if (await _deepLinkBLL.SetDeepLink(req.SiteId, req.Type, req.Domain, req.ProtocolType, req.DeepLinkId, _authorizeBLL.GetAuthData().UserName))
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                    apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }

            return apiResult;
        }
    }
}
