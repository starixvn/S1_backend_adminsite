using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class ReportBLL
    {
        private readonly ReportService reportService;
        private readonly SelLangBLL selLangBLL;

        public ReportBLL(ReportService reportService,
                         SelLangBLL selLangBLL)
        {
            this.reportService = reportService;
            this.selLangBLL = selLangBLL;
        }

        public IEnumerable<MainFinancialModel> GetMainFinancialReport(IEnumerable<int> CurrencyIds,
                                                                      string siteIds,
                                                                      string currencyIds,
                                                                      long startDate,
                                                                      long endDate)
        {
            Dictionary<string, MainFinancialModel> keyValuePairs = new Dictionary<string, MainFinancialModel>();
            string dateFormat = string.Empty;

            var reportList = reportService.GetMainFinancialReport(siteIds,
                                                                  currencyIds,
                                                                  startDate,
                                                                  endDate);

            var turnoverAndWinlossList = reportService.GetTurnoverAndWinlossSummary(siteIds,
                                                                  currencyIds,
                                                                  startDate,
                                                                  endDate);

            var currencyList = reportService.GetCurrencyList();

            #region Data Combine

            foreach (var data in reportList["Deposit"])
            {
                MainFinancialModel mainFinancialModel = null;

                dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CurrencyId}"))
                {
                    mainFinancialModel = new MainFinancialModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CurrencyId}", mainFinancialModel);
                    mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                    mainFinancialModel.CurrencyId = data.CurrencyId;
                    mainFinancialModel.ActualRate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();
                    mainFinancialModel.Date = dateFormat;
                }

                mainFinancialModel = keyValuePairs[$"{dateFormat}-{data.CurrencyId}"];

                mainFinancialModel.Deposit = data.Summary;
            }

            foreach (var data in reportList["Withdrawal"])
            {
                MainFinancialModel mainFinancialModel = null;

                dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CurrencyId}"))
                {
                    mainFinancialModel = new MainFinancialModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CurrencyId}", mainFinancialModel);
                    mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                    mainFinancialModel.CurrencyId = data.CurrencyId;
                    mainFinancialModel.ActualRate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();
                    mainFinancialModel.Date = dateFormat;
                }

                mainFinancialModel = keyValuePairs[$"{dateFormat}-{data.CurrencyId}"];

                mainFinancialModel.Withdrawal = data.Summary;

            }

            foreach (var data in reportList["Adjustment"])
            {
                MainFinancialModel mainFinancialModel = null;

                dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CurrencyId}"))
                {
                    mainFinancialModel = new MainFinancialModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CurrencyId}", mainFinancialModel);
                    mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                    mainFinancialModel.CurrencyId = data.CurrencyId;
                    mainFinancialModel.ActualRate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();
                    mainFinancialModel.Date = dateFormat;
                }

                mainFinancialModel = keyValuePairs[$"{dateFormat}-{data.CurrencyId}"];

                mainFinancialModel.Adjustment = data.Summary;
            }

            foreach (var data in reportList["Bonus"])
            {
                MainFinancialModel mainFinancialModel = null;

                dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CurrencyId}"))
                {
                    mainFinancialModel = new MainFinancialModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CurrencyId}", mainFinancialModel);
                    mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                    mainFinancialModel.CurrencyId = data.CurrencyId;
                    mainFinancialModel.ActualRate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();
                    mainFinancialModel.Date = dateFormat;
                }

                mainFinancialModel = keyValuePairs[$"{dateFormat}-{data.CurrencyId}"];

                mainFinancialModel.Bonus = data.Summary;
            }

            foreach (var data in reportList["Rebate"])
            {
                MainFinancialModel mainFinancialModel = null;

                dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CurrencyId}"))
                {
                    mainFinancialModel = new MainFinancialModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CurrencyId}", mainFinancialModel);
                    mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                    mainFinancialModel.CurrencyId = data.CurrencyId;
                    mainFinancialModel.ActualRate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();
                    mainFinancialModel.Date = dateFormat;
                }

                mainFinancialModel = keyValuePairs[$"{dateFormat}-{data.CurrencyId}"];

                mainFinancialModel.Rebate = data.Summary;
            }

            foreach (var data in turnoverAndWinlossList)
            {
                MainFinancialModel mainFinancialModel = null;

                dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CurrencyId}"))
                {
                    mainFinancialModel = new MainFinancialModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CurrencyId}", mainFinancialModel);
                    mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                    mainFinancialModel.CurrencyId = data.CurrencyId;
                    mainFinancialModel.ActualRate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();
                    mainFinancialModel.Date = dateFormat;
                }

                mainFinancialModel = keyValuePairs[$"{dateFormat}-{data.CurrencyId}"];

                mainFinancialModel.TurnOverStake = data.StakeSummary;
                mainFinancialModel.TurnOverValidBet = data.ValidBetSummary;
                mainFinancialModel.WinLoss = data.WinlostSummary;
            }

            #endregion

            #region 補0

            List<string> dateList = keyValuePairs.Select(s => s.Value).Select(x => x.Date).Distinct().ToList();

            foreach (var curr in CurrencyIds)
            {
                foreach (var date in dateList)
                {
                    MainFinancialModel mainFinancialModel = null;

                    if (!keyValuePairs.ContainsKey($"{date}-{curr}"))
                    {
                        mainFinancialModel = new MainFinancialModel();
                        keyValuePairs.Add($"{date}-{curr}", mainFinancialModel);
                        mainFinancialModel.IsoCode = currencyList.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();
                        mainFinancialModel.CurrencyId = curr;
                        mainFinancialModel.ActualRate = currencyList.Where(x => x.CurrencyId == curr).Select(x => x.ActualRate).FirstOrDefault();
                        mainFinancialModel.Date = date;
                    }
                    else
                    {
                        mainFinancialModel = keyValuePairs[$"{date}-{curr}"];

                        mainFinancialModel.Nett = mainFinancialModel.Deposit - mainFinancialModel.Withdrawal;
                    }
                }
            }

            #endregion

            var Data = keyValuePairs.Select(s => s.Value).OrderBy(key => key.Date)
                                                        .ThenBy(key => key.CurrencyId);

            return Data;
        }

    }
}
