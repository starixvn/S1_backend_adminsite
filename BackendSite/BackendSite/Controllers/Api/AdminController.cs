using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly SelLangBLL selLangBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly AdminBLL adminBLL;


        public AdminController(AdminBLL adminBLL, SelLangBLL selLangBLL, AuthorizeBLL authorizeBLL)
        {
            this.adminBLL = adminBLL;
            this.selLangBLL = selLangBLL;
            this.authorizeBLL = authorizeBLL;
        }

        [HttpPost]
        public ApiResult AddAccount([FromBody]AdminUser adminUser)
        {
            bool result = adminBLL.AddAccount(adminUser);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_InsertSuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetMsg("msg_InsertFailed");
            }
            return apiResult;
        }

        [HttpPost]
        public ApiResult EditAccount([FromBody]AdminUser adminUser)
        {
            bool result = adminBLL.EditAccount(adminUser);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        [HttpPost]
        public ApiResult ChangePwdAccount([FromBody]AdminUser adminUser)
        {
            bool result = adminBLL.ChangePwdAccount(adminUser);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdatesuccessfulNewpwd");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
        

        [HttpPost]
        public ApiResult ChangePwd([FromBody]ChangePwd changePwd)
        {
            int response = 0;
            bool result = adminBLL.ChangePwd(changePwd, out response);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                if (response == 8)
                {
                    apiResult.ErrorCode = 8;
                    apiResult.Message = selLangBLL.GetMsg("msg_OldpwdNotMatch");
                }
                else
                {
                    apiResult.ErrorCode = 9;
                    apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
                }
            }
            return apiResult;
        }

    }
}