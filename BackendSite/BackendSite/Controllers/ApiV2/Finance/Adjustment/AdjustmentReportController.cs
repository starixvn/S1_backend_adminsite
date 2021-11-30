using System;
using System.Collections.Generic;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Finance.Adjustment
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class AdjustmentReportController : ControllerBase
    {
        #region Fields

        private readonly PaymentAdjustmentReportBLL _paymentAdjustmentBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructors

        public AdjustmentReportController(IServiceProvider provider)
        {
            _paymentAdjustmentBll = provider.GetRequiredService<PaymentAdjustmentReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region Model

        public class GetAdjustmentReportReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public string TransId { get; set; }
            public int CurrencyId { get; set; } 
            public string UserName { get; set; }
            public int AdjustType { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public int PageNumber { get; set; }
        }

        public class ExportPaymentAdjustmentReportReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public string TransId { get; set; }
            public int CurrencyId { get; set; } 
            public string UserName { get; set; }
            public int AdjustType { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        
        #endregion
        
        /// <summary>
        /// AdjustmentReport/GetAdjustmentReport 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Payment Adjustment > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        public ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd> GetAdjustmentReport(GetAdjustmentReportReq req)
        {
            var result = new ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd>();
            try
            {
                var pageSize = 10;
                var pageNumber = req.PageNumber;
                var totalPages = 0;
                var rowCount = 0;
                var currencyAmount = new List<string>();
                
                var dataList = _paymentAdjustmentBll.GetPaymentAdjustmentReport(req.TransId, req.CurrencyId, req.UserName, req.AdjustType, req.StartDate.DateTime, req.EndDate.DateTime, pageSize, ref pageNumber, ref totalPages, ref rowCount, ref currencyAmount);
                
                foreach (var data in dataList)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                }
                result.Data = dataList;
                result.PageNumber = pageNumber;
                result.RowCount = rowCount;
                result.TotalPages = totalPages;
                result.ErrorCode = (int)ErrorCodeEnum.Success;
                result.CurrencyAmount = currencyAmount;
            }
            catch (Exception ex)
            {
                result.Message = "something went wrong.";
                result.ErrorCode = (int)ErrorCodeEnum.SystemException;
                result.RawMessage = ex.ToString();
            }

            return result;
        }
        
        /// <summary>
        /// AdjustmentReport/ExportPaymentAdjustmentReport 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Payment Adjustment > [Export]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public IActionResult ExportPaymentAdjustmentReport(ExportPaymentAdjustmentReportReq req)
        {
            var exportData = _paymentAdjustmentBll.GetPaymentAdjustmentExportReport(req.TransId, req.CurrencyId, req.UserName, req.AdjustType, req.StartDate.DateTime, req.EndDate.DateTime);
            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            }
            
            var stream = _excelBll.Export(exportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"PaymentAdjustmentReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

    }
}