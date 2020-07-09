using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommonController
    {
        private readonly ServerInfoBLL serverInfoBLL;

        public CommonController(ServerInfoBLL serverInfoBLL)
        {
            this.serverInfoBLL = serverInfoBLL;
        }

        public ApiResult<IEnumerable<ServerInfo>> AdminUserSiteList()
        {
            var apiResult = new ApiResult<IEnumerable<ServerInfo>>();
            try
            {
                apiResult.Data = serverInfoBLL.GetSiteListByLoginUser();
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }
    }
}
