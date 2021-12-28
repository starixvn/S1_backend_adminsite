using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using BackendSite.Extensions;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PaymentAdjustmentInfoController : Controller
    {
        private readonly PaymentAdjustmentReportBLL paymentAdjustmentBLL;

        public PaymentAdjustmentInfoController(PaymentAdjustmentReportBLL paymentAdjustmentBLL, ExcelBLL excelBLL)
        {
            this.paymentAdjustmentBLL = paymentAdjustmentBLL;
        }
        public class GetAdjustmentInfoReq
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public string RefTransId { get; set; }
            public int PageNumber { get; set; }
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
                startDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((long)getAdjustmentInfoReq.StartDate).AddHours(-8);
            }

            if (getAdjustmentInfoReq.EndDate != null)
            {
                if (getAdjustmentInfoReq.EndDate > 2147483647)
                    getAdjustmentInfoReq.EndDate = 2147483647;
                endDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((long)getAdjustmentInfoReq.EndDate).AddHours(-8);
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
    }
}
