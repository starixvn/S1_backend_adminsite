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
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReasonManagementController : Controller
    {
        private readonly ReasonManagementBLL reasonManagementBLL;
        private readonly AuthorizeBLL authorizeBLL;

        public ReasonManagementController(ReasonManagementBLL reasonManagementBLL, AuthorizeBLL authorizeBLL)
        {
            this.reasonManagementBLL = reasonManagementBLL;
            this.authorizeBLL = authorizeBLL;
        }

        public ApiResult<IEnumerable<ReasonManagementFuncKey>> GetFuncKey()
        {

            var apiResult = new ApiResult<IEnumerable<ReasonManagementFuncKey>>
            {
                Data = reasonManagementBLL.GetFuncKey(),
                ErrorCode = 0
            };

            return apiResult;

        }
        [HttpPost]
        public ApiListResult<ReasonManagementData> GetReasonManagementData(GetReasonManagementDataPara para)
        {
            var apiResult = new ApiListResult<ReasonManagementData>();
            try
            {
                var SearchResult = reasonManagementBLL.GetReasonManagementData(para);
                apiResult.Data = SearchResult.ReasonManagementData;
                apiResult.PageNumber = SearchResult.PageNumber;
                apiResult.TotalPages = SearchResult.TotalPages;
                apiResult.RowCount = SearchResult.RowCount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        [HttpPost]
        public ApiResult SetReasonManagement(SetReasonManagementPara para)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                reasonManagementBLL.SetReasonManagement(para, authData.UserId);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
        [HttpPost]
        public ApiResult InsertReasonManagement(InsertReasonManagementPara para)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                reasonManagementBLL.InsertReasonManagement(para, authData.UserId);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
    }
}
