using System;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Store.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Store.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [ApiController]
    [Route("apiV2/[controller]/[action]")]
    public class ReportController
    {
        private readonly StoreReportBLL _storeReportBLL;
        private readonly ExcelBLL _excelBll;

        public ReportController(IServiceProvider provider)
        {
            _storeReportBLL = provider.GetRequiredService<StoreReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #region DepositPoint

        [HttpPost]
        public Task<ApiResult<object>> GetDepositPointReportAsync(GetDepositPointReportRequest req)
        {
            var result = _storeReportBLL.GetDepositPointReportAsync(req);

            return result;
        }
        
        [HttpPost]
        public async Task<IActionResult> ExportDepositPointReportAsync(ExportDepositPointReportRequest req)
        {
            var result = await _storeReportBLL.ExportDepositPointReportAsync(req);
            
            return _excelBll.ExcelFile(result, "DepositPointReport");
        }

        #endregion
        
        #region AdjustmentPoint

        [HttpPost]
        public Task<ApiResult<object>> GetAdjustmentPointReportAsync(GetAdjustmentPointReportRequest req)
        {
            var result = _storeReportBLL.GetAdjustmentPointReportAsync(req);

            return result;
        }
        
        [HttpPost]
        public async Task<IActionResult> ExportAdjustmentPointReportAsync(ExportAdjustmentPointReportRequest req)
        {
            var result = await _storeReportBLL.ExportAdjustmentPointReportAsync(req);
            
            return _excelBll.ExcelFile(result, "AdjustmentPointReport");
        }

        #endregion
    }
}