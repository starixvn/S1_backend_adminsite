using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class CommonController : ControllerBase
    {
        private readonly ServerInfoBLL serverInfoBLL;

        public CommonController(ServerInfoBLL serverInfoBLL)
        {
            this.serverInfoBLL = serverInfoBLL;
        }

        [HttpPost]
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

        [HttpPost]
        public ApiResult<List<dynamic>> GetEnumType([FromQuery] string enumName)
        {
            var apiResult = new ApiResult<List<dynamic>>();
            var result = new List<dynamic>();
            try
            {
                var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
                var getType = allTypes.Where(t => string.Equals(t.Name, enumName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (getType != null)
                {
                    if (getType.Namespace == "BackendSite.Service.Model.enums" && getType.IsEnum)
                    {
                        foreach (var enumValue in Enum.GetValues(getType))
                        {
                            result.Add(new { name = Enum.GetName(getType, enumValue), value = (int)enumValue });
                        }
                        apiResult.Data = result;
                        apiResult.ErrorCode = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }

        [HttpPost]
        public ApiResult AddAdminOperateLog(AdminOperateLog adminOperateLog)
        {
            var apiResult = new ApiResult();
            var result = serverInfoBLL.AdminOperateLog(adminOperateLog.SiteId, adminOperateLog.CustId, adminOperateLog.Menu, adminOperateLog.ActionType, adminOperateLog.DetailInfo, User.Identity.Name) > 0;

            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = "";
            }
            else
            {
                apiResult.ErrorCode = 9999;
                apiResult.Message = "";
            }
            return apiResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// CSD > Customer Administration > CustInfo > Admin User Log
        /// </remarks>
        /// <param name="adminOperateLogReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<GetAdminOperateLog> GetAdminOperateLog(AdminOperateLogReq adminOperateLogReq)
        {
            var apiListResult = new ApiListResult<GetAdminOperateLog>();
            var pageNumber = adminOperateLogReq.PageNumber;
            var totalPages = 0;
            var rowCount = 0;
            var depositRequestList = serverInfoBLL.GetAdminOperateLog(adminOperateLogReq.SiteId, adminOperateLogReq.CustId, adminOperateLogReq.Menu, adminOperateLogReq.Operator, adminOperateLogReq.startDate, adminOperateLogReq.endDate, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.Data = depositRequestList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }

    }
}
