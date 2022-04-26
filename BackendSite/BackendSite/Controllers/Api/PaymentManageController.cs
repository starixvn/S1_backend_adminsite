using System;
using System.Collections.Generic;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Attributes;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PaymentManageController : Controller
    {
        private readonly PaymentManageBLL paymentManageBLL;
        
        public PaymentManageController(PaymentManageBLL paymentManageBLL, PermissionBLL permissionBLL)
        {
            this.paymentManageBLL = paymentManageBLL;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        public ApiResult GetPayInfoList()
        {
            var result = new ApiResult<FrontEndDatas>();
            try
            {
                result.Data = paymentManageBLL.GetPayInfoList();
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.Message = "something went wrong.";
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        public ApiResult UpdatePayList(ApiRequestWithLog<UpdatePayInfo> request)
        {
            var result = new ApiResult();
            var success = paymentManageBLL.UpdatePayList(new ParamsWithLog<UpdatePayInfo>(request, User.Identity.Name));
            
            if (success)
            {
                result.ErrorCode = (int)ErrorCodeEnum.Fail;
                result.Message = "Update failed.";
            }
            else
            {
                result.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            return result;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        public ApiResult UpdatePayChannel(ApiRequestWithLog<UpdatePayChannel> request)
        {
            var result = new ApiResult();

            var success = paymentManageBLL.UpdatePayChannel(new ParamsWithLog<UpdatePayChannel>(request, User.Identity.Name));
            
            if (success)
            {
                result.ErrorCode = (int)ErrorCodeEnum.Fail;
                result.Message = "Update failed.";
            }
            else
            {
                result.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            return result;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage", "f_pm_editAmountRule" })]
        public ApiResult UpdatePayCurrency(ApiRequestWithLog<UpdatePayCurrency> request)
        {
            var result = new ApiResult();

            var success = paymentManageBLL.UpdatePayCurrency(new ParamsWithLog<UpdatePayCurrency>(request, User.Identity.Name));
            
            if (success)
            {
                result.ErrorCode = (int)ErrorCodeEnum.Fail;
                result.Message = "Update failed.";
            }
            else
            {
                result.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            return result;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        public ApiResult UpdatePayCurrencyAmount(ApiRequestWithLog<UpdatePayCurrency> request)
        {
            var result = new ApiResult();

            var success = paymentManageBLL.UpdatePayCurrencyAmount(new ParamsWithLog<UpdatePayCurrency>(request, User.Identity.Name));
            
            if (success)
            {
                result.ErrorCode = (int)ErrorCodeEnum.Fail;
                result.Message = "Update failed.";
            }
            else
            {
                result.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            return result;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        public ApiResult GetBankSysInfoList([FromForm]int PayId)
        {
            var result = new ApiResult<IEnumerable<PayListBankInfoSys>>();
            try
            {
                result.Data = paymentManageBLL.GetBankSysInfoList(PayId);
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.Message = "something went wrong.";
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        public ApiResult UpdateBankInfo([FromForm]int PayId, [FromForm]int SysId, [FromForm]int Status)
        {
            var result = new ApiResult<IEnumerable<PayListBankInfoSys>>();
            try
            {
                paymentManageBLL.UpdateBankInfo(PayId, SysId, Status,User.Identity.Name);
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.Message = "something went wrong.";
                result.RawMessage = ex.ToString();
            }

            return result;
        }
    }
}