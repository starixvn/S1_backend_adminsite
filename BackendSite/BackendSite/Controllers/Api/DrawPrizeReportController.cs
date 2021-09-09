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
    public class DrawPrizeReportController : Controller
    {
        private readonly PrizeBLL prizeBLL;
        private readonly ExcelBLL excelBLL;
        public DrawPrizeReportController(PrizeBLL prizeBLL, ExcelBLL excelBLL)
        {
            this.prizeBLL = prizeBLL;
            this.excelBLL = excelBLL;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        public ApiListResultCurrencyAmount<DrawPrize> PrizeSearch(PrizeSelect prizeSelect)
        {
            var apiResult = new ApiListResultCurrencyAmount<DrawPrize>();
            try
            {
                var prizeSearchResult = prizeBLL.PrizeSearch(prizeSelect);
                apiResult.Data = prizeSearchResult.DrawPrize;
                apiResult.PageNumber = prizeSearchResult.PageNumber;
                apiResult.TotalPages = prizeSearchResult.TotalPages;
                apiResult.RowCount = prizeSearchResult.RowCount;
                apiResult.CurrencyAmount = prizeSearchResult.CurrencyAmount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        public ApiListResult<PrizeLogData> GetPrizeLog(PrizeLogPara prizeLogPara)
        {
            var apiResult = new ApiListResult<PrizeLogData>();
            try
            {
                var logResult = prizeBLL.GetPrizeLog(prizeLogPara);
                apiResult.Data = logResult.PrizeLogData;
                apiResult.PageNumber = logResult.PageNumber;
                apiResult.TotalPages = logResult.TotalPages;
                apiResult.RowCount = logResult.RowCount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        public ApiResult<IEnumerable<EventDraw>> GetEventName()
        {

            var apiResult = new ApiResult<IEnumerable<EventDraw>>
            {
                Data = prizeBLL.GetEventName(),
                ErrorCode = 0
            };

            return apiResult;

        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        public IActionResult ExportPrizeData(string SelectSite, string StartDate, string EndDate, int CurrencyId, string EventName)
        {
            var ExportData = prizeBLL.GetExportData(SelectSite, StartDate, EndDate, CurrencyId, EventName);
            var stream = excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DrawPrize_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
    }
}
