using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class AuthorizeBLL
    {
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly AdminUserService adminUserService;
        private readonly Tools tools;

        public AuthorizeBLL(ServerInfoBLL serverInfoBLL, AdminUserService adminUserService, Tools tools)
        {
            this.serverInfoBLL = serverInfoBLL;
            this.adminUserService = adminUserService;
            this.tools = tools;
        }
        public int UserLogin(string userName, string pwd, string ip, string fromUrl, string userAgent, ref AdminUser adminUser)
        {
            var serverInfo = serverInfoBLL.GetServerInfo();
            adminUser = adminUserService.GetAdminUserByUserName(userName, serverInfo.SiteId);

            if (adminUser == null)
            {
                return -1;
            }
            else if (adminUser.Userpwd != tools.MD5Encrypt(pwd))
            {
                return -2;
            }
            else if (adminUser.Status == (int)AdminUserStatus.Suspend || adminUser.Status == (int)AdminUserStatus.Closed)
            {
                return -3;
            }
            else
            {
                return 0;
            }
        }
    }
}
