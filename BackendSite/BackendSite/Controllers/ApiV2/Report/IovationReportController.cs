using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class IovationReportController : ControllerBase
    {
        private readonly IovationReportBLL _iovationReport;

        public IovationReportController(IovationReportBLL iovationReport)
        {
            _iovationReport = iovationReport;
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
            throw new NotImplementedException();
        }
    }
}