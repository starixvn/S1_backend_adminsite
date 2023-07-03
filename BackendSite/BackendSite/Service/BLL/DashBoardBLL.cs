using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class DashBoardBLL
    {
        private readonly DashBoardService _dashBoardService;
        private readonly ICacheHelper _redisHelper;
        private readonly ReportBLL _reportBll;
        private readonly TpMaintainBLL _tpMaintainBll;

        public DashBoardBLL(IServiceProvider provider)
        {
            _dashBoardService = provider.GetRequiredService<DashBoardService>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _tpMaintainBll = provider.GetRequiredService<TpMaintainBLL>();
        }

        #region DashBoardV1

        public DashBoardData GetDashBoardData(int siteId, int currencyId, int baseCurrencyId, List<int> canSearchSite, int timeZone)
        {
            var type = new List<int>() { 0, 1, 2, 3, 4 };
            var localTime = DateTime.UtcNow.AddHours(timeZone);
            var localDate = new DateTimeOffset(Convert.ToDateTime(localTime.ToShortDateString()), TimeSpan.FromHours(timeZone));
            var startTime = new DateTimeOffset(localDate.ToUniversalTime().DateTime, TimeSpan.Zero);
            var endTime = new DateTimeOffset(localDate.ToUniversalTime().DateTime, TimeSpan.Zero).AddHours(24).AddMilliseconds(-1);
            var siteList = siteId == 0 ? JsonConvert.SerializeObject(canSearchSite) : JsonConvert.SerializeObject(new List<int> { siteId });

            var key = $"DashBoard:{siteList}:{currencyId}:{baseCurrencyId}";
            return _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(15),
                () => GetDashBoardData(siteId, currencyId, baseCurrencyId, canSearchSite, startTime, endTime, type));
        }

        private DashBoardData GetDashBoardData(int siteId, int currencyId, int baseCurrencyId, List<int> canSearchSite,
            DateTimeOffset startTime, DateTimeOffset endTime, List<int> type)
        {
            var result = _dashBoardService.GetDashBoardData(siteId, currencyId, startTime.DateTime, endTime.DateTime, canSearchSite);
            result.TodayData = new List<DashBoardDetailData>
            {
                _dashBoardService.GetDashBoardTodayData(siteId, currencyId, baseCurrencyId, startTime.DateTime, endTime.DateTime,
                    canSearchSite)
            };

            type.ForEach(item =>
            {
                var data = _dashBoardService.GetDashBoardTodayExtraData(siteId, currencyId, baseCurrencyId, startTime.DateTime,
                    endTime.DateTime, canSearchSite, item);
                if (data != null)
                {
                    result.TodayData.Add(data);
                }
            });

            return result;
        }

        #endregion

        #region DashBoardMember

        public DashBoardMemBerData GetDashBoardMemberData(int siteId, IEnumerable<int> currencyIds, int timeZone)
        {
            var result = new DashBoardMemBerData
            {
                MemBerDataByCurrency = new List<DashBoardMemBerDataByCurrency>(),
                RegistrationPlatform = new List<RegistrationPlatformDetailData>()
            };

            // // MemBerDataByCurrency
            foreach (var currencyId in currencyIds)
            {
                var key = $"DashBoardMember:{siteId}:{currencyId}:{timeZone}";
                var currencyData = _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(15),
                    () => GetDashBoardMemberDataByCurrency(siteId, currencyId, timeZone));
                currencyData.CurrencyId = currencyId;
                currencyData.Currency = Enum.GetName(typeof(CurrencyEnum), currencyId);
                result.MemBerDataByCurrency.Add(currencyData);
            }

            // RegistrationPlatform
            var key2 = $"DashBoardMember:{siteId}:{JsonConvert.SerializeObject(currencyIds)}";
            result.RegistrationPlatform = _redisHelper.TryGetOrCreate(key2, TimeSpan.FromMinutes(15),
                () => GetDashBoardMemberPlatformData(siteId, currencyIds, timeZone));

            return result;
        }

        private DashBoardMemBerDataByCurrency GetDashBoardMemberDataByCurrency(int siteId, int currencyId, int timeZone)
        {
            var result = new DashBoardMemBerDataByCurrency
            {
                PerMonthData = new List<DashBoardMemBerDataByMonth>(),
                TodayData = new DashBoardMemBerDetailData(),
                YesterdayData = new DashBoardMemBerDetailData(),
                MonthToDateData = new DashBoardMemBerDetailData(),
                SamePeriodLastMonthData = new DashBoardMemBerDetailData()
            };

            //PerMonthData
            var startTime = new DateTime(DateTime.UtcNow.Year, 1, 1);
            var timeZoneString = timeZone >= 0 ? $"+{timeZone}:00" : $"-{Math.Abs(timeZone)}:00";
            result.PerMonthData = _dashBoardService.GetDashBoardMemberDataByMonth(siteId, currencyId, startTime, timeZoneString).ToList();

            //TodayData
            var localTime = DateTime.UtcNow.AddHours(timeZone);
            var todayStartTime = localTime.Date;
            var todayEndTime = todayStartTime.AddDays(1).AddMilliseconds(-1);
            var todayData = _dashBoardService.GetDashBoardMemberData(siteId, currencyId, todayStartTime, todayEndTime);
            result.TodayData = new DashBoardMemBerDetailData
            {
                NewMember = todayData.NewMember,
                UniqueLogin = todayData.UniqueLogin
            };

            //YesterdayData
            var yesterdayStartTime = todayStartTime.AddDays(-1);
            var yesterdayEndTime = yesterdayStartTime.AddDays(1).AddMilliseconds(-1);
            var yesterdayData = _dashBoardService.GetDashBoardMemberData(siteId, currencyId, yesterdayStartTime, yesterdayEndTime);
            result.YesterdayData = new DashBoardMemBerDetailData
            {
                NewMember = yesterdayData.NewMember,
                UniqueLogin = yesterdayData.UniqueLogin
            };

            //MonthToDateData
            var monthToDateStartTime = new DateTime(localTime.Year, localTime.Month, 1);
            var monthToDateData = _dashBoardService.GetDashBoardMemberData(siteId, currencyId, monthToDateStartTime, localTime);
            result.MonthToDateData = new DashBoardMemBerDetailData
            {
                NewMember = monthToDateData.NewMember,
                UniqueLogin = monthToDateData.UniqueLogin
            };

            //LastMonthData
            var lastMonthStartTime = monthToDateStartTime.AddMonths(-1);
            var lastMonthEndTime = localTime.AddMonths(-1);
            var samePeriodLastMonthData = _dashBoardService.GetDashBoardMemberData(siteId, currencyId, lastMonthStartTime, lastMonthEndTime);
            result.SamePeriodLastMonthData = new DashBoardMemBerDetailData
            {
                NewMember = samePeriodLastMonthData.NewMember,
                UniqueLogin = samePeriodLastMonthData.UniqueLogin
            };

            return result;
        }

        private List<RegistrationPlatformDetailData> GetDashBoardMemberPlatformData(int siteId, IEnumerable<int> currencyIds, int timeZone)
        {
            var startTime = new DateTime(DateTime.UtcNow.Year, 1, 1);
            var timeZoneString = timeZone >= 0 ? $"+{timeZone}:00" : $"-{Math.Abs(timeZone)}:00";
            var result = _dashBoardService.GetDashBoardMemberPlatformData(siteId, currencyIds, startTime, timeZoneString).ToList();

            return result;
        }

        public IEnumerable<NewMemberList> GetNewMemberList(
            int siteId,
            IEnumerable<int> currencyIds,
            DateTime startTime,
            DateTime endTime,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            var result = _dashBoardService.GetDashBoardNewMemberList(
                siteId,
                currencyIds,
                startTime,
                endTime,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);

            return result;
        }

        #endregion

        #region DashBoardFinancial

        public DashBoardFinancialData GetDashBoardFinancialData(int siteId, IEnumerable<int> currencyIds, int baseCurrencyId, int timeZone)
        {
            var result = new DashBoardFinancialData
            {
                FinancialDataByCurrency = new List<DashBoardFinancialDataByCurrency>()
            };

            var data = _reportBll.GetCurrency();
            decimal baseRate = 0;
            if (baseCurrencyId != 0)
            {
                baseRate = data.FirstOrDefault(x => x.CurrencyId == baseCurrencyId).ActualRate;
            }

            foreach (var currencyId in currencyIds)
            {
                var originRate = data.FirstOrDefault(x => x.CurrencyId == currencyId).ActualRate;
                var key = $"DashBoardFinancial:{siteId}:{currencyId}:{baseCurrencyId}:{timeZone}";
                var currencyData = _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(15),
                    () => GetDashBoardFinancialDataByCurrency(siteId, currencyId, originRate, baseRate, timeZone));
                currencyData.CurrencyId = currencyId;
                currencyData.Currency = Enum.GetName(typeof(CurrencyEnum), currencyId);
                result.FinancialDataByCurrency.Add(currencyData);
            }
            return result;
        }

        private DashBoardFinancialDataByCurrency GetDashBoardFinancialDataByCurrency(
            int siteId, int currencyId, decimal originRate, decimal baseRate, int timeZone)
        {
            var result = new DashBoardFinancialDataByCurrency
            {
                PerMonthData = new DashBoardFinancialDataByMonth(),
                TodayData = new DashBoardFinancialDetailData(),
                YesterdayData = new DashBoardFinancialDetailData(),
                MonthToDateData = new DashBoardFinancialDetailData(),
                SamePeriodLastMonthData = new DashBoardFinancialDetailData()
            };

            //PerMonthData

            var startTime = new DateTime(DateTime.UtcNow.Year, 1, 1);
            var timeZoneString = timeZone >= 0 ? $"+{timeZone}:00" : $"-{Math.Abs(timeZone)}:00";
            result.PerMonthData.OriginalCurrencyData = _dashBoardService.GetDashBoardFinancialDataByMonth(siteId, currencyId, startTime, timeZoneString);

            if (baseRate != 0 && originRate != baseRate)
            {

                var baseCurrencyData = result.PerMonthData.OriginalCurrencyData;

                foreach (var data in baseCurrencyData)
                {
                    data.DepositAmount = ChangeRate(data.DepositAmount, originRate, baseRate);
                    data.WithdrawalAmount = ChangeRate(data.WithdrawalAmount, originRate, baseRate);
                    data.BonusClaimAmount = ChangeRate(data.BonusClaimAmount, originRate, baseRate);
                    data.NetCashAmount = ChangeRate(data.NetCashAmount, originRate, baseRate);
                }
                result.PerMonthData.BaseCurrencyData = baseCurrencyData;
            }
            else
            {
                result.PerMonthData.BaseCurrencyData = result.PerMonthData.OriginalCurrencyData;
            }

            //TodayData
            var localTime = DateTime.UtcNow.AddHours(timeZone);
            var todayStartTime = localTime.Date;
            var todayEndTime = todayStartTime.AddDays(1).AddMilliseconds(-1);
            var todayData = _dashBoardService.GetDashBoardFinancialData(siteId, currencyId, todayStartTime, todayEndTime);
            result.TodayData = new DashBoardFinancialDetailData
            {
                DepositCount = todayData.DepositCount,
                DepositAmount = todayData.DepositAmount,
                WithdrawalAmount = todayData.WithdrawalAmount,
                BonusClaimAmount = todayData.BonusClaimAmount,
                NetCashAmount = todayData.NetCashAmount
            };

            //YesterdayData
            var yesterdayStartTime = todayStartTime.AddDays(-1);
            var yesterdayEndTime = yesterdayStartTime.AddDays(1).AddMilliseconds(-1);
            var yesterdayData = _dashBoardService.GetDashBoardFinancialData(siteId, currencyId, yesterdayStartTime, yesterdayEndTime);
            result.YesterdayData = new DashBoardFinancialDetailData
            {
                DepositCount = yesterdayData.DepositCount,
                DepositAmount = yesterdayData.DepositAmount,
                WithdrawalAmount = yesterdayData.WithdrawalAmount,
                BonusClaimAmount = yesterdayData.BonusClaimAmount,
                NetCashAmount = yesterdayData.NetCashAmount
            };

            //MonthToDateData
            var monthToDateStartTime = new DateTime(localTime.Year, localTime.Month, 1);
            var monthToDateData = _dashBoardService.GetDashBoardFinancialData(siteId, currencyId, monthToDateStartTime, localTime);
            result.MonthToDateData = new DashBoardFinancialDetailData
            {
                DepositCount = monthToDateData.DepositCount,
                DepositAmount = monthToDateData.DepositAmount,
                WithdrawalAmount = monthToDateData.WithdrawalAmount,
                BonusClaimAmount = monthToDateData.BonusClaimAmount,
                NetCashAmount = monthToDateData.NetCashAmount
            };

            //LastMonthData
            var lastMonthStartTime = monthToDateStartTime.AddMonths(-1);
            var lastMonthEndTime = localTime.AddMonths(-1);
            var samePeriodLastMonthData = _dashBoardService.GetDashBoardFinancialData(siteId, currencyId, lastMonthStartTime, lastMonthEndTime);
            result.SamePeriodLastMonthData = new DashBoardFinancialDetailData
            {
                DepositCount = samePeriodLastMonthData.DepositCount,
                DepositAmount = samePeriodLastMonthData.DepositAmount,
                WithdrawalAmount = samePeriodLastMonthData.WithdrawalAmount,
                BonusClaimAmount = samePeriodLastMonthData.BonusClaimAmount,
                NetCashAmount = samePeriodLastMonthData.NetCashAmount
            };

            //轉換幣值匯率
            if (baseRate != 0 && originRate != baseRate)
            {
                result.TodayData.DepositAmount = ChangeRate(result.TodayData.DepositAmount, originRate, baseRate);
                result.TodayData.WithdrawalAmount = ChangeRate(result.TodayData.WithdrawalAmount, originRate, baseRate);
                result.TodayData.BonusClaimAmount = ChangeRate(result.TodayData.BonusClaimAmount, originRate, baseRate);
                result.TodayData.NetCashAmount = ChangeRate(result.TodayData.NetCashAmount, originRate, baseRate);

                result.YesterdayData.DepositAmount = ChangeRate(result.YesterdayData.DepositAmount, originRate, baseRate);
                result.YesterdayData.WithdrawalAmount = ChangeRate(result.YesterdayData.WithdrawalAmount, originRate, baseRate);
                result.YesterdayData.BonusClaimAmount = ChangeRate(result.YesterdayData.BonusClaimAmount, originRate, baseRate);
                result.YesterdayData.NetCashAmount = ChangeRate(result.YesterdayData.NetCashAmount, originRate, baseRate);

                result.MonthToDateData.DepositAmount = ChangeRate(result.MonthToDateData.DepositAmount, originRate, baseRate);
                result.MonthToDateData.WithdrawalAmount = ChangeRate(result.MonthToDateData.WithdrawalAmount, originRate, baseRate);
                result.MonthToDateData.BonusClaimAmount = ChangeRate(result.MonthToDateData.BonusClaimAmount, originRate, baseRate);
                result.MonthToDateData.NetCashAmount = ChangeRate(result.MonthToDateData.NetCashAmount, originRate, baseRate);

                result.SamePeriodLastMonthData.DepositAmount = ChangeRate(result.SamePeriodLastMonthData.DepositAmount, originRate, baseRate);
                result.SamePeriodLastMonthData.WithdrawalAmount = ChangeRate(result.SamePeriodLastMonthData.WithdrawalAmount, originRate, baseRate);
                result.SamePeriodLastMonthData.BonusClaimAmount = ChangeRate(result.SamePeriodLastMonthData.BonusClaimAmount, originRate, baseRate);
                result.SamePeriodLastMonthData.NetCashAmount = ChangeRate(result.SamePeriodLastMonthData.NetCashAmount, originRate, baseRate);
            }

            return result;
        }

        public IEnumerable<TopDepositorsList> GetTopDepositorsList(
            int siteId,
            IEnumerable<int> currencyIds,
            int baseCurrencyId,
            DateTime startTime,
            DateTime endTime,
            DashboardOrderByPropertyEnum orderBy,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            var result = _dashBoardService.GetTopDepositorsList(
                siteId,
                currencyIds,
                baseCurrencyId,
                startTime,
                endTime,
                orderBy,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);

            return result;
        }

        public IEnumerable<TopWithdrawalList> GetTopWithdrawalList(
            int siteId,
            IEnumerable<int> currencyIds,
            int baseCurrencyId,
            DateTime startTime,
            DateTime endTime,
            DashboardOrderByPropertyEnum orderBy,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            var result = _dashBoardService.GetTopWithdrawalList(
                siteId,
                currencyIds,
                baseCurrencyId,
                startTime,
                endTime,
                orderBy,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);

            return result;
        }

        public IEnumerable<TopBonusClaimedList> GetTopBonusClaimedList(
            int siteId,
            IEnumerable<int> currencyIds,
            int baseCurrencyId,
            DateTime startTime,
            DateTime endTime,
            DashboardOrderByPropertyEnum orderBy,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            var result = _dashBoardService.GetTopBonusClaimedList(
                siteId,
                currencyIds,
                baseCurrencyId,
                startTime,
                endTime,
                orderBy,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);

            return result;
        }

        #endregion

        #region DashboardGame

        public DashBoardGameDto GetGame(IEnumerable<int> siteIds,
                     IEnumerable<int> currencyIds,
                     int baseCurrencyId,
                     IEnumerable<int> tpIds,
                     IEnumerable<int> categoryIds,
                     IEnumerable<int> gameTypes,
                     int timeZone)
        {
            var key = $"DashBoardGame_PieAndLineChart:{JsonConvert.SerializeObject(siteIds)}" +
               $":{JsonConvert.SerializeObject(currencyIds)}" +
               $":{baseCurrencyId}" +
               $":{JsonConvert.SerializeObject(tpIds)}" +
               $":{JsonConvert.SerializeObject(categoryIds)}" +
               $":{JsonConvert.SerializeObject(gameTypes)}" +
               $":{timeZone}";
            return _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(30),
                () => GetGameBySource(siteIds, currencyIds, baseCurrencyId, tpIds, categoryIds, gameTypes, timeZone));
        }

        public DashBoardGameDto GetGameBySource(IEnumerable<int> siteIds,
                         IEnumerable<int> currencyIds,
                         int baseCurrencyId,
                         IEnumerable<int> tpIds,
                         IEnumerable<int> categoryIds,
                         IEnumerable<int> gameTypes,
                         int timeZone)
        {
            var result = new DashBoardGameDto();
            var pieChart = new Dictionary<string, List<DashBoardGamePieChartDto>>();
            var lineChartPerMonthSummary = new List<DashBoardGameLineChartPerMonthDto>();
            var lineChartSummary = new List<DashBoardGameLineChartSummaryDto>();
            var collectSource = new List<DashBoardGameLineChartSourceCollectionDto>();
            var collectSourceMember = Enumerable.Empty<DashBoardGameUniqueMemberPerMonthDao>();

            //Step1:決定日期區間  

            var dateTimeNowInTimeZone = DateTime.UtcNow.AddHours(timeZone);

            //Step2:撈資料
            for (int i = 1; i <= dateTimeNowInTimeZone.Month; i++)
            {
                var dateRange = GenMonthDateRange(dateTimeNowInTimeZone.Year, i, timeZone);
                var source = GetGameSource(siteIds, currencyIds,
                    dateRange.StartDate, dateRange.EndDate, tpIds, categoryIds, gameTypes);
                var sourceMember = GetDashBoardGameUniqueMember(siteIds, currencyIds,
                  dateRange.StartDate, dateRange.EndDate, tpIds, categoryIds, gameTypes);

                var gameData = GetGameDataPerMonth(dateTimeNowInTimeZone.Year, i,
                    currencyIds, baseCurrencyId, source, sourceMember);

                CollectPieChartSummary(source, ref pieChart);
                lineChartPerMonthSummary.AddRange(gameData);

                CollectSourceForLineChartSummary(currencyIds, source, ref collectSource);
                CollectSourceMemberForLineChartSumary(sourceMember, ref collectSourceMember);
            }
            CombineLineChartSummary(currencyIds, baseCurrencyId, collectSource, collectSourceMember, ref lineChartSummary);

            result.PieChart = pieChart;
            result.LineChartPerMonth = lineChartPerMonthSummary;
            result.LineChartSummary = lineChartSummary;

            //Step3:組合資料
            return result;

        }

        public Dictionary<string, List<DashBoardGameOverAllDto>> GetGameOverall(IEnumerable<int> siteIds,
                     IEnumerable<int> currencyIds,
                     int baseCurrencyId,
                     IEnumerable<int> tpIds,
                     IEnumerable<int> categoryIds,
                     IEnumerable<int> gameTypes,
                     int timeZone)
        {
            var key = $"DashBoardGame_OverAll:{JsonConvert.SerializeObject(siteIds)}" +
                $":{JsonConvert.SerializeObject(currencyIds)}" +
                $":{baseCurrencyId}" +
                $":{JsonConvert.SerializeObject(tpIds)}" +
                $":{JsonConvert.SerializeObject(categoryIds)}" +
                $":{JsonConvert.SerializeObject(gameTypes)}" +
                $":{timeZone}";
            return _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(15),
                () => GetGameOverallBySource(siteIds, currencyIds, baseCurrencyId, tpIds, categoryIds, gameTypes, timeZone));
        }

        public Dictionary<string, List<DashBoardGameOverAllDto>> GetGameOverallBySource(IEnumerable<int> siteIds,
                     IEnumerable<int> currencyIds,
                     int baseCurrencyId,
                     IEnumerable<int> tpIds,
                     IEnumerable<int> categoryIds,
                     IEnumerable<int> gameTypes,
                     int timeZone)
        {
            var result = new Dictionary<string, List<DashBoardGameOverAllDto>>() {
                { "Today",new List<DashBoardGameOverAllDto>()},
                { "Yesterday",new List<DashBoardGameOverAllDto>()},
                { "MonthToDate",new List<DashBoardGameOverAllDto>()},
                { "SamePeriodLastMonth",new List<DashBoardGameOverAllDto>()}
            };

            //Step1:決定日期區間  

            var dateTimeNowInTimeZone = DateTime.UtcNow.AddHours(timeZone);
            var dateRange = CaculateOverallDateRange(true, dateTimeNowInTimeZone, timeZone);
            var timeZoneStr = ConvertTimeZone(timeZone);
            var sourceData = GetGameOverall(siteIds, currencyIds,
                  dateRange.StartDate, dateRange.EndDate, tpIds, categoryIds, gameTypes, timeZoneStr);
            var sourceMemberData = GetGameOverallMember(siteIds, currencyIds,
                  dateRange.StartDate, dateRange.EndDate, tpIds, categoryIds, gameTypes, timeZoneStr);
            //Step2:撈資料
            var currencyList = _reportBll.GetCurrency();

            foreach (var currencyId in currencyIds)
            {
                var currencyInfo = currencyList.FirstOrDefault(x => x.CurrencyId == currencyId);

                var todayData = CombineOverallSource(currencyInfo,
                    baseCurrencyId, currencyList,
                    sourceData.Where(x => x.CurrencyId == currencyId && x.Date == dateTimeNowInTimeZone.ToString("yyyy-MM-dd")),
                    sourceMemberData.Where(x => x.CurrencyId == currencyId && x.Date == dateTimeNowInTimeZone.ToString("yyyy-MM-dd")));
                result["Today"].Add(todayData);

                var yesterdayData = CombineOverallSource(currencyInfo,
                    baseCurrencyId, currencyList,
                    sourceData.Where(x => x.CurrencyId == currencyId && x.Date == dateTimeNowInTimeZone.AddDays(-1).ToString("yyyy-MM-dd")),
                    sourceMemberData.Where(x => x.CurrencyId == currencyId && x.Date == dateTimeNowInTimeZone.AddDays(-1).ToString("yyyy-MM-dd")));
                result["Yesterday"].Add(yesterdayData);

                var monthData = CombineOverallSource(currencyInfo,
                    baseCurrencyId, currencyList,
                    sourceData.Where(x => x.CurrencyId == currencyId),
                    sourceMemberData.Where(x => x.CurrencyId == currencyId));
                result["MonthToDate"].Add(monthData);
            }

            var dateRangeLastMonth = CaculateOverallDateRange(false, dateTimeNowInTimeZone, timeZone);
            var sourceLastMonthData = GetGameOverall(siteIds, currencyIds,
                  dateRangeLastMonth.StartDate, dateRangeLastMonth.EndDate, tpIds, categoryIds, gameTypes, timeZoneStr);
            var sourceLastMonthMemberData = GetGameOverallMember(siteIds, currencyIds,
                  dateRangeLastMonth.StartDate, dateRangeLastMonth.EndDate, tpIds, categoryIds, gameTypes, timeZoneStr);
            foreach (var currencyId in currencyIds)
            {
                var currencyInfo = currencyList.FirstOrDefault(x => x.CurrencyId == currencyId);

                var todayData = CombineOverallSource(currencyInfo,
                    baseCurrencyId, currencyList,
                    sourceLastMonthData.Where(x => x.CurrencyId == currencyId),
                    sourceLastMonthMemberData.Where(x => x.CurrencyId == currencyId));
                result["SamePeriodLastMonth"].Add(todayData);
            }

            //Step3:組合資料
            return result;
        }

        public IEnumerable<DashBoardGameTopDto> GetGameTopTwenty(
            DashboardGameTopDataTypeEnum type,
            IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            int baseCurrencyId,
            IEnumerable<int> tpIds,
            IEnumerable<int> categoryIds,
            IEnumerable<int> gameTypes,
            string orderByProperty,
            bool isDesc,
            int timeZone,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            var startTime = DateTime.UtcNow.AddHours(timeZone).Date.AddMonths(-1);
            var endTime = DateTime.UtcNow.AddHours(timeZone).Date.AddDays(1).AddMilliseconds(-1);

            var source = GetGameTopTwentyData(
                type,
                siteIds,
                currencyIds,
                baseCurrencyId,
                startTime,
                endTime,
                tpIds,
                categoryIds,
                gameTypes,
                orderByProperty,
                isDesc,
                pageNumber,
                pageSize,
                out totalPages,
                out rowCount);

            return source.Select(x => new DashBoardGameTopDto()
            {
                Type = type,
                CustId = x.CustId,
                UserName = x.UserName,
                CurrencyId = x.CurrencyId,
                CurrencyName = x.CurrencyName,
                BetCount = x.BetCount,
                BetAmount = x.BetAmount,
                BetAmountInRate = x.BetAmountInRate,
                Payout = x.Payout,
                PayoutInRate = x.PayoutInRate,
                WinLossAmount = x.WinLossAmount,
                WinLossAmountInRate = x.WinLossAmountInRate,
                Margin = x.Margin * 100
            });
        }

        #region Date Range Method

        private (DateTimeOffset StartDate, DateTimeOffset EndDate) GenMonthDateRange
            (int year, int month, int timeZone)
        {
            var firstDayOfMonth = new DateTime(year, month, 1).AddHours(timeZone * -1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).AddHours(timeZone * -1);
            return (StartDate: new DateTimeOffset(firstDayOfMonth, new TimeSpan(0, 0, 0)),
                    EndDate: new DateTimeOffset(lastDayOfMonth, new TimeSpan(0, 0, 0)));
        }

        private (DateTimeOffset StartDate, DateTimeOffset EndDate) CaculateOverallDateRange
            (bool isThisMonth, DateTime nowDate, int timeZone)
        {
            if (!isThisMonth)
                nowDate = nowDate.AddMonths(-1);
            var firstDayOfMonth = new DateTime(nowDate.Year, nowDate.Month, 1).AddHours(timeZone * -1);
            return (StartDate: new DateTimeOffset(firstDayOfMonth, new TimeSpan(0, 0, 0)),
                        EndDate: new DateTimeOffset(nowDate.AddHours(timeZone * -1), new TimeSpan(0, 0, 0)));
        }

        #endregion

        private void CollectPieChartSummary(IEnumerable<DashBoardGamePerMonthDao> sourceData,
            ref Dictionary<string, List<DashBoardGamePieChartDto>> pieChartSummary)
        {
            var result = new Dictionary<string, List<DashBoardGamePieChartDto>>();
            var pieChartData = Enumerable.Empty<int>();

            var tpProviderMappingResult = GenTpProviederPieChart(sourceData);

            if (!pieChartSummary.ContainsKey(tpProviderMappingResult.Key))
                pieChartSummary.Add(key: tpProviderMappingResult.Key, value: new List<DashBoardGamePieChartDto>());

            pieChartData = pieChartSummary[tpProviderMappingResult.Key].Select(x => x.Id);

            foreach (var item in tpProviderMappingResult.Value)
            {
                if (pieChartData.Contains(item.Id))
                    pieChartSummary[tpProviderMappingResult.Key].FirstOrDefault(x => x.Id == item.Id).Count
                        += tpProviderMappingResult.Value.FirstOrDefault(x => x.Id == item.Id).Count;
                else
                    pieChartSummary[tpProviderMappingResult.Key].Add(tpProviderMappingResult.Value.FirstOrDefault(x => x.Id == item.Id));
            }

            var tpCategoryMappingResult = GenTpCategoryPieChart(sourceData).Result;

            if (!pieChartSummary.ContainsKey(tpCategoryMappingResult.Key))
                pieChartSummary.Add(key: tpCategoryMappingResult.Key, value: new List<DashBoardGamePieChartDto>());

            pieChartData = pieChartSummary[tpCategoryMappingResult.Key].Select(x => x.Id);

            foreach (var item in tpCategoryMappingResult.Value)
            {
                if (pieChartData.Contains(item.Id))
                    pieChartSummary[tpCategoryMappingResult.Key].FirstOrDefault(x => x.Id == item.Id).Count
                        += tpCategoryMappingResult.Value.FirstOrDefault(x => x.Id == item.Id).Count;
                else
                    pieChartSummary[tpCategoryMappingResult.Key].Add(tpCategoryMappingResult.Value.FirstOrDefault(x => x.Id == item.Id));
            }
        }

        private (string Key, List<DashBoardGamePieChartDto> Value) GenTpProviederPieChart(IEnumerable<DashBoardGamePerMonthDao> sourceData)
        {
            var result = new List<DashBoardGamePieChartDto>();
            var tpList = _tpMaintainBll.GetCommonTpList();
            var existTpList = sourceData.Select(x => x.TpId).Distinct().OrderBy(x => x);

            foreach (var tpId in existTpList)
                result.Add(new DashBoardGamePieChartDto()
                {
                    Id = tpId,
                    Name = tpList.FirstOrDefault(x => x.Id == tpId).TpName,
                    Count = sourceData.Where(x => x.TpId == tpId).Sum(x => x.CountSum)
                });

            return (Key: "GameProvider", Value: result);
        }

        private async Task<(string Key, List<DashBoardGamePieChartDto> Value)> GenTpCategoryPieChart(IEnumerable<DashBoardGamePerMonthDao> sourceData)
        {
            var result = new List<DashBoardGamePieChartDto>();
            var categoryList = await _reportBll.GetTpCategoryList();
            var existCategoryList = sourceData.Select(x => x.CategoryId).Distinct().OrderBy(x => x);

            foreach (var categoryId in existCategoryList)
                result.Add(new DashBoardGamePieChartDto()
                {
                    Id = categoryId,
                    Name = categoryList.FirstOrDefault(x => x.CategoryId == categoryId).CategoryName,
                    Count = sourceData.Where(x => x.CategoryId == categoryId).Sum(x => x.CountSum)
                });

            return (Key: "GameCategory", Value: result);
        }

        private void CollectSourceForLineChartSummary(IEnumerable<int> currencyIds,
            IEnumerable<DashBoardGamePerMonthDao> sourceData,
            ref List<DashBoardGameLineChartSourceCollectionDto> caculateData)
        {
            var existCategoryList = sourceData.Select(x => x.CategoryId).Distinct().OrderBy(x => x);
            foreach (var currencyId in currencyIds)
            {
                foreach (var category in existCategoryList)
                {
                    caculateData.Add(new DashBoardGameLineChartSourceCollectionDto()
                    {
                        CurrencyId = currencyId,
                        CategoryId = category,
                        ValidBetSum = sourceData.Where(x => x.CurrencyId == currencyId && x.CategoryId == category).Sum(x => x.ValidBetSum),
                        RevenueSum = sourceData.Where(x => x.CurrencyId == currencyId && x.CategoryId == category).Sum(x => x.RevenueSum),
                        WinLossSum = sourceData.Where(x => x.CurrencyId == currencyId && x.CategoryId == category).Sum(x => x.WinLossSum),
                        CountSum = sourceData.Where(x => x.CurrencyId == currencyId && x.CategoryId == category).Sum(x => x.CountSum)
                    });
                }
            }
        }

        private void CollectSourceMemberForLineChartSumary(IEnumerable<DashBoardGameUniqueMemberPerMonthDao> memberSourceData,
            ref IEnumerable<DashBoardGameUniqueMemberPerMonthDao> source)
        {
            source = source.Select(x => x).Concat(memberSourceData.Select(x => x)).Distinct();
        }

        private void CombineLineChartSummary(IEnumerable<int> currencyIds,
            int baseCurrencyId,
            IEnumerable<DashBoardGameLineChartSourceCollectionDto> sourceData,
            IEnumerable<DashBoardGameUniqueMemberPerMonthDao> sourceMemberData,
            ref List<DashBoardGameLineChartSummaryDto> lineChartSummary)
        {
            var currencyList = _reportBll.GetCurrency();

            foreach (var currencyId in currencyIds)
            {
                var currencyInfo = currencyList.FirstOrDefault(x => x.CurrencyId == currencyId);
                var currencyData = sourceData.Where(x => x.CurrencyId == currencyId);
                var memberData = sourceMemberData.Where(x => x.CurrencyId == currencyId);
                var data = new DashBoardGameLineChartSummaryDto()
                {
                    CurrencyId = currencyId,
                    CurrencyName = currencyInfo.IsoCode,
                    BetCount = currencyData.Sum(x => x.CountSum),
                    UniqueMember = memberData.Select(x => x.CustId).Distinct().Count(),
                    OriginalCurrencyData = new DashBoardGameLineChartCurrencyDto()
                    {
                        CurrencyId = currencyId,
                        CurrencyName = currencyInfo.IsoCode,
                        BetAmount = currencyData.Sum(x => x.ValidBetSum),
                        AverageBetAmount = currencyData.Sum(x => x.CountSum) == 0 ? 0
                        : currencyData.Sum(x => x.ValidBetSum) / currencyData.Sum(x => x.CountSum),
                        Payout = currencyData.Sum(x => x.WinLossSum) + currencyData.Sum(x => x.ValidBetSum),
                        GGR = currencyData.Where(x => x.CategoryId == 8).Sum(x => x.RevenueSum)
                        + currencyData.Where(x => x.CategoryId != 8).Sum(x => x.WinLossSum) * -1
                    },
                    BaseCurrencyData = new DashBoardGameLineChartCurrencyDto()

                };
                data.Margin = data.OriginalCurrencyData.BetAmount == 0 ? 0
                        : data.OriginalCurrencyData.GGR / data.OriginalCurrencyData.BetAmount;
                data.Margin *= 100;

                if (baseCurrencyId != 0)
                {
                    var currencyBaseInfo = currencyList.FirstOrDefault(x => x.CurrencyId == baseCurrencyId);

                    data.BaseCurrencyData.CurrencyId = baseCurrencyId;
                    data.BaseCurrencyData.CurrencyName = currencyBaseInfo.IsoCode;
                    data.BaseCurrencyData.BetAmount = ChangeRate(data.OriginalCurrencyData.BetAmount, currencyInfo.ActualRate, currencyBaseInfo.ActualRate);
                    data.BaseCurrencyData.AverageBetAmount = ChangeRate(data.OriginalCurrencyData.AverageBetAmount, currencyInfo.ActualRate, currencyBaseInfo.ActualRate);
                    data.BaseCurrencyData.Payout = ChangeRate(data.OriginalCurrencyData.Payout, currencyInfo.ActualRate, currencyBaseInfo.ActualRate);
                    data.BaseCurrencyData.GGR = ChangeRate(data.OriginalCurrencyData.GGR, currencyInfo.ActualRate, currencyBaseInfo.ActualRate);
                }
                else
                {
                    data.BaseCurrencyData = data.OriginalCurrencyData;
                }
                lineChartSummary.Add(data);
            }
        }

        private List<DashBoardGameLineChartPerMonthDto> GetGameDataPerMonth(int year, int month, IEnumerable<int> currencyIds,
            int baseCurrencyId, IEnumerable<DashBoardGamePerMonthDao> sourceData, IEnumerable<DashBoardGameUniqueMemberPerMonthDao> memberSourceData)
        {
            var result = new List<DashBoardGameLineChartPerMonthDto>();
            var currencyList = _reportBll.GetCurrency();

            foreach (var currencyId in currencyIds)
            {
                var currencyInfo = currencyList.FirstOrDefault(x => x.CurrencyId == currencyId);
                var currencyData = sourceData.Where(x => x.CurrencyId == currencyId);
                var currencyMemberData = memberSourceData.Where(x => x.CurrencyId == currencyId);
                var currencyDetail = new DashBoardGameLineChartPerMonthDto()
                {
                    Year = year,
                    Month = month,

                    CurrencyId = currencyId,
                    CurrencyName = currencyInfo.IsoCode,
                    BetCount = currencyData.Sum(x => x.CountSum),
                    UniqueMember = currencyMemberData.Count(),
                    OriginalCurrencyData = new DashBoardGameLineChartCurrencyDto()
                    {
                        CurrencyId = currencyId,
                        CurrencyName = currencyInfo.IsoCode,
                        BetAmount = currencyData.Sum(x => x.ValidBetSum),
                        AverageBetAmount = currencyData.Sum(x => x.CountSum) == 0
                            ? 0
                            : currencyData.Sum(x => x.ValidBetSum) / currencyData.Sum(x => x.CountSum),
                        Payout = currencyData.Sum(x => x.WinLossSum) + currencyData.Sum(x => x.ValidBetSum),
                        GGR = currencyData.Where(x => x.CategoryId == 8).Sum(x => x.RevenueSum)
                            + (currencyData.Where(x => x.CategoryId != 8).Sum(x => x.WinLossSum) * -1)
                    }

                };
                currencyDetail.Margin =
                    currencyDetail.OriginalCurrencyData.BetAmount == 0 ? 0 :
                    currencyDetail.OriginalCurrencyData.GGR / currencyDetail.OriginalCurrencyData.BetAmount;
                currencyDetail.Margin *= 100;
                if (baseCurrencyId != 0)
                {
                    var currencyBaseInfo = currencyList.FirstOrDefault(x => x.CurrencyId == baseCurrencyId);

                    currencyDetail.BaseCurrencyData = new DashBoardGameLineChartCurrencyDto()
                    {
                        CurrencyId = baseCurrencyId,
                        CurrencyName = currencyBaseInfo.IsoCode,
                        BetAmount = ChangeRate(currencyDetail.OriginalCurrencyData.BetAmount, currencyInfo.ActualRate, currencyBaseInfo.ActualRate),
                        AverageBetAmount = ChangeRate(currencyDetail.OriginalCurrencyData.AverageBetAmount, currencyInfo.ActualRate, currencyBaseInfo.ActualRate),
                        Payout = ChangeRate(currencyDetail.OriginalCurrencyData.Payout, currencyInfo.ActualRate, currencyBaseInfo.ActualRate),
                        GGR = ChangeRate(currencyDetail.OriginalCurrencyData.GGR, currencyInfo.ActualRate, currencyBaseInfo.ActualRate)
                    };
                }
                else
                    currencyDetail.BaseCurrencyData = currencyDetail.OriginalCurrencyData;

                result.Add(currencyDetail);
            }
            return result;
        }

        private DashBoardGameOverAllDto CombineOverallSource(CurrencyModel currencyInfo,
            int baseCurrencyId,
            IEnumerable<CurrencyModel> currencyList,
            IEnumerable<DashBoardGameOverAllDao> source,
            IEnumerable<DashBoardGameOverAllMemberDao> sourceMember)
        {
            var result = new DashBoardGameOverAllDto()
            {
                CurrencyId = currencyInfo.CurrencyId,
                CurrencyName = currencyInfo.IsoCode,
                BetCount = source.Sum(x => x.CountSummary),
                ActiveMember = sourceMember.Select(x => x.CustId).Distinct().Count(),
                OriginalCurrencyDetail = new DashboardGameOverAllCurrencyDto()
                {
                    CurrencyId = currencyInfo.CurrencyId,
                    CurrencyName = currencyInfo.IsoCode,
                    BetAmount = source.Sum(x => x.ValidBetSummary),
                    Payout = source.Sum(x => x.WinLossSummary) + source.Sum(x => x.ValidBetSummary),
                    GGR = source.Where(x => x.CategoryId == 8).Sum(x => x.RevenueSummary)
                        + source.Where(x => x.CategoryId != 8).Sum(x => x.WinLossSummary) * -1,
                    LargestSingleWin = source.Count() == 0 ? 0 : source.Select(x => x.MaxWinlostAmount).Max()
                },
                BaseCurrencyDetail = new DashboardGameOverAllCurrencyDto()
            };
            result.OriginalCurrencyDetail.AverangeBetAmount = result.BetCount == 0 ? 0 : result.OriginalCurrencyDetail.BetAmount / result.BetCount;
            result.Margin = result.OriginalCurrencyDetail.BetAmount == 0 ? 0 : result.OriginalCurrencyDetail.GGR / result.OriginalCurrencyDetail.BetAmount;
            result.Margin *= 100;

            if (baseCurrencyId == 0)
                result.BaseCurrencyDetail = result.OriginalCurrencyDetail;
            else
            {
                var currencyBaseInfo = currencyList.FirstOrDefault(x => x.CurrencyId == baseCurrencyId);

                result.BaseCurrencyDetail.CurrencyId = baseCurrencyId;
                result.BaseCurrencyDetail.CurrencyName = currencyBaseInfo.IsoCode;
                result.BaseCurrencyDetail.BetAmount = ChangeRate(result.OriginalCurrencyDetail.BetAmount, currencyInfo.ActualRate, currencyBaseInfo.ActualRate);
                result.BaseCurrencyDetail.AverangeBetAmount = ChangeRate(result.OriginalCurrencyDetail.AverangeBetAmount, currencyInfo.ActualRate, currencyBaseInfo.ActualRate);
                result.BaseCurrencyDetail.Payout = ChangeRate(result.OriginalCurrencyDetail.Payout, currencyInfo.ActualRate, currencyBaseInfo.ActualRate);
                result.BaseCurrencyDetail.GGR = ChangeRate(result.OriginalCurrencyDetail.GGR, currencyInfo.ActualRate, currencyBaseInfo.ActualRate);
                result.BaseCurrencyDetail.LargestSingleWin = ChangeRate(result.OriginalCurrencyDetail.GGR, currencyInfo.ActualRate, currencyBaseInfo.ActualRate);
            }

            return result;
        }

        #region Repo

        private IEnumerable<DashBoardGamePerMonthDao> GetGameSource(IEnumerable<int> siteIds,
                                   IEnumerable<int> currencyIds,
                                   DateTimeOffset startTime,
                                   DateTimeOffset endTime,
                                   IEnumerable<int> tpIds,
                                   IEnumerable<int> categoryIds,
                                   IEnumerable<int> gameTypes)

