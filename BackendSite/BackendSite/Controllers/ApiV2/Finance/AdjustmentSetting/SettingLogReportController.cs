using System;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Finance.AdjustmentSetting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class SettingLogReportController : ControllerBase
    {
        #region Fields

        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;

        #endregion

        #region Constructors

        public SettingLogReportController(IServiceProvider provider)
        {
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
        }

        #endregion
        
        /// <summary>
        /// SettingLogReport/GetList 取得調整資料紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Adjustment Log Report > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        [HttpPost]
        public ApiListResult<AdminAdjustmentLogReport> GetList(GetAdminAdjustmentInfoLogInput request)
        {
            var apiResult = new ApiListResult<AdminAdjustmentLogReport>();
            try
            {
                var searchResult = _adjustmentLogReportBll.GetAdminAdjustmentInfoLog(request);

                return new ApiListResult<AdminAdjustmentLogReport>
                {
                    Data = searchResult.AdminAdjustmentLogReport,
                    PageNumber = searchResult.PageNumber,
                    TotalPages = searchResult.TotalPages,
                    RowCount = searchResult.RowCount,
                    ErrorCode = (int) ErrorCodeEnum.Success
                };
            }
            catch (Exception ex)
            {
                return new ApiListResult<AdminAdjustmentLogReport>
                {
                    ErrorCode = (int) ErrorCodeEnum.SystemException,
                    Message = ex.Message
                };
            }
        }
    }
}