using System;
using System.Collections.Generic;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Attributes;
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
        public ApiResult UpdatePayList([FromForm]int PayId, [FromForm]int Status)
        {
            var result = new ApiResult();
            try
            {
                paymentManageBLL.UpdatePayList(PayId, Status,User.Identity.Name);
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
        public ApiResult UpdatePayChannel([FromForm]int ChannelId, [FromForm]int Sort, [FromForm]int Status, [FromForm]int PointLevel)
        {
            var result = new ApiResult();
            try
            {
                paymentManageBLL.UpdatePayChannel(ChannelId, Sort, Status, PointLevel,User.Identity.Name);
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage", "f_pm_editAmountRule" })]
        public ApiResult UpdatePayCurrency([FromForm]int ChannelId, [FromForm]int CurrencyId, [FromForm]decimal MinAmount, [FromForm]decimal MaxAmount, [FromForm]int AmountRule, [FromForm]string AmountShow)
        {
            var result = new ApiResult();
            try
            {
                paymentManageBLL.UpdatePayCurrency(ChannelId, CurrencyId, MinAmount, MaxAmount, AmountRule, AmountShow,User.Identity.Name);
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
        public ApiResult UpdatePayCurrencyAmount([FromForm]int ChannelId, [FromForm]int CurrencyId, [FromForm]decimal MinAmount, [FromForm]decimal MaxAmount)
        {
            var result = new ApiResult();
            try
            {
                paymentManageBLL.UpdatePayCurrency(ChannelId, CurrencyId, MinAmount, MaxAmount,User.Identity.Name);
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