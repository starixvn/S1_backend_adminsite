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
            int pageNumber = getAdjustmentInfoReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            List<string> CurrencyAmount = new List<string>();
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