using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ReportBLL _reportBLL;
        private readonly ReportExcelBLL _reportExcelBLL;
        public ReportController(IConfiguration configuration,
                                ReportBLL reportBLL,
                                ReportExcelBLL reportExcelBLL)
        {
            this._reportBLL = reportBLL;
            this._reportExcelBLL = reportExcelBLL;
        }

        public class TitleModel
        {
            public string Name { get; set; }

            public int SortId { get; set; }
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/MainFinancialReport" })]
        public ApiResult GetMainFinancialReport([FromForm] IEnumerable<int> SiteIds,
                                                [FromForm] IEnumerable<int> CurrencyIds,
                                                [FromForm] string StartDate,
                                                [FromForm] string EndDate)
        {

            var apiResult = new ApiResult<IEnumerable<MainFinancialModel>>();

            try
            {
                apiResult.Data = _reportBLL.GetMainFinancialReport(CurrencyIds,
                                                                  JsonConvert.SerializeObject(SiteIds),
                                                                  JsonConvert.SerializeObject(CurrencyIds),
                                                                  StartDate,
                                                                  EndDate);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public IActionResult ExportMainFinancialReport(string SiteIds,
                                                       string CurrencyIds,
                                                       string StartDate,
                                                       string EndDate)
        {
            var currencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(CurrencyIds);

            var ExportData = _reportBLL.GetMainFinancialReport(currencyList,
                                                              SiteIds,
                                                              CurrencyIds,
                                                              StartDate,
                                                              EndDate);

            List<Dictionary<string, dynamic>> byDate = new List<Dictionary<string, dynamic>>();
            Dictionary<string, TitleModel> byDateTitles = new Dictionary<string, TitleModel>();
            List<Dictionary<string, int>> titlesToMerge = new List<Dictionary<string, int>>();
            List<string> notSummaryTitles = new List<string> { "Key", "Date", "CurrencyId", "Currency" };
            List<string> totalTitle = new List<string>
            {
                "Deposit in MYR", "Withdrawal in MYR",
                "Nett in MYR", "Bonus in MYR",
                "Rebate in MYR", "Adjustment in MYR",
                "TurnOver(Stake) in MYR", "TurnOver(ValidBet) in MYR",
                "WinLoss in MYR"
            };

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

                var depositSortId = 100 + item.CurrencyId;
                var withdrawalSortId = 300 + item.CurrencyId;
                var nettSortId = 500 + item.CurrencyId;
                var bonusSortId = 700 + item.CurrencyId;
                var rebateSortId = 900 + item.CurrencyId;
                var adjustmentSortId = 1100 + item.CurrencyId;
                var turnOverStakeSortId = 1300 + item.CurrencyId;
                var turnOverValidBetSortId = 1500 + item.CurrencyId;
                var winLossSortId = 1700 + item.CurrencyId;

                var getByDateKey = $"{item.Date}";
                var getByDateItem = byDate.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getByDateKey)
                );

                if (getByDateItem != null)
                {
                    getByDateItem.TryGetValue("Deposit in MYR", out dynamic deposit);
                    getByDateItem["Deposit in MYR"] = (decimal)deposit + item.DepositInMYR;

                    getByDateItem.TryGetValue("Withdrawal in MYR", out dynamic withdrawal);
                    getByDateItem["Withdrawal in MYR"] = (decimal)withdrawal + item.WithdrawalInMYR;

                    getByDateItem.TryGetValue("Nett in MYR", out dynamic nett);
                    getByDateItem["Nett in MYR"] = (decimal)nett + item.NettInMYR;

                    getByDateItem.TryGetValue("Bonus in MYR", out dynamic bonus);
                    getByDateItem["Bonus in MYR"] = (decimal)bonus + item.BonusInMYR;

                    getByDateItem.TryGetValue("Rebate in MYR", out dynamic rebate);
                    getByDateItem["Rebate in MYR"] = (decimal)rebate + item.RebateInMYR;

                    getByDateItem.TryGetValue("Adjustment in MYR", out dynamic adjustment);
                    getByDateItem["Adjustment in MYR"] = (decimal)adjustment + item.AdjustmentInMYR;

                    getByDateItem.TryGetValue("TurnOver(Stake) in MYR", out dynamic turnOverStake);
                    getByDateItem["TurnOver(Stake) in MYR"] = (decimal)turnOverStake + item.TurnOverStakeInMYR;

                    getByDateItem.TryGetValue("TurnOver(ValidBet) in MYR", out dynamic turnOverValidBet);
                    getByDateItem["TurnOver(ValidBet) in MYR"] = (decimal)turnOverValidBet + item.TurnOverValidBetInMYR;

                    getByDateItem.TryGetValue("WinLoss in MYR", out dynamic winLoss);
                    getByDateItem["WinLoss in MYR"] = (decimal)winLoss + item.WinLossInMYR;

                    //Deposit
                    if (!byDateTitles.ContainsKey(DepositPropName))
                    {
                        byDateTitles.Add(DepositPropName, new TitleModel { Name = item.IsoCode, SortId = depositSortId });
                    }
                    if (getByDateItem.ContainsKey(DepositPropName))
                    {
                        getByDateItem.TryGetValue(DepositPropName, out dynamic depositdata);
                        getByDateItem[DepositPropName] = (decimal)depositdata + item.Deposit;
                    }
                    else
                    {
                        getByDateItem.Add(DepositPropName, item.Deposit);
                    }

                    //Withdrawal
                    if (!byDateTitles.ContainsKey(WithdrawalPropName))
                    {
                        byDateTitles.Add(WithdrawalPropName, new TitleModel { Name = item.IsoCode, SortId = withdrawalSortId });
                    }
                    if (getByDateItem.ContainsKey(WithdrawalPropName))
                    {
                        getByDateItem.TryGetValue(WithdrawalPropName, out dynamic withdrawaldata);
                        getByDateItem[WithdrawalPropName] = (decimal)withdrawaldata + item.Withdrawal;
                    }
                    else
                    {
                        getByDateItem.Add(WithdrawalPropName, item.Withdrawal);
                    }

                    //Nett
                    if (!byDateTitles.ContainsKey(NettPropName))
                    {
                        byDateTitles.Add(NettPropName, new TitleModel { Name = item.IsoCode, SortId = nettSortId });
                    }
                    if (getByDateItem.ContainsKey(NettPropName))
                    {
                        getByDateItem.TryGetValue(NettPropName, out dynamic nettdata);
                        getByDateItem[NettPropName] = (decimal)nettdata + item.Nett;
                    }
                    else
                    {
                        getByDateItem.Add(NettPropName, item.Nett);
                    }

                    //Bonus
                    if (!byDateTitles.ContainsKey(BonusPropName))
                    {
                        byDateTitles.Add(BonusPropName, new TitleModel { Name = item.IsoCode, SortId = bonusSortId });
                    }
                    if (getByDateItem.ContainsKey(BonusPropName))
                    {
                        getByDateItem.TryGetValue(BonusPropName, out dynamic bonusdata);
                        getByDateItem[BonusPropName] = (decimal)bonusdata + item.Bonus;
                    }
                    else
                    {
                        getByDateItem.Add(BonusPropName, item.Bonus);
                    }

                    //Rebate
                    if (!byDateTitles.ContainsKey(RebatePropName))
                    {
                        byDateTitles.Add(RebatePropName, new TitleModel { Name = item.IsoCode, SortId = rebateSortId });
                    }
                    if (getByDateItem.ContainsKey(RebatePropName))
                    {
                        getByDateItem.TryGetValue(RebatePropName, out dynamic rebatedata);
                        getByDateItem[RebatePropName] = (decimal)rebatedata + item.Rebate;
                    }
                    else
                    {
                        getByDateItem.Add(RebatePropName, item.Rebate);
                    }

                    //Adjustment
                    if (!byDateTitles.ContainsKey(AdjustmentPropName))
                    {
                        byDateTitles.Add(AdjustmentPropName, new TitleModel { Name = item.IsoCode, SortId = adjustmentSortId });
                    }
                    if (getByDateItem.ContainsKey(AdjustmentPropName))
                    {
                        getByDateItem.TryGetValue(AdjustmentPropName, out dynamic adjustmentdata);
                        getByDateItem[AdjustmentPropName] = (decimal)adjustmentdata + item.Adjustment;
                    }
                    else
                    {
                        getByDateItem.Add(AdjustmentPropName, item.Adjustment);
                    }

                    //TurnOver(Stake)
                    if (!byDateTitles.ContainsKey(TurnOverStakePropName))
                    {
                        byDateTitles.Add(TurnOverStakePropName, new TitleModel { Name = item.IsoCode, SortId = turnOverStakeSortId });
                    }
                    if (getByDateItem.ContainsKey(TurnOverStakePropName))
                    {
                        getByDateItem.TryGetValue(TurnOverStakePropName, out dynamic turnOverStakeData);
                        getByDateItem[TurnOverStakePropName] = (decimal)turnOverStakeData + item.TurnOverStake;
                    }
                    else
                    {
                        getByDateItem.Add(TurnOverStakePropName, item.TurnOverStake);
                    }

                    //TrunOver(VaildBet)
                    if (!byDateTitles.ContainsKey(TurnOverValidBetPropName))
                    {
                        byDateTitles.Add(TurnOverValidBetPropName, new TitleModel { Name = item.IsoCode, SortId = turnOverValidBetSortId });
                    }
                    if (getByDateItem.ContainsKey(TurnOverValidBetPropName))
                    {
                        getByDateItem.TryGetValue(TurnOverValidBetPropName, out dynamic turnOverValidBetData);
                        getByDateItem[TurnOverValidBetPropName] = (decimal)turnOverValidBetData + item.TurnOverValidBet;
                    }
                    else
                    {
                        getByDateItem.Add(TurnOverValidBetPropName, item.TurnOverValidBet);
                    }

                    //Winloss
                    if (!byDateTitles.ContainsKey(WinLossPropName))
                    {
                        byDateTitles.Add(WinLossPropName, new TitleModel { Name = item.IsoCode, SortId = winLossSortId });
                    }
                    if (getByDateItem.ContainsKey(WinLossPropName))
                    {
                        getByDateItem.TryGetValue(WinLossPropName, out dynamic winLossData);
                        getByDateItem[WinLossPropName] = (decimal)winLossData + item.WinLoss;
                    }
                    else
                    {
                        getByDateItem.Add(WinLossPropName, item.WinLoss);
                    }

                }
                else
                {
                    Dictionary<string, dynamic> newItem = new Dictionary<string, dynamic>()
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

                        { "Deposit in MYR", item.DepositInMYR },
                        { "Withdrawal in MYR", item.WithdrawalInMYR },
                        { "Nett in MYR", item.NettInMYR },
                        { "Bonus in MYR", item.BonusInMYR},
                        { "Rebate in MYR", item.RebateInMYR },
                        { "Adjustment in MYR", item.AdjustmentInMYR},
                        { "TurnOver(Stake) in MYR", item.TurnOverStakeInMYR },
                        { "TurnOver(ValidBet) in MYR", item.TurnOverValidBetInMYR },
                        { "WinLoss in MYR", item.WinLossInMYR }
                    };

                    if (!byDateTitles.ContainsKey("Date"))
                    {
                        byDateTitles.Add("Date", new TitleModel { Name = "Date", SortId = 1 });
                        byDateTitles.Add("Deposit in MYR", new TitleModel { Name = "Total in MYR", SortId = 200 });
                        byDateTitles.Add("Withdrawal in MYR", new TitleModel { Name = "Total in MYR", SortId = 400 });
                        byDateTitles.Add("Nett in MYR", new TitleModel { Name = "Total in MYR", SortId = 600 });
                        byDateTitles.Add("Bonus in MYR", new TitleModel { Name = "Total in MYR", SortId = 800 });
                        byDateTitles.Add("Rebate in MYR", new TitleModel { Name = "Total in MYR", SortId = 1000 });
                        byDateTitles.Add("Adjustment in MYR", new TitleModel { Name = "Total in MYR", SortId = 1200 });
                        byDateTitles.Add("TurnOver(Stake) in MYR", new TitleModel { Name = "Total in MYR", SortId = 1400 });
                        byDateTitles.Add("TurnOver(ValidBet) in MYR", new TitleModel { Name = "Total in MYR", SortId = 1600 });
                        byDateTitles.Add("WinLoss in MYR", new TitleModel { Name = "Total in MYR", SortId = 1800 });
                    }

                    if (!byDateTitles.ContainsKey(DepositPropName))
                    {
                        byDateTitles.Add(DepositPropName, new TitleModel { Name = item.IsoCode, SortId = depositSortId });
                    }
                    if (!byDateTitles.ContainsKey(WithdrawalPropName))
                    {
                        byDateTitles.Add(WithdrawalPropName, new TitleModel { Name = item.IsoCode, SortId = withdrawalSortId });
                    }
                    if (!byDateTitles.ContainsKey(NettPropName))
                    {
                        byDateTitles.Add(NettPropName, new TitleModel { Name = item.IsoCode, SortId = nettSortId });
                    }
                    if (!byDateTitles.ContainsKey(BonusPropName))
                    {
                        byDateTitles.Add(BonusPropName, new TitleModel { Name = item.IsoCode, SortId = bonusSortId });
                    }
                    if (!byDateTitles.ContainsKey(RebatePropName))
                    {
                        byDateTitles.Add(RebatePropName, new TitleModel { Name = item.IsoCode, SortId = rebateSortId });
                    }
                    if (!byDateTitles.ContainsKey(AdjustmentPropName))
                    {
                        byDateTitles.Add(AdjustmentPropName, new TitleModel { Name = item.IsoCode, SortId = adjustmentSortId });
                    }
                    if (!byDateTitles.ContainsKey(TurnOverStakePropName))
                    {
                        byDateTitles.Add(TurnOverStakePropName, new TitleModel { Name = item.IsoCode, SortId = turnOverStakeSortId });
                    }
                    if (!byDateTitles.ContainsKey(TurnOverValidBetPropName))
                    {
                        byDateTitles.Add(TurnOverValidBetPropName, new TitleModel { Name = item.IsoCode, SortId = turnOverValidBetSortId });
                    }
                    if (!byDateTitles.ContainsKey(WinLossPropName))
                    {
                        byDateTitles.Add(WinLossPropName, new TitleModel { Name = item.IsoCode, SortId = winLossSortId });
                    }

                    byDate.Add(newItem);
                }
            }

            #region Titles To Merge

            Dictionary<string, int> mergeTitles = new Dictionary<string, int>();
            mergeTitles.Add("", 1);

            mergeTitles.Add("Deposit", currencyList.Count() + 1);
            mergeTitles.Add("Withdrawal", currencyList.Count() + 1);
            mergeTitles.Add("Nett", currencyList.Count() + 1);
            mergeTitles.Add("Bonus", currencyList.Count() + 1);
            mergeTitles.Add("Rebate", currencyList.Count() + 1);
            mergeTitles.Add("Adjustment", currencyList.Count() + 1);
            mergeTitles.Add("TurnOver(Stake)", currencyList.Count() + 1);
            mergeTitles.Add("TurnOver(ValidBet)", currencyList.Count() + 1);
            mergeTitles.Add("WinLoss", currencyList.Count() + 1);

            titlesToMerge.Add(mergeTitles);

            #endregion

            List<ComplexReportExportDataToExcel> d = new List<ComplexReportExportDataToExcel>()
            {
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "ByDate",
                    Content = byDate,
                    Titles = byDateTitles.OrderBy(x=> x.Value.SortId).Select(x=> x.Key).ToList(),
                    TotalTitles = totalTitle,
                    TitlesToMerge = titlesToMerge,
                    TitlesNameToDisplay = byDateTitles.OrderBy(x=> x.Value.SortId).Select(x=> x.Value.Name).ToList(),
                    NotSummaryTitles = notSummaryTitles
                }
            };

            var stream = _reportExcelBLL.ComplexReportExportToMultiSheet(d);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"MainFinancialReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/StatisticalReport" })]
        public ApiResult GetStatisticalReport([FromForm] IEnumerable<int> SiteIds,
                                              [FromForm] string EndDate)
        {

            var apiResult = new ApiResult<IEnumerable<StatisticalReportModel>>();

            try
            {
                #region Date Caculate

                DateTime endDate = Convert.ToDateTime(EndDate);
                string StartDate = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");

                #endregion

                apiResult.Data = _reportBLL.GetStatisticalReport(JsonConvert.SerializeObject(SiteIds),
                                                                StartDate,
                                                                EndDate);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public IActionResult ExportStatisticalReport(string SiteIds,
                                                     string EndDate)
        {

            #region Date Caculate

            DateTime endDate = Convert.ToDateTime(EndDate);
            string StartDate = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");

            #endregion

            var ExportData = _reportBLL.GetStatisticalReport(SiteIds,
                                                            StartDate,
                                                            EndDate);

            List<string> CurrencyList = new List<string>();
            List<Dictionary<string, dynamic>> turnOverContent = new List<Dictionary<string, dynamic>>();
            Dictionary<string, TitleModel> turnOverTitles = new Dictionary<string, TitleModel>();
            List<Dictionary<string, dynamic>> winLossContent = new List<Dictionary<string, dynamic>>();
            Dictionary<string, TitleModel> winLossTitles = new Dictionary<string, TitleModel>();
            List<Dictionary<string, int>> titlesToMerge = new List<Dictionary<string, int>>();
            List<string> notSummaryTitles = new List<string> { "Key", "CurrencyId", "Currency", "CategoryId", "CategoryName", "GameId", "GameName" };
            List<string> totalTitle = new List<string>
            {
                "Today in MYR", "Yesterday in MYR",
                "LastTwoDays in MYR", "ThisMonth in MYR",
                "LastMonth in MYR"
            };

            foreach (var item in ExportData)
            {
                var getCurrItem = CurrencyList.FirstOrDefault(x => x.Contains(item.IsoCode));
                if (getCurrItem == null)
                {
                    CurrencyList.Add(item.IsoCode);
                }

                var TodayPropName = $"Today-{item.IsoCode}";
                var YesterdayPropName = $"Yesterday-{item.IsoCode}";
                var LastTwoDaysPropName = $"LastTwoDays-{item.IsoCode}";
                var ThisMonthPropName = $"ThisMonth-{item.IsoCode}";
                var LastMonthPropName = $"LastMonth-{item.IsoCode}";

                var todaySortId = 100 + item.CurrencyId;
                var yesterdaySortId = 300 + item.CurrencyId;
                var lastTwoDaysSortId = 500 + item.CurrencyId;
                var thisMonthSortId = 700 + item.CurrencyId;
                var lastMonthSortId = 900 + item.CurrencyId;

                var getKey = $"{item.CategoryId}-{item.GameId}";
                var getTurnOverItem = turnOverContent.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                );

                var getWinLossItem = winLossContent.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                );

                if (getTurnOverItem != null)
                {
                    getTurnOverItem.TryGetValue("Today in MYR", out dynamic today);
                    getTurnOverItem["Today in MYR"] = (decimal)today + item.TurnOverValidBetTodaySummaryInMYR;

                    getTurnOverItem.TryGetValue("Yesterday in MYR", out dynamic withdrawal);
                    getTurnOverItem["Yesterday in MYR"] = (decimal)withdrawal + item.TurnOverValidBetYesterdaySummaryInMYR;

                    getTurnOverItem.TryGetValue("LastTwoDays in MYR", out dynamic nett);
                    getTurnOverItem["LastTwoDays in MYR"] = (decimal)nett + item.TurnOverValidBetLastTwoDaysSummaryInMYR;

                    getTurnOverItem.TryGetValue("ThisMonth in MYR", out dynamic bonus);
                    getTurnOverItem["ThisMonth in MYR"] = (decimal)bonus + item.TurnOverValidBetThisMonthSummaryInMYR;

                    getTurnOverItem.TryGetValue("LastMonth in MYR", out dynamic rebate);
                    getTurnOverItem["LastMonth in MYR"] = (decimal)rebate + item.TurnOverValidBetLastMonthSummaryInMYR;


                    //Today
                    if (!turnOverTitles.ContainsKey(TodayPropName))
                    {
                        turnOverTitles.Add(TodayPropName, new TitleModel { Name = item.IsoCode, SortId = todaySortId });
                    }
                    if (getTurnOverItem.ContainsKey(TodayPropName))
                    {
                        getTurnOverItem.TryGetValue(TodayPropName, out dynamic todaydata);
                        getTurnOverItem[TodayPropName] = (decimal)todaydata + item.TurnOverValidBetTodaySummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(TodayPropName, item.TurnOverValidBetTodaySummary);
                    }

                    //Yesterday
                    if (!turnOverTitles.ContainsKey(YesterdayPropName))
                    {
                        turnOverTitles.Add(YesterdayPropName, new TitleModel { Name = item.IsoCode, SortId = yesterdaySortId });
                    }
                    if (getTurnOverItem.ContainsKey(YesterdayPropName))
                    {
                        getTurnOverItem.TryGetValue(YesterdayPropName, out dynamic yesterdaydata);
                        getTurnOverItem[YesterdayPropName] = (decimal)yesterdaydata + item.TurnOverValidBetYesterdaySummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(YesterdayPropName, item.TurnOverValidBetYesterdaySummary);
                    }

                    //LastTwoDays
                    if (!turnOverTitles.ContainsKey(LastTwoDaysPropName))
                    {
                        turnOverTitles.Add(LastTwoDaysPropName, new TitleModel { Name = item.IsoCode, SortId = lastTwoDaysSortId });
                    }
                    if (getTurnOverItem.ContainsKey(LastTwoDaysPropName))
                    {
                        getTurnOverItem.TryGetValue(LastTwoDaysPropName, out dynamic lasttwodaysdata);
                        getTurnOverItem[LastTwoDaysPropName] = (decimal)lasttwodaysdata + item.TurnOverValidBetLastTwoDaysSummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(LastTwoDaysPropName, item.TurnOverValidBetLastTwoDaysSummary);
                    }

                    //ThisMonth
                    if (!turnOverTitles.ContainsKey(ThisMonthPropName))
                    {
                        turnOverTitles.Add(ThisMonthPropName, new TitleModel { Name = item.IsoCode, SortId = thisMonthSortId });
                    }
                    if (getTurnOverItem.ContainsKey(ThisMonthPropName))
                    {
                        getTurnOverItem.TryGetValue(ThisMonthPropName, out dynamic thismonthdata);
                        getTurnOverItem[ThisMonthPropName] = (decimal)thismonthdata + item.TurnOverValidBetThisMonthSummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(ThisMonthPropName, item.TurnOverValidBetThisMonthSummary);
                    }

                    //LastMonth
                    if (!turnOverTitles.ContainsKey(LastMonthPropName))
                    {
                        turnOverTitles.Add(LastMonthPropName, new TitleModel { Name = item.IsoCode, SortId = lastMonthSortId });
                    }
                    if (getTurnOverItem.ContainsKey(LastMonthPropName))
                    {
                        getTurnOverItem.TryGetValue(LastMonthPropName, out dynamic lastmonthdata);
                        getTurnOverItem[LastMonthPropName] = (decimal)lastmonthdata + item.TurnOverValidBetLastMonthSummary;
                    }
                    else
                    {
                        getTurnOverItem.Add(LastMonthPropName, item.TurnOverValidBetLastMonthSummary);
                    }

                }
                else
                {
                    Dictionary<string, dynamic> newItem = new Dictionary<string, dynamic>()
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

                        { "Today in MYR", item.TurnOverValidBetTodaySummaryInMYR },
                        { "Yesterday in MYR", item.TurnOverValidBetYesterdaySummaryInMYR },
                        { "LastTwoDays in MYR", item.TurnOverValidBetLastTwoDaysSummaryInMYR },
                        { "ThisMonth in MYR", item.TurnOverValidBetThisMonthSummaryInMYR},
                        { "LastMonth in MYR", item.TurnOverValidBetLastMonthSummaryInMYR }
                    };

                    if (!turnOverTitles.ContainsKey("CategoryName"))
                    {
                        turnOverTitles.Add("CategoryName", new TitleModel { Name = "CategoryName", SortId = 1 });
                        turnOverTitles.Add("GameName", new TitleModel { Name = "GameName", SortId = 2 });
                        turnOverTitles.Add("Today in MYR", new TitleModel { Name = "Total in MYR", SortId = 200 });
                        turnOverTitles.Add("Yesterday in MYR", new TitleModel { Name = "Total in MYR", SortId = 400 });
                        turnOverTitles.Add("LastTwoDays in MYR", new TitleModel { Name = "Total in MYR", SortId = 600 });
                        turnOverTitles.Add("ThisMonth in MYR", new TitleModel { Name = "Total in MYR", SortId = 800 });
                        turnOverTitles.Add("LastMonth in MYR", new TitleModel { Name = "Total in MYR", SortId = 1000 });
                    }

                    if (!turnOverTitles.ContainsKey(TodayPropName))
                    {
                        turnOverTitles.Add(TodayPropName, new TitleModel { Name = item.IsoCode, SortId = todaySortId });
                    }
                    if (!turnOverTitles.ContainsKey(YesterdayPropName))
                    {
                        turnOverTitles.Add(YesterdayPropName, new TitleModel { Name = item.IsoCode, SortId = yesterdaySortId });
                    }
                    if (!turnOverTitles.ContainsKey(LastTwoDaysPropName))
                    {
                        turnOverTitles.Add(LastTwoDaysPropName, new TitleModel { Name = item.IsoCode, SortId = lastTwoDaysSortId });
                    }
                    if (!turnOverTitles.ContainsKey(ThisMonthPropName))
                    {
                        turnOverTitles.Add(ThisMonthPropName, new TitleModel { Name = item.IsoCode, SortId = thisMonthSortId });
                    }
                    if (!turnOverTitles.ContainsKey(LastMonthPropName))
                    {
                        turnOverTitles.Add(LastMonthPropName, new TitleModel { Name = item.IsoCode, SortId = lastMonthSortId });
                    }

                    turnOverContent.Add(newItem);
                }

                if (getWinLossItem != null)
                {
                    getWinLossItem.TryGetValue("Today in MYR", out dynamic today);
                    getWinLossItem["Today in MYR"] = (decimal)today + item.WinLossTodaySummaryInMYR;

                    getWinLossItem.TryGetValue("Yesterday in MYR", out dynamic yesterday);
                    getWinLossItem["Yesterday in MYR"] = (decimal)yesterday + item.TurnOverValidBetYesterdaySummaryInMYR;

                    getWinLossItem.TryGetValue("LastTwoDays in MYR", out dynamic lasttwodays);
                    getWinLossItem["LastTwoDays in MYR"] = (decimal)lasttwodays + item.WinLossLastTwoDaysSummaryInMYR;

                    getWinLossItem.TryGetValue("ThisMonth in MYR", out dynamic thismonth);
                    getWinLossItem["ThisMonth in MYR"] = (decimal)thismonth + item.WinLossThisMonthSummaryInMYR;

                    getWinLossItem.TryGetValue("LastMonth in MYR", out dynamic lastmonth);
                    getWinLossItem["LastMonth in MYR"] = (decimal)lastmonth + item.WinLossLastMonthSummaryInMYR;


                    //Today
                    if (!turnOverTitles.ContainsKey(TodayPropName))
                    {
                        turnOverTitles.Add(TodayPropName, new TitleModel { Name = item.IsoCode, SortId = todaySortId });
                    }
                    if (getWinLossItem.ContainsKey(TodayPropName))
                    {
                        getWinLossItem.TryGetValue(TodayPropName, out dynamic todaydata);
                        getWinLossItem[TodayPropName] = (decimal)todaydata + item.WinLossTodaySummary;
                    }
                    else
                    {
                        getWinLossItem.Add(TodayPropName, item.WinLossTodaySummary);
                    }

                    //Yesterday
                    if (!turnOverTitles.ContainsKey(YesterdayPropName))
                    {
                        turnOverTitles.Add(YesterdayPropName, new TitleModel { Name = item.IsoCode, SortId = yesterdaySortId });
                    }
                    if (getWinLossItem.ContainsKey(YesterdayPropName))
                    {
                        getWinLossItem.TryGetValue(YesterdayPropName, out dynamic yesterdaydata);
                        getWinLossItem[YesterdayPropName] = (decimal)yesterdaydata + item.WinLossYesterdaySummary;
                    }
                    else
                    {
                        getWinLossItem.Add(YesterdayPropName, item.WinLossYesterdaySummary);
                    }

                    //LastTwoDays
                    if (!turnOverTitles.ContainsKey(LastTwoDaysPropName))
                    {
                        turnOverTitles.Add(LastTwoDaysPropName, new TitleModel { Name = item.IsoCode, SortId = lastTwoDaysSortId });
                    }
                    if (getWinLossItem.ContainsKey(LastTwoDaysPropName))
                    {
                        getWinLossItem.TryGetValue(LastTwoDaysPropName, out dynamic lasttwodaysdata);
                        getWinLossItem[LastTwoDaysPropName] = (decimal)lasttwodaysdata + item.WinLossLastTwoDaysSummary;
                    }
                    else
                    {
                        getWinLossItem.Add(LastTwoDaysPropName, item.WinLossLastTwoDaysSummary);
                    }

                    //ThisMonth
                    if (!turnOverTitles.ContainsKey(ThisMonthPropName))
                    {
                        turnOverTitles.Add(ThisMonthPropName, new TitleModel { Name = item.IsoCode, SortId = thisMonthSortId });
                    }
                    if (getWinLossItem.ContainsKey(ThisMonthPropName))
                    {
                        getWinLossItem.TryGetValue(ThisMonthPropName, out dynamic thismonthdata);
                        getWinLossItem[ThisMonthPropName] = (decimal)thismonthdata + item.WinLossThisMonthSummary;
                    }
                    else
                    {
                        getWinLossItem.Add(ThisMonthPropName, item.WinLossThisMonthSummary);
                    }

                    //LastMonth
                    if (!turnOverTitles.ContainsKey(LastMonthPropName))
                    {
                        turnOverTitles.Add(LastMonthPropName, new TitleModel { Name = item.IsoCode, SortId = lastMonthSortId });
                    }
                    if (getWinLossItem.ContainsKey(LastMonthPropName))
                    {
                        getWinLossItem.TryGetValue(LastMonthPropName, out dynamic lastmonthdata);
                        getWinLossItem[LastMonthPropName] = (decimal)lastmonthdata + item.WinLossLastMonthSummary;
                    }
                    else
                    {
                        getWinLossItem.Add(LastMonthPropName, item.WinLossLastMonthSummary);
                    }

                }
                else
                {
                    Dictionary<string, dynamic> newItem = new Dictionary<string, dynamic>()
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

                        { "Today in MYR", item.WinLossTodaySummaryInMYR },
                        { "Yesterday in MYR", item.WinLossYesterdaySummaryInMYR },
                        { "LastTwoDays in MYR", item.WinLossLastTwoDaysSummaryInMYR },
                        { "ThisMonth in MYR", item.WinLossThisMonthSummaryInMYR},
                        { "LastMonth in MYR", item.WinLossLastMonthSummaryInMYR }
                    };

                    if (!turnOverTitles.ContainsKey("CategoryName"))
                    {
                        turnOverTitles.Add("CategoryName", new TitleModel { Name = "CategoryName", SortId = 1 });
                        turnOverTitles.Add("GameName", new TitleModel { Name = "GameName", SortId = 2 });
                        turnOverTitles.Add("Today in MYR", new TitleModel { Name = "Total in MYR", SortId = 200 });
                        turnOverTitles.Add("Yesterday in MYR", new TitleModel { Name = "Total in MYR", SortId = 400 });
                        turnOverTitles.Add("LastTwoDays in MYR", new TitleModel { Name = "Total in MYR", SortId = 600 });
                        turnOverTitles.Add("ThisMonth in MYR", new TitleModel { Name = "Total in MYR", SortId = 800 });
                        turnOverTitles.Add("LastMonth in MYR", new TitleModel { Name = "Total in MYR", SortId = 1000 });
                    }

                    if (!turnOverTitles.ContainsKey(TodayPropName))
                    {
                        turnOverTitles.Add(TodayPropName, new TitleModel { Name = item.IsoCode, SortId = todaySortId });
                    }
                    if (!turnOverTitles.ContainsKey(YesterdayPropName))
                    {
                        turnOverTitles.Add(YesterdayPropName, new TitleModel { Name = item.IsoCode, SortId = yesterdaySortId });
                    }
                    if (!turnOverTitles.ContainsKey(LastTwoDaysPropName))
                    {
                        turnOverTitles.Add(LastTwoDaysPropName, new TitleModel { Name = item.IsoCode, SortId = lastTwoDaysSortId });
                    }
                    if (!turnOverTitles.ContainsKey(ThisMonthPropName))
                    {
                        turnOverTitles.Add(ThisMonthPropName, new TitleModel { Name = item.IsoCode, SortId = thisMonthSortId });
                    }
                    if (!turnOverTitles.ContainsKey(LastMonthPropName))
                    {
                        turnOverTitles.Add(LastMonthPropName, new TitleModel { Name = item.IsoCode, SortId = lastMonthSortId });
                    }

                    winLossContent.Add(newItem);
                }
            }

            #region Titles To Merge
            Dictionary<string, int> mergeTitles = new Dictionary<string, int>();
            mergeTitles.Add("", 2);

            mergeTitles.Add("Today", CurrencyList.Count() + 1);
            mergeTitles.Add("Yesterday", CurrencyList.Count() + 1);
            mergeTitles.Add("Last 2 Days", CurrencyList.Count() + 1);
            mergeTitles.Add("This Month", CurrencyList.Count() + 1);
            mergeTitles.Add("Last Month", CurrencyList.Count() + 1);

            titlesToMerge.Add(mergeTitles);

            #endregion

            List<ComplexReportExportDataToExcel> d = new List<ComplexReportExportDataToExcel>()
            {
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "TurnOver(ValidBet)",
                    Content = turnOverContent,
                    Titles = turnOverTitles.OrderBy(x=> x.Value.SortId).Select(x=> x.Key).ToList(),
                    TotalTitles = totalTitle,
                    TitlesToMerge = titlesToMerge,
                    TitlesNameToDisplay = turnOverTitles.OrderBy(x=> x.Value.SortId).Select(x=> x.Value.Name).ToList(),
                    NotSummaryTitles = notSummaryTitles
                },
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "WinLoss",
                    Content = winLossContent,
                    Titles = turnOverTitles.OrderBy(x=> x.Value.SortId).Select(x=> x.Key).ToList(),
                    TotalTitles = totalTitle,
                    TitlesToMerge = titlesToMerge,
                    TitlesNameToDisplay = turnOverTitles.OrderBy(x=> x.Value.SortId).Select(x=> x.Value.Name).ToList(),
                    NotSummaryTitles = notSummaryTitles
                }
            };

            var stream = _reportExcelBLL.ComplexReportExportToMultiSheet(d);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"StatisticalReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/UserOnlineReport" })]
        public ApiResult GetUserOnlineReport([FromForm] IEnumerable<int> SiteIds,
                                             [FromForm] string SelectDate)
        {

            var apiResult = new ApiResult<IEnumerable<UserOnlineReportModel>>();

            try
            {

                apiResult.Data = _reportBLL.GetUserOnlineReport(SiteIds,
                                                               JsonConvert.SerializeObject(SiteIds),
                                                               SelectDate);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public IActionResult ExportUserOnlineReport(string SiteIds,
                                                    string SelectDate)
        {
            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(SiteIds);

            var ExportData = _reportBLL.GetUserOnlineReport(siteList,
                                                           SiteIds,
                                                           SelectDate);

            int totalMaxCount = ExportData.GroupBy(x => new {
                LocalTime = x.LocalTime
            }).Select(x => x.Sum(y => y.MaxCount)).Max();

            List<string> CurrencyList = new List<string>();
            List<Dictionary<string, dynamic>> Content = new List<Dictionary<string, dynamic>>();
            Dictionary<string, TitleModel> Titles = new Dictionary<string, TitleModel>();
            List<Dictionary<string, int>> titlesToMerge = new List<Dictionary<string, int>>();
            List<string> notSummaryTitles = new List<string> { "Key", "LocalTime", "TotalOnline" };
            List<string> totalTitle = new List<string>
            {
                "TotalOnline",
            };

            foreach (var item in ExportData)
            {
                //var getCurrItem = siteList.FirstOrDefault(Contains(item.SiteId));
                //if (getCurrItem == null)
                //{
                //    siteList.Add(item.SiteId);
                //}

                var OnlinePropName = $"Online-{item.SiteName}";

                var onlineSortId = 100 + item.SiteId;

                var getKey = $"{item.LocalTime}";
                var getItem = Content.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                );

                if (getItem != null)
                {
                    getItem.TryGetValue("TotalOnline", out dynamic totalCountdata);
                    getItem["TotalOnline"] = (int)totalCountdata + item.MaxCount;

                    if (!Titles.ContainsKey(OnlinePropName))
                    {
                        Titles.Add(OnlinePropName, new TitleModel { Name = item.SiteName, SortId = onlineSortId });
                    }
                    if (getItem.ContainsKey(OnlinePropName))
                    {
                        getItem.TryGetValue(OnlinePropName, out dynamic maxCountData);
                        getItem[OnlinePropName] = (int)maxCountData + item.MaxCount;
                    }
                    else
                    {
                        getItem.Add(OnlinePropName, item.MaxCount);
                    }

                    #region

                    if (getItem["TotalOnline"] == totalMaxCount)
                    {
                        getItem["MaxCount"] = true;
                    }

                    #endregion

                }
                else
                {
                    Dictionary<string, dynamic> newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getKey },
                        { "LocalTime", item.LocalTime },

                        { OnlinePropName,  item.MaxCount },
                        { "TotalOnline", item.MaxCount }
                    };

                    if (!Titles.ContainsKey("LocalTime"))
                    {
                        Titles.Add("LocalTime", new TitleModel { Name = "Local Time(GMT +8)", SortId = 1 });
                        Titles.Add("TotalOnline", new TitleModel { Name = "Total Online", SortId = 200 });

                    }

                    if (!Titles.ContainsKey(OnlinePropName))
                    {
                        Titles.Add(OnlinePropName, new TitleModel { Name = item.SiteName, SortId = onlineSortId });
                    }

                    Content.Add(newItem);
                }
            }

            #region Titles To Merge

            Dictionary<string, int> MergeTitles = new Dictionary<string, int>();

            MergeTitles.Add(SelectDate, 1);
            MergeTitles.Add("Online Count", siteList.Count() + 1);

            titlesToMerge.Add(MergeTitles);

            #endregion

            List<ComplexReportExportDataToExcel> d = new List<ComplexReportExportDataToExcel>()
            {
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "User Online Report",
                    Content = Content,
                    Titles = Titles.OrderBy(x=> x.Value.SortId).Select(x=> x.Key).ToList(),
                    TotalTitles = totalTitle,
                    TitlesToMerge = titlesToMerge,
                    TitlesNameToDisplay = Titles.OrderBy(x=> x.Value.SortId).Select(x=> x.Value.Name).ToList(),
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
    }
}
