using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DrawPrizeReportController : Controller
    {
        private readonly PrizeBLL prizeBLL;
        private readonly ExcelBLL excelBLL;
        private readonly ServerInfoBLL serverInfoBLL;
        public DrawPrizeReportController(PrizeBLL prizeBLL, ExcelBLL excelBLL, ServerInfoBLL serverInfoBLL)
        {
            this.prizeBLL = prizeBLL;
            this.excelBLL = excelBLL;
            this.serverInfoBLL = serverInfoBLL;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        public ApiListResultCurrencyAmount<DrawPrize> PrizeSearch(PrizeSelectV1 prizeSelect)
        {
            var apiResult = new ApiListResultCurrencyAmount<DrawPrize>();
            try
            {
                var eventNameList = new List<string>();
                var currencyIdList = new List<int>();
                var siteInfoData = serverInfoBLL.GetSiteList();
                var eventNameListAll = prizeBLL.GetEventName();

                if (prizeSelect.CurrencyId == 0)
                {
                    foreach (var a in prizeSelect.SelectSite)
                    {
                        var siteData = siteInfoData.Where(x => x.SiteId == a).FirstOrDefault();

                        currencyIdList.AddRange(siteData.ICurrencyList);
                    }
                }
                else
                    currencyIdList.Add(prizeSelect.CurrencyId);

                if (string.IsNullOrEmpty(prizeSelect.EventName))
                {
                    eventNameList = eventNameListAll.Select(x => x.EventName).ToList();
                }
                else
                    eventNameList.Add(prizeSelect.EventName);

                var param = new PrizeSelect()
                {
                    SelectSite = prizeSelect.SelectSite,
                    CurrencyId = currencyIdList,
                    StartDate = prizeSelect.StartDate,
                    EndDate = prizeSelect.EndDate,
                    EventName = eventNameList,
                    PageNumber = prizeSelect.PageNumber,
                    PageSize = prizeSelect.PageSize,
                };

                var prizeSearchResult = prizeBLL.PrizeSearch(param, 8);
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
                var logResult = prizeBLL.GetPrizeLog(prizeLogPara, 0);
                foreach (var data in logResult.PrizeLogData)
                {
                    data.DrawTime = new DateTimeOffset(data.DrawTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                }
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
        public IActionResult ExportPrizeData(string selectSite, string startDate, string endDate, int currencyId, string eventName)
        {
            var currencyIdList = new List<int>();
            var eventNameList = new List<string>();
            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(selectSite);
            var siteInfoData = serverInfoBLL.GetSiteList();
            var eventNameListAll = prizeBLL.GetEventName();

            if (currencyId == 0)
            {
                foreach (var a in siteList)
                {
                    var siteData = siteInfoData.Where(x => x.SiteId == a).FirstOrDefault();

                    currencyIdList.AddRange(siteData.ICurrencyList);
                }
            }
            else
                currencyIdList.Add(currencyId);


            if (string.IsNullOrEmpty(eventName))
            {
                eventNameList = eventNameListAll.Select(x => x.EventName).ToList();
            }
            else
                eventNameList.Add(eventName);

            var exportData = prizeBLL.GetExportData(selectSite, startDate, endDate, JsonConvert.SerializeObject(currencyIdList),
               JsonConvert.SerializeObject(eventNameList), 8);

            var stream = excelBLL.Export(exportData);
            return excelBLL.ExcelFile(stream, "DrawPrizeReport");
        }
    }
}
