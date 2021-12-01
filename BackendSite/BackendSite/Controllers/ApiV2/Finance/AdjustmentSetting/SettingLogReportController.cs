using System;
using System.Collections.Generic;
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
        /// SettingLogReport/GetAdjustmentInfoLog 取得調整資料紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Adjustment Log Report > [Submit]
        /// </remarks>
        /// <param name="para">參數</param>
        [HttpPost]
        public ApiListResult<AdjustmentLogReport> GetAdjustmentInfoLog(GetAdjustmentInfoLogPara para)
        {
            var apiResult = new ApiListResult<AdjustmentLogReport>();
            try
            {
                var searchResult = _adjustmentLogReportBll.GetAdjustmentInfoLog(para);
                apiResult.Data = searchResult.AdjustmentLogReport;
                apiResult.PageNumber = searchResult.PageNumber;
                apiResult.TotalPages = searchResult.TotalPages;
                apiResult.RowCount = searchResult.RowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
    }
}