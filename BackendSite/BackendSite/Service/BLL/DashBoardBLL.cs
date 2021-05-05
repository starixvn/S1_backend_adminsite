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
            List<DashBoardDetailData> TodayData = new List<DashBoardDetailData>();
            var todayData = dashBoardService.GetDashBoardTodayData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite);
            todayData.ReportName = "Today";
            var todayDeposit = dashBoardService.GetDashBoardTodayExtraData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite,0);
            todayDeposit.ReportName = "Today Deposit";
            var todayWithdrawal = dashBoardService.GetDashBoardTodayExtraData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite,1);
            todayWithdrawal.ReportName = "Today Withdrawal";
            var todayFirstDeposit = dashBoardService.GetDashBoardTodayExtraData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite,2);
            todayFirstDeposit.ReportName = "Today First Deposit";
            var todaySecondDeposit = dashBoardService.GetDashBoardTodayExtraData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite,3);
            todaySecondDeposit.ReportName = "Today Second Deposit";
            var todayBonusClaim = dashBoardService.GetDashBoardTodayExtraData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite,4);
            todayBonusClaim.ReportName = "Today Bonus Claim";
            var retsult =  dashBoardService.GetDashBoardData(SiteId, CurrencyId, BaseCurrencyId, CanSearchSite);

            retsult.TodayData = new List<DashBoardDetailData>();
            retsult.TodayData.Add(todayData);
            retsult.TodayData.Add(todayDeposit);
            retsult.TodayData.Add(todayWithdrawal);
            retsult.TodayData.Add(todayFirstDeposit);
            retsult.TodayData.Add(todaySecondDeposit);
            retsult.TodayData.Add(todayBonusClaim);
            return retsult;
        }
    }
}
