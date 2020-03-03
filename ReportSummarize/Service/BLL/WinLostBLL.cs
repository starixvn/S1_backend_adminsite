using System;
using System.Collections.Generic;
using System.Text;
using ReportSummarize.Service.DAL;

namespace ReportSummarize.Service.BLL
{
    public class WinLostBLL
    {
        private readonly WinLostService winLostService;

        public WinLostBLL(WinLostService winLostService)
        {
            this.winLostService = winLostService;
        }

        public void SyncDailyReport()
        {
            winLostService.SyncDailyReport();
        }
    }
}
