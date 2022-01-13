using System;
using System.Collections.Generic;
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
        /// AppLink/GetAppLinkTable 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > App Link Management 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<IEnumerable<AppLinkTable>> GetAppLinkTable(GetAppLinkTableReq req)
        {
            var apiResult = new ApiResult<IEnumerable<AppLinkTable>>
            {
                Data = _appLinkTableBll.GetAppLinkTable(req.SiteId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }
        
        /// <summary>
        /// AppLink/GetAppLinkTable 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > App Link Management > [Edit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult SetAppLink(SetAppLinkReq req)
        {
            var apiResult = new ApiResult();
            try
            {
                if (_appLinkTableBll.SetAppLink(req.SiteId, req.AppType, req.Domain, req.ProtocolType, _authorizeBll.GetAuthData().UserName) > 0)
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