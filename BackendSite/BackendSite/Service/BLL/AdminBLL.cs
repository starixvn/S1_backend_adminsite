using BackendSite.ExceptionHandling;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace BackendSite.Service.BLL
{
    public class AdminBLL
    {
        private readonly Tools tools;
        private readonly AdminService adminService;
        private readonly AdminUserService adminUserService;
        private readonly ServerInfoService serverInfoService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AdminBLL(Tools tools,
            AdminService adminService,
            ServerInfoService serverInfoService,
            AdminUserService adminUserService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.tools = tools;
            this.adminService = adminService;
            this.serverInfoService = serverInfoService;
            this.adminUserService = adminUserService;
            this.httpContextAccessor = httpContextAccessor;
        }

        //暫無使用
        //public bool AddAccount(AdminUser adminUser)
        //{
        //    var serverInfo = serverInfoService.GetModel();
        //    adminUser.Userpwd = tools.MD5Encrypt(adminUser.Userpwd);
        //    return adminService.AddAccount(serverInfo.SiteId, adminUser);
        //}

        //暫無使用
        //public IEnumerable<AdminUser> AccountList()
        //{
        //    var serverInfo = serverInfoService.GetModel();
        //    return adminService.AccountList(serverInfo.SiteId);
        //}

        //暫無使用
        //public AdminUser GetAccount(int userId)
        //{
        //    var serverInfo = serverInfoService.GetModel();
        //    return adminService.GetAccount(serverInfo.SiteId, userId);
        //}

        //暫無使用
        //public bool EditAccount(AdminUser adminUser)
        //{
        //    var serverInfo = serverInfoService.GetModel();
        //    return adminService.EditAccount(serverInfo.SiteId, adminUser);
        //}

        //暫無使用
        //public bool ChangePwdAccount(AdminUser adminUser)
        //{
        //    var serverInfo = serverInfoService.GetModel();
        //    if (string.IsNullOrWhiteSpace(adminUser.Userpwd))
        //        adminUser.Userpwd = tools.MD5Encrypt("123456");
        //    else
        //        adminUser.Userpwd = tools.MD5Encrypt(adminUser.Userpwd);
        //    return adminService.ChangePwdAccount(serverInfo.SiteId, adminUser);
        //}

        public AdminUser GetUser(int userId)
        {
            return adminUserService.GetUser(userId);
        }

        public ErrorCodeEnum ChangePwd(ChangePwd changePwd)
        {
            string Md5Userpwd = null;
            var serverInfo = serverInfoService.GetModel();
            var userId = 0;
            int.TryParse(httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier).Value, out userId);
            changePwd.OldPwd = tools.MD5Encrypt(changePwd.OldPwd);
            changePwd.NewPwd = tools.MD5Encrypt(changePwd.NewPwd);

            if (adminService.ChangePwd(serverInfo.SiteId, userId, changePwd, out Md5Userpwd))
            {
                return ErrorCodeEnum.Success;
            }
            else
            {
                if (Md5Userpwd != changePwd.OldPwd)
                    return ErrorCodeEnum.OldpwdNotMatch;
                else
                    return ErrorCodeEnum.Fail;
            }
        }
        public SpecialPasswordManagement GetSpecialPasswordData(int sitId)
        {
            return adminService.GetSpecialPasswordData(sitId);
        }

        public int SetSpecialPasswordData(int SiteId, string Password, string Operator)
        {
            return adminService.SetSpecialPasswordData(SiteId, Password, Operator);
        }

        public AdminUserV2 GetUserV2(int userId)
        {
            return adminUserService.GetUserV2(userId);
        }
    }
}
