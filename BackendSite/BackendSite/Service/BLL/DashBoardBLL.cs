using BackendSite.Service.DAL;
using BackendSite.Service.Model;
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
        public DashBoardData GetDashBoardData(int siteId,int currencyId,int baseCurrencyId,List<int> canSearchSite , int timeZone)
        {
            var type = new List<int>() { 0, 1, 2, 3, 4 };
            var result =  dashBoardService.GetDashBoardData(siteId, currencyId, baseCurrencyId, canSearchSite ,timeZone);

            result.TodayData = new List<DashBoardDetailData> 
                {dashBoardService.GetDashBoardTodayData(siteId, currencyId, baseCurrencyId, canSearchSite,timeZone)};

            Parallel.ForEach(type, item =>
            {
                var data = dashBoardService.GetDashBoardTodayExtraData(siteId, currencyId, baseCurrencyId, canSearchSite, item,timeZone);
                if (data != null)
                {
                    result.TodayData.Add(data);
                }
            });

            return result;
        }
    }
}
