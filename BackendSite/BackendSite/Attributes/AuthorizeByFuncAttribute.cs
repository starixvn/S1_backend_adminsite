using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;

namespace BackendSite.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeByFuncAttribute : Attribute, IAuthorizationFilter
    {
        public string FuncKey { get; set; }
        public string SubFuncKey { get; set; }
        private readonly AuthorizeBLL authorizeBLL;
        private readonly AdminUserService adminUserService;
        private readonly PermissionBLL permissionBLL;
        
        public AuthorizeByFuncAttribute(AuthorizeBLL authorizeBLL, AdminUserService adminUserService, PermissionBLL permissionBLL, string FuncKey = null, string SubFuncKey=null)
        {
            this.authorizeBLL = authorizeBLL;
            this.adminUserService = adminUserService;
            this.permissionBLL = permissionBLL;
            this.FuncKey = FuncKey;
            this.SubFuncKey = SubFuncKey;
        }

        private bool IsJsonResult(string ContentType)
        {
            return ContentType != null && (ContentType.Contains("*/*") || ContentType.Contains("application/json"));
        }

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (!string.IsNullOrEmpty(FuncKey))
            {
                var pageStatus = PageStatus.Allow;
                if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    var authInfo = authorizeBLL.GetAuthData();
                    var loginUser = adminUserService.GetUser(authInfo.UserId);
                    if (!loginUser.IsSuper)
                    {
                        if (loginUser.Status == 0)
                        {
                            filterContext.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            pageStatus = PageStatus.DeniedGoLoginPage;
                        }
                        else if (loginUser.GroupSEQ == null)
                        {
                            pageStatus = PageStatus.DeniedReturnResult;
                        }
                        else
                        {
                            var permissions = permissionBLL.GetUserPermission((int)loginUser.GroupSEQ);
                            if (string.IsNullOrEmpty(SubFuncKey))
                            {
                                pageStatus = !permissions.Any(x => string.Equals(x.FuncKey, FuncKey, StringComparison.OrdinalIgnoreCase)) ? PageStatus.DeniedReturnResult : PageStatus.Allow;
                            }
                            else
                            {
                                pageStatus = !permissions.Any(x => 
                                    string.Equals(x.FuncKey, FuncKey, StringComparison.OrdinalIgnoreCase) 
                                    && !string.IsNullOrEmpty(x.SubFuncKeys) 
                                    && JsonConvert.DeserializeObject<string[]>(x.SubFuncKeys).Any(s=> string.Equals(s, SubFuncKey, StringComparison.OrdinalIgnoreCase))
                                    ) ? PageStatus.DeniedReturnResult : PageStatus.Allow;
                            }
                        }
                    }
                }
                else
                {
                    pageStatus = PageStatus.DeniedGoLoginPage;
                }

                if (pageStatus != PageStatus.Allow)
                {
                    if (IsJsonResult(filterContext.HttpContext.Request.ContentType))
                    {
                        var response = filterContext.HttpContext.Response;
                        response.ContentType = "application/json";
                        filterContext.Result = new JsonResult(new { errorCode = 403, message = "Permission denied." });
                    }
                    else
                    {
                        switch (pageStatus)
                        {
                            case PageStatus.DeniedReturnResult:
                                var result = new ContentResult
                                {
                                    Content = "Permission denied."
                                };
                                filterContext.Result = result;
                                break;

                            case PageStatus.DeniedGoLoginPage:
                                filterContext.Result = new RedirectResult("~/AccessDenied/RefreshPage");
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        enum PageStatus
        {
            Allow = 0,
            DeniedReturnResult = 1,
            DeniedGoLoginPage = 2
        }
    }
}
