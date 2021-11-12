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
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CommonController : ControllerBase
    {
        private readonly ServerInfoBLL serverInfoBLL;

        public CommonController(ServerInfoBLL serverInfoBLL)
        {
            this.serverInfoBLL = serverInfoBLL;
        }

        /// <summary>
        /// Common/AdminUserSiteList 取得可使用的品牌清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.
        /// 每個程式在需要選取品牌條件時，都需要確認有哪些品牌可以選擇。
        /// 
        /// </remarks>
        /// <returns>結果</returns>
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


        /// <summary>
        /// Common/GetEnumType 取得列舉清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.
        /// 
        /// </remarks>
        /// <param name="enumName">參數</param>
        /// <returns>結果</returns>
        //有新舊API，這支是舊API，另一支新的API=apiV2/Enum/GetEnumType
        [HttpPost]
        public ApiResult<List<dynamic>> GetEnumType([FromQuery] string enumName)
        {
            var apiResult = new ApiResult<List<dynamic>>();
            var result = new List<dynamic>();
            try
            {
                var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
                var getType = allTypes.FirstOrDefault(t => string.Equals(t.Name, enumName, StringComparison.OrdinalIgnoreCase));
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

        /// <summary>
        /// Common/AddAdminOperateLog 新增後台操作紀錄
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：暫時沒有找到呼叫這隻API的程式...待確認是否有使用
        /// 
        /// </remarks>
        /// <param name="adminOperateLog">參數</param>
        /// <returns>結果</returns>
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
        /// Common/GetAdminOperateLog 取得後台操作紀錄
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > CustInfo > Admin User Log > [Submit]
        /// 
        /// </remarks>
        /// <param name="adminOperateLogReq">參數</param>
        /// <returns>結果</returns>
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
