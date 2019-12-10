using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class AdminBLL
    {
        private readonly Tools tools;
        private readonly AdminService adminService;
        private readonly ServerInfoService serverInfoService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AdminBLL(Tools tools, AdminService adminService, ServerInfoService serverInfoService, IHttpContextAccessor httpContextAccessor)
        {
            this.tools = tools;
            this.adminService = adminService;
            this.serverInfoService = serverInfoService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public bool AddAccount(AdminUser adminUser)
        {
            var serverInfo = serverInfoService.GetModel();
            adminUser.Userpwd = tools.MD5Encrypt(adminUser.Userpwd);
            return adminService.AddAccount(serverInfo.SiteId, adminUser);
        }

        public IEnumerable<AdminUser> AccountList()
        {
            var serverInfo = serverInfoService.GetModel();
            return adminService.AccountList(serverInfo.SiteId);
        }

        public AdminUser GetAccount(int userId)
        {
            var serverInfo = serverInfoService.GetModel();
            return adminService.GetAccount(serverInfo.SiteId, userId);
        }

        public bool EditAccount(AdminUser adminUser)
        {
            var serverInfo = serverInfoService.GetModel();
            return adminService.EditAccount(serverInfo.SiteId, adminUser);
        }
        
        public bool ChangePwdAccount(AdminUser adminUser)
        {
            var serverInfo = serverInfoService.GetModel();
            if (string.IsNullOrWhiteSpace(adminUser.Userpwd))
                adminUser.Userpwd = tools.MD5Encrypt("123456");
            else
                adminUser.Userpwd = tools.MD5Encrypt(adminUser.Userpwd);
            return adminService.ChangePwdAccount(serverInfo.SiteId, adminUser);
        }

        public bool ChangePwd(ChangePwd changePwd, out int response)
        {
            string Md5Userpwd = null;
            var serverInfo = serverInfoService.GetModel();
            int userId = 0;
            int.TryParse(httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier).Value, out userId);
            changePwd.Oldpwd = tools.MD5Encrypt(changePwd.Oldpwd);
            changePwd.NewPwd = tools.MD5Encrypt(changePwd.NewPwd);

            if (adminService.ChangePwd(serverInfo.SiteId, userId, changePwd, out Md5Userpwd))
            {
                response = 0;
                return true;
            }
            else
            {
                if (Md5Userpwd != changePwd.Oldpwd)
                    response = 8;
                else
                    response = 9;
                return false;
            }
        }
    }
}
