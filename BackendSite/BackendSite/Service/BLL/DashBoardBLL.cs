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
            var retsult =  dashBoardService.GetDashBoardData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite);

            retsult.TodayData = new List<DashBoardDetailData>();
            retsult.TodayData.Add(dashBoardService.GetDashBoardTodayData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite));

            for(int i = 0; i < 5; i++)
            {
                retsult.TodayData.Add(dashBoardService.GetDashBoardTodayExtraData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite, i));
            }

            return retsult;
        }
    }
}
