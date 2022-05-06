using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using BackendSite.Service.Library;

namespace BackendSite.Service.BLL
{
    public class DashBoardBLL
    {
        private readonly DashBoardService dashBoardService;
        private readonly CacheHelper _cacheHelper;
        private readonly IMemoryCache cache;
        public DashBoardBLL(IServiceProvider provider)
        {
            dashBoardService = provider.GetRequiredService<DashBoardService>();
            cache = provider.GetRequiredService<IMemoryCache>();
            _cacheHelper = provider.GetRequiredService<CacheHelper>();
        }

        public DashBoardData GetDashBoardData(int siteId, int currencyId, int baseCurrencyId, List<int> canSearchSite, int timeZone)
        {
            var type = new List<int>() { 0, 1, 2, 3, 4 };
            var localTime = DateTime.UtcNow.AddHours(timeZone);
            var localDate = new DateTimeOffset(Convert.ToDateTime(localTime.ToShortDateString()), TimeSpan.FromHours(timeZone));
            var startTime = new DateTimeOffset(localDate.ToUniversalTime().DateTime, TimeSpan.Zero);
            var endTime = new DateTimeOffset(localDate.ToUniversalTime().DateTime, TimeSpan.Zero).AddHours(24).AddMilliseconds(-1);
            var siteList = siteId == 0 ? JsonConvert.SerializeObject(canSearchSite) : JsonConvert.SerializeObject(new List<int> { siteId });

            var key = $"DashBoard_{siteList}_{currencyId}_{baseCurrencyId}";
            return _cacheHelper.TryGetOrCreate(key,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)),
                i => GetDashBoardData(siteId, currencyId, baseCurrencyId, canSearchSite, startTime, endTime, type));
        }

        private DashBoardData GetDashBoardData(int siteId, int currencyId, int baseCurrencyId, List<int> canSearchSite,
            DateTimeOffset startTime, DateTimeOffset endTime, List<int> type)
        {
            var result = dashBoardService.GetDashBoardData(siteId, currencyId, startTime.DateTime, endTime.DateTime, canSearchSite);
            result.TodayData = new List<DashBoardDetailData>
            {
                dashBoardService.GetDashBoardTodayData(siteId, currencyId, baseCurrencyId, startTime.DateTime, endTime.DateTime,
                    canSearchSite)
            };

            type.ForEach(item =>
            {
                var data = dashBoardService.GetDashBoardTodayExtraData(siteId, currencyId, baseCurrencyId, startTime.DateTime,
                    endTime.DateTime, canSearchSite, item);
                if (data != null)
                {
                    result.TodayData.Add(data);
                }
            });

            return result;
        }
    }
}
