using System;
using System.Collections.Generic;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
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

        public class GetListRequest : ApiRequestPageModel
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

        public class ExportRequest
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
        /// AdjustmentReport/GetList 取得調整資訊報表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Adjustment Report > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        public ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd> GetList(GetListRequest request)
        {
            var result = new ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd>();

            var pageNumber = request.PageNumber;
            var totalPages = 0;
            var rowCount = 0;
            var currencyAmount = new List<string>();

            var dataList = _paymentAdjustmentBll.GetPaymentAdjustmentReport(request.TransId,
                request.CurrencyId,
                request.UserName,
                request.AdjustType,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.PageSize,
                ref pageNumber, ref totalPages, ref rowCount, ref currencyAmount);

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

            return result;
        }

        /// <summary>
        /// AdjustmentReport/Export 匯出調整資訊報表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Adjustment Report > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public IActionResult Export(ExportRequest request)
        {
            var exportData = _paymentAdjustmentBll.GetPaymentAdjustmentExportReport(request.TransId,
                request.CurrencyId,
                request.UserName,
                request.AdjustType,
                request.StartDate.DateTime,
                request.EndDate.DateTime);

            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            }

            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "PaymentAdjustmentReport");
        }

    }
}