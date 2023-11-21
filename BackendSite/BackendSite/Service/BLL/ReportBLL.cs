using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.GameStatisticalReport;
using BackendSite.Service.Model.Mpa.WinLoss;
using BackendSite.Service.Model.Option;
using BackendSite.Service.Model.Report;

using Newtonsoft.Json;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using BackendSite.Service.Model.ApiModel;
using Namotion.Reflection;

namespace BackendSite.Service.BLL
{
    public class ReportBLL
    {
        #region Fields

        private readonly ReportService reportService;
        private readonly Common _common;
        private readonly BetListService _betListBll;
        private readonly IEnumerable<TpGameTypeOption> _tpGameTypeList;
        private readonly IMpaCommonService _mpaCommonService;
        private readonly ICacheHelper _redisHelper;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly IMpaCommonService _mapMpaCommonService;

        #endregion

        #region Constructors

        public ReportBLL(IServiceProvider provider)
        {
            reportService = provider.GetRequiredService<ReportService>();
            _common = provider.GetRequiredService<Common>();
            _betListBll = provider.GetRequiredService<BetListService>();
            _tpGameTypeList = provider.GetRequiredService<IEnumerable<TpGameTypeOption>>();
            _mpaCommonService = provider.GetRequiredService<IMpaCommonService>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
            _mapMpaCommonService = provider.GetRequiredService<IMpaCommonService>();
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
            var baseDate = DateTime.ParseExact($"{date} 00:00", "yyyy-MM-dd HH:mm", null).ToOffset(timeZone);
            var startDate = baseDate.ToUniversalTime().DateTime;
            var endDate = baseDate.ToUniversalTime().AddDays(1).AddMilliseconds(-1).DateTime;

            var betList = _betListBll.GetTpBetRecord(custId, startDate, endDate);
            var result = betList
                .Where(i => i.GameType == gameId && (i.SettlementType == 1 || i.SettlementType == 3))
                .Join(_tpGameTypeList,
                  tpBetRecord => tpBetRecord.GameType,
                  tpGameType => tpGameType.Type,
                  (tpBetRecord, tpGameType) => new TpBetRecordDataModel
                  {
                      TpId = tpBetRecord.TpId,
                      TpName = _common.GetTpName(tpBetRecord.TpId),
                      TransId = tpBetRecord.TransId,
                      CategoryId = tpGameType.CategoryId,
                      CategoryName = tpGameType.CategoryName,
                      GameId = tpBetRecord.GameType,
                      GameName = tpGameType.TypeName,
                      CustId = tpBetRecord.CustId,
                      UserName = tpBetRecord.UserName,
                      CurrencyId = tpBetRecord.CurrencyId,
                      Code = _common.GetCurrencyIsoCode(tpBetRecord.CurrencyId),
                      SiteId = tpBetRecord.SiteId,
                      SiteName = _common.GetSiteName(tpBetRecord.SiteId),
                      TpUserName = tpBetRecord.TpUserName,
                      TransTime = tpBetRecord.TransTime.ToOffset(baseDate.Offset).DateTime,
                      SettledTime = tpBetRecord.SettledTime.ToOffset(baseDate.Offset).DateTime,
                      Stake = tpBetRecord.Stake,
                      ValidBet = tpBetRecord.ValidBet ?? 0,
                      Revenue = tpBetRecord.Revenue ?? 0,
                      WinLostAmount = tpBetRecord.WinlostAmount,
                      SettlementType = tpBetRecord.SettlementType,
                      TicketStatus = tpBetRecord.TicketStatus,
                      OriginalState = tpBetRecord.OriginalState,
                      TicketKey = (int)tpBetRecord.TicketKey,
                      BonusId = tpBetRecord.BonusId,
                      CreateTime = tpBetRecord.CreateTime.ToOffset(baseDate.Offset).DateTime
                  });
            return result;
        }

        public IEnumerable<TpFailRecordReport> GetTpFailRecordReport(GetTpFailRecordReportInput input,
            out int totalPages,
            out int rowCount)
            => reportService.GetTpFailRecordReport(input, out totalPages, out rowCount);

        public IEnumerable<TpFailRecordReport> GetTpFailRecordReportForExport(GetTpFailRecordReportInput input)
            => reportService.GetTpFailRecordReportForExport(input);

        public IEnumerable<RewardBonusReportModel> GetRewardBonusReport(GetListInput input,
            out int totalPages,
            out int rowCount)
            => reportService.GetRewardBonusReport(input, out totalPages, out rowCount);

