using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class AdminUserBLL
    {
        private readonly Tools tools;
        private readonly AdminUserService adminUserService;

        public AdminUserBLL(AdminUserService adminUserService, Tools tools)
        {
            this.adminUserService = adminUserService;
            this.tools = tools;
        }

        public IEnumerable<AdminUser> GetUserList()
        {
            return adminUserService.GetUserList();
        }

        public IEnumerable<AssignSites> GetAssignSiteList(int UserId)
        {
            return adminUserService.GetAssignSiteList(UserId);
        }

        public void AssignSiteList(int UserId, IEnumerable<int> SiteIds)
        {
            adminUserService.AssignSiteList(UserId, SiteIds);
        }

        public IEnumerable<AssignGroups> GetAssignGroupList(int UserId)
        {
            return adminUserService.GetAssignGroupList(UserId);
        }

        public void AssignGroup(int UserId, int? GroupSEQ)
        {
            adminUserService.AssignGroup(UserId, GroupSEQ);
        }

        public void SetUserPassword(int UserId, string Password)
        {
            string MD5Pwd = tools.MD5Encrypt(Password);
            adminUserService.SetUserPassword(UserId, MD5Pwd);
        }

        public void SetUserStatus(int UserId, int Status)
        {
            adminUserService.SetUserStatus(UserId, Status);
        }

        public IEnumerable<AdminUser> AddUser(string UserName, string Userpwd, int[] SiteList)
        {
            string MD5Pwd = tools.MD5Encrypt(Userpwd);
            return adminUserService.AddUser(UserName, MD5Pwd, SiteList);
        }
    }
}
