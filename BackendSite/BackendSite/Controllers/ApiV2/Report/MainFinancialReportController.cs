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
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MainFinancialReportController : ControllerBase
    {
        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly ReportExcelBLL _reportExcelBll;
        private readonly Common _common;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public MainFinancialReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
            _common = provider.GetRequiredService<Common>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }

            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }

        public class TitleModel
        {
            public string Name { get; set; }
            public int SortId { get; set; }
        }

        #endregion

        /// <summary>
        /// MainFinancialReport/GetList 取得金融報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports >  Main Financial Report > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/MainFinancialReport" })]
        public ApiResult<IEnumerable<MainFinancialModel>> GetList(GetListRequest request)
        {

            if (request.TimeZone > 14 || request.TimeZone < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            var data = _reportBll.GetMainFinancialReport(
                JsonConvert.SerializeObject(request.SiteIds),
                JsonConvert.SerializeObject(request.CurrencyIds),
                request.StartDate,
                request.EndDate,
                _common.TransferToTimeZoneFormat(request.TimeZone));

            if (!data.Any())
            {
                return new ApiResult<IEnumerable<MainFinancialModel>>
                {
                    ErrorCode = (int)ErrorCodeEnum.NoData
                };
            }

            return new ApiResult<IEnumerable<MainFinancialModel>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

        }

        /// <summary>
        /// MainFinancialReport/Export 匯出金融報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Main Financial Report > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>檔案</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/MainFinancialReport" })]
        public IActionResult Export(GetListRequest request)
        {
            #region Get Data

            var currencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(JsonConvert.SerializeObject(request.CurrencyIds));
            var exportData = _reportBll.GetMainFinancialReport(
                JsonConvert.SerializeObject(request.SiteIds),
                JsonConvert.SerializeObject(request.CurrencyIds),
                request.StartDate,
                request.EndDate,
                _common.TransferToTimeZoneFormat(request.TimeZone));
            var currencyData = _reportBll.GetCurrency();

            #endregion

            #region Initial Param

            var byDate = new List<Dictionary<string, dynamic>>();
            var titlesToMerge = new List<Dictionary<string, MergeTitleModel>>();

            #endregion

            #region Set Title(Title & Total Title & Merge Title & Not Summary Title)

            #region Merge Title

            var mergeTitles = new Dictionary<string, MergeTitleModel>
            {
                {"FirstMergeTitle", new MergeTitleModel {Name = "", MergeRange = 1}},
                {"Deposit", new MergeTitleModel {Name = "Deposit", MergeRange = currencyList.Count() + 1}},
                {"Withdrawal", new MergeTitleModel { Name = "Withdrawal", MergeRange = currencyList.Count() + 1 }},
                {"Nett", new MergeTitleModel { Name = "Nett", MergeRange = currencyList.Count() + 1 }},
                {"Bonus", new MergeTitleModel { Name = "Bonus", MergeRange = currencyList.Count() + 1 }},
                {"Rebate", new MergeTitleModel { Name = "Rebate", MergeRange = currencyList.Count() + 1 }},
                {"Adjustment", new MergeTitleModel { Name = "Adjustment", MergeRange = currencyList.Count() + 1 }},
                {"TurnOverStake", new MergeTitleModel { Name = "TurnOver(Stake)", MergeRange = currencyList.Count() + 1 }},
                {"TurnOverValidBet", new MergeTitleModel { Name = "TurnOver(ValidBet)", MergeRange = currencyList.Count() + 1 }},
                {"WinLoss", new MergeTitleModel { Name = "WinLoss", MergeRange = currencyList.Count() + 1 }}
            };

            titlesToMerge.Add(mergeTitles);

            #endregion

            #region Title

            var byDateTitles = new Dictionary<string, TitleModel>
            {
                {"Date", new TitleModel {Name = "Date", SortId = 1}},
                {"DepositInMYR", new TitleModel {Name = "Total in MYR", SortId = 200}},
                {"WithdrawalInMYR", new TitleModel { Name = "Total in MYR", SortId = 400 }},
                {"NettInMYR", new TitleModel { Name = "Total in MYR", SortId = 600 }},
                {"BonusInMYR", new TitleModel { Name = "Total in MYR", SortId = 800 }},
                {"RebateInMYR", new TitleModel { Name = "Total in MYR", SortId = 1000 }},
                {"AdjustmentInMYR", new TitleModel { Name = "Total in MYR", SortId = 1200 }},
                {"TurnOverStakeInMYR", new TitleModel { Name = "Total in MYR", SortId = 1400 }},
                {"TurnOverValidBetInMYR", new TitleModel { Name = "Total in MYR", SortId = 1600 }},
                {"WinLossInMYR", new TitleModel { Name = "Total in MYR", SortId = 1800 }}
            };

            foreach (var curr in currencyList)
            {
                var isoCode = currencyData.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();

                var depositPropName = $"Deposit-{isoCode}";
                var withdrawalPropName = $"Withdrawal-{isoCode}";
                var nettPropName = $"Nett-{isoCode}";
                var bonusPropName = $"Bonus-{isoCode}";
                var rebatePropName = $"Rebate-{isoCode}";
                var adjustmentPropName = $"Adjustment-{isoCode}";
                var turnOverStakePropName = $"TurnOverStake-{isoCode}";
                var turnOverValidBetPropName = $"TurnOverValidBet-{isoCode}";
                var winLossPropName = $"WinLoss-{isoCode}";

                var depositSortId = 100 + curr;
                var withdrawalSortId = 300 + curr;
                var nettSortId = 500 + curr;
                var bonusSortId = 700 + curr;
                var rebateSortId = 900 + curr;
                var adjustmentSortId = 1100 + curr;
                var turnOverStakeSortId = 1300 + curr;
                var turnOverValidBetSortId = 1500 + curr;
                var winLossSortId = 1700 + curr;

                byDateTitles.Add(depositPropName, new TitleModel { Name = isoCode, SortId = depositSortId });
                byDateTitles.Add(withdrawalPropName, new TitleModel { Name = isoCode, SortId = withdrawalSortId });
                byDateTitles.Add(nettPropName, new TitleModel { Name = isoCode, SortId = nettSortId });
                byDateTitles.Add(bonusPropName, new TitleModel { Name = isoCode, SortId = bonusSortId });
                byDateTitles.Add(rebatePropName, new TitleModel { Name = isoCode, SortId = rebateSortId });
                byDateTitles.Add(adjustmentPropName, new TitleModel { Name = isoCode, SortId = adjustmentSortId });
                byDateTitles.Add(turnOverStakePropName, new TitleModel { Name = isoCode, SortId = turnOverStakeSortId });
                byDateTitles.Add(turnOverValidBetPropName, new TitleModel { Name = isoCode, SortId = turnOverValidBetSortId });
                byDateTitles.Add(winLossPropName, new TitleModel { Name = isoCode, SortId = winLossSortId });

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

            foreach (var item in exportData)
            {
                var depositPropName = $"Deposit-{item.IsoCode}";
                var withdrawalPropName = $"Withdrawal-{item.IsoCode}";
                var nettPropName = $"Nett-{item.IsoCode}";
                var bonusPropName = $"Bonus-{item.IsoCode}";
                var rebatePropName = $"Rebate-{item.IsoCode}";
                var adjustmentPropName = $"Adjustment-{item.IsoCode}";
                var turnOverStakePropName = $"TurnOverStake-{item.IsoCode}";
                var turnOverValidBetPropName = $"TurnOverValidBet-{item.IsoCode}";
                var winLossPropName = $"WinLoss-{item.IsoCode}";

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
                    if (getByDateItem.ContainsKey(depositPropName))
                    {
                        getByDateItem.TryGetValue(depositPropName, out var depositData);
                        getByDateItem[depositPropName] = (decimal)depositData + item.Deposit;
                    }
                    else
                    {
                        getByDateItem.Add(depositPropName, item.Deposit);
                    }

                    //Withdrawal
                    if (getByDateItem.ContainsKey(withdrawalPropName))
                    {
                        getByDateItem.TryGetValue(withdrawalPropName, out var withdrawalData);
                        getByDateItem[withdrawalPropName] = (decimal)withdrawalData + item.Withdrawal;
                    }
                    else
                    {
                        getByDateItem.Add(withdrawalPropName, item.Withdrawal);
                    }

                    //Nett
                    if (getByDateItem.ContainsKey(nettPropName))
                    {
                        getByDateItem.TryGetValue(nettPropName, out var nettData);
                        getByDateItem[nettPropName] = (decimal)nettData + item.Nett;
                    }
                    else
                    {
                        getByDateItem.Add(nettPropName, item.Nett);
                    }

                    //Bonus
                    if (getByDateItem.ContainsKey(bonusPropName))
                    {
                        getByDateItem.TryGetValue(bonusPropName, out var bonusData);
                        getByDateItem[bonusPropName] = (decimal)bonusData + item.Bonus;
                    }
                    else
                    {
                        getByDateItem.Add(bonusPropName, item.Bonus);
                    }

                    //Rebate

                    if (getByDateItem.ContainsKey(rebatePropName))
                    {
                        getByDateItem.TryGetValue(rebatePropName, out var rebateData);
                        getByDateItem[rebatePropName] = (decimal)rebateData + item.Rebate;
                    }
                    else
                    {
                        getByDateItem.Add(rebatePropName, item.Rebate);
                    }

                    //Adjustment
                    if (getByDateItem.ContainsKey(adjustmentPropName))
                    {
                        getByDateItem.TryGetValue(adjustmentPropName, out var adjustmentData);
                        getByDateItem[adjustmentPropName] = (decimal)adjustmentData + item.Adjustment;
                    }
                    else
                    {
                        getByDateItem.Add(adjustmentPropName, item.Adjustment);
                    }

                    //TurnOver(Stake)
                    if (getByDateItem.ContainsKey(turnOverStakePropName))
                    {
                        getByDateItem.TryGetValue(turnOverStakePropName, out var turnOverStakeData);
                        getByDateItem[turnOverStakePropName] = (decimal)turnOverStakeData + item.TurnOverStake;
                    }
                    else
                    {
                        getByDateItem.Add(turnOverStakePropName, item.TurnOverStake);
                    }

                    //TurnOver(ValidBet)
                    if (getByDateItem.ContainsKey(turnOverValidBetPropName))
                    {
                        getByDateItem.TryGetValue(turnOverValidBetPropName, out var turnOverValidBetData);
                        getByDateItem[turnOverValidBetPropName] = (decimal)turnOverValidBetData + item.TurnOverValidBet;
                    }
                    else
                    {
                        getByDateItem.Add(turnOverValidBetPropName, item.TurnOverValidBet);
                    }

                    //WinLoss
                    if (getByDateItem.ContainsKey(winLossPropName))
                    {
                        getByDateItem.TryGetValue(winLossPropName, out var winLossData);
                        getByDateItem[winLossPropName] = (decimal)winLossData + item.WinLoss;
                    }
                    else
                    {
                        getByDateItem.Add(winLossPropName, item.WinLoss);
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

                        { depositPropName,  item.Deposit },
                        { withdrawalPropName, item.Withdrawal },
                        { nettPropName, item.Nett },
                        { bonusPropName, item.Bonus },
                        { rebatePropName, item.Rebate },
                        { adjustmentPropName, item.Adjustment },
                        { turnOverStakePropName, item.TurnOverStake },
                        { turnOverValidBetPropName, item.TurnOverValidBet },
                        { winLossPropName, item.WinLoss },

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

            var stream = _reportExcelBll.ComplexReportExportToMultiSheet(d);

            return _excelBll.ExcelFile(stream, "MainFinancialReport");
        }
    }
}