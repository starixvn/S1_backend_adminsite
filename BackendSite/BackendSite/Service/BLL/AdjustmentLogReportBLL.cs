using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class AdjustmentLogReportBLL
    {
        private readonly AdjustmentLogReportService adjustmentLogReportService;
        public AdjustmentLogReportBLL(AdjustmentLogReportService adjustmentLogReportService)
        {
            this.adjustmentLogReportService = adjustmentLogReportService;
        }
        public List<AdjustmentInfoLogMenu> GetAdjustmentInfoLogMenu()
        {
            return adjustmentLogReportService.GetAdjustmentInfoLogMenu();
        }
    }
}
