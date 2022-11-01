using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;


namespace BackendSite.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeByPermissionAttribute : Attribute, IAuthorizationFilter
    {
        public int MenuId { get; set; }
        public int PermissionId { get; set; }
        private readonly NewAuthorizeBLL newAuthorizeBLL;
        private readonly PermissionBLL permissionBLL;
        private readonly AdminBLL adminBll;

        public AuthorizeByPermissionAttribute(NewAuthorizeBLL newAuthorizeBLL, PermissionBLL permissionBLL, AdminBLL adminBll, int MenuId, int PermissionId)
        {
            this.newAuthorizeBLL = newAuthorizeBLL;
            this.permissionBLL = permissionBLL;
            this.adminBll = adminBll;
            this.MenuId = MenuId;
            this.PermissionId = PermissionId;
        }
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if(MenuId != 0 && PermissionId != 0)
            {
                var brand = newAuthorizeBLL.CheckBrand();
                if (brand != 0)
                {
                    if (filterContext.HttpContext.User.Identity?.IsAuthenticated == true)
                    {
                        var authInfo = newAuthorizeBLL.GetAuthData();

                        var loginUser = adminBll.GetUserV2(authInfo.UserId, brand);
                        if (!loginUser.IsSuper)
                        {
                            if (loginUser.Status == 0)
                            {
                                filterContext.Result = new RedirectResult("~/Home/AuthorizedFailed");
                            }
                            else
                            {
                                var userAllPremission = new List<UserAllPremission>();
                                if (loginUser.Role == null)
                                {
                                    userAllPremission = permissionBLL.GetUserAllPremission(null, loginUser.UserId, loginUser.SiteId).ToList();
                                }
                                else
                                {
                                    var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                                    userAllPremission = permissionBLL.GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();
                                }

                                var premissionList = userAllPremission.Find(x => x.MenuId == MenuId);
                                if (premissionList == null)
                                {
                                    filterContext.Result = new RedirectResult("~/Home/AuthorizedFailed");
                                }
                                else
                                {
                                    if (!premissionList.Permission.Exists(x => x == PermissionId))
                                    {
                                        filterContext.Result = new RedirectResult("~/Home/AuthorizedFailed");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        filterContext.Result = new RedirectResult("~/Home/AuthorizedFailed");
                    }
                }
                else
                {
                    filterContext.Result = new RedirectResult("~/Home/AuthorizedFailed");
                }

            }
            else
            {
                filterContext.Result = new RedirectResult("~/Home/AuthorizedFailed");
            }
        }
    }
}
