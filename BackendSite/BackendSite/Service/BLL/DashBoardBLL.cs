using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public DashBoardData GetDashBoardData(int SiteId,int CurrencyId,int BaseCurrencyId,List<int> CanSearchSite)
        {
            List<int> type = new List<int>() { 0, 1, 2, 3, 4 };
            var retsult =  dashBoardService.GetDashBoardData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite);

            retsult.TodayData = new List<DashBoardDetailData>();
            retsult.TodayData.Add(dashBoardService.GetDashBoardTodayData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite));

            Parallel.ForEach(type, item =>
            {
                DashBoardDetailData data = dashBoardService.GetDashBoardTodayExtraData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite, item);
                if (data != null)
                {
                    retsult.TodayData.Add(data);
                }
            });

            return retsult;
        }
    }
}
