using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using BackendSite.Extensions;
using BackendSite.Service.Model.enums;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PaymentAdjustmentInfoController : Controller
    {
        private readonly PaymentAdjustmentReportBLL paymentAdjustmentBLL;
        private readonly BankFinancialBLL bankFinancialBll;

        public PaymentAdjustmentInfoController(PaymentAdjustmentReportBLL paymentAdjustmentBLL,BankFinancialBLL bankFinancialBll, ExcelBLL excelBLL)
        {
            this.paymentAdjustmentBLL = paymentAdjustmentBLL;
            this.bankFinancialBll = bankFinancialBll;
            
        }
        public class GetAdjustmentInfoReq
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public string RefTransId { get; set; }
            public int PageNumber { get; set; }
        }
        
        public class GetReasonByFuncKeyRequest
        {
            public string FuncKey { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use：
        /// CSD > Customer Administration > CustInfo > Adjustment Info
        /// </remarks>
        /// <param name="getAdjustmentInfoReq"></param>
        /// <returns></returns>
        public ApiListResultCurrencyAmount<PaymentAdjustment> GetAdjustmentInfoByCust(GetAdjustmentInfoReq getAdjustmentInfoReq)
        {
            var totalPages = 0;
            var rowCount = 0;
            var CurrencyAmount = new List<string>();

            DateTime? startDate = null;
            DateTime? endDate = null;

            if (getAdjustmentInfoReq.StartDate != null)
            {
                if (getAdjustmentInfoReq.StartDate > 2147483647)
                    getAdjustmentInfoReq.StartDate = 2147483647;

                startDate = DateTimeExtensions.UnixToDateTime((long)getAdjustmentInfoReq.StartDate).AddHours(-8);
            }

            if (getAdjustmentInfoReq.EndDate != null)
            {
                if (getAdjustmentInfoReq.EndDate > 2147483647)
                    getAdjustmentInfoReq.EndDate = 2147483647;

                endDate = DateTimeExtensions.UnixToDateTime((long)getAdjustmentInfoReq.EndDate).AddHours(-8);
            }


            var apiListResult = new ApiListResultCurrencyAmount<PaymentAdjustment>
            {
                Data = paymentAdjustmentBLL.GetAdjustmentInfoByCust(getAdjustmentInfoReq.CustId,
                startDate,
                endDate,
                10,
                getAdjustmentInfoReq.PageNumber,
                out totalPages,
                out rowCount,
                ref CurrencyAmount),
                ErrorCode = 0,
                PageNumber = getAdjustmentInfoReq.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount,
                CurrencyAmount = CurrencyAmount
            };

            return apiListResult;
        }
        
        /// <summary>
        /// PaymentAdjustmentInfo/GetReasonList 取得原因碼清單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<BankFinancialReasonModel>> GetReasonList(GetReasonByFuncKeyRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<BankFinancialReasonModel>>();

            apiResult.Data = bankFinancialBll.GetReason(request.FuncKey);
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }
    }
}
