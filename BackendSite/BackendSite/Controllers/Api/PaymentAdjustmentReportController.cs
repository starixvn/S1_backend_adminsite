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
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/AdjustmentReport" })]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
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
                var PageSize = 10;
                var TotalPages = 0;
                var RowCount = 0;
                var CurrencyAmount = new List<string>();
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
            var totalPages = 0;
            var rowCount = 0;
            var CurrencyAmount = new List<string>();

            DateTime? startDate = null;
            DateTime? endDate = null;

            if (getAdjustmentInfoReq.StartDate != null)
            {
                if (getAdjustmentInfoReq.StartDate > 2147483647)
                    getAdjustmentInfoReq.StartDate = 2147483647;
                startDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((long)getAdjustmentInfoReq.StartDate);
            }

            if (getAdjustmentInfoReq.EndDate != null)
            {
                if (getAdjustmentInfoReq.EndDate > 2147483647)
                    getAdjustmentInfoReq.EndDate = 2147483647;
                endDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((long)getAdjustmentInfoReq.EndDate);
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