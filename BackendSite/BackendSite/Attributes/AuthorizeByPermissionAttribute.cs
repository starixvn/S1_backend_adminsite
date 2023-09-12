using BackendSite.Service.BLL;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace BackendSite.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeByPermissionAttribute : Attribute, IAuthorizationFilter
    {
        public int MenuId { get; set; }
        public int PermissionId { get; set; }
        private readonly NewAuthorizeBLL _newAuthorizeBll;
        private readonly PermissionBLL _permissionBll;
        private readonly AdminBLL _adminBll;

        public AuthorizeByPermissionAttribute(IServiceProvider provider, int menuId, int permissionId)
        {
            _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _adminBll = provider.GetRequiredService<AdminBLL>();
            MenuId = menuId;
            PermissionId = permissionId;
        }
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (MenuId != 0 && PermissionId != 0)
            {
                var brand = _newAuthorizeBll.CheckBrand();
                if (brand != 0)
                {
                    if (filterContext.HttpContext.User.Identity?.IsAuthenticated == true)
                    {
                        var authInfo = _newAuthorizeBll.GetAuthData();

                        var loginUser = _adminBll.GetUserV2(authInfo.UserId, brand);
                        if (!loginUser.IsSuper)
                        {
                            if (loginUser.Status == 0)
                            {
                                filterContext.Result = RedirectToLogin();
                            }
                            else
                            {
                                var permissions = _permissionBll.GetUserPermissionByMenuId(MenuId);

                                if (permissions == null)
                                {
                                    filterContext.Result = RedirectToLogin();
                                }
                                else
                                {
                                    if (!permissions.Contains(PermissionId))
                                    {
                                        filterContext.Result = RedirectToLogin();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        filterContext.Result = RedirectToLogin();
                    }
                }
                else
                {
                    filterContext.Result = RedirectToLogin();
                }

            }
            else
            {
                filterContext.Result = RedirectToLogin();
            }
        }

        private RedirectResult RedirectToLogin()
        {
            return new RedirectResult("/login");
        }
    }
}
