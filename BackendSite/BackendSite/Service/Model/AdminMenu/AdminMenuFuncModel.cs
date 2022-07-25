using System.Collections.Generic;

namespace BackendSite.Service.Model.AdminMenu
{
    public class AdminMenuFuncModel
    {
        public int FunctionId { get; set; }
        public string Name { get; set; }
        public IEnumerable<AdminPermissionModel> Permission { get; set; }
    }
}
