using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.Adjustment
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class AdjustmentReportController : ControllerBase
    {
        private readonly PaymentAdjustmentReportBLL _paymentAdjustmentBll;
        private readonly ExcelBLL _excelBll;
        private readonly PermissionBLL _permissionBll;
        private readonly NewAuthorizeBLL _newAuthorizeBll;

        public AdjustmentReportController(IServiceProvider provider)
        {
            _paymentAdjustmentBll = provider.GetRequiredService<PaymentAdjustmentReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
        }

        /// <summary>
        /// 取得調整資訊報表
        /// </summary>
        /// <remarks>
        /// Func Path：Finance > Adjustment Report > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 36, 1 })]
        public async Task<ApiListResultCurrencyAmount<PaymentAdjustmentReportResponse>> GetDataList(PaymentAdjustmentReportRequest request)
        {
            var apiResult = new ApiListResultCurrencyAmount<PaymentAdjustmentReportResponse>();

            try
            {
                var adjustTypeList = request.AdjustTypes.Select(x => (int)x);
                var siteId = _newAuthorizeBll.GetAuthData().SiteId;
                var queryResult = await _paymentAdjustmentBll.GetPaymentAdjustmentReportV2Async(siteId,
                    request.CurrencyIds,
                    request.UserName,
                    adjustTypeList,
                    request.StartDate.DateTime,
                    request.EndDate.DateTime,
                    request.TransId,
                    request.PageNumber,
                    request.PageSize);

                apiResult.Data = queryResult.Data;
                apiResult.PageNumber = request.PageNumber;
                apiResult.RowCount = queryResult.RowCount;
                apiResult.TotalPages = queryResult.TotalPages;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.CurrencyAmountRecord = queryResult.CurrencyAmount;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// AdjustmentReport/Export 匯出調整資訊報表
        /// </summary>
        /// <remarks>
        /// Func Path：Finance > Adjustment Report > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 36, 2 })]
        public async Task<IActionResult> Export(PaymentAdjustmentReportExportRequest request)
        {
            var adjustTypeList = request.AdjustTypes.Select(x => (int)x);
            var siteId = _newAuthorizeBll.GetAuthData().SiteId;
            var exportData = await _paymentAdjustmentBll.GetPaymentAdjustmentExportReportV2(siteId,
                request.CurrencyIds,
                request.UserName,
                adjustTypeList,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.TransId);

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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 36, 3 })]
        public ApiResult SetAdjustment(ApiRequestWithLog<SetAdjustment> request)
        {
            try
            {
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
