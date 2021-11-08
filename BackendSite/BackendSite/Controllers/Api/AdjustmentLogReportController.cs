using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitAdjust" })]
    public class AdjustmentLogReportController : Controller
    {
        private readonly AdjustmentLogReportBLL adjustmentLogReportBLL;
        public AdjustmentLogReportController(AdjustmentLogReportBLL adjustmentLogReportBLL)
        {
            this.adjustmentLogReportBLL = adjustmentLogReportBLL;
        }

        /// <summary>
        /// AdjustmentLogReport/GetAdjustmentInfoLogMenu 取得調整資料記錄程式清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Adjustment Log Report > DropDownList[Menu]
        /// 
        /// </remarks>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<IEnumerable<AdjustmentInfoLogMenu>> GetAdjustmentInfoLogMenu()
        {

            var apiResult = new ApiResult<IEnumerable<AdjustmentInfoLogMenu>>
            {
                Data = adjustmentLogReportBLL.GetAdjustmentInfoLogMenu(),
                ErrorCode = 0
            };

            return apiResult;

        }

        /// <summary>
        /// AdjustmentLogReport/GetAdjustmentInfoLog 取得調整資料紀錄
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Adjustment Log Report > [Submit]
        /// 
        /// </remarks>
        /// <param name="para">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiListResult<AdjustmentLogReport> GetAdjustmentInfoLog(GetAdjustmentInfoLogPara para)
        {
            var apiResult = new ApiListResult<AdjustmentLogReport>();
            try
            {
                var SearchResult = adjustmentLogReportBLL.GetAdjustmentInfoLog(para);
                apiResult.Data = SearchResult.AdjustmentLogReport;
                apiResult.PageNumber = SearchResult.PageNumber;
                apiResult.TotalPages = SearchResult.TotalPages;
                apiResult.RowCount = SearchResult.RowCount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
    }
}
