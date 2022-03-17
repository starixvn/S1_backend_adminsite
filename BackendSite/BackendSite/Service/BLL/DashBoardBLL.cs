using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class DashBoardBLL
    {
        private readonly DashBoardService dashBoardService;
        private readonly IMemoryCache cache;
        public DashBoardBLL(IServiceProvider provider)
        {
            dashBoardService = provider.GetRequiredService<DashBoardService>();
            cache = provider.GetRequiredService<IMemoryCache>();
        }
        public DashBoardData GetDashBoardData(int siteId, int currencyId, int baseCurrencyId, List<int> canSearchSite, int timeZone)
        {
            var type = new List<int>() { 0, 1, 2, 3, 4 };

            var localTime = DateTime.UtcNow.AddHours(timeZone);
            var localDate = new DateTimeOffset(Convert.ToDateTime(localTime.ToShortDateString()), TimeSpan.FromHours(timeZone));
            var startTime = new DateTimeOffset(localDate.ToUniversalTime().DateTime, TimeSpan.Zero);
            var endTime = new DateTimeOffset(localDate.ToUniversalTime().DateTime, TimeSpan.Zero).AddHours(24).AddMilliseconds(-1);

            var result = dashBoardService.GetDashBoardData(siteId, currencyId, baseCurrencyId, startTime.DateTime, endTime.DateTime, canSearchSite, timeZone);
            var siteList = siteId == 0 ? JsonConvert.SerializeObject(canSearchSite) : JsonConvert.SerializeObject(new List<int> { siteId });
            var sKey = $"DashBoard_{siteList}_{currencyId}_{baseCurrencyId}";
            var dashBoardData = cache.Get<DashBoardData>(sKey);

            if (dashBoardData != null)
            {
                return dashBoardData;
            }

            result.TodayData = new List<DashBoardDetailData>
                {dashBoardService.GetDashBoardTodayData(siteId, currencyId, baseCurrencyId,startTime.DateTime,endTime.DateTime, canSearchSite,timeZone)};

            type.ForEach(item =>
            {
                var data = dashBoardService.GetDashBoardTodayExtraData(siteId, currencyId, baseCurrencyId, startTime.DateTime, endTime.DateTime, canSearchSite, item, timeZone);
                if (data != null)
                {
                    result.TodayData.Add(data);
                }
            });

            if(result != null)
            {
                cache.Set(sKey, result, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30)));
            }

            return result;
        }
    }
}
