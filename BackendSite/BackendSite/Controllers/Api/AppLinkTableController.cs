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
    public class AppLinkTableController
    {
        private readonly AppLinkTableBLL appLinkTableBLL;
        public AppLinkTableController(AppLinkTableBLL appLinkTableBLL)
        {
            this.appLinkTableBLL = appLinkTableBLL;
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
            return apiResult;
        }
    }
}
