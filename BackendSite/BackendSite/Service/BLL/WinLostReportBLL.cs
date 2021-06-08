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
        #region Fields

        private readonly WinLostReportService winLostReportService;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly SelLangBLL selLangBLL;

        #endregion

        #region Constructors

        public WinLostReportBLL(WinLostReportService winLostReportService, ServerInfoBLL serverInfoBLL, SelLangBLL selLangBLL)
        {
            this.winLostReportService = winLostReportService;
            this.serverInfoBLL = serverInfoBLL;
            this.selLangBLL = selLangBLL;
        }

        #endregion

        #region Public Methods

        public IEnumerable<WinLostReport> GetWinLostReport(DateTime StartDate,
                                                           DateTime EndDate,
                                                           int Currency,
                                                           bool isExport)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser();
            var report = winLostReportService.GetWinLostReport(StartDate, EndDate, siteList?.Select(x => x.SiteId), Currency);
            if (report != null && report.Any())
            {
                var gameTypeList = winLostReportService.GetAllGameType().ToDictionary(row => (int)row.Type, row => (string)row.TypeName);
                foreach (var row in report)
                {
                    if (isExport)
                    {
                        row.SettledDate = row.SettledDate.AddHours(8);
                    }
                    row.CurrencyName = selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}");
                    row.SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName;

                    gameTypeList.TryGetValue(row.GameType, out string GameName);
                    row.GameName = GameName;
                }
            }

            return report;
        }

        #endregion
    }
}
