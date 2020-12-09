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
                                                                      string startDate,
                                                                      string endDate)
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
                mainFinancialModel.DepositInMYR = Math.Round(data.Summary * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);
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
                mainFinancialModel.WithdrawalInMYR = Math.Round(data.Summary * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);

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
                mainFinancialModel.AdjustmentInMYR = Math.Round(data.Summary * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);

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
                mainFinancialModel.BonusInMYR = Math.Round(data.Summary * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);
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
                mainFinancialModel.RebateInMYR = Math.Round(data.Summary * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);
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

                mainFinancialModel.TurnOverStakeInMYR = Math.Round(data.StakeSummary * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                mainFinancialModel.TurnOverValidBetInMYR = Math.Round(data.ValidBetSummary * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                mainFinancialModel.WinLossInMYR = Math.Round(data.WinlostSummary * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);
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
                        mainFinancialModel.NettInMYR = Math.Round(mainFinancialModel.Nett * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                    }
                }
            }

            #endregion

            var Data = keyValuePairs.Select(s => s.Value).OrderBy(key => key.Date)
                                                        .ThenBy(key => key.CurrencyId);

            return Data;
        }

        public IEnumerable<StatisticalReportModel> GetStatisticalReport(string siteIds,
                                                                        string startDate,
                                                                        string endDate)
        {
            Dictionary<string, StatisticalReportModel> keyValuePairs = new Dictionary<string, StatisticalReportModel>();
            string dateFormat = string.Empty;
            var statisticalList = reportService.GetStatisticalReport(siteIds,
                                                                     startDate,
                                                                     endDate);

            var currencyList = reportService.GetCurrencyList();

            #region Data Statistical

            foreach (var data in statisticalList)
            {
                StatisticalReportModel statisticalReportModel = null;

                //dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{data.CategoryId}-{data.GameId}-{data.CurrencyId}"))
                {
                    statisticalReportModel = new StatisticalReportModel();
                    keyValuePairs.Add($"{data.CategoryId}-{data.GameId}-{data.CurrencyId}", statisticalReportModel);
                    statisticalReportModel.CurrencyId = data.CurrencyId;
                    statisticalReportModel.IsoCode = data.Code;
                    statisticalReportModel.CategoryId = data.CategoryId;
                    statisticalReportModel.CategoryName = data.CategoryName;
                    statisticalReportModel.GameId = data.GameId;
                    statisticalReportModel.GameName = data.GameName;
                    statisticalReportModel.ActualRate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();
                }

                statisticalReportModel = keyValuePairs[$"{data.CategoryId}-{data.GameId}-{data.CurrencyId}"];

                dateFormat = data.Date.ToString("yyyy-MM-dd");

                if (dateFormat == endDate)
                {
                    statisticalReportModel.TurnOverValidBetTodaySummary = data.ValidBetSummary;
                    statisticalReportModel.WinLossTodaySummary = data.WinLostSummary;

                    statisticalReportModel.TurnOverValidBetTodaySummaryInMYR = Math.Round(statisticalReportModel.TurnOverValidBetTodaySummary * statisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                    statisticalReportModel.WinLossTodaySummaryInMYR = Math.Round(statisticalReportModel.WinLossTodaySummary * statisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                }
                else if (dateFormat == (Convert.ToDateTime(endDate).AddDays(-1).ToString("yyyy-MM-dd")))
                {
                    statisticalReportModel.TurnOverValidBetYesterdaySummary = data.ValidBetSummary;
                    statisticalReportModel.WinLossYesterdaySummary = data.WinLostSummary;

                    statisticalReportModel.TurnOverValidBetYesterdaySummaryInMYR = Math.Round(statisticalReportModel.TurnOverValidBetYesterdaySummary * statisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                    statisticalReportModel.WinLossYesterdaySummaryInMYR = Math.Round(statisticalReportModel.WinLossYesterdaySummary * statisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                }
                else if (dateFormat == (Convert.ToDateTime(endDate).AddDays(-2).ToString("yyyy-MM-dd")))
                {
                    statisticalReportModel.TurnOverValidBetLastTwoDaysSummary = data.ValidBetSummary;
                    statisticalReportModel.WinLossLastTwoDaysSummary = data.WinLostSummary;

                    statisticalReportModel.TurnOverValidBetLastTwoDaysSummaryInMYR = Math.Round(statisticalReportModel.TurnOverValidBetLastTwoDaysSummary * statisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                    statisticalReportModel.WinLossLastTwoDaysSummaryInMYR = Math.Round(statisticalReportModel.WinLossLastTwoDaysSummary * statisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                }

                if (data.Date.Month == Convert.ToDateTime(endDate).Month)
                {
                    statisticalReportModel.TurnOverValidBetThisMonthSummary += data.ValidBetSummary;
                    statisticalReportModel.WinLossThisMonthSummary += data.WinLostSummary;

                    statisticalReportModel.TurnOverValidBetThisMonthSummaryInMYR += Math.Round(data.ValidBetSummary * statisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                    statisticalReportModel.WinLossThisMonthSummaryInMYR += Math.Round(data.WinLostSummary * statisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                }
                else
                {
                    statisticalReportModel.TurnOverValidBetLastMonthSummary += data.ValidBetSummary;
                    statisticalReportModel.WinLossLastMonthSummary += data.WinLostSummary;

                    statisticalReportModel.TurnOverValidBetLastMonthSummaryInMYR += Math.Round(data.ValidBetSummary * statisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                    statisticalReportModel.WinLossLastMonthSummaryInMYR += Math.Round(data.WinLostSummary * statisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                }
            }

            #endregion

            #region 補0

            List<GameModel> gameList = keyValuePairs.Select(s => s.Value).GroupBy(c => new { c.GameId, c.GameName })
                                    .Select(x => new GameModel
                                    {
                                        GameId = x.Key.GameId,
                                        GameName = x.Key.GameName
                                    }).ToList();

            List<CategoryModel> categoryList = keyValuePairs.Select(s => s.Value).GroupBy(c => new { c.CategoryId, c.CategoryName })
                                    .Select(x => new CategoryModel
                                    {
                                        CategoryId = x.Key.CategoryId,
                                        CategoryName = x.Key.CategoryName
                                    }).ToList();

            List<CategoryGameModel> categoryGameList = keyValuePairs.Select(s => s.Value).GroupBy(c => new { c.CategoryId, c.GameId })
                                    .Select(x => new CategoryGameModel
                                    {
                                        IdentifyString = $"{x.Key.CategoryId}-{x.Key.GameId}",
                                        CategoryId = x.Key.CategoryId,
                                        GameId = x.Key.GameId
                                    }).ToList();

            List<int> currencyIds = keyValuePairs.Select(s => s.Value).Select(x => x.CurrencyId).Distinct().ToList();

            foreach (var curr in currencyIds)
            {
                foreach (var item in categoryGameList)
                {
                    StatisticalReportModel statisticalReportModel = null;

                    if (!keyValuePairs.ContainsKey($"{item.IdentifyString}-{curr}"))
                    {
                        statisticalReportModel = new StatisticalReportModel();
                        keyValuePairs.Add($"{item.IdentifyString}-{curr}", statisticalReportModel);
                        statisticalReportModel.CurrencyId = curr;
                        statisticalReportModel.IsoCode = currencyList.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();
                        statisticalReportModel.CategoryId = item.CategoryId;
                        statisticalReportModel.CategoryName = categoryList.Where(x => x.CategoryId == item.CategoryId).Select(x => x.CategoryName).FirstOrDefault();
                        statisticalReportModel.GameId = item.GameId;
                        statisticalReportModel.GameName = gameList.Where(x => x.GameId == item.GameId).Select(x => x.GameName).FirstOrDefault();
                        statisticalReportModel.ActualRate = currencyList.Where(x => x.CurrencyId == curr).Select(x => x.ActualRate).FirstOrDefault();
                    }
                }
            }

            #endregion

            var Data = keyValuePairs.Select(s => s.Value).OrderBy(value => value.CategoryId)
                                                         .ThenBy(value => value.GameId)
                                                         .ThenBy(Value => Value.CurrencyId);

            return Data;
        }

        public IEnumerable<UserOnlineReportModel> GetUserOnlineReport(IEnumerable<int> siteList,
                                                                      string siteIds,
                                                                      string selectDate)
        {
            Dictionary<string, UserOnlineReportModel> keyValuePairs = new Dictionary<string, UserOnlineReportModel>();
            string dateTimeFormat = string.Empty;
            var userOnlineList = reportService.GetUserOnlineReport(siteIds,
                                                                   selectDate);

            var serverInfoList = reportService.GetServerInfoList();

            #region Data Statistical

            foreach (var data in userOnlineList)
            {
                UserOnlineReportModel userOnlineReportModel = null;

                dateTimeFormat = data.Time.ToString("HH:00");

                userOnlineReportModel = new UserOnlineReportModel();
                keyValuePairs.Add($"{dateTimeFormat}_{data.SiteId}", userOnlineReportModel);
                userOnlineReportModel.LocalTime = $"{dateTimeFormat}~{data.Time.AddHours(1).ToString("HH:00")}";
                userOnlineReportModel.SiteId = data.SiteId;
                userOnlineReportModel.SiteName = serverInfoList.Where(y => y.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                userOnlineReportModel.MaxCount = data.MaxCount;

            }

            #endregion

            #region 補0

            for (int i = 0; i < 24; i++)
            {
                UserOnlineReportModel userOnlineReportModel = null;

                foreach (int site in siteList)
                {
                    dateTimeFormat = Convert.ToDateTime(selectDate).AddHours(i).ToString("HH:00");

                    if (!keyValuePairs.ContainsKey($"{dateTimeFormat}_{site}"))
                    {
                        userOnlineReportModel = new UserOnlineReportModel();
                        keyValuePairs.Add($"{dateTimeFormat}_{site}", userOnlineReportModel);
                        userOnlineReportModel.LocalTime = $"{dateTimeFormat}~{Convert.ToDateTime(selectDate).AddHours(i + 1).ToString("HH:00")}";
                        userOnlineReportModel.SiteName = serverInfoList.Where(y => y.SiteId == site).Select(x => x.SiteName).FirstOrDefault();
                        userOnlineReportModel.SiteId = site;
                    }
                }
            }

            #endregion

            var Data = keyValuePairs.Select(s => s.Value).OrderBy(value => value.LocalTime)
                                                         .ThenBy(value => value.SiteId);

            return Data;
        }

    }
}
