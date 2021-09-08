using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
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
            var pageNumber = getAdjustmentInfoReq.PageNumber;
            var totalPages = 0;
            var rowCount = 0;
            var CurrencyAmount = new List<string>();
            var apiListResult = new ApiListResultCurrencyAmount<PaymentAdjustment>();
            apiListResult.Data = paymentAdjustmentBLL.GetAdjustmentInfoByCust(getAdjustmentInfoReq.CustId, getAdjustmentInfoReq.StartDate, getAdjustmentInfoReq.EndDate, 10, ref pageNumber, out totalPages, out rowCount,ref CurrencyAmount);
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            apiListResult.CurrencyAmount = CurrencyAmount;

            return apiListResult;
        }
    }
}