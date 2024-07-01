using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class PaymentReportController : ControllerBase
    {
        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public PaymentReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        [HttpPost]
        public async Task<ApiResult> GetDepositReportAsync(GetDepositPaymentReportRequest request)
        {
            var data = await _reportBll.GetDepositPaymentReportAsync(
                new GetPaymentReportParams(request), request.StartTime.Offset, request.GroupBy);

            return new ApiResult<IEnumerable<DepositPaymentReportSummary>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public async Task<IActionResult> ExportDepositReport(GetDepositPaymentReportRequest request)
        {
            var data = await _reportBll.GetDepositPaymentReportAsync(
                new GetPaymentReportParams(request), request.StartTime.Offset, request.GroupBy);

            var result = ReportBLL.ExportPaymentReport<DepositPaymentReportSummary, DepositPaymentReportDetail>(data);

            return _excelBll.ExcelFile(result, "DepositReport");
        }

        [HttpPost]
        public async Task<ApiResult> GetWithdrawalReport(GetWithdrawalPaymentReportRequest request)
        {
            var data = await _reportBll.GetWithdrawalPaymentReportAsync(
                new GetPaymentReportParams(request), request.StartTime.Offset, request.GroupBy);

            return new ApiResult<IEnumerable<WithdrawalPaymentReportSummary>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public async Task<IActionResult> ExportWithdrawalReport(GetWithdrawalPaymentReportRequest request)
        {
            var data = await _reportBll.GetWithdrawalPaymentReportAsync(
                new GetPaymentReportParams(request), request.StartTime.Offset, request.GroupBy);

            var result = ReportBLL.ExportPaymentReport<WithdrawalPaymentReportSummary, WithdrawalPaymentReportDetail>(data);

            return _excelBll.ExcelFile(result, "WithdrawalReport");
        }

        [HttpPost]
        public ApiResult GetDepositType()
        {
            var data = EnumExtension
                .EnumToSelectListItem(new[] { DepositTypeEnum.LocalBanking });

            return new ApiResult<IEnumerable<SelectListItem>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult GetWithdrawalType()
        {
            var data = EnumExtension
                .EnumToSelectListItem(new[]
                {
                    WithdrawalTypeEnum.Undefined,
                    WithdrawalTypeEnum.LocalBank,
                    WithdrawalTypeEnum.CryptoWallet,
                    WithdrawalTypeEnum.Settlement
                });

            return new ApiResult<IEnumerable<SelectListItem>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
    }
}