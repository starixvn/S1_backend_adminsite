using System.Globalization;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class ConfiscatedSummaryReportController : ControllerBase
    {
        #region Fields
        
        private readonly ReportBLL _reportBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public ConfiscatedSummaryReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 149, 1 })]
        public ApiResult GetList(GetConfiscatedSummaryReportRequest request)
        {
            var data = _reportBll.GetConfiscatedSummaryReportList(request ,request.PageNumber, out var pageInfo);
            
            return new ApiListResult<ConfiscatedSummaryReportDetail>(ErrorCodeEnum.Success, pageInfo, data);
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 149, 1 })]
        public ApiResult GetSummary(GetConfiscatedSummaryReportSummaryRequest request)
        {
            var data = _reportBll.GetConfiscatedSummary(request);
            
            return new ApiResult<ConfiscatedSummary>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 149, 2 })]
        public IActionResult ExportList(GetConfiscatedSummaryReportSummaryExportRequest request)
        {
            var getRequest = new GetConfiscatedSummaryReportRequest
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                SiteIds = request.SiteIds,
                CurrencyIds = request.CurrencyIds,
                BaseCurrencyId = request.BaseCurrencyId,
                UserName = request.UserName
            };
            
            var data = _reportBll.GetConfiscatedSummaryReportList(getRequest , -1 , out _);
            
            var exportList = data.Select(d => new ConfiscatedSummaryReportDetailExport
            {
                SuspendDate = d.SuspendDate.ToString(CultureInfo.InvariantCulture),
                CustId = d.CustId,
                CurrencyId = d.CurrencyId,
                UserName = d.UserName,
                SabaId = d.SabaId,
                Currency = d.Currency,
                RegistrationDate = d.RegistrationDate.ToString(CultureInfo.InvariantCulture),
                LevelName = d.LevelName,
                SeamlessWallet = d.SeamlessWallet,
                TotalDeposit = d.TotalDeposit,
                TotalWithdrawal = d.TotalWithdrawal,
                NetDeposit = d.NetDeposit,
                NetConfiscatedCash = d.NetConfiscatedCash
            }).ToList();
            
            var stream = _excelBll.ExportV2(exportList);
            return _excelBll.ExcelFile(stream, "ConfiscatedSummaryReport");
        }
    }
}