=> _dashBoardService.GetDashBoardGameData(JsonConvert.SerializeObject(siteIds),
   JsonConvert.SerializeObject(currencyIds),
   startTime.DateTime,
   endTime.DateTime,
   JsonConvert.SerializeObject(tpIds),
   JsonConvert.SerializeObject(categoryIds),
   JsonConvert.SerializeObject(gameTypes));

        private IEnumerable<DashBoardGameUniqueMemberPerMonthDao> GetDashBoardGameUniqueMember(IEnumerable<int> siteIds,
                                       IEnumerable<int> currencyIds,
                                       DateTimeOffset startTime,
                                       DateTimeOffset endTime,
                                       IEnumerable<int> tpIds,
                                       IEnumerable<int> categoryIds,
                                       IEnumerable<int> gameTypes)
        => _dashBoardService.GetDashBoardGameUniqueMemberData(JsonConvert.SerializeObject(siteIds),
           JsonConvert.SerializeObject(currencyIds),
           startTime.DateTime,
           endTime.DateTime,
           JsonConvert.SerializeObject(tpIds),
           JsonConvert.SerializeObject(categoryIds),
           JsonConvert.SerializeObject(gameTypes));

        private IEnumerable<DashBoardGameOverAllDao> GetGameOverall(IEnumerable<int> siteIds,
                                   IEnumerable<int> currencyIds,
                                   DateTimeOffset startTime,
                                   DateTimeOffset endTime,
                                   IEnumerable<int> tpIds,
                                   IEnumerable<int> categoryIds,
                                   IEnumerable<int> gameTypes,
                                   string timeZone)
            => _dashBoardService.GetDashBoardGameOverallData(JsonConvert.SerializeObject(siteIds),
                JsonConvert.SerializeObject(currencyIds),
                startTime.DateTime,
                endTime.DateTime,
                JsonConvert.SerializeObject(tpIds),
                JsonConvert.SerializeObject(categoryIds),
                JsonConvert.SerializeObject(gameTypes),
                timeZone);

        private IEnumerable<DashBoardGameOverAllMemberDao> GetGameOverallMember(IEnumerable<int> siteIds,
                           IEnumerable<int> currencyIds,
                           DateTimeOffset startTime,
                           DateTimeOffset endTime,
                           IEnumerable<int> tpIds,
                           IEnumerable<int> categoryIds,
                           IEnumerable<int> gameTypes,
                           string timeZone)
    => _dashBoardService.GetDashBoardGameOverallMemberData(JsonConvert.SerializeObject(siteIds),
        JsonConvert.SerializeObject(currencyIds),
        startTime.DateTime,
        endTime.DateTime,
        JsonConvert.SerializeObject(tpIds),
        JsonConvert.SerializeObject(categoryIds),
        JsonConvert.SerializeObject(gameTypes),
        timeZone);

        private IEnumerable<DashBoardGameTopDao> GetGameTopTwentyData(
                   DashboardGameTopDataTypeEnum type,
                   IEnumerable<int> siteIds,
                   IEnumerable<int> currencyIds,
                   int baseCurrencyId,
                   DateTimeOffset startTime,
                   DateTimeOffset endTime,
                   IEnumerable<int> tpIds,
                   IEnumerable<int> categoryIds,
                   IEnumerable<int> gameTypes,
                   string orderByProperty,
                   bool isDesc,
                   int pageNumber,
                   int pageSize,
                   out int totalPages,
                   out int rowCount)
        => _dashBoardService.GetGameTopTwenty(type,
            JsonConvert.SerializeObject(siteIds),
            JsonConvert.SerializeObject(currencyIds),
            baseCurrencyId,
            startTime.DateTime,
            endTime.DateTime,
            JsonConvert.SerializeObject(tpIds),
            JsonConvert.SerializeObject(categoryIds),
            JsonConvert.SerializeObject(gameTypes),
            orderByProperty,
            isDesc,
           (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
            Math.Abs(pageSize),
            out totalPages,
            out rowCount);

        #endregion

        #endregion

        private decimal ChangeRate(decimal originalAmount, decimal originalRate, decimal baseRate)
            => originalAmount * originalRate / baseRate;

        private string ConvertTimeZone(int timeZone)
            => (timeZone >= 0 ? timeZone.ToString("+00") : timeZone.ToString("00")) + ":00";
    }
}
