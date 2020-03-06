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
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentAdjustment" })]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PaymentAdjustmentController : Controller
    {
        private readonly PaymentAdjustmentBLL paymentAdjustmentBLL;
        private readonly ExcelBLL excelBLL;

        public PaymentAdjustmentController(PaymentAdjustmentBLL paymentAdjustmentBLL, ExcelBLL excelBLL)
        {
            this.paymentAdjustmentBLL = paymentAdjustmentBLL;
            this.excelBLL = excelBLL;
        }

        public ApiListResult<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentReport([FromForm]string TransId, [FromForm]int CurrencyId, [FromForm]string UserName, [FromForm]int AdjustType, [FromForm]long StartDate, [FromForm]long EndDate, [FromForm]int PageNumber)
        {
            var result = new ApiListResult<PaymentAdjustmentFrontEnd>();
            try
            {
                int PageSize = 10;
                int TotalPages = 0;
                int RowCount = 0;
                result.Data = paymentAdjustmentBLL.GetPaymentAdjustmentReport(TransId, CurrencyId, UserName, AdjustType, StartDate, EndDate, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
                result.PageNumber = PageNumber;
                result.RowCount = RowCount;
                result.TotalPages = TotalPages;
                result.ErrorCode = 0;
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
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"PaymentAdjustment_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
    }
}