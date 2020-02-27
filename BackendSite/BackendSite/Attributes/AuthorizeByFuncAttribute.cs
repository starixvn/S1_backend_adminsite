using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace BackendSite.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeByFuncAttribute : Attribute, IAuthorizationFilter
    {
        public string FuncKey { get; set; }
        private readonly AuthorizeBLL authorizeBLL;
        private readonly AdminUserService adminUserService;
        private readonly PermissionBLL permissionBLL;
        
        public AuthorizeByFuncAttribute(AuthorizeBLL authorizeBLL, AdminUserService adminUserService, PermissionBLL permissionBLL, string FuncKey = null)
        {
            this.authorizeBLL = authorizeBLL;
            this.adminUserService = adminUserService;
            this.permissionBLL = permissionBLL;
            this.FuncKey = FuncKey;
        }

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            var authorizeResult = filterContext.Result;
            var isAuthorized = true;
            if (authorizeResult is RedirectToRouteResult)
            {
                var temp = authorizeResult as RedirectToRouteResult;
                temp.RouteValues.TryGetValue("controller", out var objFromController);
                var fromController = objFromController == null ? "" : objFromController as string;
                isAuthorized = !string.Equals(fromController, "auth", StringComparison.OrdinalIgnoreCase);
            }

            if (isAuthorized && !string.IsNullOrEmpty(FuncKey) && filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var authInfo = authorizeBLL.GetAuthData();
                var loginUser = adminUserService.GetUser(authInfo.UserId);
                if (!loginUser.IsSuper)
                {
                    if (loginUser.Status == 0)
                    {
                        filterContext.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        filterContext.Result = new RedirectResult("~/Home/RefreshPage");
                    }
                    else if (loginUser.GroupSEQ == null)
                    {
                        ContentResult result = new ContentResult();
                        result.Content = "Permission denied.";
                        filterContext.Result = result;
                    }
                    else
                    {
                        var permissions = permissionBLL.GetUserPermission((int)loginUser.GroupSEQ);
                        if(!permissions.Any(x=> string.Equals(x.FuncKey, FuncKey, StringComparison.OrdinalIgnoreCase)))
                        {
                            ContentResult result = new ContentResult();
                            result.Content = "Permission denied.";
                            filterContext.Result = result;
                        }
                    }
                }
            }
        }
    }
}
