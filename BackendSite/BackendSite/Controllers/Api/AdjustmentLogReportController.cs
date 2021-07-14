using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdjustmentLogReportController : Controller
    {
        private readonly AdjustmentLogReportBLL adjustmentLogReportBLL;
        public AdjustmentLogReportController(AdjustmentLogReportBLL adjustmentLogReportBLL)
        {
            this.adjustmentLogReportBLL = adjustmentLogReportBLL;
        }

        [HttpPost]
        public ApiResult<IEnumerable<AdjustmentInfoLogMenu>> GetAdjustmentInfoLogMenu()
        {

            var apiResult = new ApiResult<IEnumerable<AdjustmentInfoLogMenu>>();
            apiResult.Data = adjustmentLogReportBLL.GetAdjustmentInfoLogMenu();
            apiResult.ErrorCode = 0;

            return apiResult;

        }
        [HttpPost]
        public ApiListResult<AdjustmentLogReport> GetAdjustmentInfoLog(GetAdjustmentInfoLogPara para)
        {
            var apiResult = new ApiListResult<AdjustmentLogReport>();
            try
            {
                //var prizeSearchResult = prizeBLL.PrizeSearch(prizeSelect);
                //apiResult.Data = prizeSearchResult.DrawPrize;
                //apiResult.PageNumber = prizeSearchResult.PageNumber;
                //apiResult.TotalPages = prizeSearchResult.TotalPages;
                //apiResult.RowCount = prizeSearchResult.RowCount;
                //apiResult.CurrencyAmount = prizeSearchResult.CurrencyAmount;
                //apiResult.ErrorCode = 0;
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
