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
        private readonly ExcelBLL _excelBll;

        public IovationReportController(IovationReportBLL iovationReport, ExcelBLL excelBll)
        {
            _iovationReport = iovationReport;
            _excelBll = excelBll;
        }

        /// <summary>
        /// 查詢iovation列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 匯出iovation報表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Export(IovationReportExportRequest request)
        {
            var exportData = await _iovationReport.GetListWithoutPageAsync(request);

            var stream = _excelBll.Export(exportData, offset: request.StartTime.Offset);
            return _excelBll.ExcelFile(stream, "IovationReport");
        }

        /// <summary>
        /// 取得iovation相關連帳號
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<DeviceUser>>> GetAssociateAccount(GetAssociateAccountRequest request)
        {
            var deviceUserList = await _iovationReport.GetAssociateAccount(request.DeviceId);
            return new ApiResult<IEnumerable<DeviceUser>>
            {
                Data = deviceUserList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
    }
}