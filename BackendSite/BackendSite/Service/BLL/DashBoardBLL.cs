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
        public DashBoardBLL(IServiceProvider provider)
        {
            _dashBoardService = provider.GetRequiredService<DashBoardService>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
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

        public DashBoardMemBerData GetDashBoardMemberData(int siteId, IEnumerable<int> currencyIds ,int timeZone)
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
                    () => GetDashBoardMemberDataByCurrency(siteId, currencyId , timeZone));
                currencyData.CurrencyId = currencyId;
                currencyData.Currency = Enum.GetName(typeof(CurrencyEnum),currencyId);
                result.MemBerDataByCurrency.Add(currencyData);
            }
            
            // RegistrationPlatform
            var key2 = $"DashBoardMember:{siteId}:{JsonConvert.SerializeObject(currencyIds)}";
            result.RegistrationPlatform = _redisHelper.TryGetOrCreate(key2, TimeSpan.FromMinutes(15), 
                () => GetDashBoardMemberPlatformData(siteId, currencyIds, timeZone));
            
            
             return result;
        }

        private DashBoardMemBerDataByCurrency GetDashBoardMemberDataByCurrency(int siteId, int currencyId ,int timeZone)
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
            result.TodayData = new DashBoardMemBerDetailData
            {
                NewMember = yesterdayData.NewMember,
                UniqueLogin = yesterdayData.UniqueLogin
            };
            
            //MonthToDateData
            var monthToDateStartTime = new DateTime(localTime.Year, localTime.Month, 1);
            var monthToDateData = _dashBoardService.GetDashBoardMemberData(siteId, currencyId, monthToDateStartTime, localTime);
            result.TodayData = new DashBoardMemBerDetailData
            {
                NewMember = monthToDateData.NewMember,
                UniqueLogin = monthToDateData.UniqueLogin
            };
            
            //LastMonthData
            var lastMonthStartTime = monthToDateStartTime.AddMonths(-1);
            var lastMonthEndTime = localTime.AddMonths(-1);
            var samePeriodLastMonthData = _dashBoardService.GetDashBoardMemberData(siteId, currencyId, lastMonthStartTime, lastMonthEndTime);
            result.TodayData = new DashBoardMemBerDetailData
            {
                NewMember = samePeriodLastMonthData.NewMember,
                UniqueLogin = samePeriodLastMonthData.UniqueLogin
            };
            
            return result;
        }
        
        private List<RegistrationPlatformDataByMonth> GetDashBoardMemberPlatformData(int siteId, IEnumerable<int> currencyIds ,int timeZone)
        {
            var result = new List<RegistrationPlatformDataByMonth>();
            for (var month = 1 ; month <= DateTime.UtcNow.Month; month++)
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
        
    }
}
