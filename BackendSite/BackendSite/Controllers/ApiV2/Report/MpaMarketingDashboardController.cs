using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MpaMarketingDashboardController : ControllerBase
    {
        #region Fields
        
        private readonly MpaMarketingDashboardBLL _mpaMarketingDashboardBll;
        private readonly ExcelBLL _excelBll;
        private readonly ReportExcelBLL _reportExcelBll;

        #endregion

        #region Constructor

        public MpaMarketingDashboardController(IServiceProvider provider)
        {
            _mpaMarketingDashboardBll = provider.GetRequiredService<MpaMarketingDashboardBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
        }

        #endregion
        

        #region MpaBonusInfo
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 1 })]
        public ApiResult<PerformanceHighlightData> GetPerformanceHighlightData(GetDataRequest request)
        {
            var apiResult = new ApiResult<PerformanceHighlightData>();
            
            try
            {
                var data = _mpaMarketingDashboardBll.GetPerformanceHighlightData(request);
                apiResult.Data = data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        
        #endregion
        
    }
}