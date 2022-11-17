using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class AuthorizeByPermissionBLL
    {
        private readonly NewAuthorizeBLL _newAuthorizeBll;
        private readonly PermissionBLL _permissionBll;
        private readonly AdminBLL _adminBll;
        
        public AuthorizeByPermissionBLL(
            NewAuthorizeBLL newAuthorizeBll, 
            PermissionBLL permissionBll,
            AdminBLL adminBll)
        {
            _newAuthorizeBll = newAuthorizeBll;
            _permissionBll = permissionBll;
            _adminBll = adminBll;
        }
        
        public List<int> GetUserPermission(int menuId)
        {
            var brand = _newAuthorizeBll.CheckBrand();
            var authInfo = _newAuthorizeBll.GetAuthData();
            var loginUser = _adminBll.GetUserV2(authInfo.UserId, brand);
            var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
            var userAllPermission = _permissionBll.GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();
            var permissionList = userAllPermission.Find(x => x.MenuId == menuId)?.Permission;

            return permissionList;
        }
    }
}
