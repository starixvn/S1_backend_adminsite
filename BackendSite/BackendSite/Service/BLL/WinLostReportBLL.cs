using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class WinLostReportBLL
    {
        private readonly WinLostReportService winLostReportService;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly SelLangBLL selLangBLL;

        public WinLostReportBLL(WinLostReportService winLostReportService, ServerInfoBLL serverInfoBLL, SelLangBLL selLangBLL)
        {
            this.winLostReportService = winLostReportService;
            this.serverInfoBLL = serverInfoBLL;
            this.selLangBLL = selLangBLL;
        }

        public IEnumerable<WinLostReport> GetWinLostReport(DateTime StartDate, DateTime EndDate, int Currency)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser();
            var report = winLostReportService.GetWinLostReport(StartDate, EndDate, siteList?.Select(x=>x.SiteId), Currency);
            if(report != null && report.Any())
            {
                var gameTypeList = winLostReportService.GetAllGameType();
                foreach (var row in report)
                {
                    row.CurrencyName = selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}");
                    row.SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName;

                    gameTypeList.TryGetValue(row.GameType, out string GameName);
                    row.GameName = GameName;
                }
            }

            return report;
        }
    }
}
