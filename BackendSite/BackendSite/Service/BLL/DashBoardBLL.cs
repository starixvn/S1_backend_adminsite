using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using System.Reflection;

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
                RegistrationPlatform = new List<RegistrationPlatformDataByMonth>()
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
            for (var month = 1; month <= DateTime.UtcNow.Month; month++)
            {
                var startTime = new DateTime(DateTime.UtcNow.Year, month, 1).AddHours(timeZone);
                var endTime = startTime.AddMonths(1).AddMilliseconds(-1).AddHours(timeZone);
                var dataByMonth = _dashBoardService.GetDashBoardMemberData(siteId, currencyId, startTime, endTime);
                dataByMonth.Month = month;
                result.PerMonthData.Add(dataByMonth);
            }

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

        private List<RegistrationPlatformDataByMonth> GetDashBoardMemberPlatformData(int siteId, IEnumerable<int> currencyIds, int timeZone)
        {
            var result = new List<RegistrationPlatformDataByMonth>();
            for (var month = 1; month <= DateTime.UtcNow.Month; month++)
            {
                var data = new RegistrationPlatformDataByMonth();
                var startTime = new DateTime(DateTime.UtcNow.Year, month, 1).AddHours(timeZone);
                var endTime = startTime.AddMonths(1).AddMilliseconds(-1).AddHours(timeZone);
                data.Platform = _dashBoardService.GetDashBoardMemberPlatformData(siteId, currencyIds, startTime, endTime);
                data.Month = month;
                result.Add(data);
            }

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
                PerMonthData = new List<DashBoardFinancialDataByMonth>(),
                TodayData = new DashBoardFinancialDetailData(),
                YesterdayData = new DashBoardFinancialDetailData(),
                MonthToDateData = new DashBoardFinancialDetailData(),
                SamePeriodLastMonthData = new DashBoardFinancialDetailData()
            };

            //PerMonthData
            for (var month = 1; month <= DateTime.UtcNow.Month; month++)
            {
                var monthData = new DashBoardFinancialDataByMonth();
                var startTime = new DateTime(DateTime.UtcNow.Year, month, 1).AddHours(timeZone);
                var endTime = startTime.AddMonths(1).AddMilliseconds(-1).AddHours(timeZone);

                monthData.Month = month;
                monthData.OriginalCurrencyData = _dashBoardService.GetDashBoardFinancialData(siteId, currencyId, startTime, endTime);

                if (baseRate != 0 && originRate != baseRate)
                {
                    monthData.BaseCurrencyData = new DashBoardFinancialDetailData()
                    {
                        DepositCount = monthData.OriginalCurrencyData.DepositCount,
                        DepositAmount = ChangeRate(monthData.OriginalCurrencyData.DepositAmount, originRate, baseRate),
                        WithdrawalAmount = ChangeRate(monthData.OriginalCurrencyData.WithdrawalAmount, originRate, baseRate),
                        BonusClaimAmount = ChangeRate(monthData.OriginalCurrencyData.BonusClaimAmount, originRate, baseRate),
                        NetCashAmount = ChangeRate(monthData.OriginalCurrencyData.NetCashAmount, originRate, baseRate)
                    };
                }
                else
                {
                    monthData.BaseCurrencyData = monthData.OriginalCurrencyData;
                }
                result.PerMonthData.Add(monthData);
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
        #endregion

        #region DashboardGame

        public DashBoardGameDto GetGameData(IEnumerable<int> siteIds,
                             IEnumerable<int> currencyIds,
                             int baseCurrencyId,
                             IEnumerable<int> tpIds,
                             IEnumerable<int> categoryIds,
                             IEnumerable<int> gameTypes,
                             int timeZone)
        {
            var result = new DashBoardGameDto();
            var pieChartSummary = new Dictionary<string, List<DashBoardGameSimpleSummaryDto>>();
            var lineChartPerMonthSummary = new List<DashBoardGamePerMonthDto>();
            var lineChartSummary = new List<CurrencySummaryDto>();
            var caculateData = new List<DashboardGameForCaculateDao>();
            var caculateMemberData = Enumerable.Empty<DashBoardGameUniqueMemberPerMonthDao>();

            //Step1:決定日期區間  

            var dateTimeNowInTimeZone = DateTime.UtcNow.AddHours(timeZone);

            //Step2:撈資料
            for (int i = 1; i <= dateTimeNowInTimeZone.Month; i++)
            {
                var dateRange = CaculateDateRange(dateTimeNowInTimeZone.Year, i, timeZone);
                var sourceData = GetGameSourceDataPerMonth(siteIds, currencyIds,
                    dateRange.StartDate, dateRange.EndDate, tpIds, categoryIds, gameTypes);
                var sourceMemberData = GetDashBoardGameUniqueMemberDataPerMonth(siteIds, currencyIds,
                  dateRange.StartDate, dateRange.EndDate, tpIds, categoryIds, gameTypes);

                var gameData = GetGameDataPerMonth(dateTimeNowInTimeZone.Year, i,
                    currencyIds, baseCurrencyId, sourceData, sourceMemberData);

                CombinePieChartSummary(sourceData, ref pieChartSummary);
                lineChartPerMonthSummary.AddRange(gameData);

                CombineCaculateData(currencyIds, sourceData, ref caculateData);
                CombineSourceMemberData(sourceMemberData, ref caculateMemberData);
            }
            CombineLineChartSummary(currencyIds, baseCurrencyId, caculateMemberData, caculateData, ref lineChartSummary);

            result.PieChartSummary = pieChartSummary;
            result.LineChartPerMonthSummary = lineChartPerMonthSummary;
            result.LineChartSummary = lineChartSummary;

            //Step3:組合資料
            return result;
        }

        private void CombineCaculateData(IEnumerable<int> currencyIds,
            IEnumerable<DashBoardGamePerMonthDao> sourceData,
            ref List<DashboardGameForCaculateDao> caculateData)
        {
            var existCategoryList = sourceData.Select(x => x.CategoryId).Distinct().OrderBy(x => x);
            foreach (var currencyId in currencyIds)
            {
                foreach (var category in existCategoryList)
                {
                    caculateData.Add(new DashboardGameForCaculateDao()
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

        private (DateTimeOffset StartDate, DateTimeOffset EndDate) CaculateDateRange(int year, int month, int timeZone)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            return (StartDate: new DateTimeOffset(firstDayOfMonth, new TimeSpan(timeZone, 0, 0)),
                    EndDate: new DateTimeOffset(lastDayOfMonth, new TimeSpan(timeZone, 0, 0)));
        }

        private IEnumerable<DashBoardGamePerMonthDao> GetGameSourceDataPerMonth(IEnumerable<int> siteIds,
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

        private IEnumerable<DashBoardGameUniqueMemberPerMonthDao> GetDashBoardGameUniqueMemberDataPerMonth(IEnumerable<int> siteIds,
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

        private void CombinePieChartSummary(IEnumerable<DashBoardGamePerMonthDao> sourceData,
            ref Dictionary<string, List<DashBoardGameSimpleSummaryDto>> pieChartSummary)
        {
            var result = new Dictionary<string, List<DashBoardGameSimpleSummaryDto>>();
            var pieChartData = Enumerable.Empty<int>();

            var tpProviderMappingResult = MappingTpProvieder(sourceData);

            if (!pieChartSummary.ContainsKey(tpProviderMappingResult.Key))
                pieChartSummary.Add(key: tpProviderMappingResult.Key, value: new List<DashBoardGameSimpleSummaryDto>());

            pieChartData = pieChartSummary[tpProviderMappingResult.Key].Select(x => x.Id);

            foreach (var item in tpProviderMappingResult.Value)
            {
                if (pieChartData.Contains(item.Id))
                    pieChartSummary[tpProviderMappingResult.Key].FirstOrDefault(x => x.Id == item.Id).Count
                        += tpProviderMappingResult.Value.FirstOrDefault(x => x.Id == item.Id).Count;
                else
                    pieChartSummary[tpProviderMappingResult.Key].Add(tpProviderMappingResult.Value.FirstOrDefault(x => x.Id == item.Id));
            }

            var tpCategoryMappingResult = MappingTpCategory(sourceData).Result;

            if (!pieChartSummary.ContainsKey(tpCategoryMappingResult.Key))
                pieChartSummary.Add(key: tpCategoryMappingResult.Key, value: new List<DashBoardGameSimpleSummaryDto>());

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

        private void CombineSourceMemberData(IEnumerable<DashBoardGameUniqueMemberPerMonthDao> memberSourceData,
            ref IEnumerable<DashBoardGameUniqueMemberPerMonthDao> source)
        {
            source = source.Select(x=>x).Concat(memberSourceData.Select(x => x)).Distinct();
        }

        private void CombineLineChartSummary(IEnumerable<int> currencyIds,
            int baseCurrencyId,
            IEnumerable<DashBoardGameUniqueMemberPerMonthDao> sourceMemberData,
            List<DashboardGameForCaculateDao> sourceData,
            ref List<CurrencySummaryDto> lineChartSummary)
        {
            var result = new List<DashBoardCurrencySimpleSummaryDto>();
            var currencyList = _reportBll.GetCurrency();

            foreach (var currencyId in currencyIds)
            {
                var currencyInfo = currencyList.FirstOrDefault(x => x.CurrencyId == currencyId);
                var currencyData = sourceData.Where(x => x.CurrencyId == currencyId);
                var memberData = sourceMemberData.Where(x => x.CurrencyId == currencyId);
                var data = new CurrencySummaryDto()
                {
                    CurrencyId = currencyId,
                    CurrencyName = currencyInfo.IsoCode,
                    BetCount = currencyData.Sum(x => x.CountSum),
                    UniqueMember = memberData.Select(x=>x.CustId).Distinct().Count(),
                    OriginalCurrencyData = new DashboardGamePerMonthCurrencyDto()
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
                    BaseCurrencyData = new DashboardGamePerMonthCurrencyDto()

                };
                data.Margin = data.OriginalCurrencyData.BetAmount == 0 ? 0
                        : data.OriginalCurrencyData.GGR / data.OriginalCurrencyData.BetAmount;

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

        private (string Key, List<DashBoardGameSimpleSummaryDto> Value) MappingTpProvieder(IEnumerable<DashBoardGamePerMonthDao> sourceData)
        {
            var result = new List<DashBoardGameSimpleSummaryDto>();
            var tpList = _tpMaintainBll.GetCommonTpList();
            var existTpList = sourceData.Select(x => x.TpId).Distinct().OrderBy(x => x);

            foreach (var tpId in existTpList)
                result.Add(new DashBoardGameSimpleSummaryDto()
                {
                    Id = tpId,
                    Name = tpList.FirstOrDefault(x => x.Id == tpId).TpName,
                    Count = sourceData.Where(x => x.TpId == tpId).Sum(x => x.CountSum)
                });

            return (Key: "GameProvider", Value: result);
        }

        private async Task<(string Key, List<DashBoardGameSimpleSummaryDto> Value)> MappingTpCategory(IEnumerable<DashBoardGamePerMonthDao> sourceData)
        {
            var result = new List<DashBoardGameSimpleSummaryDto>();
            var categoryList = await _reportBll.GetTpCategoryList();
            var existCategoryList = sourceData.Select(x => x.CategoryId).Distinct().OrderBy(x => x);

            foreach (var categoryId in existCategoryList)
                result.Add(new DashBoardGameSimpleSummaryDto()
                {
                    Id = categoryId,
                    Name = categoryList.FirstOrDefault(x => x.CategoryId == categoryId).CategoryName,
                    Count = sourceData.Where(x => x.CategoryId == categoryId).Sum(x => x.CountSum)
                });

            return (Key: "GameCategory", Value: result);
        }

        private List<DashBoardGamePerMonthDto> GetGameDataPerMonth(int year, int month, IEnumerable<int> currencyIds,
            int baseCurrencyId, IEnumerable<DashBoardGamePerMonthDao> sourceData, IEnumerable<DashBoardGameUniqueMemberPerMonthDao> memberSourceData)
        {
            var result = new List<DashBoardGamePerMonthDto>();
            var currencyItem = new DashboardGamePerMonthCurrencyDto();
            var currencyList = _reportBll.GetCurrency();

            foreach (var currencyId in currencyIds)
            {
                var currencyInfo = currencyList.FirstOrDefault(x => x.CurrencyId == currencyId);
                var currencyData = sourceData.Where(x => x.CurrencyId == currencyId);
                var currencyMemberData = memberSourceData.Where(x => x.CurrencyId == currencyId);
                var currencyDetail = new DashBoardGamePerMonthDto()
                {
                    Year = year,
                    Month = month,
                    CurrencyDetail = new CurrencySummaryDto()
                    {
                        CurrencyId = currencyId,
                        CurrencyName = currencyInfo.IsoCode,
                        BetCount = currencyData.Sum(x => x.CountSum),
                        UniqueMember = currencyMemberData.Count(),
                        OriginalCurrencyData = new DashboardGamePerMonthCurrencyDto()
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
                    }
                };
                currencyDetail.CurrencyDetail.Margin =
                    currencyDetail.CurrencyDetail.OriginalCurrencyData.BetAmount == 0 ? 0 :
                    currencyDetail.CurrencyDetail.OriginalCurrencyData.GGR / currencyDetail.CurrencyDetail.OriginalCurrencyData.BetAmount;

                if (baseCurrencyId != 0)
                {
                    var currencyBaseInfo = currencyList.FirstOrDefault(x => x.CurrencyId == baseCurrencyId);

                    currencyDetail.CurrencyDetail.BaseCurrencyData = new DashboardGamePerMonthCurrencyDto()
                    {
                        CurrencyId = baseCurrencyId,
                        CurrencyName = currencyBaseInfo.IsoCode,
                        BetAmount = ChangeRate(currencyDetail.CurrencyDetail.OriginalCurrencyData.BetAmount, currencyInfo.ActualRate, currencyBaseInfo.ActualRate),
                        AverageBetAmount = ChangeRate(currencyDetail.CurrencyDetail.OriginalCurrencyData.AverageBetAmount, currencyInfo.ActualRate, currencyBaseInfo.ActualRate),
                        Payout = ChangeRate(currencyDetail.CurrencyDetail.OriginalCurrencyData.Payout, currencyInfo.ActualRate, currencyBaseInfo.ActualRate),
                        GGR = ChangeRate(currencyDetail.CurrencyDetail.OriginalCurrencyData.GGR, currencyInfo.ActualRate, currencyBaseInfo.ActualRate)
                    };
                }
                else
                    currencyDetail.CurrencyDetail.BaseCurrencyData = currencyDetail.CurrencyDetail.OriginalCurrencyData;

                result.Add(currencyDetail);
            }
            return result;
        }

        private decimal ChangeRate(decimal originalAmount, decimal originalRate, decimal baseRate)
        => originalAmount * originalRate / baseRate;

        #endregion
    }
}
