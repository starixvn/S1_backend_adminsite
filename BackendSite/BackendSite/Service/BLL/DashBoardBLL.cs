using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class DashBoardBLL
    {
        private readonly DashBoardService dashBoardService;
        public DashBoardBLL(DashBoardService dashBoardService)
        {
            this.dashBoardService = dashBoardService;
        }
        public DashBoardData GetDashBoardData(int siteId, int currencyId, int baseCurrencyId, List<int> canSearchSite, int timeZone)
        {
            var type = new List<int>() { 0, 1, 2, 3, 4 };

            var localTime = DateTime.UtcNow.AddHours(timeZone);
            var localDate = new DateTimeOffset(Convert.ToDateTime(localTime.ToShortDateString()), TimeSpan.FromHours(timeZone));
            var startTime = new DateTimeOffset(localDate.ToUniversalTime().DateTime, TimeSpan.Zero);
            var endTime = new DateTimeOffset(localDate.ToUniversalTime().DateTime, TimeSpan.Zero).AddHours(24).AddMilliseconds(-1);

            var result = dashBoardService.GetDashBoardData(siteId, currencyId, baseCurrencyId, startTime.DateTime, endTime.DateTime, canSearchSite, timeZone);

            result.TodayData = new List<DashBoardDetailData>
                {dashBoardService.GetDashBoardTodayData(siteId, currencyId, baseCurrencyId,startTime.DateTime,endTime.DateTime, canSearchSite,timeZone)};

            Parallel.ForEach(type, item =>
            {
                var data = dashBoardService.GetDashBoardTodayExtraData(siteId, currencyId, baseCurrencyId, startTime.DateTime, endTime.DateTime, canSearchSite, item, timeZone);
                if (data != null)
                {
                    result.TodayData.Add(data);
                }
            });

            return result;
        }
    }
}
