using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AppLinkTableController
    {
        private readonly AppLinkTableBLL appLinkTableBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly SelLangBLL selLangBLL;
        public AppLinkTableController(AppLinkTableBLL appLinkTableBLL, AuthorizeBLL authorizeBLL, SelLangBLL selLangBLL)
        {
            this.appLinkTableBLL = appLinkTableBLL;
            this.authorizeBLL = authorizeBLL;
            this.selLangBLL = selLangBLL;
        }

        [HttpPost]
        public ApiResult<IEnumerable<AppLinkTable>> GetAppLinkTable(GetAppLinkTableReq getAppLinkTableReq)
        {
            var apiResult = new ApiResult<IEnumerable<AppLinkTable>>
            {
                Data = appLinkTableBLL.GetAppLinkTable(getAppLinkTableReq.SiteId),
                ErrorCode = 0
            };

            return apiResult;
        }

        [HttpPost]
        public ApiResult SetAppLink(SetAppLinkReq setAppLinkReq)
        {
            var apiResult = new ApiResult();
            try
            {
                if (appLinkTableBLL.SetAppLink(setAppLinkReq.SiteId, setAppLinkReq.AppType, setAppLinkReq.Domain, setAppLinkReq.ProtocolType, authorizeBLL.GetAuthData().UserName) > 0)
                {
                    apiResult.ErrorCode = 0;
                    apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
                }
                else
                {
                    apiResult.ErrorCode = 9;
                    apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 91;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            
            return apiResult;
        }
    }
}
