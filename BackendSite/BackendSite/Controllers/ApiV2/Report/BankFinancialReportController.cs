using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BankFinancialReportController : ControllerBase
    {
        #region Fields
        
        private readonly BankFinancialBLL _bankFinancialBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructors

        public BankFinancialReportController(IServiceProvider provider)
        {
            _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region BankFinancial Report

        [HttpPost]
        public async Task<ApiResult<IEnumerable<BankFinancialReportCurrencySummary>>> Get(GetFinancialReportRequest request)
        {
            var data = await _bankFinancialBll.GetBankFinancialReportSummaryAsync(
                new GetFinancialReportParams(request));

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            
            return new ApiResult<IEnumerable<BankFinancialReportCurrencySummary>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        [HttpPost]
        public async Task<IActionResult> Export(GetFinancialReportRequest request)
        {
            var stream = await _bankFinancialBll
                .ExportBankFinancialReportAsync(new GetFinancialReportParams(request));
            
            return _excelBll.ExcelFile(stream, "BankFinancialReport");
        }

        #endregion
    }
}