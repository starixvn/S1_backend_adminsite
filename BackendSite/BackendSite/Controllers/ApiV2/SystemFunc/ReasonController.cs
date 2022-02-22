using System;
using System.Collections.Generic;
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
        
        public ApiResult<IEnumerable<ReasonManagementFuncKey>> GetFuncKey()
        {
            var apiResult = new ApiResult<IEnumerable<ReasonManagementFuncKey>>
            {
                Data = _reasonManagementBll.GetFuncKey(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;

        }
        
        [HttpPost]
        public ApiListResult<ReasonManagementData> GetReasonManagementData(GetReasonManagementDataPara para)
        {
            var apiResult = new ApiListResult<ReasonManagementData>();
            try
            {
                var searchResult = _reasonManagementBll.GetReasonManagementData(para);
                apiResult.Data = searchResult.ReasonManagementData;
                apiResult.PageNumber = searchResult.PageNumber;
                apiResult.TotalPages = searchResult.TotalPages;
                apiResult.RowCount = searchResult.RowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
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
                var authData = _authorizeBll.GetAuthData();
                _reasonManagementBll.SetReasonManagement(para, authData.UserId);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
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
                var authData = _authorizeBll.GetAuthData();
                _reasonManagementBll.InsertReasonManagement(para, authData.UserId);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
    }
}