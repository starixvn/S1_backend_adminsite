using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Attributes;

namespace BackendSite.Controllers.Api
{
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/AdjustmentReport" })]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PaymentAdjustmentReportController : Controller
    {
        private readonly PaymentAdjustmentReportBLL paymentAdjustmentBLL;
        private readonly ExcelBLL excelBLL;

        public PaymentAdjustmentReportController(PaymentAdjustmentReportBLL paymentAdjustmentBLL, ExcelBLL excelBLL)
        {
            this.paymentAdjustmentBLL = paymentAdjustmentBLL;
            this.excelBLL = excelBLL;
        }

        public ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentReport([FromForm] string TransId, [FromForm] int CurrencyId, [FromForm] string UserName, [FromForm] int AdjustType, [FromForm] long StartDate, [FromForm] long EndDate, [FromForm] int PageNumber)
        {
            var result = new ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd>();
            try
            {
                int PageSize = 10;
                int TotalPages = 0;
                int RowCount = 0;
                List<string> CurrencyAmount = new List<string>();
                result.Data = paymentAdjustmentBLL.GetPaymentAdjustmentReport(TransId, CurrencyId, UserName, AdjustType, StartDate, EndDate, PageSize, ref PageNumber, ref TotalPages, ref RowCount, ref CurrencyAmount);
                result.PageNumber = PageNumber;
                result.RowCount = RowCount;
                result.TotalPages = TotalPages;
                result.ErrorCode = 0;
                result.CurrencyAmount = CurrencyAmount;
            }
            catch (Exception ex)
            {
                result.Message = "something went wrong.";
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }


        [HttpPost]
        public IActionResult ExportPaymentAdjustmentReport(string TransId, int CurrencyId, string UserName, int AdjustType, long StartDate, long EndDate)
        {
            var ExportData = paymentAdjustmentBLL.GetPaymentAdjustmentExportReport(TransId, CurrencyId, UserName, AdjustType, StartDate, EndDate);
            var stream = excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"PaymentAdjustmentReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        public class GetAdjustmentInfoReq
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public string RefTransId { get; set; }
            public int PageNumber { get; set; }
        }
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