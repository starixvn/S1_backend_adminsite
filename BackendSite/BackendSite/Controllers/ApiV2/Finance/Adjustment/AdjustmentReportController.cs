using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Finance.Adjustment
{
    [Authorize(AuthenticationSchemes = "backendV2")]
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
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public string UserName { get; set; }
            public IEnumerable<AdjustTypeEnum> AdjustTypes { get; set; }
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
            public string TransId { get; set; }
        }

        public class ExportRequest
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public string UserName { get; set; }
            public IEnumerable<AdjustTypeEnum> AdjustTypes { get; set; }
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
            public string TransId { get; set; }
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
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/AdjustmentReport" })]
        [HttpPost]
        public ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd> GetList(GetListRequest request)
        {
            var result = new ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd>();

            var currencyAmount = new List<string>();
            var adjustTypeList = request.AdjustTypes.Select(x => (int)x);

            var dataList = _paymentAdjustmentBll.GetPaymentAdjustmentReport(request.SiteIds,
                request.CurrencyIds,
                request.UserName,
                adjustTypeList,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.TransId,
                request.PageNumber,
                request.PageSize,
                out int totalPages, out int rowCount, out currencyAmount);

            foreach (var data in dataList)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            }

            result.Data = dataList;
            result.PageNumber = request.PageNumber;
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/AdjustmentReport" })]
        [HttpPost]
        public IActionResult Export(ExportRequest request)
        {
            var adjustTypeList = request.AdjustTypes.Select(x => (int)x);
            var exportData = _paymentAdjustmentBll.GetPaymentAdjustmentExportReport(request.SiteIds,
                request.CurrencyIds,
                request.UserName,
                adjustTypeList,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.TransId);

            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            }

            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "PaymentAdjustmentReport");
        }

    }
}