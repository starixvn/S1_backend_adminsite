using System;
using System.Collections.Generic;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly PermissionBLL _permissionBll;

        public PaymentAdjustmentReportController(IServiceProvider provider)
        {
            this.paymentAdjustmentBLL = provider.GetRequiredService<PaymentAdjustmentReportBLL>();
            this.excelBLL = provider.GetRequiredService<ExcelBLL>();
            this._permissionBll = provider.GetRequiredService<PermissionBLL>();
        }

        public ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentReport([FromForm] string TransId, [FromForm] int CurrencyId, [FromForm] string UserName, [FromForm] int AdjustType, [FromForm] long StartDate, [FromForm] long EndDate, [FromForm] int PageNumber)
        {
            var result = new ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd>();
            try
            {
                var startTime = DateTimeExtensions.UnixToDateTime(StartDate);
                var endTime = DateTimeExtensions.UnixToDateTime(EndDate);
                
                var PageSize = 10;
                var TotalPages = 0;
                var RowCount = 0;
                var CurrencyAmount = new List<string>();
                var dataList = paymentAdjustmentBLL.GetPaymentAdjustmentReport(TransId, CurrencyId, UserName, AdjustType, startTime, endTime, PageSize, ref PageNumber, ref TotalPages, ref RowCount, ref CurrencyAmount);
                
                foreach (var data in dataList)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                }
                result.Data = dataList; 
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
            var startTime = DateTimeExtensions.UnixToDateTime(StartDate);
            var endTime = DateTimeExtensions.UnixToDateTime(EndDate);
            var exportData = paymentAdjustmentBLL.GetPaymentAdjustmentExportReport(TransId, CurrencyId, UserName, AdjustType, startTime, endTime);
            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }
            
            var stream = excelBLL.Export(exportData);
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
                startDate = DateTimeExtensions.UnixToDateTime((long)getAdjustmentInfoReq.StartDate);
            }

            if (getAdjustmentInfoReq.EndDate != null)
            {
                if (getAdjustmentInfoReq.EndDate > 2147483647)
                    getAdjustmentInfoReq.EndDate = 2147483647;
                endDate = DateTimeExtensions.UnixToDateTime((long)getAdjustmentInfoReq.EndDate);
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

        public class SetAdjustmentReq
        {
            public string TransId { get; set; }
            public int Category { get; set; }
            public string Memo { get; set; }
        }

        [HttpPost]
        public ApiResult SetAdjustment([FromForm] SetAdjustmentReq req)
        {
            try
            {
                if (!_permissionBll.CheckPermission("Finance/AdjustmentReport", "rm_edit_reason")
                    && !_permissionBll.CheckPermission("Finance/AdjustmentReport", "rm_edit_memo"))
                {
                    return new ApiResult
                    {
                        ErrorCode = (int)ErrorCodeEnum.PermissionDenied,
                        Message = "Permission denied."
                    };
                }

                var success =
                    paymentAdjustmentBLL.SetAdjustment(req.TransId, req.Category, req.Memo, User.Identity.Name);

                return new ApiResult
                {
                    ErrorCode = (int)(success 
                        ? ErrorCodeEnum.Success 
                        : ErrorCodeEnum.SetAdjustmentError)
                };
            }
            catch (Exception ex)
            {
                return new ApiResult
                {
                    Message = "Something went wrong.",
                    ErrorCode = (int)ErrorCodeEnum.SystemException,
                    RawMessage = ex.ToString()
                };
            }
        }
    }
}