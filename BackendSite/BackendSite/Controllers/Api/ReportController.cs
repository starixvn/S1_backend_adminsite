using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReportController : ControllerBase
    {

        #region Fields

        private readonly ReportBLL _reportBLL;
        private readonly ReportExcelBLL _reportExcelBLL;
        private readonly ResponseSettingBLL _responseSettingBLL;

        #endregion

        #region Constructors

        public ReportController(IConfiguration configuration,
                                ReportBLL reportBLL,
                                ReportExcelBLL reportExcelBLL,
                                ResponseSettingBLL responseSettingBLL)
        {
            _reportBLL = reportBLL;
            _reportExcelBLL = reportExcelBLL;
            _responseSettingBLL = responseSettingBLL;
        }

        #endregion

        #region API Model

        public class GetMainFinancialReportReq
        {
            public IEnumerable<int> SiteIds
            {
                get;
                set;
            }

            public IEnumerable<int> CurrencyIds
            {
                get;
                set;
            }

            public string StartDate
            {
                get;
                set;
            }

            public string EndDate
            {
                get;
                set;
            }
        }

        public class GetStatisticalReportReq
        {
            public IEnumerable<int> SiteIds
            {
                get;
                set;
            }

            public string TimeZone
            {
                get;
                set;
            }

            public string EndDate
            {
                get;
                set;
            }
        }

        public class GetUserOnlineReportReq
        {
            public IEnumerable<int> SiteIds
            {
                get;
                set;
            }


            public string SelectDate
            {
                get;
                set;
            }
        }

        public class GetDepositReportReq
        {
            public IEnumerable<int> SiteIds
            {
                get;
                set;
            }

            public IEnumerable<int> CurrencyIds
            {
                get;
                set;
            }

            public string StartDate
            {
                get;
                set;
            }

            public string EndDate
            {
                get;
                set;
            }
        }

        public class GetCustomerStatisticalReportReq
        {
            public IEnumerable<int> SiteIds { get; set; }

            public IEnumerable<int> CurrencyIds { get; set; }

            public string StartDate { get; set; }

            public string EndDate { get; set; }

            public string UserName { get; set; }
        }

        public class GetCustomerDetailReq
        {
            public int CustId { get; set; }

            public int GameId { get; set; }

            public string Date { get; set; }
        }

        #endregion

        public class TitleModel
        {
            public string Name { get; set; }

            public int SortId { get; set; }
        }

        #region MainFinancial Report

        /// <summary>
        /// Report/GetMainFinancialReport 取得金融報表資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Reports >  Main Financial Report > [Submit]
        /// 
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/MainFinancialReport" })]
        public ApiResult GetMainFinancialReport(GetMainFinancialReportReq request)
        {
            var data = _reportBLL.GetMainFinancialReport(JsonConvert.SerializeObject(request.SiteIds),
                                                              JsonConvert.SerializeObject(request.CurrencyIds),
                                                              request.StartDate,
                                                              request.EndDate);

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<MainFinancialModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// Report/ExportMainFinancialReport 匯出金融報表資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Main Financial Report > [Export Excel]
        /// 
        /// </remarks>
        /// <param name="SiteIds"></param>
        /// <param name="CurrencyIds"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns>檔案</returns>
        [HttpPost]
        public IActionResult ExportMainFinancialReport(string SiteIds,
                                                       string CurrencyIds,
                                                       string StartDate,
                                                       string EndDate)
        {
            #region Get Data

            var currencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(CurrencyIds);

            var ExportData = _reportBLL.GetMainFinancialReport(SiteIds,
                                                              CurrencyIds,
                                                              StartDate,
                                                              EndDate);

            var currencyData = _reportBLL.GetCurrency();

            #endregion

            #region Initial Param

            var byDate = new List<Dictionary<string, dynamic>>();
            var byDateTitles = new Dictionary<string, TitleModel>();
            var titlesToMerge = new List<Dictionary<string, MergeTitleModel>>();

            #endregion

            #region Set Title(Title & Total Title & Merge Title & Not Summary Title)

            #region Merge Title

            var mergeTitles = new Dictionary<string, MergeTitleModel>();
            mergeTitles.Add("FirstMergeTitle", new MergeTitleModel { Name = "", MergeRange = 1 });

            mergeTitles.Add("Deposit", new MergeTitleModel { Name = "Deposit", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("Withdrawal", new MergeTitleModel { Name = "Withdrawal", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("Nett", new MergeTitleModel { Name = "Nett", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("Bonus", new MergeTitleModel { Name = "Bonus", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("Rebate", new MergeTitleModel { Name = "Rebate", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("Adjustment", new MergeTitleModel { Name = "Adjustment", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("TurnOverStake", new MergeTitleModel { Name = "TurnOver(Stake)", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("TurnOverValidBet", new MergeTitleModel { Name = "TurnOver(ValidBet)", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("WinLoss", new MergeTitleModel { Name = "WinLoss", MergeRange = currencyList.Count() + 1 });

            titlesToMerge.Add(mergeTitles);

            #endregion

            #region Title

            byDateTitles.Add("Date", new TitleModel { Name = "Date", SortId = 1 });
            byDateTitles.Add("DepositInMYR", new TitleModel { Name = "Total in MYR", SortId = 200 });
            byDateTitles.Add("WithdrawalInMYR", new TitleModel { Name = "Total in MYR", SortId = 400 });
            byDateTitles.Add("NettInMYR", new TitleModel { Name = "Total in MYR", SortId = 600 });
            byDateTitles.Add("BonusInMYR", new TitleModel { Name = "Total in MYR", SortId = 800 });
            byDateTitles.Add("RebateInMYR", new TitleModel { Name = "Total in MYR", SortId = 1000 });
            byDateTitles.Add("AdjustmentInMYR", new TitleModel { Name = "Total in MYR", SortId = 1200 });
            byDateTitles.Add("TurnOverStakeInMYR", new TitleModel { Name = "Total in MYR", SortId = 1400 });
            byDateTitles.Add("TurnOverValidBetInMYR", new TitleModel { Name = "Total in MYR", SortId = 1600 });
            byDateTitles.Add("WinLossInMYR", new TitleModel { Name = "Total in MYR", SortId = 1800 });

            foreach (var curr in currencyList)
            {
                var isoCode = currencyData.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();

                var DepositPropName = $"Deposit-{isoCode}";
                var WithdrawalPropName = $"Withdrawal-{isoCode}";
                var NettPropName = $"Nett-{isoCode}";
                var BonusPropName = $"Bonus-{isoCode}";
                var RebatePropName = $"Rebate-{isoCode}";
                var AdjustmentPropName = $"Adjustment-{isoCode}";
                var TurnOverStakePropName = $"TurnOverStake-{isoCode}";
                var TurnOverValidBetPropName = $"TurnOverValidBet-{isoCode}";
                var WinLossPropName = $"WinLoss-{isoCode}";

                var depositSortId = 100 + curr;
                var withdrawalSortId = 300 + curr;
                var nettSortId = 500 + curr;
                var bonusSortId = 700 + curr;
                var rebateSortId = 900 + curr;
                var adjustmentSortId = 1100 + curr;
                var turnOverStakeSortId = 1300 + curr;
                var turnOverValidBetSortId = 1500 + curr;
                var winLossSortId = 1700 + curr;

                byDateTitles.Add(DepositPropName, new TitleModel { Name = isoCode, SortId = depositSortId });
                byDateTitles.Add(WithdrawalPropName, new TitleModel { Name = isoCode, SortId = withdrawalSortId });
                byDateTitles.Add(NettPropName, new TitleModel { Name = isoCode, SortId = nettSortId });
                byDateTitles.Add(BonusPropName, new TitleModel { Name = isoCode, SortId = bonusSortId });
                byDateTitles.Add(RebatePropName, new TitleModel { Name = isoCode, SortId = rebateSortId });
                byDateTitles.Add(AdjustmentPropName, new TitleModel { Name = isoCode, SortId = adjustmentSortId });
                byDateTitles.Add(TurnOverStakePropName, new TitleModel { Name = isoCode, SortId = turnOverStakeSortId });
                byDateTitles.Add(TurnOverValidBetPropName, new TitleModel { Name = isoCode, SortId = turnOverValidBetSortId });
                byDateTitles.Add(WinLossPropName, new TitleModel { Name = isoCode, SortId = winLossSortId });

            }

            #endregion

            var totalTitle = new List<string>
            {
                "DepositInMYR", "WithdrawalInMYR",
                "NettInMYR", "BonusInMYR",
                "RebateInMYR", "AdjustmentInMYR",
                "TurnOverStakeInMYR", "TurnOverValidBetInMYR",
                "WinLossInMYR"
            };

            var notSummaryTitles = new List<string>
            {
                "Key", "Date",
                "CurrencyId", "Currency"
            };

            #endregion

            #region Set Content

            foreach (var item in ExportData)
            {
                var DepositPropName = $"Deposit-{item.IsoCode}";
                var WithdrawalPropName = $"Withdrawal-{item.IsoCode}";
                var NettPropName = $"Nett-{item.IsoCode}";
                var BonusPropName = $"Bonus-{item.IsoCode}";
                var RebatePropName = $"Rebate-{item.IsoCode}";
                var AdjustmentPropName = $"Adjustment-{item.IsoCode}";
                var TurnOverStakePropName = $"TurnOverStake-{item.IsoCode}";
                var TurnOverValidBetPropName = $"TurnOverValidBet-{item.IsoCode}";
                var WinLossPropName = $"WinLoss-{item.IsoCode}";

                var getByDateKey = $"{item.Date}";
                var getByDateItem = byDate.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getByDateKey)
                );

                if (getByDateItem != null)
                {
                    getByDateItem.TryGetValue("DepositInMYR", out var deposit);
                    getByDateItem["DepositInMYR"] = (decimal)deposit + item.DepositInMYR;

                    getByDateItem.TryGetValue("WithdrawalInMYR", out var withdrawal);
                    getByDateItem["WithdrawalInMYR"] = (decimal)withdrawal + item.WithdrawalInMYR;

                    getByDateItem.TryGetValue("NettInMYR", out var nett);
                    getByDateItem["NettInMYR"] = (decimal)nett + item.NettInMYR;

                    getByDateItem.TryGetValue("BonusInMYR", out var bonus);
                    getByDateItem["BonusInMYR"] = (decimal)bonus + item.BonusInMYR;

                    getByDateItem.TryGetValue("RebateInMYR", out var rebate);
                    getByDateItem["RebateInMYR"] = (decimal)rebate + item.RebateInMYR;

                    getByDateItem.TryGetValue("AdjustmentInMYR", out var adjustment);
                    getByDateItem["AdjustmentInMYR"] = (decimal)adjustment + item.AdjustmentInMYR;

                    getByDateItem.TryGetValue("TurnOverStakeInMYR", out var turnOverStake);
                    getByDateItem["TurnOverStakeInMYR"] = (decimal)turnOverStake + item.TurnOverStakeInMYR;

                    getByDateItem.TryGetValue("TurnOverValidBetInMYR", out var turnOverValidBet);
                    getByDateItem["TurnOverValidBetInMYR"] = (decimal)turnOverValidBet + item.TurnOverValidBetInMYR;

                    getByDateItem.TryGetValue("WinLossInMYR", out var winLoss);
                    getByDateItem["WinLossInMYR"] = (decimal)winLoss + item.WinLossInMYR;

                    //Deposit
                    if (getByDateItem.ContainsKey(DepositPropName))
                    {
                        getByDateItem.TryGetValue(DepositPropName, out var depositdata);
                        getByDateItem[DepositPropName] = (decimal)depositdata + item.Deposit;
                    }
                    else
                    {
                        getByDateItem.Add(DepositPropName, item.Deposit);
                    }

                    //Withdrawal
                    if (getByDateItem.ContainsKey(WithdrawalPropName))
                    {
                        getByDateItem.TryGetValue(WithdrawalPropName, out var withdrawaldata);
                        getByDateItem[WithdrawalPropName] = (decimal)withdrawaldata + item.Withdrawal;
                    }
                    else
                    {
                        getByDateItem.Add(WithdrawalPropName, item.Withdrawal);
                    }

                    //Nett
                    if (getByDateItem.ContainsKey(NettPropName))
                    {
                        getByDateItem.TryGetValue(NettPropName, out var nettdata);
                        getByDateItem[NettPropName] = (decimal)nettdata + item.Nett;
                    }
                    else
                    {
                        getByDateItem.Add(NettPropName, item.Nett);
                    }

                    //Bonus
                    if (getByDateItem.ContainsKey(BonusPropName))
                    {
                        getByDateItem.TryGetValue(BonusPropName, out var bonusdata);
                        getByDateItem[BonusPropName] = (decimal)bonusdata + item.Bonus;
                    }
                    else
                    {
                        getByDateItem.Add(BonusPropName, item.Bonus);
                    }

                    //Rebate

                    if (getByDateItem.ContainsKey(RebatePropName))
                    {
                        getByDateItem.TryGetValue(RebatePropName, out var rebatedata);
                        getByDateItem[RebatePropName] = (decimal)rebatedata + item.Rebate;
                    }
                    else
                    {
                        getByDateItem.Add(RebatePropName, item.Rebate);
                    }

                    //Adjustment
                    if (getByDateItem.ContainsKey(AdjustmentPropName))
                    {
                        getByDateItem.TryGetValue(AdjustmentPropName, out var adjustmentdata);
                        getByDateItem[AdjustmentPropName] = (decimal)adjustmentdata + item.Adjustment;
                    }
                    else
                    {
                        getByDateItem.Add(AdjustmentPropName, item.Adjustment);
                    }

                    //TurnOver(Stake)
                    if (getByDateItem.ContainsKey(TurnOverStakePropName))
                    {
                        getByDateItem.TryGetValue(TurnOverStakePropName, out var turnOverStakeData);
                        getByDateItem[TurnOverStakePropName] = (decimal)turnOverStakeData + item.TurnOverStake;
                    }
                    else
                    {
                        getByDateItem.Add(TurnOverStakePropName, item.TurnOverStake);
                    }

                    //TrunOver(VaildBet)
                    if (getByDateItem.ContainsKey(TurnOverValidBetPropName))
                    {
                        getByDateItem.TryGetValue(TurnOverValidBetPropName, out var turnOverValidBetData);
                        getByDateItem[TurnOverValidBetPropName] = (decimal)turnOverValidBetData + item.TurnOverValidBet;
                    }
                    else
                    {
                        getByDateItem.Add(TurnOverValidBetPropName, item.TurnOverValidBet);
                    }

                    //Winloss
                    if (getByDateItem.ContainsKey(WinLossPropName))
                    {
                        getByDateItem.TryGetValue(WinLossPropName, out var winLossData);
                        getByDateItem[WinLossPropName] = (decimal)winLossData + item.WinLoss;
                    }
                    else
                    {
                        getByDateItem.Add(WinLossPropName, item.WinLoss);
                    }

                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getByDateKey },
                        { "Date", item.Date },
                        { "CurrencyId", item.CurrencyId },
                        { "Currency", item.IsoCode },

                        { DepositPropName,  item.Deposit },
                        { WithdrawalPropName, item.Withdrawal },
                        { NettPropName, item.Nett },
                        { BonusPropName, item.Bonus },
                        { RebatePropName, item.Rebate },
                        { AdjustmentPropName, item.Adjustment },
                        { TurnOverStakePropName, item.TurnOverStake },
                        { TurnOverValidBetPropName, item.TurnOverValidBet },
                        { WinLossPropName, item.WinLoss },

                        { "DepositInMYR", item.DepositInMYR },
                        { "WithdrawalInMYR", item.WithdrawalInMYR },
                        { "NettInMYR", item.NettInMYR },
                        { "BonusInMYR", item.BonusInMYR},
                        { "RebateInMYR", item.RebateInMYR },
                        { "AdjustmentInMYR", item.AdjustmentInMYR},
                        { "TurnOverStakeInMYR", item.TurnOverStakeInMYR },
                        { "TurnOverValidBetInMYR", item.TurnOverValidBetInMYR },
                        { "WinLossInMYR", item.WinLossInMYR }
                    };

                    byDate.Add(newItem);
                }
            }

            #endregion

            var d = new List<ComplexReportExportDataToExcel>()
            {
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "ByDate",
                    Content = byDate,
                    Titles = byDateTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = titlesToMerge,
                    NotSummaryTitles = notSummaryTitles
                }
            };

            var stream = _reportExcelBLL.ComplexReportExportToMultiSheet(d);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"MainFinancialReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion

        #region StatisticalReport By Products

        /// <summary>
        /// Report/GetStatisticalReport 取得統計報表資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Statistical Report > [Submit]
        /// 
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/StatisticalReport" })]
        public ApiResult GetStatisticalReport(GetStatisticalReportReq request)
        {
            #region Date Caculate

            var endDate = Convert.ToDateTime(request.EndDate);
            var StartDate = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");

            #endregion

            var data = _reportBLL.GetStatisticalReport(JsonConvert.SerializeObject(request.SiteIds),
                                                             request.TimeZone,
                                                             StartDate,
                                                             request.EndDate);

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<StatisticalReportModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// Report/ExportStatisticalReport 匯出統計報表資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Statistical Report > [Export Excel]
        /// 
        /// </remarks>
        /// <param name="SiteIds"></param>
        /// <param name="TimeZone"></param>
        /// <param name="EndDate"></param>
        /// <returns>檔案</returns>
        [HttpPost]
        public IActionResult ExportStatisticalReport(string SiteIds,
                                                     string TimeZone,
                                                     string EndDate)
        {

            #region Date Caculate

            var endDate = Convert.ToDateTime(EndDate);
            var StartDate = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");

            #endregion

            #region Get Data

            var ExportData = _reportBLL.GetStatisticalReport(SiteIds,
                                                             TimeZone,
                                                             StartDate,
                                                             EndDate);

            var currencyList = ExportData.Select(x => x.CurrencyId).Distinct();

            var currencyData = _reportBLL.GetCurrency();

            #endregion

            #region Initial Param

            var titlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var Titles = new Dictionary<string, TitleModel>();

            var turnOverContent = new List<Dictionary<string, dynamic>>();
            var winLossContent = new List<Dictionary<string, dynamic>>();

            #endregion

            #region Set Title(Title & Total Title & Merge Title & Not Summary Title)

            #region Titles To Merge

            var mergeTitles = new Dictionary<string, MergeTitleModel>();
            mergeTitles.Add("FirstMergeTitle", new MergeTitleModel { Name = "", MergeRange = 2 });

            mergeTitles.Add("Today", new MergeTitleModel { Name = "Today", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("Yesterday", new MergeTitleModel { Name = "Yesterday", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("LastTwoDays", new MergeTitleModel { Name = "Last 2 Days", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("ThisMonth", new MergeTitleModel { Name = "This Month", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("LastMonth", new MergeTitleModel { Name = "Last Month", MergeRange = currencyList.Count() + 1 });

            titlesToMerge.Add(mergeTitles);

            #endregion

            #region Title

            Titles.Add("CategoryName", new TitleModel { Name = "CategoryName", SortId = 1 });
            Titles.Add("GameName", new TitleModel { Name = "GameName", SortId = 2 });
            Titles.Add("TodayInMYR", new TitleModel { Name = "Total in MYR", SortId = 200 });
            Titles.Add("YesterdayInMYR", new TitleModel { Name = "Total in MYR", SortId = 400 });
            Titles.Add("LastTwoDaysInMYR", new TitleModel { Name = "Total in MYR", SortId = 600 });
            Titles.Add("ThisMonthInMYR", new TitleModel { Name = "Total in MYR", SortId = 800 });
            Titles.Add("LastMonthInMYR", new TitleModel { Name = "Total in MYR", SortId = 1000 });

            foreach (var curr in currencyList)
            {
                var isoCode = currencyData.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();

                var TodayPropName = $"Today-{isoCode}";
                var YesterdayPropName = $"Yesterday-{isoCode}";
                var LastTwoDaysPropName = $"LastTwoDays-{isoCode}";
                var ThisMonthPropName = $"ThisMonth-{isoCode}";
                var LastMonthPropName = $"LastMonth-{isoCode}";

                var todaySortId = 100 + curr;
                var yesterdaySortId = 300 + curr;
                var lastTwoDaysSortId = 500 + curr;
                var thisMonthSortId = 700 + curr;
                var lastMonthSortId = 900 + curr;

                Titles.Add(TodayPropName, new TitleModel { Name = isoCode, SortId = todaySortId });
                Titles.Add(YesterdayPropName, new TitleModel { Name = isoCode, SortId = yesterdaySortId });
                Titles.Add(LastTwoDaysPropName, new TitleModel { Name = isoCode, SortId = lastTwoDaysSortId });
                Titles.Add(ThisMonthPropName, new TitleModel { Name = isoCode, SortId = thisMonthSortId });
                Titles.Add(LastMonthPropName, new TitleModel { Name = isoCode, SortId = lastMonthSortId });
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

            foreach (var item in ExportData)
            {

                var TodayPropName = $"Today-{item.IsoCode}";
                var YesterdayPropName = $"Yesterday-{item.IsoCode}";
                var LastTwoDaysPropName = $"LastTwoDays-{item.IsoCode}";
                var ThisMonthPropName = $"ThisMonth-{item.IsoCode}";
                var LastMonthPropName = $"LastMonth-{item.IsoCode}";

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
                    if (getTurnOverItem.ContainsKey(TodayPropName))
                    {
                        getTurnOverItem.TryGetValue(TodayPropName, out var todaydata);
                        getTurnOverItem[TodayPropName] = (decimal)todaydata + item.TurnOverValidBetTodaySummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(TodayPropName, item.TurnOverValidBetTodaySummary);
                    }

                    //Yesterday
                    if (getTurnOverItem.ContainsKey(YesterdayPropName))
                    {
                        getTurnOverItem.TryGetValue(YesterdayPropName, out var yesterdaydata);
                        getTurnOverItem[YesterdayPropName] = (decimal)yesterdaydata + item.TurnOverValidBetYesterdaySummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(YesterdayPropName, item.TurnOverValidBetYesterdaySummary);
                    }

                    //LastTwoDays
                    if (getTurnOverItem.ContainsKey(LastTwoDaysPropName))
                    {
                        getTurnOverItem.TryGetValue(LastTwoDaysPropName, out var lasttwodaysdata);
                        getTurnOverItem[LastTwoDaysPropName] = (decimal)lasttwodaysdata + item.TurnOverValidBetLastTwoDaysSummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(LastTwoDaysPropName, item.TurnOverValidBetLastTwoDaysSummary);
                    }

                    //ThisMonth
                    if (getTurnOverItem.ContainsKey(ThisMonthPropName))
                    {
                        getTurnOverItem.TryGetValue(ThisMonthPropName, out var thismonthdata);
                        getTurnOverItem[ThisMonthPropName] = (decimal)thismonthdata + item.TurnOverValidBetThisMonthSummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(ThisMonthPropName, item.TurnOverValidBetThisMonthSummary);
                    }

                    //LastMonth
                    if (getTurnOverItem.ContainsKey(LastMonthPropName))
                    {
                        getTurnOverItem.TryGetValue(LastMonthPropName, out var lastmonthdata);
                        getTurnOverItem[LastMonthPropName] = (decimal)lastmonthdata + item.TurnOverValidBetLastMonthSummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(LastMonthPropName, item.TurnOverValidBetLastMonthSummary);
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

                        { TodayPropName,  item.TurnOverValidBetTodaySummary },
                        { YesterdayPropName, item.TurnOverValidBetYesterdaySummary },
                        { LastTwoDaysPropName, item.TurnOverValidBetLastTwoDaysSummary },
                        { ThisMonthPropName, item.TurnOverValidBetThisMonthSummary },
                        { LastMonthPropName, item.TurnOverValidBetLastMonthSummary },

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
                    if (getTurnOverCategoryItem.ContainsKey(TodayPropName))
                    {
                        getTurnOverCategoryItem.TryGetValue(TodayPropName, out var todaydata);
                        getTurnOverCategoryItem[TodayPropName] = (decimal)todaydata + item.TurnOverValidBetTodaySummary;
                    }
                    else
                    {
                        getTurnOverCategoryItem.Add(TodayPropName, item.TurnOverValidBetTodaySummary);
                    }

                    //Yesterday
                    if (getTurnOverCategoryItem.ContainsKey(YesterdayPropName))
                    {
                        getTurnOverCategoryItem.TryGetValue(YesterdayPropName, out var yesterdaydata);
                        getTurnOverCategoryItem[YesterdayPropName] = (decimal)yesterdaydata + item.TurnOverValidBetYesterdaySummary;
                    }
                    else
                    {
                        getTurnOverCategoryItem.Add(YesterdayPropName, item.TurnOverValidBetYesterdaySummary);
                    }

                    //LastTwoDays
                    if (getTurnOverCategoryItem.ContainsKey(LastTwoDaysPropName))
                    {
                        getTurnOverCategoryItem.TryGetValue(LastTwoDaysPropName, out var lasttwodaysdata);
                        getTurnOverCategoryItem[LastTwoDaysPropName] = (decimal)lasttwodaysdata + item.TurnOverValidBetLastTwoDaysSummary;
                    }
                    else
                    {
                        getTurnOverCategoryItem.Add(LastTwoDaysPropName, item.TurnOverValidBetLastTwoDaysSummary);
                    }

                    //ThisMonth
                    if (getTurnOverCategoryItem.ContainsKey(ThisMonthPropName))
                    {
                        getTurnOverCategoryItem.TryGetValue(ThisMonthPropName, out var thismonthdata);
                        getTurnOverCategoryItem[ThisMonthPropName] = (decimal)thismonthdata + item.TurnOverValidBetThisMonthSummary;
                    }
                    else
                    {
                        getTurnOverCategoryItem.Add(ThisMonthPropName, item.TurnOverValidBetThisMonthSummary);
                    }

                    //LastMonth
                    if (getTurnOverCategoryItem.ContainsKey(LastMonthPropName))
                    {
                        getTurnOverCategoryItem.TryGetValue(LastMonthPropName, out var lastmonthdata);
                        getTurnOverCategoryItem[LastMonthPropName] = (decimal)lastmonthdata + item.TurnOverValidBetLastMonthSummary;
                    }
                    else
                    {
                        getTurnOverCategoryItem.Add(LastMonthPropName, item.TurnOverValidBetLastMonthSummary);
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

                        { TodayPropName,  item.TurnOverValidBetTodaySummary },
                        { YesterdayPropName, item.TurnOverValidBetYesterdaySummary },
                        { LastTwoDaysPropName, item.TurnOverValidBetLastTwoDaysSummary },
                        { ThisMonthPropName, item.TurnOverValidBetThisMonthSummary },
                        { LastMonthPropName, item.TurnOverValidBetLastMonthSummary },

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
                    if (getWinLossItem.ContainsKey(TodayPropName))
                    {
                        getWinLossItem.TryGetValue(TodayPropName, out var todaydata);
                        getWinLossItem[TodayPropName] = (decimal)todaydata + item.WinLossTodaySummary;
                    }
                    else
                    {
                        getWinLossItem.Add(TodayPropName, item.WinLossTodaySummary);
                    }

                    //Yesterday
                    if (getWinLossItem.ContainsKey(YesterdayPropName))
                    {
                        getWinLossItem.TryGetValue(YesterdayPropName, out var yesterdaydata);
                        getWinLossItem[YesterdayPropName] = (decimal)yesterdaydata + item.WinLossYesterdaySummary;
                    }
                    else
                    {
                        getWinLossItem.Add(YesterdayPropName, item.WinLossYesterdaySummary);
                    }

                    //LastTwoDays
                    if (getWinLossItem.ContainsKey(LastTwoDaysPropName))
                    {
                        getWinLossItem.TryGetValue(LastTwoDaysPropName, out var lasttwodaysdata);
                        getWinLossItem[LastTwoDaysPropName] = (decimal)lasttwodaysdata + item.WinLossLastTwoDaysSummary;
                    }
                    else
                    {
                        getWinLossItem.Add(LastTwoDaysPropName, item.WinLossLastTwoDaysSummary);
                    }

                    //ThisMonth
                    if (getWinLossItem.ContainsKey(ThisMonthPropName))
                    {
                        getWinLossItem.TryGetValue(ThisMonthPropName, out var thismonthdata);
                        getWinLossItem[ThisMonthPropName] = (decimal)thismonthdata + item.WinLossThisMonthSummary;
                    }
                    else
                    {
                        getWinLossItem.Add(ThisMonthPropName, item.WinLossThisMonthSummary);
                    }

                    //LastMonth
                    if (getWinLossItem.ContainsKey(LastMonthPropName))
                    {
                        getWinLossItem.TryGetValue(LastMonthPropName, out var lastmonthdata);
                        getWinLossItem[LastMonthPropName] = (decimal)lastmonthdata + item.WinLossLastMonthSummary;
                    }
                    else
                    {
                        getWinLossItem.Add(LastMonthPropName, item.WinLossLastMonthSummary);
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

                        { TodayPropName,  item.WinLossTodaySummary },
                        { YesterdayPropName, item.WinLossYesterdaySummary },
                        { LastTwoDaysPropName, item.WinLossLastTwoDaysSummary },
                        { ThisMonthPropName, item.WinLossThisMonthSummary },
                        { LastMonthPropName, item.WinLossLastMonthSummary },

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

                    getWinLossCategoryItem.TryGetValue("LastTwoDaysInMYR", out var lasttwodays);
                    getWinLossCategoryItem["LastTwoDaysInMYR"] = (decimal)lasttwodays + item.WinLossLastTwoDaysSummaryInMYR;

                    getWinLossCategoryItem.TryGetValue("ThisMonthInMYR", out var thismonth);
                    getWinLossCategoryItem["ThisMonthInMYR"] = (decimal)thismonth + item.WinLossThisMonthSummaryInMYR;

                    getWinLossCategoryItem.TryGetValue("LastMonthInMYR", out var lastmonth);
                    getWinLossCategoryItem["LastMonthInMYR"] = (decimal)lastmonth + item.WinLossLastMonthSummaryInMYR;


                    //Today
                    if (getWinLossCategoryItem.ContainsKey(TodayPropName))
                    {
                        getWinLossCategoryItem.TryGetValue(TodayPropName, out var todaydata);
                        getWinLossCategoryItem[TodayPropName] = (decimal)todaydata + item.WinLossTodaySummary;
                    }
                    else
                    {
                        getWinLossCategoryItem.Add(TodayPropName, item.WinLossTodaySummary);
                    }

                    //Yesterday
                    if (getWinLossCategoryItem.ContainsKey(YesterdayPropName))
                    {
                        getWinLossCategoryItem.TryGetValue(YesterdayPropName, out var yesterdaydata);
                        getWinLossCategoryItem[YesterdayPropName] = (decimal)yesterdaydata + item.WinLossYesterdaySummary;
                    }
                    else
                    {
                        getWinLossCategoryItem.Add(YesterdayPropName, item.WinLossYesterdaySummary);
                    }

                    //LastTwoDays
                    if (getWinLossCategoryItem.ContainsKey(LastTwoDaysPropName))
                    {
                        getWinLossCategoryItem.TryGetValue(LastTwoDaysPropName, out var lasttwodaysdata);
                        getWinLossCategoryItem[LastTwoDaysPropName] = (decimal)lasttwodaysdata + item.WinLossLastTwoDaysSummary;
                    }
                    else
                    {
                        getWinLossCategoryItem.Add(LastTwoDaysPropName, item.WinLossLastTwoDaysSummary);
                    }

                    //ThisMonth
                    if (getWinLossCategoryItem.ContainsKey(ThisMonthPropName))
                    {
                        getWinLossCategoryItem.TryGetValue(ThisMonthPropName, out var thismonthdata);
                        getWinLossCategoryItem[ThisMonthPropName] = (decimal)thismonthdata + item.WinLossThisMonthSummary;
                    }
                    else
                    {
                        getWinLossCategoryItem.Add(ThisMonthPropName, item.WinLossThisMonthSummary);
                    }

                    //LastMonth
                    if (getWinLossCategoryItem.ContainsKey(LastMonthPropName))
                    {
                        getWinLossCategoryItem.TryGetValue(LastMonthPropName, out var lastmonthdata);
                        getWinLossCategoryItem[LastMonthPropName] = (decimal)lastmonthdata + item.WinLossLastMonthSummary;
                    }
                    else
                    {
                        getWinLossCategoryItem.Add(LastMonthPropName, item.WinLossLastMonthSummary);
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

                        { TodayPropName,  item.WinLossTodaySummary },
                        { YesterdayPropName, item.WinLossYesterdaySummary },
                        { LastTwoDaysPropName, item.WinLossLastTwoDaysSummary },
                        { ThisMonthPropName, item.WinLossThisMonthSummary },
                        { LastMonthPropName, item.WinLossLastMonthSummary },

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

            var categoryIdList = ExportData.Select(x => x.CategoryId).Distinct().OrderBy(x => x);
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
                    Titles = Titles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
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
                    Titles = Titles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = titlesToMerge,
                    NotSummaryTitles = notSummaryTitles,
                    MergeContentRules = mergeContentRule,
                    GroupTitles = groupTitle
                }
            };

            var stream = _reportExcelBLL.ComplexReportExportToMultiSheet(d);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"StatisticalReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion

        #region UserOnlineReport

        /// <summary>
        /// Report/GetUserOnlineReport 取得使用者線上報表資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Reports > User Online Report > [Submit]
        /// 
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/UserOnlineReport" })]
        public ApiResult GetUserOnlineReport(GetUserOnlineReportReq request)
        {
            var data = _reportBLL.GetUserOnlineReport(JsonConvert.SerializeObject(request.SiteIds), request.SelectDate ,"+08:00");

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<UserOnlineReportModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// Report/ExportUserOnlineReport 匯出使用者線上報表資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Reports > User Online Report > [Export Excel]
        /// 
        /// </remarks>
        /// <param name="SiteIds"></param>
        /// <param name="SelectDate"></param>
        /// <returns>檔案</returns>
        [HttpPost]
        public IActionResult ExportUserOnlineReport(string SiteIds,
                                                    string SelectDate)
        {
            #region Get Data

            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(SiteIds);

            var ExportData = _reportBLL.GetUserOnlineReport(SiteIds, SelectDate ,"+08:00");

            var totalMaxCount = ExportData.GroupBy(x => new
            {
                LocalTime = x.LocalTime
            }).Select(x => x.Sum(y => y.MaxCount)).Max();

            var serverData = _reportBLL.GetServer();

            #endregion

            #region Initial Param

            var Content = new List<Dictionary<string, dynamic>>();
            var Titles = new Dictionary<string, TitleModel>();
            var titlesToMerge = new List<Dictionary<string, MergeTitleModel>>();

            #endregion

            #region Set Title(Merge Title & Title & Total Title & Not Summary Title)

            #region Merge Title

            var MergeTitles = new Dictionary<string, MergeTitleModel>();

            MergeTitles.Add("Date", new MergeTitleModel { Name = SelectDate, MergeRange = 1 });
            MergeTitles.Add("OnlineCount", new MergeTitleModel { Name = "Online Count", MergeRange = siteList.Count() + 1 });

            titlesToMerge.Add(MergeTitles);

            #endregion

            #region Title

            Titles.Add("LocalTime", new TitleModel { Name = "Local Time(GMT +8)", SortId = 1 });
            Titles.Add("TotalOnline", new TitleModel { Name = "Total Online", SortId = 200 });

            foreach (var site in siteList)
            {
                var siteName = serverData.Where(x => x.SiteId == site).Select(x => x.SiteName).FirstOrDefault();

                var OnlinePropName = $"Online-{siteName}";

                var onlineSortId = 100 + site;

                Titles.Add(OnlinePropName, new TitleModel { Name = siteName, SortId = onlineSortId });
            }

            #endregion

            var totalTitle = new List<string>
            {
                "TotalOnline",
            };

            var notSummaryTitles = new List<string>
            {
                "Key", "LocalTime",
                "TotalOnline"
            };

            #endregion

            foreach (var item in ExportData)
            {
                var OnlinePropName = $"Online-{item.SiteName}";

                var getKey = $"{item.LocalTime}";
                var getItem = Content.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                );

                if (getItem != null)
                {
                    getItem.TryGetValue("TotalOnline", out var totalCountdata);
                    getItem["TotalOnline"] = (int)totalCountdata + item.MaxCount;

                    if (getItem.ContainsKey(OnlinePropName))
                    {
                        getItem.TryGetValue(OnlinePropName, out var maxCountData);
                        getItem[OnlinePropName] = (int)maxCountData + item.MaxCount;
                    }
                    else
                    {
                        getItem.Add(OnlinePropName, item.MaxCount);
                    }

                    if (getItem["TotalOnline"] == totalMaxCount)
                    {
                        getItem["MaxCount"] = true;
                    }

                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getKey },
                        { "LocalTime", item.LocalTime },

                        { OnlinePropName,  item.MaxCount },
                        { "TotalOnline", item.MaxCount }
                    };

                    Content.Add(newItem);
                }
            }

            var d = new List<ComplexReportExportDataToExcel>()
            {
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "User Online Report",
                    Content = Content,
                    Titles = Titles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = titlesToMerge,
                    NotSummaryTitles = notSummaryTitles,
                    IsSummary = false,
                    IsTotalTitleMax = true
                },
            };

            var stream = _reportExcelBLL.ComplexReportExportToMultiSheet(d);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"UserOnlineReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion

        #region Deposit Report

        /// <summary>
        /// Report/GetDepositReport 取得存款報表資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Deposit Report > [Submit]
        /// 
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/DepositReport" })]
        public ApiResult GetDepositReport(GetDepositReportReq request)
        {
            var data = _reportBLL.GetDepositReport(request.CurrencyIds,
                                                   JsonConvert.SerializeObject(request.SiteIds),
                                                   JsonConvert.SerializeObject(request.CurrencyIds),
                                                   request.StartDate,
                                                   request.EndDate);

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<DepositReportModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// Report/ExportDepositReport 匯出存款報表資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Deposit Report > [Export Excel]
        /// 
        /// </remarks>
        /// <param name="SiteIds"></param>
        /// <param name="CurrencyIds"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns>檔案</returns>
        [HttpPost]
        public IActionResult ExportDepositReport(string SiteIds,
                                               string CurrencyIds,
                                               string StartDate,
                                               string EndDate)
        {
            #region Get Data

            var currencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(CurrencyIds);

            var ExportData = _reportBLL.GetDepositReport(currencyList,
                                                         SiteIds,
                                                         CurrencyIds,
                                                         StartDate,
                                                         EndDate);

            var currencyData = _reportBLL.GetCurrency();

            #endregion

            #region Initial Param

            var summaryTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var summaryTitles = new Dictionary<string, TitleModel>();
            var summaryTotalTitle = new List<string>();
            var summaryContent = new List<Dictionary<string, dynamic>>();
            var summaryTotalContent = new List<Dictionary<string, dynamic>>();

            var dateTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var dateTitles = new Dictionary<string, TitleModel>();
            var dateTotalTitle = new List<string>();
            var dateContent = new List<Dictionary<string, dynamic>>();
            var dateTotalContent = new List<Dictionary<string, dynamic>>();

            #endregion

            #region Set Title(Merge Title & Title & Total Title)

            #region Summary Merge Title

            var depositTypeTitles = new Dictionary<string, MergeTitleModel>();
            depositTypeTitles.Add("FirstMergeTitle", new MergeTitleModel { Name = "", MergeRange = 1 });

            depositTypeTitles.Add("OnlineBanking", new MergeTitleModel { Name = "Online Banking", MergeRange = (currencyList.Count() + 1) * 2 });
            depositTypeTitles.Add("MobilePay", new MergeTitleModel { Name = "Mobile Pay", MergeRange = (currencyList.Count() + 1) * 2 });
            depositTypeTitles.Add("LocalBanking", new MergeTitleModel { Name = "Local Banking", MergeRange = (currencyList.Count() + 1) * 2 });
            depositTypeTitles.Add("TotalData", new MergeTitleModel { Name = "", MergeRange = (currencyList.Count() + 1) * 6 });

            summaryTitlesToMerge.Add(depositTypeTitles);

            var mergeTitles = new Dictionary<string, MergeTitleModel>();
            mergeTitles.Add("SecondMergeTitle", new MergeTitleModel { Name = "", MergeRange = 1 });

            mergeTitles.Add("OnlineAmount", new MergeTitleModel { Name = "Amount", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("OnlinePrecent", new MergeTitleModel { Name = "%", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("MobileAmount", new MergeTitleModel { Name = "Amount", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("MobilePrecent", new MergeTitleModel { Name = "%", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("LocalAmount", new MergeTitleModel { Name = "Amount", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("LocalPrecent", new MergeTitleModel { Name = "%", MergeRange = currencyList.Count() + 1 });

            mergeTitles.Add("TotalAmount", new MergeTitleModel { Name = "Total Amount", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("TotalCount", new MergeTitleModel { Name = "Total Count", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("AverangeAmount", new MergeTitleModel { Name = "Averange Amount", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("SuccessRate", new MergeTitleModel { Name = "Success Rate(%)", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("FTDCount", new MergeTitleModel { Name = "FTD Count", MergeRange = currencyList.Count() + 1 });
            mergeTitles.Add("AverangeProcessingTime", new MergeTitleModel { Name = "Averange Processing Time(D.HH:MM:SS)", MergeRange = currencyList.Count() + 1 });

            summaryTitlesToMerge.Add(mergeTitles);

            #endregion

            #region Summary Title

            summaryTitles.Add("AccountName", new TitleModel { Name = "AccountName", SortId = 1 });
            summaryTitles.Add("OnlineAmount-in-MYR", new TitleModel { Name = "Total in MYR", SortId = 200 });
            summaryTitles.Add("OnlinePrecent-in-MYR", new TitleModel { Name = "Total in MYR", SortId = 400 });
            summaryTitles.Add("MobileAmount-in-MYR", new TitleModel { Name = "Total in MYR", SortId = 600 });
            summaryTitles.Add("MobilePrecent-in-MYR", new TitleModel { Name = "Total in MYR", SortId = 800 });
            summaryTitles.Add("LocalAmount-in-MYR", new TitleModel { Name = "Total in MYR", SortId = 1000 });
            summaryTitles.Add("LocalPrecent-in-MYR", new TitleModel { Name = "Total in MYR", SortId = 1200 });

            summaryTitles.Add("TotalAmount", new TitleModel { Name = "Total Amount In MYR", SortId = 1400 });
            summaryTitles.Add("TotalCount", new TitleModel { Name = "Total Count", SortId = 1600 });
            summaryTitles.Add("AverangeAmount", new TitleModel { Name = "Averange Amount In MYR", SortId = 1800 });
            summaryTitles.Add("SuccessRate", new TitleModel { Name = "Total Success Rate", SortId = 2000 });
            summaryTitles.Add("FTDCount", new TitleModel { Name = "Total FTD Count", SortId = 2200 });
            summaryTitles.Add("ApTime", new TitleModel { Name = "Total Averange Processing Time", SortId = 2400 });

            foreach (var curr in currencyList)
            {
                var isoCode = currencyData.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();

                var OnlineAmountPropName = $"OnlineAmount-{isoCode}";
                var OnlinePrecentPropName = $"OnlinePrecent-{isoCode}";
                var MobileAmountPropName = $"MobileAmount-{isoCode}";
                var MobilePrecentPropName = $"MobilePrecent-{isoCode}";
                var LocalAmountPropName = $"LocalAmount-{isoCode}";
                var LocalPrecentPropName = $"LocalPrecent-{isoCode}";

                var TotalAmountPropName = $"TotalAmount-{isoCode}";
                var TotalCountPropName = $"TotalCount-{isoCode}";
                var AverangeAmountPropName = $"AverangeAmount-{isoCode}";
                var SuccessRatePropName = $"SuccessRate-{isoCode}";
                var FTDCountPropName = $"FTDCount-{isoCode}";
                var ApTimePropName = $"ApTime-{isoCode}";

                var onlineAmountSortId = 100 + curr;
                var onlinePrecentSortId = 300 + curr;
                var mobileAmountSortId = 500 + curr;
                var mobilePrecentSortId = 700 + curr;
                var localAmountSortId = 900 + curr;
                var localPrecentSortId = 1100 + curr;

                var totalAmountSortId = 1300 + curr;
                var totalCountSortId = 1500 + curr;
                var averangeAmountSortId = 1700 + curr;
                var successRateSortId = 1900 + curr;
                var ftdCountSortId = 2100 + curr;
                var apTimeSortId = 2300 + curr;

                summaryTitles.Add(OnlineAmountPropName, new TitleModel { Name = isoCode, SortId = onlineAmountSortId });
                summaryTitles.Add(OnlinePrecentPropName, new TitleModel { Name = isoCode, SortId = onlinePrecentSortId });
                summaryTitles.Add(MobileAmountPropName, new TitleModel { Name = isoCode, SortId = mobileAmountSortId });
                summaryTitles.Add(MobilePrecentPropName, new TitleModel { Name = isoCode, SortId = mobilePrecentSortId });
                summaryTitles.Add(LocalAmountPropName, new TitleModel { Name = isoCode, SortId = localAmountSortId });
                summaryTitles.Add(LocalPrecentPropName, new TitleModel { Name = isoCode, SortId = localPrecentSortId });

                summaryTitles.Add(TotalAmountPropName, new TitleModel { Name = isoCode, SortId = totalAmountSortId });
                summaryTitles.Add(TotalCountPropName, new TitleModel { Name = isoCode, SortId = totalCountSortId });
                summaryTitles.Add(AverangeAmountPropName, new TitleModel { Name = isoCode, SortId = averangeAmountSortId });
                summaryTitles.Add(SuccessRatePropName, new TitleModel { Name = isoCode, SortId = successRateSortId });
                summaryTitles.Add(FTDCountPropName, new TitleModel { Name = isoCode, SortId = ftdCountSortId });
                summaryTitles.Add(ApTimePropName, new TitleModel { Name = isoCode, SortId = apTimeSortId });

            }

            #endregion

            #region Summary Total Title

            summaryTotalTitle = new List<string>
            {
                "OnlineAmount-in-MYR", "OnlinePrecent-in-MYR",
                "MobileAmount-in-MYR", "MobilePrecent-in-MYR",
                "LocalAmount-in-MYR","LocalPrecent-in-MYR",

                "TotalAmount","TotalCount",
                "AverangeAmount","SuccessRate",
                "FTDCount","ApTime"
            };

            #endregion

            #region Date Merge Title

            var accountNameTitles = new Dictionary<string, MergeTitleModel>();
            var dateMergeTitles = new Dictionary<string, MergeTitleModel>();

            accountNameTitles.Add("Account", new MergeTitleModel { Name = "Account", MergeRange = 1 });
            dateMergeTitles.Add("SecondMergeTitle", new MergeTitleModel { Name = "", MergeRange = 1 });

            foreach (var data in ExportData)
            {
                foreach (var account in data.SummaryAccountData)
                {
                    accountNameTitles.Add($"{account.AccountType}-{account.AccountId}-AccountName", new MergeTitleModel { Name = account.AccountName, MergeRange = (currencyList.Count() + 1) * 5 });

                    dateMergeTitles.Add($"{account.AccountType}-{account.AccountId}-TotalAmount", new MergeTitleModel { Name = "Total Amount", MergeRange = currencyList.Count() + 1 });
                    dateMergeTitles.Add($"{account.AccountType}-{account.AccountId}-TotalCount", new MergeTitleModel { Name = "Total Count", MergeRange = currencyList.Count() + 1 });
                    dateMergeTitles.Add($"{account.AccountType}-{account.AccountId}-AverangeAmount", new MergeTitleModel { Name = "Averange Amount", MergeRange = currencyList.Count() + 1 });
                    dateMergeTitles.Add($"{account.AccountType}-{account.AccountId}-SuccessRate", new MergeTitleModel { Name = "Success Rate(%)", MergeRange = currencyList.Count() + 1 });
                    dateMergeTitles.Add($"{account.AccountType}-{account.AccountId}-FTDCount", new MergeTitleModel { Name = "FTD Count", MergeRange = currencyList.Count() + 1 });
                }
            }

            dateTitlesToMerge.Add(accountNameTitles);
            dateTitlesToMerge.Add(dateMergeTitles);

            #endregion

            #region Date Title & Date Total Title

            foreach (var data in ExportData)
            {
                var i = 0;
                dateTitles.Add($"Date", new TitleModel { Name = "AccountName", SortId = 1 });

                foreach (var account in data.SummaryAccountData)
                {
                    dateTitles.Add($"{account.AccountType}-{account.AccountId}-TotalAmount", new TitleModel { Name = "Total Amount in MYR", SortId = i + 200 });
                    dateTitles.Add($"{account.AccountType}-{account.AccountId}-TotalCount", new TitleModel { Name = "Total Count", SortId = i + 400 });
                    dateTitles.Add($"{account.AccountType}-{account.AccountId}-AverangeAmount", new TitleModel { Name = "Averange Amount in MYR", SortId = i + 600 });
                    dateTitles.Add($"{account.AccountType}-{account.AccountId}-SuccessRate", new TitleModel { Name = "Total Success Rate", SortId = i + 800 });
                    dateTitles.Add($"{account.AccountType}-{account.AccountId}-FTDCount", new TitleModel { Name = "Total FTD Count", SortId = i + 1000 });

                    dateTotalTitle.Add($"{account.AccountType}-{account.AccountId}-TotalAmount");
                    dateTotalTitle.Add($"{account.AccountType}-{account.AccountId}-TotalCount");
                    dateTotalTitle.Add($"{account.AccountType}-{account.AccountId}-AverangeAmount");
                    dateTotalTitle.Add($"{account.AccountType}-{account.AccountId}-SuccessRate");
                    dateTotalTitle.Add($"{account.AccountType}-{account.AccountId}-FTDCount");

                    foreach (var curr in currencyList)
                    {
                        var isoCode = currencyData.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();

                        var TotalAmountPropName = $"{account.AccountType}-{account.AccountId}-TotalAmount-{isoCode}";
                        var TotalCountPropName = $"{account.AccountType}-{account.AccountId}-TotalCount-{isoCode}";
                        var AverangeAmountPropName = $"{account.AccountType}-{account.AccountId}-AverangeAmount-{isoCode}";
                        var SuccessRatePropName = $"{account.AccountType}-{account.AccountId}-SuccessRate-{isoCode}";
                        var FTDCountPropName = $"{account.AccountType}-{account.AccountId}-FTDCount-{isoCode}";

                        var totalAmountSortId = i + 100 + curr;
                        var totalCountSortId = i + 300 + curr;
                        var averangeAmountSortId = i + 500 + curr;
                        var successRateSortId = i + 700 + curr;
                        var ftdCountSortId = i + 900 + curr;

                        dateTitles.Add(TotalAmountPropName, new TitleModel { Name = isoCode, SortId = totalAmountSortId });
                        dateTitles.Add(TotalCountPropName, new TitleModel { Name = isoCode, SortId = totalCountSortId });
                        dateTitles.Add(AverangeAmountPropName, new TitleModel { Name = isoCode, SortId = averangeAmountSortId });
                        dateTitles.Add(SuccessRatePropName, new TitleModel { Name = isoCode, SortId = successRateSortId });
                        dateTitles.Add(FTDCountPropName, new TitleModel { Name = isoCode, SortId = ftdCountSortId });
                    }

                    i = i + 1000;
                }
            }

            #endregion

            #endregion

            #region Set Content

            foreach (var item in ExportData)
            {
                //Summary Data Filling
                foreach (var accountItem in item.SummaryAccountData)
                {
                    foreach (var accountCurrencyItem in accountItem.SummaryData)
                    {
                        var OnlineAmountPropName = $"OnlineAmount-{accountCurrencyItem.IsoCode}";
                        var OnlinePrecentPropName = $"OnlinePrecent-{accountCurrencyItem.IsoCode}";
                        var MobileAmountPropName = $"MobileAmount-{accountCurrencyItem.IsoCode}";
                        var MobilePrecentPropName = $"MobilePrecent-{accountCurrencyItem.IsoCode}";
                        var LocalAmountPropName = $"LocalAmount-{accountCurrencyItem.IsoCode}";
                        var LocalPrecentPropName = $"LocalPrecent-{accountCurrencyItem.IsoCode}";

                        var TotalAmountPropName = $"TotalAmount-{accountCurrencyItem.IsoCode}";
                        var TotalCountPropName = $"TotalCount-{accountCurrencyItem.IsoCode}";
                        var AverangeAmountPropName = $"AverangeAmount-{accountCurrencyItem.IsoCode}";
                        var SuccessRatePropName = $"SuccessRate-{accountCurrencyItem.IsoCode}";
                        var FTDCountPropName = $"FTDCount-{accountCurrencyItem.IsoCode}";
                        var ApTimePropName = $"ApTime-{accountCurrencyItem.IsoCode}";

                        var getKey = $"{accountItem.AccountType}-{accountItem.AccountId}";

                        var getItem = summaryContent.FirstOrDefault(x =>
                            x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                        );

                        if (getItem != null)
                        {
                            getItem.Add(OnlineAmountPropName, accountCurrencyItem.OnlineDeposit);
                            getItem.Add(OnlinePrecentPropName, accountCurrencyItem.OnlinePrecent);
                            getItem.Add(MobileAmountPropName, accountCurrencyItem.MobileDeposit);
                            getItem.Add(MobilePrecentPropName, accountCurrencyItem.MobilePrecent);
                            getItem.Add(LocalAmountPropName, accountCurrencyItem.LocalDeposit);
                            getItem.Add(LocalPrecentPropName, accountCurrencyItem.LocalPrecent);

                            getItem.Add(TotalAmountPropName, accountCurrencyItem.TotalAmount);
                            getItem.Add(TotalCountPropName, accountCurrencyItem.TotalCount);
                            getItem.Add(AverangeAmountPropName, accountCurrencyItem.AverangeDeposit);
                            getItem.Add(SuccessRatePropName, accountCurrencyItem.SuccessRate);
                            getItem.Add(FTDCountPropName, accountCurrencyItem.FTDCount);
                            getItem.Add(ApTimePropName, accountCurrencyItem.AverangeProcessingTime);
                        }
                        else
                        {
                            var newItem = new Dictionary<string, dynamic>()
                            {
                                { "Key", $"{accountItem.AccountType}-{accountItem.AccountId}" },
                                { "AccountId", accountItem.AccountId },
                                { "AccountName", accountItem.AccountName },

                                { OnlineAmountPropName, accountCurrencyItem.OnlineDeposit },
                                { OnlinePrecentPropName, accountCurrencyItem.OnlinePrecent },
                                { MobileAmountPropName, accountCurrencyItem.MobileDeposit },
                                { MobilePrecentPropName, accountCurrencyItem.MobilePrecent },
                                { LocalAmountPropName, accountCurrencyItem.LocalDeposit },
                                { LocalPrecentPropName, accountCurrencyItem.LocalPrecent },

                                { TotalAmountPropName, accountCurrencyItem.TotalAmount },
                                { TotalCountPropName, accountCurrencyItem.TotalCount },
                                { AverangeAmountPropName, accountCurrencyItem.AverangeDeposit },
                                { SuccessRatePropName, accountCurrencyItem.SuccessRate },
                                { FTDCountPropName, accountCurrencyItem.FTDCount },
                                { ApTimePropName, accountCurrencyItem.AverangeProcessingTime },

                                { "OnlineAmount-in-MYR", accountItem.SummaryTotalData.OnlineDeposit },
                                { "OnlinePrecent-in-MYR", accountItem.SummaryTotalData.OnlinePrecent },
                                { "MobileAmount-in-MYR", accountItem.SummaryTotalData.MobileDeposit },
                                { "MobilePrecent-in-MYR", accountItem.SummaryTotalData.MobilePrecent },
                                { "LocalAmount-in-MYR", accountItem.SummaryTotalData.LocalDeposit },
                                { "LocalPrecent-in-MYR", accountItem.SummaryTotalData.LocalPrecent },

                                { "TotalAmount", accountItem.SummaryTotalData.TotalAmount },
                                { "TotalCount", accountItem.SummaryTotalData.TotalCount },
                                { "AverangeAmount", accountItem.SummaryTotalData.AverangeDeposit },
                                { "SuccessRate", accountItem.SummaryTotalData.SuccessRate },
                                { "FTDCount", accountItem.SummaryTotalData.FTDCount },
                                { "ApTime", accountItem.SummaryTotalData.AverangeProcessingTime }

                            };

                            summaryContent.Add(newItem);

                        }

                    }
                }

                //Summary Total Data Filling
                foreach (var currencyItem in item.SummaryCurrencyData)
                {
                    var OnlineAmountPropName = $"OnlineAmount-{currencyItem.IsoCode}";
                    var OnlinePrecentPropName = $"OnlinePrecent-{currencyItem.IsoCode}";
                    var MobileAmountPropName = $"MobileAmount-{currencyItem.IsoCode}";
                    var MobilePrecentPropName = $"MobilePrecent-{currencyItem.IsoCode}";
                    var LocalAmountPropName = $"LocalAmount-{currencyItem.IsoCode}";
                    var LocalPrecentPropName = $"LocalPrecent-{currencyItem.IsoCode}";

                    var TotalAmountPropName = $"TotalAmount-{currencyItem.IsoCode}";
                    var TotalCountPropName = $"TotalCount-{currencyItem.IsoCode}";
                    var AverangeAmountPropName = $"AverangeAmount-{currencyItem.IsoCode}";
                    var SuccessRatePropName = $"SuccessRate-{currencyItem.IsoCode}";
                    var FTDCountPropName = $"FTDCount-{currencyItem.IsoCode}";
                    var ApTimePropName = $"ApTime-{currencyItem.IsoCode}";

                    var getKey = "TOTAL";

                    var getItem = summaryContent.FirstOrDefault(x =>
                        x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                    );

                    if (getItem != null)
                    {
                        getItem.Add(OnlineAmountPropName, currencyItem.OnlineDeposit);
                        getItem.Add(OnlinePrecentPropName, currencyItem.OnlinePrecent);
                        getItem.Add(MobileAmountPropName, currencyItem.MobileDeposit);
                        getItem.Add(MobilePrecentPropName, currencyItem.MobilePrecent);
                        getItem.Add(LocalAmountPropName, currencyItem.LocalDeposit);
                        getItem.Add(LocalPrecentPropName, currencyItem.LocalPrecent);

                        getItem.Add(TotalAmountPropName, currencyItem.TotalAmount);
                        getItem.Add(TotalCountPropName, currencyItem.TotalCount);
                        getItem.Add(AverangeAmountPropName, currencyItem.AverangeDeposit);
                        getItem.Add(SuccessRatePropName, currencyItem.SuccessRate);
                        getItem.Add(FTDCountPropName, currencyItem.FTDCount);
                        getItem.Add(ApTimePropName, currencyItem.AverangeProcessingTime);

                    }
                    else
                    {
                        var newItem = new Dictionary<string, dynamic>()
                        {
                            { "Key", "TOTAL" },
                            { "AccountId", "" },
                            { "AccountName", "TOTAL：" },
                            { "CurrencyId", currencyItem.CurrencyId },
                            { "Currency", currencyItem.IsoCode },

                            { OnlineAmountPropName, currencyItem.OnlineDeposit },
                            { OnlinePrecentPropName, currencyItem.OnlinePrecent },
                            { MobileAmountPropName, currencyItem.MobileDeposit },
                            { MobilePrecentPropName, currencyItem.MobilePrecent },
                            { LocalAmountPropName, currencyItem.LocalDeposit },
                            { LocalPrecentPropName, currencyItem.LocalPrecent },

                            { TotalAmountPropName, currencyItem.TotalAmount },
                            { TotalCountPropName, currencyItem.TotalCount },
                            { AverangeAmountPropName, currencyItem.AverangeDeposit },
                            { SuccessRatePropName, currencyItem.SuccessRate },
                            { FTDCountPropName, currencyItem.FTDCount },
                            { ApTimePropName, currencyItem.AverangeProcessingTime },

                            { "OnlineAmount-in-MYR",  item.SummaryTotalData.OnlineDeposit },
                            { "OnlinePrecent-in-MYR", item.SummaryTotalData.OnlinePrecent },
                            { "MobileAmount-in-MYR", item.SummaryTotalData.MobileDeposit },
                            { "MobilePrecent-in-MYR", item.SummaryTotalData.MobilePrecent },
                            { "LocalAmount-in-MYR", item.SummaryTotalData.LocalDeposit },
                            { "LocalPrecent-in-MYR", item.SummaryTotalData.LocalPrecent },
                            { "TotalAmount", item.SummaryTotalData.TotalAmount },
                            { "TotalCount", item.SummaryTotalData.TotalCount },
                            { "AverangeAmount", item.SummaryTotalData.AverangeDeposit },
                            { "SuccessRate", item.SummaryTotalData.SuccessRate },
                            { "FTDCount", item.SummaryTotalData.FTDCount },
                            { "ApTime", item.SummaryTotalData.AverangeProcessingTime }
                        };

                        summaryTotalContent.Add(newItem);
                    }
                }

                //Date Data
                foreach (var accountItem in item.SummaryAccountData)
                {
                    //Date Data Filling
                    foreach (var accountDateItem in accountItem.DateData)
                    {
                        var TotalAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalAmount-{accountDateItem.IsoCode}";
                        var TotalCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalCount-{accountDateItem.IsoCode}";
                        var AverangeAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-AverangeAmount-{accountDateItem.IsoCode}";
                        var SuccessRatePropName = $"{accountItem.AccountType}-{accountItem.AccountId}-SuccessRate-{accountDateItem.IsoCode}";
                        var FTDCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-FTDCount-{accountDateItem.IsoCode}";

                        var getKey = $"{accountDateItem.Date}";

                        var getItem = dateContent.FirstOrDefault(x =>
                            x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                        );

                        if (getItem != null)
                        {
                            getItem.Add(TotalAmountPropName, accountDateItem.TotalAmount);
                            getItem.Add(TotalCountPropName, accountDateItem.TotalCount);
                            getItem.Add(AverangeAmountPropName, accountDateItem.AverangeDeposit);
                            getItem.Add(SuccessRatePropName, accountDateItem.SuccessRate);
                            getItem.Add(FTDCountPropName, accountDateItem.FTDCount);
                        }
                        else
                        {
                            var newItem = new Dictionary<string, dynamic>()
                            {
                                { "Key", $"{accountDateItem.Date}" },
                                { "Date", accountDateItem.Date },
                                { "AccountId", accountItem.AccountId },
                                { "AccountName", accountItem.AccountName },

                                { TotalAmountPropName, accountDateItem.TotalAmount },
                                { TotalCountPropName, accountDateItem.TotalCount },
                                { AverangeAmountPropName, accountDateItem.AverangeDeposit },
                                { SuccessRatePropName, accountDateItem.SuccessRate },
                                { FTDCountPropName, accountDateItem.FTDCount },
                            };

                            dateContent.Add(newItem);

                        }

                    }

                    foreach (var accountDateItem in accountItem.DateTotalData)
                    {
                        var TotalAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalAmount";
                        var TotalCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalCount";
                        var AverangeAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-AverangeAmount";
                        var SuccessRatePropName = $"{accountItem.AccountType}-{accountItem.AccountId}-SuccessRate";
                        var FTDCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-FTDCount";

                        var getKey = $"{accountDateItem.Date}";

                        var getItem = dateContent.FirstOrDefault(x =>
                            x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                        );

                        //if (getItem != null)
                        //{
                        getItem.Add(TotalAmountPropName, accountDateItem.TotalAmount);
                        getItem.Add(TotalCountPropName, accountDateItem.TotalCount);
                        getItem.Add(AverangeAmountPropName, accountDateItem.AverangeDeposit);
                        getItem.Add(SuccessRatePropName, accountDateItem.SuccessRate);
                        getItem.Add(FTDCountPropName, accountDateItem.FTDCount);
                        //}
                        //else
                        //{
                        //    Dictionary<string, dynamic> newItem = new Dictionary<string, dynamic>()
                        //    {
                        //        { "Date", $"{accountDateItem.Date}" },
                        //        { "AccountId", accountItem.AccountId },
                        //        { "AccountName", accountItem.AccountName },

                        //        { TotalAmountPropName, accountDateItem.TotalAmount },
                        //        { TotalCountPropName, accountDateItem.TotalCount },
                        //        { AverangeAmountPropName, accountDateItem.AverangeDeposit },
                        //        { SuccessRatePropName, accountDateItem.SuccessRate },
                        //        { FTDCountPropName, accountDateItem.FTDCount },
                        //    };

                        //    dateContent.Add(newItem);

                        //}

                    }

                    //Date Total Data Filling
                    foreach (var accountCurrencyItem in accountItem.SummaryData)
                    {

                        var TotalAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalAmount-{accountCurrencyItem.IsoCode}";
                        var TotalCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-TotalCount-{accountCurrencyItem.IsoCode}";
                        var AverangeAmountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-AverangeAmount-{accountCurrencyItem.IsoCode}";
                        var SuccessRatePropName = $"{accountItem.AccountType}-{accountItem.AccountId}-SuccessRate-{accountCurrencyItem.IsoCode}";
                        var FTDCountPropName = $"{accountItem.AccountType}-{accountItem.AccountId}-FTDCount-{accountCurrencyItem.IsoCode}";

                        var getKey = $"TOTAL";

                        var getItem = dateTotalContent.FirstOrDefault(x =>
                            x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                        );

                        if (getItem != null)
                        {
                            getItem.Add(TotalAmountPropName, accountCurrencyItem.TotalAmount);
                            getItem.Add(TotalCountPropName, accountCurrencyItem.TotalCount);
                            getItem.Add(AverangeAmountPropName, accountCurrencyItem.AverangeDeposit);
                            getItem.Add(SuccessRatePropName, accountCurrencyItem.SuccessRate);
                            getItem.Add(FTDCountPropName, accountCurrencyItem.FTDCount);
                        }
                        else
                        {
                            var newItem = new Dictionary<string, dynamic>()
                            {
                                { "Key", "TOTAL" },
                                { "Date", "TOTAL：" },

                                { TotalAmountPropName, accountCurrencyItem.TotalAmount },
                                { TotalCountPropName, accountCurrencyItem.TotalCount },
                                { AverangeAmountPropName, accountCurrencyItem.AverangeDeposit },
                                { SuccessRatePropName, accountCurrencyItem.SuccessRate },
                                { FTDCountPropName, accountCurrencyItem.FTDCount },

                            };

                            dateTotalContent.Add(newItem);

                        }

                    }

                    var getTotalKey = $"TOTAL";

                    var getTotalItem = dateTotalContent.FirstOrDefault(x =>
                        x.Any(y => y.Key == "Key" && (string)y.Value == getTotalKey)
                    );

                    getTotalItem.Add($"{accountItem.AccountType}-{accountItem.AccountId}-TotalAmount", accountItem.SummaryTotalData.TotalAmount);
                    getTotalItem.Add($"{accountItem.AccountType}-{accountItem.AccountId}-TotalCount", accountItem.SummaryTotalData.TotalCount);
                    getTotalItem.Add($"{accountItem.AccountType}-{accountItem.AccountId}-AverangeAmount", accountItem.SummaryTotalData.AverangeDeposit);
                    getTotalItem.Add($"{accountItem.AccountType}-{accountItem.AccountId}-SuccessRate", accountItem.SummaryTotalData.SuccessRate);
                    getTotalItem.Add($"{accountItem.AccountType}-{accountItem.AccountId}-FTDCount", accountItem.SummaryTotalData.FTDCount);
                }
            }

            #endregion

            var exelParam = new List<ComplexReportExportDataToExcel>()
            {
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "Summary",
                    Content = summaryContent,
                    TotalContent=summaryTotalContent,
                    Titles = summaryTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = summaryTotalTitle,
                    TitlesToMerge = summaryTitlesToMerge,
                    IsSummary = true
                },
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "ByDate",
                    Content = dateContent,
                    TotalContent=dateTotalContent,
                    Titles = dateTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = dateTotalTitle,
                    TitlesToMerge = dateTitlesToMerge,
                    IsSummary = true
                }
            };

            var stream = _reportExcelBLL.ComplexReportExportToMultiSheet(exelParam);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"DepositReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion

        #region StatisticalReport By Customers

        /// <summary>
        /// Report/GetCustomerStatisticalReport 取得客戶下注紀錄明細
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Customer Statistical Report > [Submit]
        /// 
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/CustomerStatisticalReport" })]
        public ApiResult GetCustomerStatisticalReport(GetCustomerStatisticalReportReq request)
        {

            var data = _reportBLL.GetCustomerStatisticalReport(request.CurrencyIds,
                                                                     JsonConvert.SerializeObject(request.SiteIds),
                                                                     JsonConvert.SerializeObject(request.CurrencyIds),
                                                                     request.StartDate,
                                                                     request.EndDate,
                                                                     request.UserName);

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
        /// Report/ExportCustomerStatisticalReport 匯出客戶統計報表
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Customer Statistical Report > [Export Excel]
        /// 
        /// </remarks>
        /// <param name="SiteIds"></param>
        /// <param name="CurrencyIds"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="UserName"></param>
        /// <returns>檔案</returns>
        [HttpPost]
        public IActionResult ExportCustomerStatisticalReport(string SiteIds,
                                               string CurrencyIds,
                                               string StartDate,
                                               string EndDate,
                                               string UserName)
        {

            #region Get Data

            var currencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(CurrencyIds);

            var ExportData = _reportBLL.GetCustomerStatisticalReport(currencyList,
                                                                     SiteIds,
                                                                     CurrencyIds,
                                                                     StartDate,
                                                                     EndDate,
                                                                     UserName);

            var currencyData = _reportBLL.GetCurrency();

            #endregion

            #region Initial Param

            currencyList = ExportData.Select(x => x.CurrencyId).Distinct();

            var summaryTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var byDateTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var SummaryTitles = new Dictionary<string, TitleModel>();
            var ByDateTitles = new Dictionary<string, TitleModel>();

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

            SummaryTitles.Add("UserName", new TitleModel { Name = "User Name", SortId = 1 });
            SummaryTitles.Add("CategoryName", new TitleModel { Name = "Category Name", SortId = 2 });
            SummaryTitles.Add("GameName", new TitleModel { Name = "Game Name", SortId = 3 });
            SummaryTitles.Add("TurnOverValidBetInMYR", new TitleModel { Name = "Total in MYR", SortId = 200 });
            SummaryTitles.Add("WinLossInMYR", new TitleModel { Name = "Total in MYR", SortId = 400 });

            ByDateTitles.Add("UserName", new TitleModel { Name = "User Name", SortId = 1 });
            ByDateTitles.Add("Date", new TitleModel { Name = "Date", SortId = 2 });
            ByDateTitles.Add("CategoryName", new TitleModel { Name = "Category Name", SortId = 3 });
            ByDateTitles.Add("GameName", new TitleModel { Name = "Game Name", SortId = 4 });
            ByDateTitles.Add("TurnOverValidBetInMYR", new TitleModel { Name = "Total in MYR", SortId = 200 });
            ByDateTitles.Add("WinLossInMYR", new TitleModel { Name = "Total in MYR", SortId = 400 });

            foreach (var curr in currencyList)
            {
                var isoCode = currencyData.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();

                var TurnOverValidBetPropName = $"TurnOverValidBet-{isoCode}";
                var WinLossPropName = $"WinLoss-{isoCode}";

                var turnOverValidBetSortId = 100 + curr;
                var winLossSortId = 300 + curr;

                SummaryTitles.Add(TurnOverValidBetPropName, new TitleModel { Name = isoCode, SortId = turnOverValidBetSortId });
                SummaryTitles.Add(WinLossPropName, new TitleModel { Name = isoCode, SortId = winLossSortId });

                ByDateTitles.Add(TurnOverValidBetPropName, new TitleModel { Name = isoCode, SortId = turnOverValidBetSortId });
                ByDateTitles.Add(WinLossPropName, new TitleModel { Name = isoCode, SortId = winLossSortId });
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

            foreach (var item in ExportData)
            {
                //CurrencyPropName
                var TurnOverValidBetPropName = $"TurnOverValidBet-{item.IsoCode}";
                var WinLossPropName = $"WinLoss-{item.IsoCode}";

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
                    if (getSummaryItem.ContainsKey(TurnOverValidBetPropName))
                    {
                        getSummaryItem.TryGetValue(TurnOverValidBetPropName, out var turnoverdata);
                        getSummaryItem[TurnOverValidBetPropName] = (decimal)turnoverdata + item.TurnOverValidBetSummary;
                    }
                    else
                    {
                        getSummaryItem.Add(TurnOverValidBetPropName, item.TurnOverValidBetSummary);
                    }

                    //WinLoss
                    if (getSummaryItem.ContainsKey(WinLossPropName))
                    {
                        getSummaryItem.TryGetValue(WinLossPropName, out var winlossdata);
                        getSummaryItem[WinLossPropName] = (decimal)winlossdata + item.WinLossSummary;
                    }
                    else
                    {
                        getSummaryItem.Add(WinLossPropName, item.WinLossSummary);
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

                        { TurnOverValidBetPropName,  item.TurnOverValidBetSummary },
                        { WinLossPropName, item.WinLossSummary },

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
                    if (getSummaryCategoryItem.ContainsKey(TurnOverValidBetPropName))
                    {
                        getSummaryCategoryItem.TryGetValue(TurnOverValidBetPropName, out var turnOverValidBet);
                        getSummaryCategoryItem[TurnOverValidBetPropName] = (decimal)turnOverValidBet + item.TurnOverValidBetSummary;
                    }
                    else
                    {
                        getSummaryCategoryItem.Add(TurnOverValidBetPropName, item.TurnOverValidBetSummary);
                    }

                    //WinLoss
                    if (getSummaryCategoryItem.ContainsKey(WinLossPropName))
                    {
                        getSummaryCategoryItem.TryGetValue(WinLossPropName, out var winLoss);
                        getSummaryCategoryItem[WinLossPropName] = (decimal)winLoss + item.WinLossSummary;
                    }
                    else
                    {
                        getSummaryCategoryItem.Add(WinLossPropName, item.WinLossSummary);
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

                        { TurnOverValidBetPropName,  item.TurnOverValidBetSummary },
                        { WinLossPropName, item.WinLossSummary },

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
                    if (getSummaryUserNameItem.ContainsKey(TurnOverValidBetPropName))
                    {
                        getSummaryUserNameItem.TryGetValue(TurnOverValidBetPropName, out var turnOverValidBet);
                        getSummaryUserNameItem[TurnOverValidBetPropName] = (decimal)turnOverValidBet + item.TurnOverValidBetSummary;
                    }
                    else
                    {
                        getSummaryUserNameItem.Add(TurnOverValidBetPropName, item.TurnOverValidBetSummary);
                    }

                    //WinLoss
                    if (getSummaryUserNameItem.ContainsKey(WinLossPropName))
                    {
                        getSummaryUserNameItem.TryGetValue(WinLossPropName, out var winLoss);
                        getSummaryUserNameItem[WinLossPropName] = (decimal)winLoss + item.WinLossSummary;
                    }
                    else
                    {
                        getSummaryUserNameItem.Add(WinLossPropName, item.WinLossSummary);
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

                        { TurnOverValidBetPropName,  item.TurnOverValidBetSummary },
                        { WinLossPropName, item.WinLossSummary },

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
                    if (getUserItem.ContainsKey(TurnOverValidBetPropName))
                    {
                        getUserItem.TryGetValue(TurnOverValidBetPropName, out var turnoverdata);
                        getUserItem[TurnOverValidBetPropName] = (decimal)turnoverdata + item.TurnOverValidBetSummaryInMYR;
                    }
                    else
                    {
                        getUserItem.Add(TurnOverValidBetPropName, item.TurnOverValidBetSummaryInMYR);
                    }

                    //WinLoss
                    if (getUserItem.ContainsKey(WinLossPropName))
                    {
                        getUserItem.TryGetValue(WinLossPropName, out var winlossdata);
                        getUserItem[WinLossPropName] = (decimal)winlossdata + item.WinLossSummaryInMYR;
                    }
                    else
                    {
                        getUserItem.Add(WinLossPropName, item.WinLossSummaryInMYR);
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

                        { TurnOverValidBetPropName,  item.TurnOverValidBetSummary },
                        { WinLossPropName, item.WinLossSummary },

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
                    if (getByDateUserItem.ContainsKey(TurnOverValidBetPropName))
                    {
                        getByDateUserItem.TryGetValue(TurnOverValidBetPropName, out var turnOverValidBet);
                        getByDateUserItem[TurnOverValidBetPropName] = (decimal)turnOverValidBet + item.TurnOverValidBetSummary;
                    }
                    else
                    {
                        getByDateUserItem.Add(TurnOverValidBetPropName, item.TurnOverValidBetSummary);
                    }

                    //WinLoss
                    if (getByDateUserItem.ContainsKey(WinLossPropName))
                    {
                        getByDateUserItem.TryGetValue(WinLossPropName, out var winLoss);
                        getByDateUserItem[WinLossPropName] = (decimal)winLoss + item.WinLossSummary;
                    }
                    else
                    {
                        getByDateUserItem.Add(WinLossPropName, item.WinLossSummary);
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

                        { TurnOverValidBetPropName,  item.TurnOverValidBetSummary },
                        { WinLossPropName, item.WinLossSummary },

                        { "TurnOverValidBetInMYR", item.TurnOverValidBetSummaryInMYR },
                        { "WinLossInMYR", item.WinLossSummaryInMYR }
                    };

                    byDateContent.Add(newItem);
                }

                #endregion
            }

            #region Merge Content Rule

            #region Summary

            var custIdList = ExportData.Select(x => x.CustId).Distinct().OrderBy(x => x);
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
                    Titles = SummaryTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
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
                    Titles = ByDateTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = byDateTitlesToMerge,
                    NotSummaryTitles = notSummaryTitles,
                    MergeContentRules = byDateMergeContentRule,
                    GroupTitles = groupTitle
                }
            };

            var stream = _reportExcelBLL.ComplexReportExportToMultiSheet(d);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"CustomerStatisticalReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        /// <summary>
        /// Report/GetCustomerDetail 取得客戶下注紀錄明細
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Customer Statistical Report > Click [YYYY/MM/DD]
        /// 
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult GetCustomerDetail(GetCustomerDetailReq request)
        {
            var data = _reportBLL.GetCustomerDetail(request.CustId,
                                                          request.GameId,
                                                          request.Date);


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
