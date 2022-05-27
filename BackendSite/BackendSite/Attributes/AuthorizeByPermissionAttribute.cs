using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeByPermissionAttribute : Attribute, IAuthorizationFilter
    {
        public int MenuId { get; set; }
        public int PermissionId { get; set; }
        private readonly NewAuthorizeBLL newAuthorizeBLL;
        private readonly PermissionBLL permissionBLL;
        private readonly AdminUserService adminUserService;

        enum PageStatus
        {
            Allow = 0,
            DeniedReturnResult = 1,
            DeniedGoLoginPage = 2
        }

        public AuthorizeByPermissionAttribute(NewAuthorizeBLL newAuthorizeBLL, AdminUserService adminUserService, int MenuId,int PermissionId)
        {
            this.newAuthorizeBLL = newAuthorizeBLL;
            this.adminUserService = adminUserService;
            this.MenuId = MenuId;
            this.PermissionId = PermissionId;
        }
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if(MenuId != 0 && PermissionId != 0)
            {
                var pageStatus = PageStatus.Allow;
                if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    var authInfo = newAuthorizeBLL.GetAuthData();
                    var loginUser = adminUserService.GetUserV2(authInfo.UserId);
                    if (!loginUser.IsSuper)
                    {
                        if (loginUser.Status == 0)
                        {
                            filterContext.HttpContext.SignOutAsync("backendV2");
                            pageStatus = PageStatus.DeniedGoLoginPage;
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    pageStatus = PageStatus.DeniedGoLoginPage;
                }
            }
        }
    }
}
