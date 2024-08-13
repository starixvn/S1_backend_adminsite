using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json.Linq;

namespace BackendSite.Service.BLL
{
    public class AdjustmentLogReportBLL
    {
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly Tools _tools;
        private readonly AdminBLL _adminBll;
        private readonly PermissionBLL _permissionBll;
        private readonly NewAuthorizeBLL _newAuthorizeBLL;
        public AdjustmentLogReportBLL(IServiceProvider provider)
        {
            _adjustmentLogReportService = provider.GetRequiredService<AdjustmentLogReportService>();
            _tools = provider.GetRequiredService<Tools>();
            _adminBll = provider.GetRequiredService<AdminBLL>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _newAuthorizeBLL = provider.GetService<NewAuthorizeBLL>();
        }
        public List<AdjustmentInfoLogMenu> GetAdjustmentInfoLogMenu()
        {
            return _adjustmentLogReportService.GetAdjustmentInfoLogMenu();
        }
        public AdjustmentLogReportResult GetAdjustmentInfoLog(GetAdjustmentInfoLogPara para)
        {
            return _adjustmentLogReportService.GetAdjustmentInfoLog(para);
        }

        public bool CreateAdminAdjustmentInfoLog(CreateAdminAdjustmentInfoLogInput input)
        {
            return _adjustmentLogReportService.CreateAdminAdjustmentInfoLog(input);
        }

        public AdminAdjustmentLogReportResult GetAdminAdjustmentInfoLog(GetAdminAdjustmentInfoLogInput input)
        {
            var result = _adjustmentLogReportService.GetAdminAdjustmentInfoLog(input);
            var authData = _newAuthorizeBLL.GetAuthData();
            var loginUser = _adminBll.GetUserV2(authData.UserId, authData.SiteId);
            var hasPermission = _permissionBll.CheckPermission(loginUser, 131, 39);

            if (!hasPermission)
            {
                foreach (var item in result.AdminAdjustmentLogReport)
                {
                    if (item.Menu == AdminAdjustmentLogMenuEnum.SetCustomerInfo)
                    {
                        var jsonObjBefore = item.BeforeDetailInfoObject as JObject;
                        var jsonObjAfter = item.AfterDetailInfoObject as JObject;

                        // 修改 Email&PhoneNo 屬性
                        var emailBefore = jsonObjBefore["Email"]?.ToString();
                        var emailAfter = jsonObjAfter["Email"]?.ToString();
                        var phoneNoBefore = jsonObjBefore["PhoneNo"]?.ToString();
                        var phoneNoAfter = jsonObjAfter["PhoneNo"]?.ToString();

                        if (emailBefore != null && !string.IsNullOrEmpty(emailBefore))
                        {
                            jsonObjBefore["Email"] = _tools.MaskEmail(jsonObjBefore["Email"].ToString());
                        }

                        if (emailAfter != null && !string.IsNullOrEmpty(emailAfter))
                        {
                            jsonObjAfter["Email"] = _tools.MaskEmail(jsonObjAfter["Email"].ToString());
                        }

                        if (phoneNoBefore != null && !string.IsNullOrEmpty(phoneNoBefore))
                        {
                            jsonObjBefore["PhoneNo"] = _tools.MaskPhone(jsonObjBefore["PhoneNo"].ToString());
                        }

                        if (phoneNoAfter != null && !string.IsNullOrEmpty(phoneNoAfter))
                        {
                            jsonObjAfter["PhoneNo"] = _tools.MaskPhone(jsonObjAfter["PhoneNo"].ToString());
                        }

                        // 將修改後的 JObject 轉換回 JSON 字符串
                        item.BeforeDetailInfo = jsonObjBefore.ToString();
                        item.AfterDetailInfo = jsonObjAfter.ToString();
                    }
                }
            }
            
            return result;
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
                    Value = (int)x
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
                    Value = (int)x
                });

            return menus;
        }
    }
}
