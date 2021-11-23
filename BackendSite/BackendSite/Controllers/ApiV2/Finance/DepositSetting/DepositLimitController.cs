using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Finance.DepositSetting
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DepositLimitController : ControllerBase
    {
        #region Fields

        private readonly DepositBLL _depositBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public DepositLimitController(IServiceProvider provider)
        {
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region Model

       
        #endregion

        #region Finance > Deposit Limit Management 

        /// <summary>
        /// DepositLimit/GetDepositLimitCurrencyList
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        public ApiResult<IEnumerable<DepositLimitCurrencyInfo>> GetDepositLimitCurrencyList(DepositLimit req)
        {
            var apiResult = new ApiResult<IEnumerable<DepositLimitCurrencyInfo>>();
            var depositLimitCurrencyList = _depositBll.GetDepositLimitCurrencyList(req.SiteId, req.DepositType);
            apiResult.Data = depositLimitCurrencyList;
            apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            return apiResult;
        }
        
        /// <summary>
        /// DepositLimit/GetDepositLimitInfos
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        public ApiResult<IEnumerable<DepositLimitInfo>> GetDepositLimitInfos(DepositLimit req)
        {
            var apiResult = new ApiResult<IEnumerable<DepositLimitInfo>>();
            var depositLimitInfos = _depositBll.GetDepositLimitInfos(req.SiteId, req.DepositType, req.CurrencyId);
            apiResult.Data = depositLimitInfos;
            apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            return apiResult;
        }

        /// <summary>
        /// DepositLimit/AddDepositLimitInfo
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > [Submit] > Add
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        public ApiResult AddDepositLimitInfo(DepositLimitInfo req)
        {
            var result = _depositBll.AddDepositLimit(req, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorMessageCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
        
        /// <summary>
        /// DepositLimit/EditDepositLimitInfo
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > [Submit] > Edit
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        public ApiResult EditDepositLimitInfo(DepositLimitInfo req)
        {
            var result = _depositBll.EditDepositLimit(req, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorMessageCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
        
        /// <summary>
        /// DepositLimit/DeleteDepositLimitInfo
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > [Submit] > Delete
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        [HttpPost]
        public ApiResult DeleteDepositLimitInfo(DepositLimitInfo req)
        {
            var result = _depositBll.DeleteDepositLimit(req, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorMessageCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
       
        #endregion
    }
}