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
        private readonly ReportExcelBLL _reportExcelBll;

        #endregion

        #region Constructor

        public ConfiscatedSummaryReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
        }

        #endregion

        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 1 })]
        public ApiResult GetList(GetConfiscatedSummaryReportRequest request)
        {
            var data = _reportBll.GetConfiscatedSummaryReport(request);
            
            return new ApiResult<IEnumerable<ConfiscatedSummaryReportDetail>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            
        }
        
    }
}