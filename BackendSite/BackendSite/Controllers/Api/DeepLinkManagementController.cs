using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.Api
{
    [Authorize]
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
                Data = _deepLinkBLL.GetDeepLinkTable(req.SiteId, req.DeepLinkId),
                ErrorCode = 0
            };

            return apiResult;
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/DeepLinkManagement" })]
        public ApiResult SetDeepLink(SetDeepLinkReq req)
        {
            var apiResult = new ApiResult();
            try
            {
                if (_deepLinkBLL.SetDeepLink(req.SiteId, req.Type, req.Domain, req.ProtocolType, req.DeepLinkId, _authorizeBLL.GetAuthData().UserName) > 0)
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
