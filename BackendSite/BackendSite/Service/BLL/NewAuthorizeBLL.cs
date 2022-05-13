using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class NewAuthorizeBLL
    {
        private readonly NewAdminUserService newAdminUserService;
        private readonly Tools tools;
        private readonly IHttpContextAccessor httpContextAccessor;

        public NewAuthorizeBLL(IHttpContextAccessor httpContextAccessor, NewAdminUserService newAdminUserService, Tools tools)
        {
            this.newAdminUserService = newAdminUserService;
            this.tools = tools;
            this.httpContextAccessor = httpContextAccessor;
        }

        public ErrorCodeEnum UserLogin(string userName,
                                              string pwd,
                                              string ip,
                                              string fromUrl,
                                              string userAgent,
                                              ref NewAdminUser adminUser)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return ErrorCodeEnum.AccountInvalid;
            }
            if (string.IsNullOrEmpty(pwd))
            {
                return ErrorCodeEnum.PasswordInvalid;
            }

            adminUser = newAdminUserService.GetAdminUserByUserName(userName);

            if (adminUser == null)
            {
                return ErrorCodeEnum.AccountError;
            }
            else if (adminUser.Userpwd != tools.MD5Encrypt(pwd))
            {
                return ErrorCodeEnum.PasswordError;
            }
            else if (adminUser.Status != (int)AdminUserStatusEnum.Running)
            {
                return ErrorCodeEnum.AccountLocked;
            }
            else
            {
                return ErrorCodeEnum.Success;
            }
        }
    }
}
