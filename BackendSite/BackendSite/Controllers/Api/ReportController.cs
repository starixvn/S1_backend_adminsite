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
        private readonly ReportBLL reportBLL;
        private readonly ExcelBLL excelBLL;
        public ReportController(IConfiguration configuration,
                                ReportBLL reportBLL,
                                ExcelBLL excelBLL)
        {
            this.reportBLL = reportBLL;
            this.excelBLL = excelBLL;
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
                apiResult.Data = reportBLL.GetMainFinancialReport(CurrencyIds,
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

            var ExportData = reportBLL.GetMainFinancialReport(currencyList,
                                                              SiteIds,
                                                              CurrencyIds,
                                                              StartDate,
                                                              EndDate);

            List<Dictionary<string, dynamic>> byDate = new List<Dictionary<string, dynamic>>();
            Dictionary<string, TitleModel> byDateTitles = new Dictionary<string, TitleModel>();
            Dictionary<string, int> titlesToMerge = new Dictionary<string, int>();
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

            titlesToMerge.Add("", 1);

            titlesToMerge.Add("Deposit", currencyList.Count() + 1);
            titlesToMerge.Add("Withdrawal", currencyList.Count() + 1);
            titlesToMerge.Add("Nett", currencyList.Count() + 1);
            titlesToMerge.Add("Bonus", currencyList.Count() + 1);
            titlesToMerge.Add("Rebate", currencyList.Count() + 1);
            titlesToMerge.Add("Adjustment", currencyList.Count() + 1);
            titlesToMerge.Add("TurnOver(Stake)", currencyList.Count() + 1);
            titlesToMerge.Add("TurnOver(ValidBet)", currencyList.Count() + 1);
            titlesToMerge.Add("WinLoss", currencyList.Count() + 1);

            #endregion

            List<ExportReportDataToExcel> d = new List<ExportReportDataToExcel>()
            {
                new ExportReportDataToExcel()
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

            var stream = excelBLL.ExportToMultiSheetForReport(d);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"MainFinancialReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

    }
}
