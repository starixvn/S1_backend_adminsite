using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;

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
        public ApiListResult<PaymentAdjustment> GetAdjustmentInfoByCust(GetAdjustmentInfoReq getAdjustmentInfoReq)
        {
            int pageNumber = getAdjustmentInfoReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var apiListResult = new ApiListResult<PaymentAdjustment>();
            apiListResult.Data = paymentAdjustmentBLL.GetAdjustmentInfoByCust(getAdjustmentInfoReq.CustId, getAdjustmentInfoReq.StartDate, getAdjustmentInfoReq.EndDate, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }
    }
}