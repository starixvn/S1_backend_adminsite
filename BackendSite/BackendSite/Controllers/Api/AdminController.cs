using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
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
                apiResult.Message = selLangBLL.GetResourceValue("lbl_InsertSuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetResourceValue("lbl_InsertFailed");
            }
            return apiResult;
        }
    }
}