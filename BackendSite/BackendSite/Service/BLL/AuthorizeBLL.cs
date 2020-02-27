using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class AuthorizeBLL
    {
        private readonly AdminUserService adminUserService;
        private readonly Tools tools;
        private readonly IHttpContextAccessor httpContextAccessor;
        
        public AuthorizeBLL()
        {

        }

        public AuthorizeBLL(IHttpContextAccessor httpContextAccessor, AdminUserService adminUserService, Tools tools)
        {
            this.adminUserService = adminUserService;
            this.tools = tools;
            this.httpContextAccessor = httpContextAccessor;
        }

        public int UserLogin(string userName, string pwd, string ip, string fromUrl, string userAgent, ref AdminUser adminUser)
        {
            adminUser = adminUserService.GetAdminUserByUserName(userName);

            if (adminUser == null)
            {
                return -1;
            }
            else if (adminUser.Userpwd != tools.MD5Encrypt(pwd))
            {
                return -2;
            }
            else if (adminUser.Status != (int)AdminUserStatus.Running)
            {
                return -3;
            }
            else
            {
                return 0;
            }
        }

        public AuthData GetAuthData()
        {
            AuthData authData = null;
            if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                authData = new AuthData();
                int userId = 0;
                int.TryParse(httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier).Value, out userId);
                authData.UserId = userId;
                authData.UserName = httpContextAccessor.HttpContext.User.Identity.Name;
                //authData.Token = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(m => m.Type == "Token").Value;
                //authData.LangId = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(m => m.Type == "LangId").Value;
            }

            return authData;
        }
    }
}
