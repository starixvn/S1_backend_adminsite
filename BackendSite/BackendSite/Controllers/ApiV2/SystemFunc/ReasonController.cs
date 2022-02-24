using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.SystemFunc
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class ReasonController : ControllerBase
    {
        #region Fields

        private readonly ReasonManagementBLL _reasonManagementBll;
        private readonly AuthorizeBLL _authorizeBll;

        #endregion

        #region Constructors

        public ReasonController(IServiceProvider provider)
        {
            _reasonManagementBll = provider.GetRequiredService<ReasonManagementBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
        }

        #endregion

        /// <summary>
        /// Reason/GetList 取得原因碼清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：BankFinancial > Reason Management > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/ReasonManagement" })]
        [HttpPost]
        public ApiListResult<ReasonManagementData> GetList(GetReasonManagementDataPara request)
        {
            var apiResult = new ApiListResult<ReasonManagementData>();

            var searchResult = _reasonManagementBll.GetReasonManagementData(request);
            apiResult.Data = searchResult.ReasonManagementData;
            apiResult.PageNumber = searchResult.PageNumber;
            apiResult.TotalPages = searchResult.TotalPages;
            apiResult.RowCount = searchResult.RowCount;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// Reason/Set 更新原因碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：BankFinancial > Reason Management > [Edit] > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/ReasonManagement" })]
        [HttpPost]
        public ApiResult Set(SetReasonManagementPara request)
        {
            var apiResult = new ApiResult();

            var authData = _authorizeBll.GetAuthData();
            _reasonManagementBll.SetReasonManagement(request, authData.UserId);
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// Reason/Insert 新增原因碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：BankFinancial > Reason Management > [Create] > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/ReasonManagement" })]
        [HttpPost]
        public ApiResult Insert(InsertReasonManagementPara request)
        {
            var apiResult = new ApiResult();

            var authData = _authorizeBll.GetAuthData();
            _reasonManagementBll.InsertReasonManagement(request, authData.UserId);
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }
    }
}