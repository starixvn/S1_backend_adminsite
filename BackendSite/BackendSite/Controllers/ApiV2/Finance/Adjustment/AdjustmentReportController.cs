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
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class AdjustmentReportController : ControllerBase
    {
        #region Fields

        private readonly PaymentAdjustmentReportBLL _paymentAdjustmentBll;
        private readonly ExcelBLL _excelBll;
        private readonly PermissionBLL _permissionBll;

        #endregion

        #region Constructors

        public AdjustmentReportController(IServiceProvider provider)
        {
            _paymentAdjustmentBll = provider.GetRequiredService<PaymentAdjustmentReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
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
        
        public class SetAdjustmentReq
        {
            public string TransId { get; set; }
            public int BeforeCategory { get; set; }
            public int AfterCategory { get; set; }
            public string BeforeMemo { get; set; }
            public string AfterMemo { get; set; }
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

        /// <summary>
        /// AdjustmentReport/SetAdjustment 變更調帳（原因和備註）
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Adjustment Report > [Submit] > [V]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult SetAdjustment(ApiRequestWithLog<SetAdjustment> request)
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
                    _paymentAdjustmentBll.SetAdjustment(new ParamsWithLog<SetAdjustment>(request, User.Identity.Name));

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
