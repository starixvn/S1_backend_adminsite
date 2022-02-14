using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
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
    public class CustomerStatisticalReportController : ControllerBase
    {
        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly ReportExcelBLL _reportExcelBll;
        private readonly Common _common;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructors

        public CustomerStatisticalReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
            _common = provider.GetRequiredService<Common>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region Modle

        public class GetListRequest
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string UserName { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }
        
        public class GetDetailRequest
        {
            public int CustId { get; set; }
            public int GameId { get; set; }
            public string Date { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }

        #endregion

        #region StatisticalReport By Customers

        /// <summary>
        /// CustomerStatisticalReport/GetList 取得客戶下注紀錄明細
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Customer Statistical Report > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/CustomerStatisticalReport" })]
        public ApiResult GetList(GetListRequest request)
        {
            var data = _reportBll.GetCustomerStatisticalReport(
                request.CurrencyIds,
                JsonConvert.SerializeObject(request.SiteIds),
                JsonConvert.SerializeObject(request.CurrencyIds),
                request.StartDate,
                request.EndDate,
                request.UserName,
                _common.TransferToTimeZoneFormat(request.TimeZone));

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<CustomerStatisticalReportModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// CustomerStatisticalReport/Export 匯出客戶統計報表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Customer Statistical Report > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>檔案</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/CustomerStatisticalReport" })]
        public IActionResult Export(GetListRequest request)
        {

            #region Get Data
            
            var exportData = _reportBll.GetCustomerStatisticalReport(
                request.CurrencyIds,
                JsonConvert.SerializeObject(request.SiteIds),
                JsonConvert.SerializeObject(request.CurrencyIds),
                request.StartDate,
                request.EndDate,
                request.UserName,
                _common.TransferToTimeZoneFormat(request.TimeZone));

            var currencyData = _reportBll.GetCurrency();

            #endregion

            #region Initial Param

            var currencyList = exportData.Select(x => x.CurrencyId).Distinct();

            var summaryTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var byDateTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var summaryTitles = new Dictionary<string, Api.ReportController.TitleModel>();
            var byDateTitles = new Dictionary<string, Api.ReportController.TitleModel>();

            var summaryContent = new List<Dictionary<string, dynamic>>();
            var byDateContent = new List<Dictionary<string, dynamic>>();

            #endregion

            #region Set Title(Title & Total Title & Merge Title & Not Summary Title)

            #region Titles To Merge

            var summaryMergeTitles = new Dictionary<string, MergeTitleModel>();
            var byDateMergeTitles = new Dictionary<string, MergeTitleModel>();

            summaryMergeTitles.Add("FirstMergeTitle", new MergeTitleModel { Name = "", MergeRange = 3 });

            summaryMergeTitles.Add("TurnOverValidBet", new MergeTitleModel { Name = "TurnOver(ValidBet)", MergeRange = currencyList.Count() + 1 });
            summaryMergeTitles.Add("WinLoss", new MergeTitleModel { Name = "WinLoss", MergeRange = currencyList.Count() + 1 });

            summaryTitlesToMerge.Add(summaryMergeTitles);

            byDateMergeTitles.Add("FirstMergeTitle", new MergeTitleModel { Name = "", MergeRange = 4 });

            byDateMergeTitles.Add("TurnOverValidBet", new MergeTitleModel { Name = "TurnOver(ValidBet)", MergeRange = currencyList.Count() + 1 });
            byDateMergeTitles.Add("WinLoss", new MergeTitleModel { Name = "WinLoss", MergeRange = currencyList.Count() + 1 });

            byDateTitlesToMerge.Add(byDateMergeTitles);

            #endregion

            #region Title

            summaryTitles.Add("UserName", new Api.ReportController.TitleModel { Name = "User Name", SortId = 1 });
            summaryTitles.Add("CategoryName", new Api.ReportController.TitleModel { Name = "Category Name", SortId = 2 });
            summaryTitles.Add("GameName", new Api.ReportController.TitleModel { Name = "Game Name", SortId = 3 });
            summaryTitles.Add("TurnOverValidBetInMYR", new Api.ReportController.TitleModel { Name = "Total in MYR", SortId = 200 });
            summaryTitles.Add("WinLossInMYR", new Api.ReportController.TitleModel { Name = "Total in MYR", SortId = 400 });

            byDateTitles.Add("UserName", new Api.ReportController.TitleModel { Name = "User Name", SortId = 1 });
            byDateTitles.Add("Date", new Api.ReportController.TitleModel { Name = "Date", SortId = 2 });
            byDateTitles.Add("CategoryName", new Api.ReportController.TitleModel { Name = "Category Name", SortId = 3 });
            byDateTitles.Add("GameName", new Api.ReportController.TitleModel { Name = "Game Name", SortId = 4 });
            byDateTitles.Add("TurnOverValidBetInMYR", new Api.ReportController.TitleModel { Name = "Total in MYR", SortId = 200 });
            byDateTitles.Add("WinLossInMYR", new Api.ReportController.TitleModel { Name = "Total in MYR", SortId = 400 });

            foreach (var curr in currencyList)
            {
                var isoCode = currencyData.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();

                var TurnOverValidBetPropName = $"TurnOverValidBet-{isoCode}";
                var WinLossPropName = $"WinLoss-{isoCode}";

                var turnOverValidBetSortId = 100 + curr;
                var winLossSortId = 300 + curr;

                summaryTitles.Add(TurnOverValidBetPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = turnOverValidBetSortId });
                summaryTitles.Add(WinLossPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = winLossSortId });

                byDateTitles.Add(TurnOverValidBetPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = turnOverValidBetSortId });
                byDateTitles.Add(WinLossPropName, new Api.ReportController.TitleModel { Name = isoCode, SortId = winLossSortId });
            }

            #endregion

            #region Total Title

            var totalTitle = new List<string>
            {
                "TurnOverValidBetInMYR", "WinLossInMYR",
            };

            #endregion

            #region Group Title

            var groupTitle = new List<string>
            {
                "GameName","CategoryName"
            };

            #endregion

            #region Not Summary Title

            var notSummaryTitles = new List<string>
            {   "Key", "Date",
                "CustId", "UserName",
                "CurrencyId", "Currency",
                "CategoryId", "CategoryName",
                "GameId", "GameName"
            };

            #endregion

            #endregion

            foreach (var item in exportData)
            {
                //CurrencyPropName
                var turnOverValidBetPropName = $"TurnOverValidBet-{item.IsoCode}";
                var winLossPropName = $"WinLoss-{item.IsoCode}";

                #region Summary Find Define

                var getSummaryKey = $"{item.CustId}-{item.CategoryId}-{item.GameId}";

                var getSummaryItem = summaryContent.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getSummaryKey)
                );

                var getCategoryKey = $"{item.CustId}-{item.CategoryId}-Total";

                var getSummaryCategoryItem = summaryContent.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getCategoryKey)
                );

                var getUserNameKey = $"{item.CustId}-Total-Total";

                var getSummaryUserNameItem = summaryContent.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getUserNameKey)
                );

                #endregion

                #region ByDate Find Define

                var getByDateKey = $"{item.Date}-{item.CustId}-{item.CategoryId}-{item.GameId}";

                var getUserItem = byDateContent.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getByDateKey)
                );

                var getByDateUserNameKey = $"{item.CustId}-Total-Total";

                var getByDateUserItem = byDateContent.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getByDateUserNameKey)
                );

                #endregion

                #region Summary Content Add

                if (getSummaryItem != null)
                {
                    getSummaryItem.TryGetValue("TurnOverValidBetInMYR", out var turnOverValidBet);
                    getSummaryItem["TurnOverValidBetInMYR"] = (decimal)turnOverValidBet + item.TurnOverValidBetSummaryInMYR;

                    getSummaryItem.TryGetValue("WinLossInMYR", out var winLoss);
                    getSummaryItem["WinLossInMYR"] = (decimal)winLoss + item.WinLossSummaryInMYR;

                    //TurnOverValidBet
                    if (getSummaryItem.ContainsKey(turnOverValidBetPropName))
                    {
                        getSummaryItem.TryGetValue(turnOverValidBetPropName, out var turnoverdata);
                        getSummaryItem[turnOverValidBetPropName] = (decimal)turnoverdata + item.TurnOverValidBetSummary;
                    }
                    else
                    {
                        getSummaryItem.Add(turnOverValidBetPropName, item.TurnOverValidBetSummary);
                    }

                    //WinLoss
                    if (getSummaryItem.ContainsKey(winLossPropName))
                    {
                        getSummaryItem.TryGetValue(winLossPropName, out var winlossdata);
                        getSummaryItem[winLossPropName] = (decimal)winlossdata + item.WinLossSummary;
                    }
                    else
                    {
                        getSummaryItem.Add(winLossPropName, item.WinLossSummary);
                    }
                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getSummaryKey },
                        { "CustId", item.CustId },
                        { "UserName", item.UserName },
                        { "CategoryId", item.CategoryId },
                        { "CategoryName", item.CategoryName },
                        { "GameId", item.GameId },
                        { "GameName", item.GameName },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.IsoCode },

                        { turnOverValidBetPropName,  item.TurnOverValidBetSummary },
                        { winLossPropName, item.WinLossSummary },

                        { "TurnOverValidBetInMYR", item.TurnOverValidBetSummaryInMYR },
                        { "WinLossInMYR", item.WinLossSummaryInMYR }
                    };

                    summaryContent.Add(newItem);
                }

                if (getSummaryCategoryItem != null)
                {
                    getSummaryCategoryItem.TryGetValue("TurnOverValidBetInMYR", out var today);
                    getSummaryCategoryItem["TurnOverValidBetInMYR"] = (decimal)today + item.TurnOverValidBetSummaryInMYR;

                    getSummaryCategoryItem.TryGetValue("WinLossInMYR", out var withdrawal);
                    getSummaryCategoryItem["WinLossInMYR"] = (decimal)withdrawal + item.WinLossSummaryInMYR;

                    //TurnOverValidBet
                    if (getSummaryCategoryItem.ContainsKey(turnOverValidBetPropName))
                    {
                        getSummaryCategoryItem.TryGetValue(turnOverValidBetPropName, out var turnOverValidBet);
                        getSummaryCategoryItem[turnOverValidBetPropName] = (decimal)turnOverValidBet + item.TurnOverValidBetSummary;
                    }
                    else
                    {
                        getSummaryCategoryItem.Add(turnOverValidBetPropName, item.TurnOverValidBetSummary);
                    }

                    //WinLoss
                    if (getSummaryCategoryItem.ContainsKey(winLossPropName))
                    {
                        getSummaryCategoryItem.TryGetValue(winLossPropName, out var winLoss);
                        getSummaryCategoryItem[winLossPropName] = (decimal)winLoss + item.WinLossSummary;
                    }
                    else
                    {
                        getSummaryCategoryItem.Add(winLossPropName, item.WinLossSummary);
                    }

                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getCategoryKey },
                        { "CustId", item.CustId },
                        { "UserName", item.UserName },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.IsoCode },
                        { "CategoryId", item.CategoryId },
                        { "CategoryName", item.CategoryName },
                        { "GameId", 999999999 },
                        { "GameName", "TOTAL：" },

                        { turnOverValidBetPropName,  item.TurnOverValidBetSummary },
                        { winLossPropName, item.WinLossSummary },

                        { "TurnOverValidBetInMYR", item.TurnOverValidBetSummaryInMYR },
                        { "WinLossInMYR", item.WinLossSummaryInMYR }
                    };

                    summaryContent.Add(newItem);
                }

                if (getSummaryUserNameItem != null)
                {
                    getSummaryUserNameItem.TryGetValue("TurnOverValidBetInMYR", out var turnOverValidBetInMYR);
                    getSummaryUserNameItem["TurnOverValidBetInMYR"] = (decimal)turnOverValidBetInMYR + item.TurnOverValidBetSummaryInMYR;

                    getSummaryUserNameItem.TryGetValue("WinLossInMYR", out var winLossInMYR);
                    getSummaryUserNameItem["WinLossInMYR"] = (decimal)winLossInMYR + item.WinLossSummaryInMYR;

                    //TurnOverValidBet
                    if (getSummaryUserNameItem.ContainsKey(turnOverValidBetPropName))
                    {
                        getSummaryUserNameItem.TryGetValue(turnOverValidBetPropName, out var turnOverValidBet);
                        getSummaryUserNameItem[turnOverValidBetPropName] = (decimal)turnOverValidBet + item.TurnOverValidBetSummary;
                    }
                    else
                    {
                        getSummaryUserNameItem.Add(turnOverValidBetPropName, item.TurnOverValidBetSummary);
                    }

                    //WinLoss
                    if (getSummaryUserNameItem.ContainsKey(winLossPropName))
                    {
                        getSummaryUserNameItem.TryGetValue(winLossPropName, out var winLoss);
                        getSummaryUserNameItem[winLossPropName] = (decimal)winLoss + item.WinLossSummary;
                    }
                    else
                    {
                        getSummaryUserNameItem.Add(winLossPropName, item.WinLossSummary);
                    }

                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getUserNameKey },
                        { "CustId", item.CustId },
                        { "UserName", item.UserName },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.IsoCode },
                        { "CategoryId", 999999999 },
                        { "CategoryName", "USER" },
                        { "GameId", 999999999 },
                        { "GameName", "TOTAL：" },

                        { turnOverValidBetPropName,  item.TurnOverValidBetSummary },
                        { winLossPropName, item.WinLossSummary },

                        { "TurnOverValidBetInMYR", item.TurnOverValidBetSummaryInMYR },
                        { "WinLossInMYR", item.WinLossSummaryInMYR }
                    };

                    summaryContent.Add(newItem);
                }

                #endregion

                #region ByDate Content Add

                if (getUserItem != null)
                {
                    getUserItem.TryGetValue("TurnOverValidBetInMYR", out var turnOverValidBet);
                    getUserItem["TurnOverValidBetInMYR"] = (decimal)turnOverValidBet + item.TurnOverValidBetSummaryInMYR;

                    getUserItem.TryGetValue("WinLossInMYR", out var winLoss);
                    getUserItem["WinLossInMYR"] = (decimal)winLoss + item.WinLossSummaryInMYR;

                    //TurnOver
                    if (getUserItem.ContainsKey(turnOverValidBetPropName))
                    {
                        getUserItem.TryGetValue(turnOverValidBetPropName, out var turnoverdata);
                        getUserItem[turnOverValidBetPropName] = (decimal)turnoverdata + item.TurnOverValidBetSummaryInMYR;
                    }
                    else
                    {
                        getUserItem.Add(turnOverValidBetPropName, item.TurnOverValidBetSummaryInMYR);
                    }

                    //WinLoss
                    if (getUserItem.ContainsKey(winLossPropName))
                    {
                        getUserItem.TryGetValue(winLossPropName, out var winlossdata);
                        getUserItem[winLossPropName] = (decimal)winlossdata + item.WinLossSummaryInMYR;
                    }
                    else
                    {
                        getUserItem.Add(winLossPropName, item.WinLossSummaryInMYR);
                    }
                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getByDateKey },
                        { "Date", item.Date },
                        { "CustId", item.CustId },
                        { "UserName", item.UserName },
                        { "CategoryId", item.CategoryId },
                        { "CategoryName", item.CategoryName },
                        { "GameId", item.GameId },
                        { "GameName", item.GameName },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.IsoCode },

                        { turnOverValidBetPropName,  item.TurnOverValidBetSummary },
                        { winLossPropName, item.WinLossSummary },

                        { "TurnOverValidBetInMYR", item.TurnOverValidBetSummaryInMYR },
                        { "WinLossInMYR", item.WinLossSummaryInMYR }
                    };

                    byDateContent.Add(newItem);
                }

                if (getByDateUserItem != null)
                {
                    getByDateUserItem.TryGetValue("TurnOverValidBetInMYR", out var turnOverValidBetInMYR);
                    getByDateUserItem["TurnOverValidBetInMYR"] = (decimal)turnOverValidBetInMYR + item.TurnOverValidBetSummaryInMYR;

                    getByDateUserItem.TryGetValue("WinLossInMYR", out var winLossInMYR);
                    getByDateUserItem["WinLossInMYR"] = (decimal)winLossInMYR + item.WinLossSummaryInMYR;

                    //TurnOverValidBet
                    if (getByDateUserItem.ContainsKey(turnOverValidBetPropName))
                    {
                        getByDateUserItem.TryGetValue(turnOverValidBetPropName, out var turnOverValidBet);
                        getByDateUserItem[turnOverValidBetPropName] = (decimal)turnOverValidBet + item.TurnOverValidBetSummary;
                    }
                    else
                    {
                        getByDateUserItem.Add(turnOverValidBetPropName, item.TurnOverValidBetSummary);
                    }

                    //WinLoss
                    if (getByDateUserItem.ContainsKey(winLossPropName))
                    {
                        getByDateUserItem.TryGetValue(winLossPropName, out var winLoss);
                        getByDateUserItem[winLossPropName] = (decimal)winLoss + item.WinLossSummary;
                    }
                    else
                    {
                        getByDateUserItem.Add(winLossPropName, item.WinLossSummary);
                    }

                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getByDateUserNameKey },
                        { "Date", "USER" },
                        { "CustId", item.CustId },
                        { "UserName", item.UserName },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.IsoCode },
                        { "CategoryId", 999999999 },
                        { "CategoryName", "DATE" },
                        { "GameId", 999999999 },
                        { "GameName", "TOTAL：" },

                        { turnOverValidBetPropName,  item.TurnOverValidBetSummary },
                        { winLossPropName, item.WinLossSummary },

                        { "TurnOverValidBetInMYR", item.TurnOverValidBetSummaryInMYR },
                        { "WinLossInMYR", item.WinLossSummaryInMYR }
                    };

                    byDateContent.Add(newItem);
                }

                #endregion
            }

            #region Merge Content Rule

            #region Summary

            var custIdList = exportData.Select(x => x.CustId).Distinct().OrderBy(x => x);
            var summaryMergeContentRule = new List<int>();

            foreach (var custId in custIdList)
            {
                summaryMergeContentRule.Add(summaryContent.Count(x => x["CustId"] == custId));
            }

            #endregion

            #region ByDate

            var byDateMergeContentRule = new List<int>();

            foreach (var custId in custIdList)
            {
                byDateMergeContentRule.Add(byDateContent.Count(x => x["CustId"] == custId));
            }

            #endregion

            #endregion

            var d = new List<ComplexReportExportDataToExcel>()
            {
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "UserSummary",
                    Content = summaryContent.OrderBy(x => x["CustId"]).ThenBy(x=>x["CategoryId"]).ThenBy(x=>x["GameId"]).ThenBy(x=>x["Key"]),
                    Titles = summaryTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = summaryTitlesToMerge,
                    NotSummaryTitles = notSummaryTitles,
                    MergeContentRules = summaryMergeContentRule,
                    GroupTitles = groupTitle
                },
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "UserByDate",
                    Content = byDateContent.OrderBy(x => x["CustId"]).ThenBy(x=>x["Date"]),
                    Titles = byDateTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = byDateTitlesToMerge,
                    NotSummaryTitles = notSummaryTitles,
                    MergeContentRules = byDateMergeContentRule,
                    GroupTitles = groupTitle
                }
            };

            var stream = _reportExcelBll.ComplexReportExportToMultiSheet(d);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"CustomerStatisticalReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        /// <summary>
        /// CustomerStatisticalReport/GetDetail 取得客戶下注紀錄明細
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Customer Statistical Report > Click [YYYY/MM/DD]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/CustomerStatisticalReport" })]
        public ApiResult GetDetail(GetDetailRequest request)
        {
            var data = _reportBll.GetCustomerDetail(request.CustId, request.GameId, request.Date ,_common.TransferToTimeZoneFormat(request.TimeZone));
            
            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<TpBetRecordDataModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        #endregion
    }
}