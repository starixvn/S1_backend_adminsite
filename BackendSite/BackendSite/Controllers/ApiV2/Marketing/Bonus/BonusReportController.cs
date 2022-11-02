using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BonusReportController : ControllerBase
    {
        private readonly BonusReportBLL _bonusReportBll;
        private readonly ExcelBLL _excelBll;

        public BonusReportController(IServiceProvider provider)
        {
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _bonusReportBll = provider.GetRequiredService<BonusReportBLL>();
        }

        /// <summary>
        /// BonusReport/GetList 取得獎金報表資料清單
        /// </summary>
        /// <param name="request">
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Report > [Submit]
        /// </param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 73, 1 })]
        [HttpPost]
        public async Task<ApiListResultCurrencyAmount<BonusReportResponse>> GetDataList(BonusReportRequest request)
        {
            var apiResult = new ApiListResultCurrencyAmount<BonusReportResponse>();
            try
            {
                var result = await _bonusReportBll.GetBonusReportAsync(request);
                apiResult.Data = result.Data;
                apiResult.PageNumber = request.PageNumber;
                apiResult.RowCount = result.RowCount;
                apiResult.TotalPages = result.TotalPages;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.CurrencyAmount = result.TempCurrencyTotalAmount;
            }
            catch (Exception ex)
            {

                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// BonusReport/Export 匯出獎金報表
        /// </summary>
        /// <param name="request">
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Report > [Submit] > [Export Excel] 
        /// </param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 73, 2 })]
        [HttpPost]
        public async Task<IActionResult> Export(BonusReportExportRequest request)
        {
            var exportData = await _bonusReportBll.GetBonusReportExportData(request);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "BonusReport");
        }
    }
}