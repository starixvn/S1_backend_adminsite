using BackendSite.ExceptionHandling;
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

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            
            return new ApiResult<IEnumerable<DepositPaymentReportSummary>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        // [HttpPost]
        // public IActionResult ExportDepositReport(GetDepositPaymentReportRequest request)
        // {
        //     var data = _reportBll.GetDepositPaymentReport(
        //         new GetPaymentReportParams(request), request.StartTime.Offset, request.GroupBy);
        //     
        //     var result = _reportBll.ExportPaymentReport(data);
        //
        //     return _excelBll.ExcelFile(result, "DepositReport");
        // }
        
        [HttpPost]
        public async Task<ApiResult> GetWithdrawalReport(GetWithdrawalPaymentReportRequest request)
        {
            var data = await _reportBll.GetWithdrawalPaymentReportAsync(
                new GetPaymentReportParams(request), request.StartTime.Offset, request.GroupBy);
            
            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            
            return new ApiResult<IEnumerable<WithdrawalPaymentReportSummary>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        // [HttpPost]
        // public IActionResult ExportWithdrawalReport(GetWithdrawalPaymentReportRequest request)
        // {
        //     var data = _reportBll.GetWithdrawalPaymentReport(
        //         new GetPaymentReportParams(request), request.StartTime.Offset, request.GroupBy);
        //     
        //     var result = _reportBll.ExportPaymentReport(data);
        //
        //     return _excelBll.ExcelFile(result, "WithdrawalReport");
        // }
        
        [HttpPost]
        public ApiResult GetDepositType()
        {
            var data = EnumExtension
                .EnumToSelectListItem(new []{DepositTypeEnum.Undefined, DepositTypeEnum.LocalBanking});

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
                .EnumToSelectListItem(new []{WithdrawalTypeEnum.Undefined, WithdrawalTypeEnum.LocalBank});

            return new ApiResult<IEnumerable<SelectListItem>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
    }
}