using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;

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
        public AdjustmentLogReportResult GetAdjustmentInfoLog(GetAdjustmentInfoLogPara para)
        {
            return adjustmentLogReportService.GetAdjustmentInfoLog(para); 
        }
    }
}
