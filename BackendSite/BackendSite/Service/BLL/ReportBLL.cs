using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace BackendSite.Service.BLL
{
    public class ReportBLL
    {
        #region Fields

        private readonly ReportService reportService;
        private readonly SelLangBLL selLangBLL;
        private readonly Common _common;
        private readonly PartitionService _partitionService;
        private readonly RedisHelper _redisHelper;

        #endregion

        #region Constructors

        public ReportBLL(IServiceProvider provider)
        {
            reportService = provider.GetRequiredService<ReportService>();
            selLangBLL = provider.GetRequiredService<SelLangBLL>();
            _common = provider.GetRequiredService<Common>();
            _partitionService = provider.GetRequiredService<PartitionService>();
            _redisHelper = provider.GetRequiredService<RedisHelper>();
        }

        #endregion

        #region Library Formula

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

        #region Library Data

        public IEnumerable<ServerInfoModel> GetServer()
        {
            return reportService.GetServerInfoList();
        }

        public IEnumerable<CurrencyModel> GetCurrency()
        {
            return reportService.GetCurrencyList();
        }

        public async Task<IEnumerable<TpCategoryBetLimit>> GetTpCategoryList()
        {
            var tpCategories = await reportService.GetTpCategoryList();

            return tpCategories.Select(category => new TpCategoryBetLimit() { CategoryId = category.CategoryId, CategoryName = category.CategoryName }).ToList();
        }


        #endregion

        public IEnumerable<MainFinancialModel> GetMainFinancialReport(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
        {
            var keyValuePairs = new Dictionary<string, MainFinancialModel>();
            string dateFormat;
            var currencyIdList = JsonConvert.DeserializeObject<List<int>>(currencyIds);
            var reportList = reportService.GetMainFinancialReport(siteIds, currencyIds, startDate, endDate, timeZone);
            var turnoverAndWinLossList = reportService.GetTurnoverAndWinLossSummary(siteIds, currencyIds, startDate, endDate, timeZone);
            var currencyList = reportService.GetCurrencyList();

            #region Data Combine

            foreach (var data in reportList["Deposit"])
            {
                MainFinancialModel mainFinancialModel;
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
                MainFinancialModel mainFinancialModel;

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
                MainFinancialModel mainFinancialModel;

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
                MainFinancialModel mainFinancialModel;

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
                MainFinancialModel mainFinancialModel;

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

            foreach (var data in turnoverAndWinLossList)
            {
                MainFinancialModel mainFinancialModel;

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

                mainFinancialModel.TurnOverStakeInMYR += Math.Round(data.StakeSummary * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                mainFinancialModel.TurnOverValidBetInMYR += Math.Round(data.ValidBetSummary * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                mainFinancialModel.WinLossInMYR += Math.Round(data.WinlostSummary * mainFinancialModel.ActualRate, 2, MidpointRounding.AwayFromZero);
            }

            #endregion

            #region 補0

            var dateList = keyValuePairs.Select(s => s.Value).Select(x => x.Date).Distinct().ToList();

            foreach (var curr in currencyIdList)
            {
                foreach (var date in dateList)
                {
                    MainFinancialModel mainFinancialModel;

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

            var response = keyValuePairs.Select(s => s.Value).OrderBy(key => key.Date)
                                                        .ThenBy(key => key.CurrencyId);
            return response;
        }

        public IEnumerable<StatisticalReportModel> GetStatisticalReport(string siteIds, string timeZone, string startDate, string endDate)
        {
            var keyValuePairs = new Dictionary<string, StatisticalReportModel>();
            var statisticalList = reportService.GetStatisticalReport(siteIds, timeZone, startDate, endDate);
            var currencyList = reportService.GetCurrencyList();

            #region Data Statistical

            foreach (var data in statisticalList)
            {
                StatisticalReportModel statisticalReportModel;

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

                var dateFormat = data.Date.ToString("yyyy-MM-dd");

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

            var gameList = keyValuePairs.Select(s => s.Value).GroupBy(c => new { c.GameId, c.GameName })
                                    .Select(x => new GameModel
                                    {
                                        GameId = x.Key.GameId,
                                        GameName = x.Key.GameName
                                    }).ToList();

            var categoryList = keyValuePairs.Select(s => s.Value).GroupBy(c => new { c.CategoryId, c.CategoryName })
                                    .Select(x => new CategoryModel
                                    {
                                        CategoryId = x.Key.CategoryId,
                                        CategoryName = x.Key.CategoryName
                                    }).ToList();

            var categoryGameList = keyValuePairs.Select(s => s.Value).GroupBy(c => new { c.CategoryId, c.GameId })
                                    .Select(x => new CategoryGameModel
                                    {
                                        IdentifyString = $"{x.Key.CategoryId}-{x.Key.GameId}",
                                        CategoryId = x.Key.CategoryId,
                                        GameId = x.Key.GameId
                                    }).ToList();

            var currencyIds = keyValuePairs.Select(s => s.Value).Select(x => x.CurrencyId).Distinct().ToList();

            foreach (var curr in currencyIds)
            {
                foreach (var item in categoryGameList)
                {
                    if (!keyValuePairs.ContainsKey($"{item.IdentifyString}-{curr}"))
                    {
                        var statisticalReportModel = new StatisticalReportModel();
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

            var response = keyValuePairs.Select(s => s.Value).OrderBy(value => value.CategoryId)
                                                         .ThenBy(value => value.GameId)
                                                         .ThenBy(value => value.CurrencyId);

            return response;
        }

        public IEnumerable<UserOnlineReportModel> GetUserOnlineReport(string siteIds, string selectDate, int timeZone)
        {
            var keyValuePairs = new Dictionary<string, UserOnlineReportModel>();
            string dateTimeFormat;
            var siteList = JsonConvert.DeserializeObject<List<int>>(siteIds);
            var deviceIdList = Enum.GetValues(typeof(DeviceId)).Cast<DeviceId>();
            var serverInfoList = reportService.GetServerInfoList();

            var date = new DateTimeOffset(Convert.ToDateTime(selectDate), TimeSpan.FromHours(timeZone));
            var startTime = new DateTimeOffset(date.ToUniversalTime().DateTime, TimeSpan.Zero);
            var endTime = new DateTimeOffset(date.ToUniversalTime().DateTime, TimeSpan.Zero).AddHours(24).AddMilliseconds(-1);
            var timeZoneString = _common.TransferToTimeZoneFormat(timeZone);

            var userOnlineList = reportService.GetUserOnlineReport(siteIds, startTime.DateTime, endTime.DateTime, timeZoneString);

            #region Data Statistical

            foreach (var data in userOnlineList)
            {
                dateTimeFormat = data.Time.ToString("HH:00");
                var userOnlineReportModel = new UserOnlineReportModel();
                keyValuePairs.Add($"{dateTimeFormat}_{data.SiteId}_{data.DeviceType}", userOnlineReportModel);
                userOnlineReportModel.LocalTime = $"{dateTimeFormat}~{data.Time.AddHours(1).ToString("HH:00")}";
                userOnlineReportModel.SiteId = data.SiteId;
                userOnlineReportModel.DeviceType = data.DeviceType;
                userOnlineReportModel.SiteName = serverInfoList.Where(y => y.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                userOnlineReportModel.DeviceName = Enum.GetName(typeof(DeviceId), data.DeviceType);
                userOnlineReportModel.MaxCount = data.MaxCount;
            }

            #endregion

            #region 補0

            for (var i = 0; i < 24; i++)
            {
                foreach (var site in siteList)
                {
                    dateTimeFormat = Convert.ToDateTime(selectDate).AddHours(i).ToString("HH:00");

                    foreach (var id in deviceIdList)
                    {
                        if (!keyValuePairs.ContainsKey($"{dateTimeFormat}_{site}_{(short)id}"))
                        {
                            var userOnlineReportModel = new UserOnlineReportModel();
                            keyValuePairs.Add($"{dateTimeFormat}_{site}_{id}", userOnlineReportModel);
                            userOnlineReportModel.LocalTime = $"{dateTimeFormat}~{Convert.ToDateTime(selectDate).AddHours(i + 1).ToString("HH:00")}";
                            userOnlineReportModel.SiteName = serverInfoList.Where(y => y.SiteId == site).Select(x => x.SiteName).FirstOrDefault();
                            userOnlineReportModel.SiteId = site;
                            userOnlineReportModel.DeviceType = (short)id;
                            userOnlineReportModel.DeviceName = Enum.GetName(typeof(DeviceId), id);
                        }
                    }
                }
            }

            #endregion

            // 不顯示Old登入次數
            var oldList = keyValuePairs.Where(x => x.Value.DeviceType == (short)DeviceId.Old);
            var isRemoveOld = oldList.All(x => x.Value.MaxCount == 0);
            if (isRemoveOld)
            {
                foreach (var item in oldList)
                {
                    keyValuePairs.Remove(item.Key);
                }
            }

            return keyValuePairs
                .Select(s => s.Value)
                .OrderBy(value => value.LocalTime)
                .ThenBy(value => value.SiteId);
        }

        public IEnumerable<DepositReportModel> GetDepositReport(IEnumerable<int> CurrencyIds, string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
        {
            var accountKeyValuePairs = new Dictionary<string, DepositAccountModel>();
            var accountDateKeyValuePairs = new Dictionary<string, DepositByDateReportModel>();
            var accountDateTotalKeyValuePairs = new Dictionary<string, DepositByDateReportModel>();

            var summaryCurrencyData = new List<DepositSummaryReportModel>();
            var summaryTotalData = new DepositSummaryReportModel();

            #region Get Data

            var depositDetailList = reportService.GetDepositReport(siteIds, currencyIds, startDate, endDate, timeZone);
            var currencyList = reportService.GetCurrencyList();

            #endregion

            #region Combine Data：Summary Data

            #region Account Total Data And Summary Total Data

            //Base Value Caculate
            foreach (var data in depositDetailList)
            {
                DepositAccountModel depositAccountModel;
                int accountType;

                switch (data.DepositType)
                {
                    case DepositTypeEnum.LocalBanking:
                        accountType = 3;
                        break;
                    default:
                        accountType = 99;
                        break;
                }

                if (!accountKeyValuePairs.ContainsKey($"{accountType}-{data.AccountId}"))
                {
                    depositAccountModel = new DepositAccountModel();
                    accountKeyValuePairs.Add($"{accountType}-{data.AccountId}", depositAccountModel);
                    depositAccountModel.AccountType = accountType;
                    depositAccountModel.AccountId = data.AccountId;
                    depositAccountModel.AccountName = data.AccountName;
                }

                var rate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();

                depositAccountModel = accountKeyValuePairs[$"{accountType}-{data.AccountId}"];

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
                    case DepositTypeEnum.OnlineBanking:
                        depositAccountModel.SummaryTotalData.OnlineDeposit += RoundingNumber(data.DepositSummary * rate, 2);
                        summaryTotalData.OnlineDeposit += RoundingNumber(data.DepositSummary * rate, 2);
                        break;
                    case DepositTypeEnum.MobilePay:
                        depositAccountModel.SummaryTotalData.MobileDeposit += RoundingNumber(data.DepositSummary * rate, 2);
                        summaryTotalData.MobileDeposit += RoundingNumber(data.DepositSummary * rate, 2);
                        break;
                    case DepositTypeEnum.LocalBanking:
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
                DepositSummaryReportModel accountCurrencyModel;
                DepositSummaryReportModel summaryCurrencyModel;
                int accountType;

                switch (data.DepositType)
                {
                    case DepositTypeEnum.LocalBanking:
                        accountType = 3;
                        break;
                    default:
                        accountType = 99;
                        break;
                }

                var rate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();

                var depositAccountModel = accountKeyValuePairs[$"{accountType}-{data.AccountId}"];

                if (!depositAccountModel.SummaryData.Select(x => x.CurrencyId).Contains(data.CurrencyId))
                {
                    accountCurrencyModel = new DepositSummaryReportModel
                    {
                        CurrencyId = data.CurrencyId,
                        IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault()
                    };

                    depositAccountModel.SummaryData.Add(accountCurrencyModel);
                }

                if (!summaryCurrencyData.Select(x => x.CurrencyId).Contains(data.CurrencyId))
                {
                    summaryCurrencyModel = new DepositSummaryReportModel
                    {
                        CurrencyId = data.CurrencyId,
                        IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault()
                    };

                    summaryCurrencyData.Add(summaryCurrencyModel);
                }

                accountCurrencyModel = depositAccountModel.SummaryData.FirstOrDefault(x => x.CurrencyId == data.CurrencyId);
                summaryCurrencyModel = summaryCurrencyData.FirstOrDefault(x => x.CurrencyId == data.CurrencyId);

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
                    case DepositTypeEnum.OnlineBanking:
                        accountCurrencyModel.OnlineDeposit += data.DepositSummary;
                        accountCurrencyModel.OnlineDepositInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                        summaryCurrencyModel.OnlineDeposit += data.DepositSummary;
                        summaryCurrencyModel.OnlineDepositInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                        break;
                    case DepositTypeEnum.MobilePay:
                        accountCurrencyModel.MobileDeposit += data.DepositSummary;
                        accountCurrencyModel.MobileDepositInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                        summaryCurrencyModel.MobileDeposit += data.DepositSummary;
                        summaryCurrencyModel.MobileDepositInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                        break;
                    case DepositTypeEnum.LocalBanking:
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
                //補0
                foreach (var curr in CurrencyIds)
                {
                    if (!accountItem.Value.SummaryData.Select(x => x.CurrencyId).Contains(curr))
                    {
                        var accountCurrencyModel = new DepositSummaryReportModel
                        {
                            CurrencyId = curr,
                            IsoCode = currencyList.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault()
                        };

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
                if (!summaryCurrencyData.Select(x => x.CurrencyId).Contains(curr))
                {
                    var summaryCurrencyModel = new DepositSummaryReportModel
                    {
                        CurrencyId = curr,
                        IsoCode = currencyList.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault()
                    };

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

            var dateList = depositDetailList.Select(x => x.Date.ToString("yyyy/MM/dd")).Distinct().ToList();

            #region Account Total Data And Summary Total Data

            //Base Value Caculate
            foreach (var data in depositDetailList)
            {
                DepositByDateReportModel depositByDateReportModel;
                DepositByDateReportModel depositByDateReportTotalModel;
                int accountType;
                var dateFormat = data.Date.ToString("yyyy/MM/dd");

                switch (data.DepositType)
                {
                    case DepositTypeEnum.LocalBanking:
                        accountType = 3;
                        break;
                    default:
                        accountType = 99;
                        break;
                }

                if (!accountDateKeyValuePairs.ContainsKey($"{accountType}-{data.AccountId}-{dateFormat}-{data.CurrencyId}"))
                {
                    depositByDateReportModel = new DepositByDateReportModel
                    {
                        Date = dateFormat,
                        CurrencyId = data.CurrencyId,
                        IsoCode = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault()
                    };

                    accountDateKeyValuePairs.Add($"{accountType}-{data.AccountId}-{dateFormat}-{data.CurrencyId}", depositByDateReportModel);
                }

                depositByDateReportModel = accountDateKeyValuePairs[$"{accountType}-{data.AccountId}-{dateFormat}-{data.CurrencyId}"];
                var rate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();

                depositByDateReportModel.TotalAmount += RoundingNumber(data.DepositSummary, 2); ;
                depositByDateReportModel.TotalAmountInMYR += RoundingNumber(data.DepositSummary * rate, 2);
                depositByDateReportModel.TotalCount += data.TotalCount;
                depositByDateReportModel.SuccessCount += data.SuccessCount;
                depositByDateReportModel.FTDCount += data.FTDCount;

                if (!accountDateTotalKeyValuePairs.ContainsKey($"{accountType}-{data.AccountId}-{dateFormat}"))
                {
                    depositByDateReportTotalModel = new DepositByDateReportModel
                    {
                        Date = dateFormat
                    };

                    accountDateTotalKeyValuePairs.Add($"{accountType}-{data.AccountId}-{dateFormat}", depositByDateReportTotalModel);
                }

                depositByDateReportTotalModel = accountDateTotalKeyValuePairs[$"{accountType}-{data.AccountId}-{dateFormat}"];
                rate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();

                depositByDateReportTotalModel.TotalAmount += RoundingNumber(data.DepositSummary * rate, 2);
                depositByDateReportTotalModel.TotalCount += data.TotalCount;
                depositByDateReportTotalModel.SuccessCount += data.SuccessCount;
                depositByDateReportTotalModel.FTDCount += data.FTDCount;

            }

            //Account Data Special Averange Value Caculate
            foreach (var accountItem in accountKeyValuePairs)
            {
                foreach (var date in dateList)
                {

                    foreach (var curr in CurrencyIds)
                    {
                        if (!accountDateKeyValuePairs.ContainsKey($"{accountItem.Value.AccountType}-{accountItem.Value.AccountId}-{date}-{curr}"))
                        {
                            var depositByDateReportModel = new DepositByDateReportModel
                            {
                                Date = date,
                                CurrencyId = curr,
                                IsoCode = currencyList.Where(x => x.CurrencyId == curr).Select(x => x.IsoCode).FirstOrDefault()
                            };

                            accountDateKeyValuePairs.Add($"{accountItem.Value.AccountType}-{accountItem.Value.AccountId}-{date}-{curr}", depositByDateReportModel);
                        }

                        var dateItem = accountDateKeyValuePairs[$"{accountItem.Value.AccountType}-{accountItem.Value.AccountId}-{date}-{curr}"];

                        dateItem.AverangeDeposit = dateItem.SuccessCount == 0 ? 0 : RoundingNumber(dateItem.TotalAmount / dateItem.SuccessCount, 2);
                        dateItem.SuccessRate = dateItem.TotalCount == 0 ? "0%" : TransformToPercentage((decimal)dateItem.SuccessCount / dateItem.TotalCount, 2);

                    }

                    if (!accountDateTotalKeyValuePairs.ContainsKey($"{accountItem.Value.AccountType}-{accountItem.Value.AccountId}-{date}"))
                    {
                        var depositByDateReportTotalModel = new DepositByDateReportModel
                        {
                            Date = date
                        };

                        accountDateTotalKeyValuePairs.Add($"{accountItem.Value.AccountType}-{accountItem.Value.AccountId}-{date}", depositByDateReportTotalModel);
                    }

                    var dateTotalItem = accountDateTotalKeyValuePairs[$"{accountItem.Value.AccountType}-{accountItem.Value.AccountId}-{date}"];

                    dateTotalItem.AverangeDeposit = dateTotalItem.SuccessCount == 0 ? 0 : RoundingNumber(dateTotalItem.TotalAmount / dateTotalItem.SuccessCount, 2);
                    dateTotalItem.SuccessRate = dateTotalItem.TotalCount == 0 ? "0%" : TransformToPercentage((decimal)dateTotalItem.SuccessCount / dateTotalItem.TotalCount, 2);

                }

                accountItem.Value.DateData = accountDateKeyValuePairs.Where(x => x.Key.StartsWith($"{accountItem.Value.AccountType}-{accountItem.Value.AccountId}-"))
                                                                     .Select(s => s.Value)
                                                                     .OrderBy(value => value.Date)
                                                                     .ThenBy(value => value.CurrencyId)
                                                                     .ToList();

                accountItem.Value.DateTotalData = accountDateTotalKeyValuePairs.Where(x => x.Key.StartsWith($"{accountItem.Value.AccountType}-{accountItem.Value.AccountId}-"))
                                                                     .Select(s => s.Value)
                                                                     .OrderBy(value => value.Date)
                                                                     .ToList();


            }

            #endregion

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

        public IEnumerable<CustomerStatisticalReportModel> GetCustomerStatisticalReport(IEnumerable<int> CurrencyIds,
                                                                                        string siteIds,
                                                                                        string currencyIds,
                                                                                        string startDate,
                                                                                        string endDate,
                                                                                        string userName,
                                                                                        string timeZone)
        {
            var keyValuePairs = new Dictionary<string, CustomerStatisticalReportModel>();
            var statisticalList = reportService.GetCustomerStatisticalReport(siteIds, currencyIds, startDate, endDate, userName, timeZone);
            var currencyList = reportService.GetCurrencyList();

            #region Data Statistical

            foreach (var data in statisticalList)
            {
                CustomerStatisticalReportModel customerStatisticalReportModel;

                var dateFormat = data.Date.ToString("yyyy/MM/dd");

                if (!keyValuePairs.ContainsKey($"{dateFormat}-{data.CategoryId}-{data.GameId}-{data.CurrencyId}-{data.CustId}"))
                {
                    customerStatisticalReportModel = new CustomerStatisticalReportModel();
                    keyValuePairs.Add($"{dateFormat}-{data.CategoryId}-{data.GameId}-{data.CurrencyId}-{data.CustId}", customerStatisticalReportModel);
                    customerStatisticalReportModel.Date = dateFormat;
                    customerStatisticalReportModel.CurrencyId = data.CurrencyId;
                    customerStatisticalReportModel.IsoCode = data.Code;
                    customerStatisticalReportModel.CategoryId = data.CategoryId;
                    customerStatisticalReportModel.CategoryName = data.CategoryName;
                    customerStatisticalReportModel.GameId = data.GameId;
                    customerStatisticalReportModel.GameName = data.GameName;
                    customerStatisticalReportModel.CustId = data.CustId;
                    customerStatisticalReportModel.UserName = data.UserName;
                    customerStatisticalReportModel.ActualRate = currencyList.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.ActualRate).FirstOrDefault();
                }

                customerStatisticalReportModel = keyValuePairs[$"{dateFormat}-{data.CategoryId}-{data.GameId}-{data.CurrencyId}-{data.CustId}"];

                customerStatisticalReportModel.TurnOverValidBetSummary = Math.Round(data.ValidBetSummary, 2, MidpointRounding.AwayFromZero);
                customerStatisticalReportModel.WinLossSummary = Math.Round(data.WinLostSummary, 2, MidpointRounding.AwayFromZero);

                customerStatisticalReportModel.TurnOverValidBetSummaryInMYR = Math.Round(customerStatisticalReportModel.TurnOverValidBetSummary * customerStatisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);
                customerStatisticalReportModel.WinLossSummaryInMYR = Math.Round(customerStatisticalReportModel.WinLossSummary * customerStatisticalReportModel.ActualRate, 2, MidpointRounding.AwayFromZero);

            }

            #endregion

            var Data = keyValuePairs.Select(s => s.Value).OrderBy(value => value.CustId)
                                                         .ThenBy(value => value.Date)
                                                         .ThenBy(value => value.CategoryId)
                                                         .ThenBy(value => value.GameId)
                                                         .ThenBy(value => value.CurrencyId);

            return Data;
        }

        public IEnumerable<TpBetRecordDataModel> GetCustomerDetail(int custId, int gameId, string date, string timeZone)
        {
            var detailList = reportService.GetCustomerDetail(custId, gameId, date, timeZone);
            return detailList;
        }

        public IEnumerable<TpFailRecordReport> GetTpFailRecordReport(GetTpFailRecordReportInput input,
            out int totalPages,
            out int rowCount)
            => reportService.GetTpFailRecordReport(input, out totalPages, out rowCount);

        public IEnumerable<TpFailRecordReport> GetTpFailRecordReportForExport(GetTpFailRecordReportInput input)
            => reportService.GetTpFailRecordReportForExport(input);

        public List<BetListReport> GetBetListReport(GetBetListRequest input, out int totalPages, out int rowCount)
        {
            totalPages = 0;
            rowCount = 0;

            var data = _redisHelper.TryGetOrCreate(input.GetRedisKey(), TimeSpan.FromMinutes(15), i =>
            {
                var StartDate = new DateTimeOffset(input.StartTime.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                var EndDate = new DateTimeOffset(input.EndTime.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                var parameters = new DynamicParameters(input);
                parameters.AddDynamicParams(new
                {
                    SiteIdJson = JsonConvert.SerializeObject(input.SiteIds),
                    CurrencyIdJson = JsonConvert.SerializeObject(input.CurrencyIds),
                    TagJson = input.Tags.Any() ? JsonConvert.SerializeObject(input.Tags) : null,
                    StartDate = StartDate.DateTime,
                    EndDate = EndDate.DateTime,
                });
                var result = _partitionService.Query<BetListReport>(StartDate, EndDate, "Admin_TpBetRecordExtend_Get", parameters);
                return result.OrderByDescending(i => i.SettledTime).ToList();
            });

            //Export
            if (input.PageNumber == -1)
            {
                return data;
            }

            //Pagination
            totalPages = (int)Math.Ceiling(data.Count / (float)input.PageSize);
            rowCount = data.Count;
            var index = (Math.Abs(input.PageNumber) - 1) * Math.Abs(input.PageSize);
            var count = index + input.PageSize <= data.Count ? input.PageSize : data.Count - index;
            return data.GetRange(index, count);
        }
    }
}
