using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class IovationReportController : ControllerBase
    {
        private readonly IovationReportBLL _iovationReport;
        private readonly ExcelBLL _excelBll;
        
        public IovationReportController(IovationReportBLL iovationReport, ExcelBLL excelBll)
        {
            _iovationReport = iovationReport;
            _excelBll = excelBll;
        }

        [HttpPost]
        public async Task<ApiListResult<IovationReport>> GetList(IovationReportGetListRequest request)
        {
            var result = await _iovationReport.GetListAsync(request);
            
            var apiResult = new ApiListResult<IovationReport>
            {
                Data = result.IovationReport,
                PageNumber = result.PageNumber,
                TotalPages = result.TotalPages,
                RowCount = result.RowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        [HttpPost]
        public async Task<IActionResult> Export(IovationReportExportRequest request)
        {
            var exportData = await _iovationReport.GetListWithoutPageAsync(request);

            var stream = _excelBll.Export(exportData, offset: request.StartTime.Offset);
            return _excelBll.ExcelFile(stream, "IovationReport");
        }
    }
}