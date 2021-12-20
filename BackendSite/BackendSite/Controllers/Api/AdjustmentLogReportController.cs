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
    [ApiExplorerSettings(IgnoreApi = true)]
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
        /// </remarks>
        /// <param name="para">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiListResult<AdjustmentLogReport> GetAdjustmentInfoLog(GetAdjustmentInfoLogParaOld para)
        {
            var startTime = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(para.StartDate),TimeSpan.Zero) ;
            var endTime =new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(para.EndDate),TimeSpan.Zero) ;
            var apiResult = new ApiListResult<AdjustmentLogReport>();
            try
            {
                var req = new GetAdjustmentInfoLogPara() 
                {
                    StartDate = startTime,
                    EndDate = endTime,
                    User = para.User,
                    Menu= para.Menu,
                    PageNumber= para.PageNumber,
                    PageSize= para.PageSize
                };
                var searchResult = adjustmentLogReportBLL.GetAdjustmentInfoLog(req);
                
                apiResult.Data = searchResult.AdjustmentLogReport;
                apiResult.PageNumber = searchResult.PageNumber;
                apiResult.TotalPages = searchResult.TotalPages;
                apiResult.RowCount = searchResult.RowCount;
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
