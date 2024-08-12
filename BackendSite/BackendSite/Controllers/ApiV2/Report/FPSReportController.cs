using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class FPSReportController : ControllerBase
    {
        private readonly IFPSReportBLL _fpsReportBLL;
        private readonly ExcelBLL _excelBll;

        public FPSReportController(IServiceProvider provider)
        {
            _fpsReportBLL =  provider.GetRequiredService<IFPSReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }
        
        /// <summary>
        /// 查詢FPS列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiListResult<FPSReport>> GetList(FPSReportGetListRequest request)
        {
            var result = await _fpsReportBLL.GetFPSListAsync(request);

            var apiResult = new ApiListResult<FPSReport>
            {
                Data = result.FPSDataReport,
                TotalPages = result.TotalPages,
                RowCount = result.RowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// 匯出FPS報表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Export(FPSReportExportRequest request)
        {
            var exportData = await _fpsReportBLL.GetFPSListWithoutPageAsync(request);
            
            var stream = _excelBll.Export(exportData, offset: request.StartTime.Offset);
            return _excelBll.ExcelFile(stream, "FPSReport");
            return null;
        }

        /// <summary>
        /// 取得FPS相關連帳號
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<DeviceUser>>> GetAssociateAccount(GetAssociateAccountRequest request)
        {
            var deviceUserList = await _fpsReportBLL.GetFPSAssociateAccount(request.DeviceId);
            return new ApiResult<IEnumerable<DeviceUser>>
            {
                Data = deviceUserList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

    }
}