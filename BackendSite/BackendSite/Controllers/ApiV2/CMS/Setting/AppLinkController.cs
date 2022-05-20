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
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class AppLinkController : ControllerBase
    {
        #region Fields

        private readonly AppLinkTableBLL _appLinkTableBll;
        private readonly SelLangBLL _selLangBll;
        private readonly AuthorizeBLL _authorizeBll;


        #endregion

        #region Constructor

        public AppLinkController(IServiceProvider provider)
        {
            _appLinkTableBll = provider.GetRequiredService<AppLinkTableBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
        }

        #endregion

        /// <summary>
        /// AppLink/GetList 取得App連結清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > App Link Management > Select [Site Name]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/AppLinkManagement" })]
        [HttpPost]
        public ApiResult<IEnumerable<AppLinkTable>> GetList(GetAppLinkTableReq request)
        {
            var apiResult = new ApiResult<IEnumerable<AppLinkTable>>
            {
                Data = _appLinkTableBll.GetAppLinkTable(request.SiteId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// AppLink/Set 新增/更新App連結清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > App Link Management > Select [Site Name] > [Edit] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/AppLinkManagement" })]
        [HttpPost]
        public ApiResult Set(SetAppLinkReq request)
        {
            var apiResult = new ApiResult();
            try
            {
                if (_appLinkTableBll.SetAppLink(request.SiteId, (int)request.AppType, request.Domain, (int)request.ProtocolType,
                    _authorizeBll.GetAuthData().UserName) > 0)
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
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }

            return apiResult;
        }
    }
}