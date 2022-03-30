using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DeepLinkController : ControllerBase
    {
        #region Fields
        private readonly DeepLinkBLL _deepLinkBLL;
        private readonly AuthorizeBLL _authorizeBLL;
        private readonly SelLangBLL _selLangBll;
        #endregion

        #region Constructor
        public DeepLinkController(IServiceProvider provider)
        {
            _deepLinkBLL = provider.GetRequiredService<DeepLinkBLL>();
            _authorizeBLL = provider.GetRequiredService<AuthorizeBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }
        #endregion

        /// <summary>
        /// DeepLink/GetDeepLinkList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Deep Link Management 
        /// </remarks>
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

        /// <summary>
        /// DeepLink/GetDeepLinkTable 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Deep Link Management 
        /// </remarks>
        // <param name="req"></param>
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

        /// <summary>
        /// DeepLink/SetDeepLink 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Deep Link Management > [Edit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/DeepLinkManagement" })]
        public ApiResult SetDeepLink(SetDeepLinkReq req)
        {
            var apiResult = new ApiResult();
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

            return apiResult;
        }
    }
}
