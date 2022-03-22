using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class StatisticalReportController : ControllerBase
    {
        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly ReportExcelBLL _reportExcelBll;
        private readonly Common _common;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public StatisticalReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
            _common = provider.GetRequiredService<Common>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region Model

        public class GetListRequest
        {
            public IEnumerable<int> SiteIds { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
            public string EndDate { get; set; }
        }

        public class TitleModel
        {
            public string Name { get; set; }
            public int SortId { get; set; }
        }

        #endregion

        /// <summary>
        /// StatisticalReport/GetList 取得統計報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Statistical Report > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/StatisticalReport" })]
        public ApiResult GetList(GetListRequest request)
        {
            #region Date Caculate

            var endDate = Convert.ToDateTime(request.EndDate);
            var startDate = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");

            #endregion

            var data = _reportBll.GetStatisticalReport(JsonConvert.SerializeObject(request.SiteIds), _common.TransferToTimeZoneFormat(request.TimeZone), startDate, request.EndDate);

            if (!data.Any())
            {
                return new ApiResult<IEnumerable<StatisticalReportModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.NoData
                };
            }

            return new ApiResult<IEnumerable<StatisticalReportModel>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// StatisticalReport/Export 匯出統計報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Statistical Report > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>檔案</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/StatisticalReport" })]
        public IActionResult Export(GetListRequest request)
        {

            #region Date Caculate

            var endDate = Convert.ToDateTime(request.EndDate);
            var startDate = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");

            #endregion

            #region Get Data

            var exportData = _reportBll.GetStatisticalReport(JsonConvert.SerializeObject(request.SiteIds), _common.TransferToTimeZoneFormat(request.TimeZone), startDate, request.EndDate);
            var currencyList = exportData.Select(x => x.CurrencyId).Distinct();
            var currencyData = _reportBll.GetCurrency();

            #endregion

            #region Initial Param

            var titlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var titles = new Dictionary<string, TitleModel>();
            var turnOverContent = new List<Dictionary<string, dynamic>>();
            var winLossContent = new List<Dictionary<string, dynamic>>();

            #endregion

            #region Set Title(Title & Total Title & Merge Title & Not Summary Title)

            #region Titles To Merge

            var mergeTitles = new Dictionary<string, MergeTitleModel>
            {
                {"FirstMergeTitle", new MergeTitleModel {Name = "", MergeRange = 2}},
                {"Today", new MergeTitleModel {Name = "Today", MergeRange = currencyList.Count() + 1}},
                {"Yesterday", new MergeTitleModel { Name = "Yesterday", MergeRange = currencyList.Count() + 1 }},
                {"LastTwoDays", new MergeTitleModel { Name = "Last 2 Days", MergeRange = currencyList.Count() + 1 }},
                {"ThisMonth", new MergeTitleModel { Name = "This Month", MergeRange = currencyList.Count() + 1 }},
                {"LastMonth", new MergeTitleModel { Name = "Last Month", MergeRange = currencyList.Count() + 1 }}
            };

            titlesToMerge.Add(mergeTitles);

            #endregion

            #region Title

            titles.Add("CategoryName", new TitleModel { Name = "CategoryName", SortId = 1 });
            titles.Add("GameName", new TitleModel { Name = "GameName", SortId = 2 });
            titles.Add("TodayInMYR", new TitleModel { Name = "Total in MYR", SortId = 200 });
            titles.Add("YesterdayInMYR", new TitleModel { Name = "Total in MYR", SortId = 400 });
            titles.Add("LastTwoDaysInMYR", new TitleModel { Name = "Total in MYR", SortId = 600 });
            titles.Add("ThisMonthInMYR", new TitleModel { Name = "Total in MYR", SortId = 800 });
            titles.Add("LastMonthInMYR", new TitleModel { Name = "Total in MYR", SortId = 1000 });

            foreach (var curr in currencyList)
            {
                var isoCode = currencyData.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();

                var todayPropName = $"Today-{isoCode}";
                var yesterdayPropName = $"Yesterday-{isoCode}";
                var lastTwoDaysPropName = $"LastTwoDays-{isoCode}";
                var thisMonthPropName = $"ThisMonth-{isoCode}";
                var lastMonthPropName = $"LastMonth-{isoCode}";

                var todaySortId = 100 + curr;
                var yesterdaySortId = 300 + curr;
                var lastTwoDaysSortId = 500 + curr;
                var thisMonthSortId = 700 + curr;
                var lastMonthSortId = 900 + curr;

                titles.Add(todayPropName, new TitleModel { Name = isoCode, SortId = todaySortId });
                titles.Add(yesterdayPropName, new TitleModel { Name = isoCode, SortId = yesterdaySortId });
                titles.Add(lastTwoDaysPropName, new TitleModel { Name = isoCode, SortId = lastTwoDaysSortId });
                titles.Add(thisMonthPropName, new TitleModel { Name = isoCode, SortId = thisMonthSortId });
                titles.Add(lastMonthPropName, new TitleModel { Name = isoCode, SortId = lastMonthSortId });
            }

            #endregion

            #region Total Title

            var totalTitle = new List<string>
            {
                "TodayInMYR", "YesterdayInMYR",
                "LastTwoDaysInMYR", "ThisMonthInMYR",
                "LastMonthInMYR"
            };

            #endregion

            #region Group Title

            var groupTitle = new List<string>
            {
                "GameName"
            };

            #endregion

            #region Not Summary Title

            var notSummaryTitles = new List<string>
            {   "Key", "CurrencyId",
                "Currency", "CategoryId",
                "CategoryName", "GameId",
                "GameName"
            };

            #endregion

            #endregion

            foreach (var item in exportData)
            {
                var todayPropName = $"Today-{item.IsoCode}";
                var yesterdayPropName = $"Yesterday-{item.IsoCode}";
                var lastTwoDaysPropName = $"LastTwoDays-{item.IsoCode}";
                var thisMonthPropName = $"ThisMonth-{item.IsoCode}";
                var lastMonthPropName = $"LastMonth-{item.IsoCode}";

                var getKey = $"{item.CategoryId}-{item.GameId}";

                var getTurnOverItem = turnOverContent.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                );

                var getWinLossItem = winLossContent.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                );

                var getCategoryKey = $"{item.CategoryId}-Total";

                var getTurnOverCategoryItem = turnOverContent.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getCategoryKey)
                );

                var getWinLossCategoryItem = winLossContent.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getCategoryKey)
                );

                if (getTurnOverItem != null)
                {
                    getTurnOverItem.TryGetValue("TodayInMYR", out var today);
                    getTurnOverItem["TodayInMYR"] = (decimal)today + item.TurnOverValidBetTodaySummaryInMYR;

                    getTurnOverItem.TryGetValue("YesterdayInMYR", out var withdrawal);
                    getTurnOverItem["YesterdayInMYR"] = (decimal)withdrawal + item.TurnOverValidBetYesterdaySummaryInMYR;

                    getTurnOverItem.TryGetValue("LastTwoDaysInMYR", out var nett);
                    getTurnOverItem["LastTwoDaysInMYR"] = (decimal)nett + item.TurnOverValidBetLastTwoDaysSummaryInMYR;

                    getTurnOverItem.TryGetValue("ThisMonthInMYR", out var bonus);
                    getTurnOverItem["ThisMonthInMYR"] = (decimal)bonus + item.TurnOverValidBetThisMonthSummaryInMYR;

                    getTurnOverItem.TryGetValue("LastMonthInMYR", out var rebate);
                    getTurnOverItem["LastMonthInMYR"] = (decimal)rebate + item.TurnOverValidBetLastMonthSummaryInMYR;


                    //Today
                    if (getTurnOverItem.ContainsKey(todayPropName))
                    {
                        getTurnOverItem.TryGetValue(todayPropName, out var todayData);
                        getTurnOverItem[todayPropName] = (decimal)todayData + item.TurnOverValidBetTodaySummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(todayPropName, item.TurnOverValidBetTodaySummary);
                    }

                    //Yesterday
                    if (getTurnOverItem.ContainsKey(yesterdayPropName))
                    {
                        getTurnOverItem.TryGetValue(yesterdayPropName, out var yesterdayData);
                        getTurnOverItem[yesterdayPropName] = (decimal)yesterdayData + item.TurnOverValidBetYesterdaySummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(yesterdayPropName, item.TurnOverValidBetYesterdaySummary);
                    }

                    //LastTwoDays
                    if (getTurnOverItem.ContainsKey(lastTwoDaysPropName))
                    {
                        getTurnOverItem.TryGetValue(lastTwoDaysPropName, out var lastTwoDaysData);
                        getTurnOverItem[lastTwoDaysPropName] = (decimal)lastTwoDaysData + item.TurnOverValidBetLastTwoDaysSummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(lastTwoDaysPropName, item.TurnOverValidBetLastTwoDaysSummary);
                    }

                    //ThisMonth
                    if (getTurnOverItem.ContainsKey(thisMonthPropName))
                    {
                        getTurnOverItem.TryGetValue(thisMonthPropName, out var thisMonthData);
                        getTurnOverItem[thisMonthPropName] = (decimal)thisMonthData + item.TurnOverValidBetThisMonthSummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(thisMonthPropName, item.TurnOverValidBetThisMonthSummary);
                    }

                    //LastMonth
                    if (getTurnOverItem.ContainsKey(lastMonthPropName))
                    {
                        getTurnOverItem.TryGetValue(lastMonthPropName, out var lastMonthData);
                        getTurnOverItem[lastMonthPropName] = (decimal)lastMonthData + item.TurnOverValidBetLastMonthSummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(lastMonthPropName, item.TurnOverValidBetLastMonthSummary);
                    }
                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getKey },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.IsoCode },
                        { "CategoryId", item.CategoryId },
                        { "CategoryName", item.CategoryName },
                        { "GameId", item.GameId },
                        { "GameName", item.GameName },

                        { todayPropName,  item.TurnOverValidBetTodaySummary },
                        { yesterdayPropName, item.TurnOverValidBetYesterdaySummary },
                        { lastTwoDaysPropName, item.TurnOverValidBetLastTwoDaysSummary },
                        { thisMonthPropName, item.TurnOverValidBetThisMonthSummary },
                        { lastMonthPropName, item.TurnOverValidBetLastMonthSummary },

                        { "TodayInMYR", item.TurnOverValidBetTodaySummaryInMYR },
                        { "YesterdayInMYR", item.TurnOverValidBetYesterdaySummaryInMYR },
                        { "LastTwoDaysInMYR", item.TurnOverValidBetLastTwoDaysSummaryInMYR },
                        { "ThisMonthInMYR", item.TurnOverValidBetThisMonthSummaryInMYR},
                        { "LastMonthInMYR", item.TurnOverValidBetLastMonthSummaryInMYR }
                    };

                    turnOverContent.Add(newItem);
                }

                if (getTurnOverCategoryItem != null)
                {
                    getTurnOverCategoryItem.TryGetValue("TodayInMYR", out var today);
                    getTurnOverCategoryItem["TodayInMYR"] = (decimal)today + item.TurnOverValidBetTodaySummaryInMYR;

                    getTurnOverCategoryItem.TryGetValue("YesterdayInMYR", out var withdrawal);
                    getTurnOverCategoryItem["YesterdayInMYR"] = (decimal)withdrawal + item.TurnOverValidBetYesterdaySummaryInMYR;

                    getTurnOverCategoryItem.TryGetValue("LastTwoDaysInMYR", out var nett);
                    getTurnOverCategoryItem["LastTwoDaysInMYR"] = (decimal)nett + item.TurnOverValidBetLastTwoDaysSummaryInMYR;

                    getTurnOverCategoryItem.TryGetValue("ThisMonthInMYR", out var bonus);
                    getTurnOverCategoryItem["ThisMonthInMYR"] = (decimal)bonus + item.TurnOverValidBetThisMonthSummaryInMYR;

                    getTurnOverCategoryItem.TryGetValue("LastMonthInMYR", out var rebate);
                    getTurnOverCategoryItem["LastMonthInMYR"] = (decimal)rebate + item.TurnOverValidBetLastMonthSummaryInMYR;


                    //Today
                    if (getTurnOverCategoryItem.ContainsKey(todayPropName))
                    {
                        getTurnOverCategoryItem.TryGetValue(todayPropName, out var todayData);
                        getTurnOverCategoryItem[todayPropName] = (decimal)todayData + item.TurnOverValidBetTodaySummary;
                    }
                    else
                    {
                        getTurnOverCategoryItem.Add(todayPropName, item.TurnOverValidBetTodaySummary);
                    }

                    //Yesterday
                    if (getTurnOverCategoryItem.ContainsKey(yesterdayPropName))
                    {
                        getTurnOverCategoryItem.TryGetValue(yesterdayPropName, out var yesterdayData);
                        getTurnOverCategoryItem[yesterdayPropName] = (decimal)yesterdayData + item.TurnOverValidBetYesterdaySummary;
                    }
                    else
                    {
                        getTurnOverCategoryItem.Add(yesterdayPropName, item.TurnOverValidBetYesterdaySummary);
                    }

                    //LastTwoDays
                    if (getTurnOverCategoryItem.ContainsKey(lastTwoDaysPropName))
                    {
                        getTurnOverCategoryItem.TryGetValue(lastTwoDaysPropName, out var lasttwodaysData);
                        getTurnOverCategoryItem[lastTwoDaysPropName] = (decimal)lasttwodaysData + item.TurnOverValidBetLastTwoDaysSummary;
                    }
                    else
                    {
                        getTurnOverCategoryItem.Add(lastTwoDaysPropName, item.TurnOverValidBetLastTwoDaysSummary);
                    }

                    //ThisMonth
                    if (getTurnOverCategoryItem.ContainsKey(thisMonthPropName))
                    {
                        getTurnOverCategoryItem.TryGetValue(thisMonthPropName, out var thismonthData);
                        getTurnOverCategoryItem[thisMonthPropName] = (decimal)thismonthData + item.TurnOverValidBetThisMonthSummary;
                    }
                    else
                    {
                        getTurnOverCategoryItem.Add(thisMonthPropName, item.TurnOverValidBetThisMonthSummary);
                    }

                    //LastMonth
                    if (getTurnOverCategoryItem.ContainsKey(lastMonthPropName))
                    {
                        getTurnOverCategoryItem.TryGetValue(lastMonthPropName, out var lastmonthData);
                        getTurnOverCategoryItem[lastMonthPropName] = (decimal)lastmonthData + item.TurnOverValidBetLastMonthSummary;
                    }
                    else
                    {
                        getTurnOverCategoryItem.Add(lastMonthPropName, item.TurnOverValidBetLastMonthSummary);
                    }
                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getCategoryKey },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.IsoCode },
                        { "CategoryId", item.CategoryId },
                        { "CategoryName", item.CategoryName },
                        { "GameId", "TOTAL" },
                        { "GameName", "TOTAL：" },

                        { todayPropName,  item.TurnOverValidBetTodaySummary },
                        { yesterdayPropName, item.TurnOverValidBetYesterdaySummary },
                        { lastTwoDaysPropName, item.TurnOverValidBetLastTwoDaysSummary },
                        { thisMonthPropName, item.TurnOverValidBetThisMonthSummary },
                        { lastMonthPropName, item.TurnOverValidBetLastMonthSummary },

                        { "TodayInMYR", item.TurnOverValidBetTodaySummaryInMYR },
                        { "YesterdayInMYR", item.TurnOverValidBetYesterdaySummaryInMYR },
                        { "LastTwoDaysInMYR", item.TurnOverValidBetLastTwoDaysSummaryInMYR },
                        { "ThisMonthInMYR", item.TurnOverValidBetThisMonthSummaryInMYR},
                        { "LastMonthInMYR", item.TurnOverValidBetLastMonthSummaryInMYR }
                    };

                    turnOverContent.Add(newItem);
                }

                if (getWinLossItem != null)
                {
                    getWinLossItem.TryGetValue("TodayInMYR", out var today);
                    getWinLossItem["TodayInMYR"] = (decimal)today + item.WinLossTodaySummaryInMYR;

                    getWinLossItem.TryGetValue("YesterdayInMYR", out var yesterday);
                    getWinLossItem["YesterdayInMYR"] = (decimal)yesterday + item.TurnOverValidBetYesterdaySummaryInMYR;

                    getWinLossItem.TryGetValue("LastTwoDaysInMYR", out var lasttwodays);
                    getWinLossItem["LastTwoDaysInMYR"] = (decimal)lasttwodays + item.WinLossLastTwoDaysSummaryInMYR;

                    getWinLossItem.TryGetValue("ThisMonthInMYR", out var thismonth);
                    getWinLossItem["ThisMonthInMYR"] = (decimal)thismonth + item.WinLossThisMonthSummaryInMYR;

                    getWinLossItem.TryGetValue("LastMonthInMYR", out var lastmonth);
                    getWinLossItem["LastMonthInMYR"] = (decimal)lastmonth + item.WinLossLastMonthSummaryInMYR;


                    //Today
                    if (getWinLossItem.ContainsKey(todayPropName))
                    {
                        getWinLossItem.TryGetValue(todayPropName, out var todayData);
                        getWinLossItem[todayPropName] = (decimal)todayData + item.WinLossTodaySummary;
                    }
                    else
                    {
                        getWinLossItem.Add(todayPropName, item.WinLossTodaySummary);
                    }

                    //Yesterday
                    if (getWinLossItem.ContainsKey(yesterdayPropName))
                    {
                        getWinLossItem.TryGetValue(yesterdayPropName, out var yesterdayData);
                        getWinLossItem[yesterdayPropName] = (decimal)yesterdayData + item.WinLossYesterdaySummary;
                    }
                    else
                    {
                        getWinLossItem.Add(yesterdayPropName, item.WinLossYesterdaySummary);
                    }

                    //LastTwoDays
                    if (getWinLossItem.ContainsKey(lastTwoDaysPropName))
                    {
                        getWinLossItem.TryGetValue(lastTwoDaysPropName, out var lasttwodaysData);
                        getWinLossItem[lastTwoDaysPropName] = (decimal)lasttwodaysData + item.WinLossLastTwoDaysSummary;
                    }
                    else
                    {
                        getWinLossItem.Add(lastTwoDaysPropName, item.WinLossLastTwoDaysSummary);
                    }

                    //ThisMonth
                    if (getWinLossItem.ContainsKey(thisMonthPropName))
                    {
                        getWinLossItem.TryGetValue(thisMonthPropName, out var thismonthData);
                        getWinLossItem[thisMonthPropName] = (decimal)thismonthData + item.WinLossThisMonthSummary;
                    }
                    else
                    {
                        getWinLossItem.Add(thisMonthPropName, item.WinLossThisMonthSummary);
                    }

                    //LastMonth
                    if (getWinLossItem.ContainsKey(lastMonthPropName))
                    {
                        getWinLossItem.TryGetValue(lastMonthPropName, out var lastmonthData);
                        getWinLossItem[lastMonthPropName] = (decimal)lastmonthData + item.WinLossLastMonthSummary;
                    }
                    else
                    {
                        getWinLossItem.Add(lastMonthPropName, item.WinLossLastMonthSummary);
                    }

                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getKey },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.IsoCode },
                        { "CategoryId", item.CategoryId },
                        { "CategoryName", item.CategoryName },
                        { "GameId", item.GameId },
                        { "GameName", item.GameName },

                        { todayPropName,  item.WinLossTodaySummary },
                        { yesterdayPropName, item.WinLossYesterdaySummary },
                        { lastTwoDaysPropName, item.WinLossLastTwoDaysSummary },
                        { thisMonthPropName, item.WinLossThisMonthSummary },
                        { lastMonthPropName, item.WinLossLastMonthSummary },

                        { "TodayInMYR", item.WinLossTodaySummaryInMYR },
                        { "YesterdayInMYR", item.WinLossYesterdaySummaryInMYR },
                        { "LastTwoDaysInMYR", item.WinLossLastTwoDaysSummaryInMYR },
                        { "ThisMonthInMYR", item.WinLossThisMonthSummaryInMYR},
                        { "LastMonthInMYR", item.WinLossLastMonthSummaryInMYR }
                    };

                    winLossContent.Add(newItem);
                }

                if (getWinLossCategoryItem != null)
                {
                    getWinLossCategoryItem.TryGetValue("TodayInMYR", out var today);
                    getWinLossCategoryItem["TodayInMYR"] = (decimal)today + item.WinLossTodaySummaryInMYR;

                    getWinLossCategoryItem.TryGetValue("YesterdayInMYR", out var yesterday);
                    getWinLossCategoryItem["YesterdayInMYR"] = (decimal)yesterday + item.TurnOverValidBetYesterdaySummaryInMYR;

                    getWinLossCategoryItem.TryGetValue("LastTwoDaysInMYR", out var lastTwoDays);
                    getWinLossCategoryItem["LastTwoDaysInMYR"] = (decimal)lastTwoDays + item.WinLossLastTwoDaysSummaryInMYR;

                    getWinLossCategoryItem.TryGetValue("ThisMonthInMYR", out var thisMonth);
                    getWinLossCategoryItem["ThisMonthInMYR"] = (decimal)thisMonth + item.WinLossThisMonthSummaryInMYR;

                    getWinLossCategoryItem.TryGetValue("LastMonthInMYR", out var lastMonth);
                    getWinLossCategoryItem["LastMonthInMYR"] = (decimal)lastMonth + item.WinLossLastMonthSummaryInMYR;


                    //Today
                    if (getWinLossCategoryItem.ContainsKey(todayPropName))
                    {
                        getWinLossCategoryItem.TryGetValue(todayPropName, out var todayData);
                        getWinLossCategoryItem[todayPropName] = (decimal)todayData + item.WinLossTodaySummary;
                    }
                    else
                    {
                        getWinLossCategoryItem.Add(todayPropName, item.WinLossTodaySummary);
                    }

                    //Yesterday
                    if (getWinLossCategoryItem.ContainsKey(yesterdayPropName))
                    {
                        getWinLossCategoryItem.TryGetValue(yesterdayPropName, out var yesterdayData);
                        getWinLossCategoryItem[yesterdayPropName] = (decimal)yesterdayData + item.WinLossYesterdaySummary;
                    }
                    else
                    {
                        getWinLossCategoryItem.Add(yesterdayPropName, item.WinLossYesterdaySummary);
                    }

                    //LastTwoDays
                    if (getWinLossCategoryItem.ContainsKey(lastTwoDaysPropName))
                    {
                        getWinLossCategoryItem.TryGetValue(lastTwoDaysPropName, out var lastTwoDaysData);
                        getWinLossCategoryItem[lastTwoDaysPropName] = (decimal)lastTwoDaysData + item.WinLossLastTwoDaysSummary;
                    }
                    else
                    {
                        getWinLossCategoryItem.Add(lastTwoDaysPropName, item.WinLossLastTwoDaysSummary);
                    }

                    //ThisMonth
                    if (getWinLossCategoryItem.ContainsKey(thisMonthPropName))
                    {
                        getWinLossCategoryItem.TryGetValue(thisMonthPropName, out var thismonthData);
                        getWinLossCategoryItem[thisMonthPropName] = (decimal)thismonthData + item.WinLossThisMonthSummary;
                    }
                    else
                    {
                        getWinLossCategoryItem.Add(thisMonthPropName, item.WinLossThisMonthSummary);
                    }

                    //LastMonth
                    if (getWinLossCategoryItem.ContainsKey(lastMonthPropName))
                    {
                        getWinLossCategoryItem.TryGetValue(lastMonthPropName, out var lastmonthData);
                        getWinLossCategoryItem[lastMonthPropName] = (decimal)lastmonthData + item.WinLossLastMonthSummary;
                    }
                    else
                    {
                        getWinLossCategoryItem.Add(lastMonthPropName, item.WinLossLastMonthSummary);
                    }

                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getCategoryKey },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.IsoCode },
                        { "CategoryId", item.CategoryId },
                        { "CategoryName", item.CategoryName },
                        { "GameId", "TOTAL" },
                        { "GameName", "TOTAL：" },

                        { todayPropName,  item.WinLossTodaySummary },
                        { yesterdayPropName, item.WinLossYesterdaySummary },
                        { lastTwoDaysPropName, item.WinLossLastTwoDaysSummary },
                        { thisMonthPropName, item.WinLossThisMonthSummary },
                        { lastMonthPropName, item.WinLossLastMonthSummary },

                        { "TodayInMYR", item.WinLossTodaySummaryInMYR },
                        { "YesterdayInMYR", item.WinLossYesterdaySummaryInMYR },
                        { "LastTwoDaysInMYR", item.WinLossLastTwoDaysSummaryInMYR },
                        { "ThisMonthInMYR", item.WinLossThisMonthSummaryInMYR},
                        { "LastMonthInMYR", item.WinLossLastMonthSummaryInMYR }
                    };

                    winLossContent.Add(newItem);
                }
            }

            #region Merge Content Rule

            var categoryIdList = exportData.Select(x => x.CategoryId).Distinct().OrderBy(x => x);
            var mergeContentRule = new List<int>();

            foreach (var categoryId in categoryIdList)
            {
                mergeContentRule.Add(turnOverContent.Count(x => x["CategoryId"] == categoryId));
            }

            #endregion

            var d = new List<ComplexReportExportDataToExcel>()
            {
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "TurnOver(ValidBet)",
                    Content = turnOverContent.OrderBy(x => x["Key"]),
                    Titles = titles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = titlesToMerge,
                    NotSummaryTitles = notSummaryTitles,
                    MergeContentRules = mergeContentRule,
                    GroupTitles = groupTitle
                },
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "WinLoss",
                    Content = winLossContent.OrderBy(x => x["Key"]),
                    Titles = titles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = titlesToMerge,
                    NotSummaryTitles = notSummaryTitles,
                    MergeContentRules = mergeContentRule,
                    GroupTitles = groupTitle
                }
            };

            var stream = _reportExcelBll.ComplexReportExportToMultiSheet(d);

            return _excelBll.ExcelFile(stream, "StatisticalReport");
        }
    }
}