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
    public class ReasonManagementController : Controller
    {
        private readonly ReasonManagementBLL reasonManagementBLL;
        private readonly AuthorizeBLL authorizeBLL;

        public ReasonManagementController(ReasonManagementBLL reasonManagementBLL, AuthorizeBLL authorizeBLL)
        {
            this.reasonManagementBLL = reasonManagementBLL;
            this.authorizeBLL = authorizeBLL;
        }

        #region API Model

        public class GetReasonManagementDataRequest
        {
            public string FuncKey { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        #endregion

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
        public ApiListResult<ReasonManagementData> GetReasonManagementData(GetReasonManagementDataRequest para)
        {
            var apiResult = new ApiListResult<ReasonManagementData>();
            try
            {
                var funcKeyList = new List<string>();
                if (para.FuncKey == "0")
                    funcKeyList = reasonManagementBLL.GetFuncKey().Select(z => z.FuncKey).ToList();
                else 
                    funcKeyList.Add(para.FuncKey);

                var SearchResult = reasonManagementBLL.GetReasonManagementData(new GetReasonManagementDataPara()
                {
                    FuncKeys = funcKeyList,
                    PageNumber = para.PageNumber,
                    PageSize = para.PageSize
                });
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
