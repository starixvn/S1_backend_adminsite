using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class AdminBLL
    {
        private readonly Tools tools;
        private readonly AdminService adminService;
        private readonly ServerInfoService serverInfoService;

        public AdminBLL(Tools tools, AdminService adminService, ServerInfoService serverInfoService)
        {
            this.tools = tools;
            this.adminService = adminService;
            this.serverInfoService = serverInfoService;
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
    }
}
