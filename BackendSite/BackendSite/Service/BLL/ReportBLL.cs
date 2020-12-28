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


        #region Liberay Formula

        /// <summary>
        /// 亞洲四捨五入
        /// </summary>
        /// <param name="number">判斷數字</param>
        /// <param name="point">小數點取位</param>
        /// <returns>四捨五入過後的數字</returns>
        private decimal RoundingNumber(decimal number, int point)
        {
            return Math.Round(number, point, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 數字轉換為百分比
        /// </summary>
        /// <param name="number">數字</param>
        /// <returns>百分比文字</returns>
        private string TransformToPercentage(decimal number, int point)
        {
            return Math.Round(number * 100, point, MidpointRounding.AwayFromZero).ToString() + "%";
        }

        #endregion

        #region Liberay Data

        public IEnumerable<CurrencyModel> GetCurrency()
        {
            return reportService.GetCurrencyList();
        }

        #endregion

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

                mainFinancialModel.TurnOverStake += data.StakeSummary;
                mainFinancialModel.TurnOverValidBet += data.ValidBetSummary;
                mainFinancialModel.WinLoss += data.WinlostSummary;

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

        public IEnumerable<DepositReportModel> GetDepositReport(IEnumerable<int> CurrencyIds,
                                                                      string siteIds,
                                                                      string currencyIds,
                                                                      string startDate,
                                                                      string endDate)
        {

            Dictionary<string, DepositAccountModel> accountKeyValuePairs = new Dictionary<string, DepositAccountModel>();

            List<DepositSummaryReportModel> summaryCurrencyData = new List<DepositSummaryReportModel>();
            DepositSummaryReportModel summaryTotalData = new DepositSummaryReportModel();
            
            #region Get Data

            var depositDetailList = reportService.GetDepositReport(siteIds,
                                                                currencyIds,
                                                                startDate,
                                                                endDate);

            var currencyList = reportService.GetCurrencyList();

            #endregion

            #region Combine Data：Summary Data

            #region Account Total Data And Summary Total Data

            //Base Value Caculate
            foreach (var data in depositDetailList)
            {
                
                DepositAccountModel depositAccountModel = null;
                decimal rate = 0;
                int AccountType = 0;

                switch (data.DepositType)
                {
                    case DepositType.LocalBank:
                        AccountType = 3;
                        break;
                    default:
                        AccountType = 99;
                        break;
                }

                if (!accountKeyValuePairs.ContainsKey($"{AccountType}-{data.AccountId}"))
                {
                    depositAccountModel = new DepositAccountModel();
                    accountKeyValuePairs.Add($"{AccountType}-{data.AccountId}", depositAccountModel);
                    depositAccountModel.AccountType = AccountType;
                    depositAccountModel.AccountId = data.AccountId;
                    depositAccountModel.AccountName = data.AccountName;
                }

                rate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();

                depositAccountModel = accountKeyValuePairs[$"{AccountType}-{data.AccountId}"];

                depositAccountModel.SummaryTotalData.TotalCount += data.TotalCount;
                depositAccountModel.SummaryTotalData.SuccessCount += data.SuccessCount;
                depositAccountModel.SummaryTotalData.FTDCount += data.FTDCount;
                depositAccountModel.SummaryTotalData.TotalProcessingTime += data.TotalProcessingTime;

                summaryTotalData.TotalCount += data.TotalCount;
                summaryTotalData.SuccessCount += data.SuccessCount;
                summaryTotalData.FTDCount += data.FTDCount;
                summaryTotalData.TotalProcessingTime += data.TotalProcessingTime;

                switch (data.DepositType)
                {
                    case DepositType.OnlineBanking:
                        depositAccountModel.SummaryTotalData.OnlineDeposit += RoundingNumber(data.DepositSummary * rate, 2);
                        summaryTotalData.OnlineDeposit += RoundingNumber(data.DepositSummary * rate, 2);
                        break;
                    case DepositType.MobilePayment:
                        depositAccountModel.SummaryTotalData.MobileDeposit += RoundingNumber(data.DepositSummary * rate, 2);
                        summaryTotalData.MobileDeposit += RoundingNumber(data.DepositSummary * rate, 2);
                        break;
                    case DepositType.LocalBank:
                        depositAccountModel.SummaryTotalData.LocalDeposit += RoundingNumber(data.DepositSummary * rate, 2);
                        summaryTotalData.LocalDeposit += RoundingNumber(data.DepositSummary * rate, 2);
                        break;
                }
            }

            //Account Data Special Averange Value Caculate
            foreach (var item in accountKeyValuePairs)
            {
                item.Value.SummaryTotalData.TotalAmount = item.Value.SummaryTotalData.OnlineDeposit + item.Value.SummaryTotalData.MobileDeposit + item.Value.SummaryTotalData.LocalDeposit;
                item.Value.SummaryTotalData.AverangeDeposit = item.Value.SummaryTotalData.SuccessCount == 0 ? 0 : RoundingNumber((item.Value.SummaryTotalData.OnlineDeposit + item.Value.SummaryTotalData.MobileDeposit + item.Value.SummaryTotalData.LocalDeposit) / item.Value.SummaryTotalData.SuccessCount, 2);
                item.Value.SummaryTotalData.SuccessRate = item.Value.SummaryTotalData.TotalCount == 0 ? "0%" : TransformToPercentage((decimal)item.Value.SummaryTotalData.SuccessCount / item.Value.SummaryTotalData.TotalCount, 2);
                item.Value.SummaryTotalData.AverangeProcessingTime = item.Value.SummaryTotalData.SuccessCount == 0 ? TimeSpan.FromSeconds(0).ToString() : TimeSpan.FromSeconds(item.Value.SummaryTotalData.TotalProcessingTime / item.Value.SummaryTotalData.SuccessCount).ToString();

                item.Value.SummaryTotalData.OnlinePrecent = item.Value.SummaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(item.Value.SummaryTotalData.OnlineDeposit / item.Value.SummaryTotalData.TotalAmount, 2);
                item.Value.SummaryTotalData.MobilePrecent = item.Value.SummaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(item.Value.SummaryTotalData.MobileDeposit / item.Value.SummaryTotalData.TotalAmount, 2);
                item.Value.SummaryTotalData.LocalPrecent = item.Value.SummaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(item.Value.SummaryTotalData.LocalDeposit / item.Value.SummaryTotalData.TotalAmount, 2);

            }

            //Summary Data Special Averange Value Caculate
            summaryTotalData.TotalAmount = summaryTotalData.OnlineDeposit + summaryTotalData.MobileDeposit + summaryTotalData.LocalDeposit;
            summaryTotalData.AverangeDeposit = summaryTotalData.SuccessCount == 0 ? 0 : RoundingNumber((summaryTotalData.OnlineDeposit + summaryTotalData.MobileDeposit + summaryTotalData.LocalDeposit) / summaryTotalData.SuccessCount, 2);
            summaryTotalData.SuccessRate = summaryTotalData.TotalCount == 0 ? "0%" : TransformToPercentage((decimal)summaryTotalData.SuccessCount / summaryTotalData.TotalCount, 2);
            summaryTotalData.AverangeProcessingTime = summaryTotalData.SuccessCount == 0 ? TimeSpan.FromSeconds(0).ToString() : TimeSpan.FromSeconds(summaryTotalData.TotalProcessingTime / summaryTotalData.SuccessCount).ToString();

            summaryTotalData.OnlinePrecent = summaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(summaryTotalData.OnlineDeposit / summaryTotalData.TotalAmount, 2);
            summaryTotalData.MobilePrecent = summaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(summaryTotalData.MobileDeposit / summaryTotalData.TotalAmount, 2);
            summaryTotalData.LocalPrecent = summaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(summaryTotalData.LocalDeposit / summaryTotalData.TotalAmount, 2);

            #endregion

            #region Account Currency Data And Summary Currency Data

            //Base Value Caculate
            foreach (var data in depositDetailList)
            {
                DepositAccountModel depositAccountModel = null;
                DepositSummaryReportModel accountCurrencyModel = null;
                DepositSummaryReportModel summaryCurrencyModel = null;
                decimal rate = 0;
                int AccountType = 0;

                switch (data.DepositType)
                {
                    case DepositType.LocalBank:
                        AccountType = 3;
                        break;
                    default:
                        AccountType = 99;
                        break;
                }

                rate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();

                depositAccountModel = accountKeyValuePairs[$"{AccountType}-{data.AccountId}"];

                //accountCurrencyDataModel = depositAccountModel.SummaryData.Where(x => x.CurrencyId == data.CurrencyId).FirstOrDefault();

                if (!depositAccountModel.SummaryData.Select(x => x.CurrencyId).Contains(data.CurrencyId))
                {
                    accountCurrencyModel = new DepositSummaryReportModel();
                    accountCurrencyModel.CurrencyId = data.CurrencyId;
                    accountCurrencyModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();

                    depositAccountModel.SummaryData.Add(accountCurrencyModel);
                }

                if (!summaryCurrencyData.Select(x => x.CurrencyId).Contains(data.CurrencyId))
                {
                    summaryCurrencyModel = new DepositSummaryReportModel();
                    summaryCurrencyModel.CurrencyId = data.CurrencyId;
                    summaryCurrencyModel.IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();

                    summaryCurrencyData.Add(summaryCurrencyModel);
                }

                accountCurrencyModel = depositAccountModel.SummaryData.Where(x => x.CurrencyId == data.CurrencyId).FirstOrDefault();
                summaryCurrencyModel = summaryCurrencyData.Where(x => x.CurrencyId == data.CurrencyId).FirstOrDefault();

                accountCurrencyModel.TotalAmount += data.DepositSummary;
                accountCurrencyModel.TotalAmountInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                accountCurrencyModel.TotalCount += data.TotalCount;
                accountCurrencyModel.SuccessCount += data.SuccessCount;
                accountCurrencyModel.FTDCount += data.FTDCount;
                accountCurrencyModel.TotalProcessingTime += data.TotalProcessingTime;

                summaryCurrencyModel.TotalAmount += data.DepositSummary;
                summaryCurrencyModel.TotalAmountInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                summaryCurrencyModel.TotalCount += data.TotalCount;
                summaryCurrencyModel.SuccessCount += data.SuccessCount;
                summaryCurrencyModel.FTDCount += data.FTDCount;
                summaryCurrencyModel.TotalProcessingTime += data.TotalProcessingTime;

                switch (data.DepositType)
                {
                    case DepositType.OnlineBanking:
                        accountCurrencyModel.OnlineDeposit += data.DepositSummary;
                        accountCurrencyModel.OnlineDepositInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                        summaryCurrencyModel.OnlineDeposit += data.DepositSummary;
                        summaryCurrencyModel.OnlineDepositInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                        break;
                    case DepositType.MobilePayment:
                        accountCurrencyModel.MobileDeposit += data.DepositSummary;
                        accountCurrencyModel.MobileDepositInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                        summaryCurrencyModel.MobileDeposit += data.DepositSummary;
                        summaryCurrencyModel.MobileDepositInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                        break;
                    case DepositType.LocalBank:
                        accountCurrencyModel.LocalDeposit += data.DepositSummary;
                        accountCurrencyModel.LocalDepositInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                        summaryCurrencyModel.LocalDeposit += data.DepositSummary;
                        summaryCurrencyModel.LocalDepositInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                        break;
                }
            }

            //Account Data Special Averange Value Caculate
            foreach (var accountItem in accountKeyValuePairs)
            {
                DepositSummaryReportModel accountCurrencyModel = null;
                //補0
                foreach (var curr in CurrencyIds)
                {
                    if (!accountItem.Value.SummaryData.Select(x => x.CurrencyId).Contains(curr))
                    {
                        accountCurrencyModel = new DepositSummaryReportModel();
                        accountCurrencyModel.CurrencyId = curr;
                        accountCurrencyModel.IsoCode = currencyList.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();

                        accountItem.Value.SummaryData.Add(accountCurrencyModel);
                    }
                }

                foreach (var accountCurrencyItem in accountItem.Value.SummaryData)
                {
                    accountCurrencyItem.OnlinePrecent = accountItem.Value.SummaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(accountCurrencyItem.OnlineDepositInMYR / accountItem.Value.SummaryTotalData.TotalAmount, 2);
                    accountCurrencyItem.MobilePrecent = accountItem.Value.SummaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(accountCurrencyItem.MobileDepositInMYR / accountItem.Value.SummaryTotalData.TotalAmount, 2);
                    accountCurrencyItem.LocalPrecent = accountItem.Value.SummaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(accountCurrencyItem.LocalDepositInMYR / accountItem.Value.SummaryTotalData.TotalAmount, 2);

                    accountCurrencyItem.AverangeDeposit = accountCurrencyItem.SuccessCount == 0 ? 0 : RoundingNumber((accountCurrencyItem.OnlineDeposit + accountCurrencyItem.MobileDeposit + accountCurrencyItem.LocalDeposit) / accountCurrencyItem.SuccessCount, 2);
                    accountCurrencyItem.SuccessRate = accountCurrencyItem.TotalCount == 0 ? "0%" : TransformToPercentage((decimal)accountCurrencyItem.SuccessCount / accountCurrencyItem.TotalCount, 2);
                    accountCurrencyItem.AverangeProcessingTime = accountCurrencyItem.SuccessCount == 0 ? TimeSpan.FromSeconds(0).ToString() : TimeSpan.FromSeconds(accountCurrencyItem.TotalProcessingTime / accountCurrencyItem.SuccessCount).ToString();
                }

            }

            //Summary Data Zero Fill
            foreach (var curr in CurrencyIds)
            {
                DepositSummaryReportModel summaryCurrencyModel = null;

                if (!summaryCurrencyData.Select(x => x.CurrencyId).Contains(curr))
                {
                    summaryCurrencyModel = new DepositSummaryReportModel();
                    summaryCurrencyModel.CurrencyId = curr;
                    summaryCurrencyModel.IsoCode = currencyList.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault();
                    
                    summaryCurrencyData.Add(summaryCurrencyModel);
                }
            }

            //Summary Data Special Averange Value Caculate
            foreach (var currencyItem in summaryCurrencyData)
            {
                currencyItem.OnlinePrecent = summaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(currencyItem.OnlineDepositInMYR / summaryTotalData.TotalAmount, 2);
                currencyItem.MobilePrecent = summaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(currencyItem.MobileDepositInMYR / summaryTotalData.TotalAmount, 2);
                currencyItem.LocalPrecent = summaryTotalData.TotalAmount == 0 ? "0%" : TransformToPercentage(currencyItem.LocalDepositInMYR / summaryTotalData.TotalAmount, 2);

                currencyItem.AverangeDeposit = currencyItem.SuccessCount == 0 ? 0 : RoundingNumber((currencyItem.OnlineDeposit + currencyItem.MobileDeposit + currencyItem.LocalDeposit) / currencyItem.SuccessCount, 2);
                currencyItem.SuccessRate = summaryTotalData.TotalCount == 0 ? "0%" : TransformToPercentage((decimal)currencyItem.SuccessCount / summaryTotalData.TotalCount, 2);
                currencyItem.AverangeProcessingTime = currencyItem.SuccessCount == 0 ? TimeSpan.FromSeconds(0).ToString() : TimeSpan.FromSeconds(currencyItem.TotalProcessingTime / currencyItem.SuccessCount).ToString();
            }

            #endregion

            #endregion

            #region  Combine Data：Date Data

            #endregion


            var accountData = accountKeyValuePairs.Select(s => s.Value).OrderBy(value => value.AccountType)
                                                         .ThenBy(value => value.AccountId);

            yield return new DepositReportModel()
            {
                SummaryAccountData = accountData.ToList(),
                SummaryCurrencyData = summaryCurrencyData,
                SummaryTotalData = summaryTotalData
            };
        }
    }
}
