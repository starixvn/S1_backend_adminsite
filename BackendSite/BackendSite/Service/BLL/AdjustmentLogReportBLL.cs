using System;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.Model.enums;

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
        
        public bool CreateAdminAdjustmentInfoLog(CreateAdminAdjustmentInfoLogInput input)
        {
            return adjustmentLogReportService.CreateAdminAdjustmentInfoLog(input); 
        }
        
        public AdminAdjustmentLogReportResult GetAdminAdjustmentInfoLog(GetAdminAdjustmentInfoLogInput input)
        {
            return adjustmentLogReportService.GetAdminAdjustmentInfoLog(input); 
        }
        
        public IEnumerable<SelectListItem> GetActionTypeList()
        {
            var type = typeof(ActionType);
            var menus = Enum.GetValues(type)
                .Cast<ActionType>()
                .Where(x => x != ActionType.Read)
                .Select(x => new SelectListItem
                {
                    Name = x.ToString(),
                    Value = (int) x
                });
            
            return menus;
        }
        
        public IEnumerable<SelectListItem> GetAdminAdjustmentInfoLogMenu()
        {
            var type = typeof(AdminAdjustmentLogMenuEnum);
            var menus = Enum.GetValues(type)
                .Cast<AdminAdjustmentLogMenuEnum>()
                .Where(x => x != AdminAdjustmentLogMenuEnum.Undefined)
                .Select(x => new SelectListItem
                {
                    Name = x.ToString(),
                    Value = (int) x
                });
            
            return menus;
        }
    }
}