        public IEnumerable<RewardBonusReportModel> GetRewardBonusReportExport(GetListInput input)
            => reportService.GetRewardBonusReportExport(input);

        private decimal GetBaseCurrencyTotalAmount(IList<MpaRateDto> rateList,
            IEnumerable<IPaymentReportDetail> source,
            int baseCurrencyId)
        {
            var result = source.Sum(x =>
            {
                var rate = _mpaCommonService.MapperExchangeRate(rateList, x.FlowEndTime.UtcDateTime, (int)x.CurrencyId,
                    baseCurrencyId);
                var amount = x.Amount * rate;

                return amount;
            });

            return result;
        }

        private static DateTimeOffset GetEndDateForExchangeRate(DateTimeOffset endTime)
        {
            var now = DateTimeOffset.Now.ToUniversalTime();
            // 當月不需做處理
            if (endTime.ToString("yyyyMM") == now.ToString("yyyyMM"))
            {
                return endTime;
            }

            if (endTime > now)
            {
                return now;
            }

            // 該月最後一天
            var lastDate = new DateTime(endTime.Year, endTime.Month, DateTime.DaysInMonth(endTime.Year, endTime.Month));

            return new DateTimeOffset(lastDate, TimeSpan.Zero);
        }

        public async Task<IEnumerable<DepositPaymentReportSummary>> GetDepositPaymentReportAsync(
            GetPaymentReportParams input, TimeSpan offset, PaymentReportGroupByEnum groupBy)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)input).ConvertToMultiSiteParam(userServerInfo);
            var detailList = reportService.GetPaymentReportDepositDetail(input, JsonConvert.SerializeObject(multiSiteRequest))
                .ToList();

            if (groupBy is PaymentReportGroupByEnum.Date or PaymentReportGroupByEnum.Month)
            {
                detailList.ForEach(x =>
                {
                    x.CreateTime = x.CreateTime.ToOffset(offset);
                    x.FlowEndTime = x.FlowEndTime.ToOffset(offset);
                });
            }

            var rateList = (await _mpaCommonService.GetMpaExchangeRate(input.StartTime, GetEndDateForExchangeRate(input.EndTime))).ToList();
            var gData = detailList.GroupBy(x =>
            {
                return groupBy switch
                {
                    PaymentReportGroupByEnum.Currency => x.CurrencyId.ToString(),
                    PaymentReportGroupByEnum.Date => x.FlowEndTime.ToOffset(offset).ToString("yyyy-MM-dd"),
                    PaymentReportGroupByEnum.Month => x.FlowEndTime.ToOffset(offset).ToString("yyyy-MM"),
                    PaymentReportGroupByEnum.Gateway => x.PayName,
                    PaymentReportGroupByEnum.Type => x.DepositType.ToString(),
                    _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy,
                        $"Group by {groupBy.ToString()} not support")
                };
            });

            var result = gData.Select(g =>
            {
                var count = g.Count();
                var details = g
                    .GroupBy(x => new { x.CurrencyId, x.SiteName })
                    .Select(gg =>
                    {
                        var gCount = gg.Count();
                        var gTotalDepositAmount = gg.Sum(y => y.Amount);

                        return new DepositPaymentReportDetail
                        {
                            Key = gg.Key.CurrencyId.ToString(),
                            SiteName = gg.Key.SiteName,
                            TotalDepositCount = gCount,
                            AverageDepositAmount = decimal.Round(gTotalDepositAmount / gCount, 4),
                            FirstDepositCount = gg.Count(x => x.FirstDeposit),
                            TotalDepositAmount = decimal.Round(gTotalDepositAmount, 4),
                            AverageProcessingTime =
                                TimeSpan.FromMilliseconds(
                                        gg.Average(x => (x.FlowEndTime - x.CreateTime).TotalMilliseconds))
                                    .ToString(@"d\.hh\:mm\:ss")
                        };
                    });

                var totalDepositAmount =
                    GetBaseCurrencyTotalAmount(rateList, g.Select(x => x), input.BaseCurrencyId);

                return new DepositPaymentReportSummary
                {
                    Key = g.Key,
                    Children = details,
                    TotalDepositCount = count,
                    AverageDepositAmount = decimal.Round(totalDepositAmount / count, 4),
                    FirstDepositCount = g.Count(x => x.FirstDeposit),
                    TotalDepositAmount = decimal.Round(totalDepositAmount, 4),
                    AverageProcessingTime =
                        TimeSpan.FromMilliseconds(g.Average(x => (x.FlowEndTime - x.CreateTime).TotalMilliseconds))
                            .ToString(@"d\.hh\:mm\:ss")
                };
            });

            return result;
        }

        public async Task<IEnumerable<WithdrawalPaymentReportSummary>> GetWithdrawalPaymentReportAsync(
            GetPaymentReportParams input, TimeSpan offset, PaymentReportGroupByEnum groupBy)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)input).ConvertToMultiSiteParam(userServerInfo);
            var detailList = reportService.GetPaymentReportWithdrawalDetail(input, JsonConvert.SerializeObject(multiSiteRequest))
                .ToList();

            if (groupBy is PaymentReportGroupByEnum.Date or PaymentReportGroupByEnum.Month)
            {
                detailList.ForEach(x =>
                {
                    x.CreateTime = x.CreateTime.ToOffset(offset);
                    x.FlowEndTime = x.FlowEndTime.ToOffset(offset);
                });
            }

            var rateList = (await _mpaCommonService.GetMpaExchangeRate(input.StartTime, input.EndTime)).ToList();
            var gData = detailList.GroupBy(x =>
            {
                return groupBy switch
                {
                    PaymentReportGroupByEnum.Currency => x.CurrencyId.ToString(),
                    PaymentReportGroupByEnum.Date => x.FlowEndTime.ToOffset(offset).ToString("yyyy-MM-dd"),
                    PaymentReportGroupByEnum.Month => x.FlowEndTime.ToOffset(offset).ToString("yyyy-MM"),
                    PaymentReportGroupByEnum.Gateway => x.PayName,
                    PaymentReportGroupByEnum.Type => x.WithdrawalType.ToString(),
                    _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy,
                        $"Group by {groupBy.ToString()} not support")
                };
            });

            var result = gData.Select(g =>
            {
                var count = g.Count();
                var details = g
                    .GroupBy(x => new { x.CurrencyId, x.SiteName })
                    .Select(gg =>
                    {
                        var gCount = gg.Count();
                        var gTotalDepositAmount = gg.Sum(y => y.Amount);

                        return new WithdrawalPaymentReportDetail
                        {
                            Key = gg.Key.CurrencyId.ToString(),
                            SiteName = gg.Key.SiteName,
                            TotalWithdrawalCount = gCount,
                            AverageWithdrawalAmount = decimal.Round(gTotalDepositAmount / gCount, 4),
                            TotalWithdrawalAmount = decimal.Round(gTotalDepositAmount, 4),
                            AverageProcessingTime =
                                TimeSpan.FromMilliseconds(
                                        gg.Average(x => (x.FlowEndTime - x.CreateTime).TotalMilliseconds))
                                    .ToString(@"d\.hh\:mm\:ss")
                        };
                    });

                var totalDepositAmount =
                    GetBaseCurrencyTotalAmount(rateList, g.Select(x => x), input.BaseCurrencyId);

                return new WithdrawalPaymentReportSummary
                {
                    Key = g.Key,
                    Children = details,
                    TotalWithdrawalCount = count,
                    AverageWithdrawalAmount = decimal.Round(totalDepositAmount / count, 4),
                    TotalWithdrawalAmount = decimal.Round(totalDepositAmount, 4),
                    AverageProcessingTime =
                        TimeSpan.FromMilliseconds(g.Average(x => (x.FlowEndTime - x.CreateTime).TotalMilliseconds))
                            .ToString(@"d\.hh\:mm\:ss")
                };
            });

            return result;
        }

        public static byte[] ExportPaymentReport<TSummary, TDetail>(IEnumerable<TSummary> data)
            where TSummary : class, IPaymentReportSummary<TDetail>
            where TDetail : class
        {
            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Report");

            sheet.CreateHeaders<TDetail>(freeze: true, index: 1);

            var gCellStyle = workbook.CreateCellStyle();
            gCellStyle.BorderBottom = BorderStyle.Medium;
            gCellStyle.FillForegroundColor = IndexedColors.LightCornflowerBlue.Index;
            gCellStyle.FillPattern = FillPattern.SolidForeground;

            var subCellStyle = workbook.CreateCellStyle();
            subCellStyle.BorderBottom = BorderStyle.Thin;
            subCellStyle.FillForegroundColor = IndexedColors.LightTurquoise.Index;
            subCellStyle.FillPattern = FillPattern.SolidForeground;

            var skipSProps = new[] { "Children" };
            var dProps = typeof(TDetail).GetProperties();
            var sProps = typeof(TSummary).GetProperties()
                .Where(x => !skipSProps.Contains(x.Name));
            var index = 1;
            foreach (var gData in data)
            {
                var groupRow = sheet.CreateRow(index);

                var idx = 0;
                foreach (var prop in sProps)
                {
                    var groupCell = groupRow.CreateCell(idx);
                    var val = typeof(TSummary).GetProperty(prop.Name)?.GetValue(gData);
                    groupRow.CreateCell(idx).SetCellValue(val?.ToString());
                    groupCell.CellStyle = gCellStyle;

                    idx++;
                }

                index++;

                var children = typeof(TSummary).GetProperty("Children")?.GetValue(gData) as IEnumerable<TDetail>;
                foreach (var detail in children)
                {
                    var row = sheet.CreateRow(index);

                    var i = 0;
                    foreach (var prop in dProps)
                    {
                        var subGroupCell = row.CreateCell(i);
                        var val = typeof(TDetail).GetProperty(prop.Name)?.GetValue(detail);
                        row.CreateCell(i).SetCellValue(val?.ToString());

                        if (i == 0)
                            subGroupCell.CellStyle = subCellStyle;

                        i++;
                    }
                    index++;
                }
            }

            sheet.AutoSizeColumn(0);

            using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            var result = memoryStream.ToArray();

            return result;
        }

        #region Game Statistical Report

        public IEnumerable<GameStatisticalByCurrencyDto> GetGameStatisticalReport(GetListInputModel getListInput)
        {
            var key = $"GameStatisticalReport:{getListInput.SiteId}:{getListInput.StartDate}:{getListInput.EndDate}";
            var source = _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(15),
                () => GetGameStatisticalSource(getListInput));
            return ProcessGameStatisticalSource(FilterSorce(getListInput.SiteId, source));
        }

        public Dictionary<string, IEnumerable<GameStatisticalDao>> ConvertToExport(IEnumerable<GameStatisticalByCurrencyDto> data)
        {
            var result = new Dictionary<string, IEnumerable<GameStatisticalDao>>();
            foreach (var d in data)
                result.Add(d.CurrencyName, d.CategoryDetail.SelectMany(y => y.GameDetail));

            return result;
        }

        private IEnumerable<GameStatisticalDao> GetGameStatisticalSource(GetListInputModel getListInput)
            => reportService.GetGameStatisticalReport(getListInput);

        private IEnumerable<GameStatisticalDao> FilterSorce(int siteId, IEnumerable<GameStatisticalDao> source)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2().First(x => x.SiteId == siteId);
            return source.Where(x => userServerInfo.ICurrencyList.Contains(x.CurrencyId));
        }

        private List<GameStatisticalByCurrencyDto> ProcessGameStatisticalSource(IEnumerable<GameStatisticalDao> source)
        {
            var userServerInfos = _serverInfoBll.GetSiteListByLoginUserV2();
            var currencyList = source.Select(t => new { CurrencyId = t.CurrencyId, CurrencyName = t.CurrencyName }).Distinct()
                .ToDictionary(t => t.CurrencyId, t => t.CurrencyName).OrderBy(x => x.Key);
            var categoryList = source.Select(t => new { CategoryId = t.CategoryId, CategoryName = t.CategoryName }).Distinct()
                .ToDictionary(t => t.CategoryId, t => t.CategoryName).OrderBy(x => x.Key);

            var result = new List<GameStatisticalByCurrencyDto>();

            foreach (var currency in currencyList)
            {
                var currencySummaryItem = new GameStatisticalByCurrencyDto() { CurrencyId = currency.Key, CurrencyName = currency.Value };
                var categorySummaryList = new List<GameStatisticalByCategoryDto>();
                foreach (var category in categoryList)
                {
                    var categorySummaryItem = new GameStatisticalByCategoryDto() { CategoryId = category.Key, CategoryName = category.Value };
                    var detail = source.Where(x => x.CurrencyId == currency.Key && x.CategoryId == category.Key).OrderByDescending(x => x.ValidBetSummary);

                    if (detail.Count() == 0) continue;
                    categorySummaryItem.GameDetail = detail;
                    categorySummaryList.Add(categorySummaryItem);
                }
                currencySummaryItem.CategoryDetail = categorySummaryList;
                result.Add(currencySummaryItem);
            }
            return result;
        }

        #endregion

        #region Confiscated Summary Report

        public IEnumerable<ConfiscatedSummaryReportDetail> GetConfiscatedSummaryReportList(
            GetConfiscatedSummaryReportRequest request,
            int pageNumber,
            out IPageInfo pageInfo)
        {
            var dataList = GetConfiscatedSummaryReport(request, pageNumber , out pageInfo).ToList();
            
            if (request.TimeZone is not null)
            {
                TimeSpan.TryParseExact(request.TimeZone, @"\+hh\:mm", null, out var offset);
                foreach (var data in dataList)
                {
                    data.RegistrationDate += offset;
                    data.SuspendDate += offset;
                }
            }
            return dataList;
        }
        
        private IEnumerable<ConfiscatedSummaryReportDetail> GetConfiscatedSummaryReport(
            GetConfiscatedSummaryReportRequest request,
            int pageNumber,
            out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var key = $"ConfiscatedSummaryReport:{JsonConvert.SerializeObject(request.SiteIds)}:" +
                      $"{JsonConvert.SerializeObject(request.CurrencyIds)}:" +
                      $"{request.StartDate?.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{request.EndDate?.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{request.UserName}:" +
                      $"{request.BaseCurrencyId}";
            
            var cacheTime = TimeSpan.FromMinutes(2);
            if (_redisHelper.KeyExists(key))
            {
                //Export
                if (pageNumber == -1)
                {
                    return _redisHelper.GetListByRange<ConfiscatedSummaryReportDetail>(key, 0, pageNumber);
                }

                //Redis Pagination
                var total = (int) _redisHelper.GetListLength(key);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, pageNumber, request.PageSize);
                return _redisHelper.GetListByRange<ConfiscatedSummaryReportDetail>(key, startIndex, endIndex);
            }
            
            //Search range table
            var data = GetConfiscatedSummaryReportDetail(request).ToList();
            
            AddCache(data, key, cacheTime);

            //Export
            if (pageNumber == -1 || data.Count == 0)
            {
                return data;
            }

            var (index, count) = pageInfo.ListPagination(data.Count, pageNumber, request.PageSize);
            return data.GetRange(index, count);
        }
        
        public ConfiscatedSummary GetConfiscatedSummary(GetConfiscatedSummaryReportSummaryRequest request)
        {
            var getRequest = new GetConfiscatedSummaryReportRequest
            {
                SiteIds = request.SiteIds,
                CurrencyIds = request.CurrencyIds,
                BaseCurrencyId = request.BaseCurrencyId,
                UserName = request.UserName
            };
            var data = GetConfiscatedSummaryReport(getRequest, -1 , out _ ).ToList();

            var result = new ConfiscatedSummary();
            if (data.Any())
            {
                result = new ConfiscatedSummary
                {
                    TotalUser = data.Count,
                    TotalSeamlessWallet = data.Sum(x=>x.SeamlessWallet),
                    TotalDeposit = data.Sum(x=>x.TotalDeposit),
                    TotalWithdrawal = data.Sum(x=>x.TotalWithdrawal),
                    TotalNetDeposit = data.Sum(x=>x.NetDeposit),
                    TotalNetConfiscatedCash = data.Sum(x=>x.NetConfiscatedCash)
                };
            }
            
            return result;
        }
        
        private IEnumerable<ConfiscatedSummaryReportDetail> GetConfiscatedSummaryReportDetail(GetConfiscatedSummaryReportRequest request)
        {
            var dataList = reportService.GetConfiscatedSummaryReport(request).ToList();

            //抓3天內 避免MPA匯率未更新
            var rateStartTime = DateTime.UtcNow.AddDays(-3);
            var rateEndTime = DateTime.UtcNow;
            var rateList = _mapMpaCommonService.GetMpaExchangeRate(rateStartTime, rateEndTime).Result.ToList();
            
            foreach (var data in dataList)
            {
                var rate = rateList.Where(x=>x.CurrencyId == data.CurrencyId && x.TargetCurrencyId == request.BaseCurrencyId)
                    .MaxBy(item => item.RateDate).Rate;
                data.TotalDeposit *= rate;
                data.TotalWithdrawal *= rate;
                data.SeamlessWallet *= rate;
            }
            
            return dataList;
        }
        
        private void AddCache(IEnumerable<object> list, string masterKey, TimeSpan cacheTime)
        {
            _ = Task.Run(() =>
            {
                _redisHelper.InsertList(masterKey, cacheTime, list);
            });
        }
        
        #endregion
    }
}
