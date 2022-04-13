using System.Collections.Generic;

namespace BackendSite.Service.Model.AdminMenu
{
    public class AdminMenuModel
    {
        public int ModuleId { get; set; }

        public string Name { get; set; }

        public IEnumerable<AdminPermissionModel> Permission { get; set; }
        public IEnumerable<AdminMenuFuncModel> FunctionDatas { get; set; }
        public IEnumerable<AdminMenuModel> SubLayerDatas { get; set; }
    }
}
