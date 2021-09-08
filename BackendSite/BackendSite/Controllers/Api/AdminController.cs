using System;
using System.Collections.Generic;
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
        private readonly PermissionBLL permissionBLL;
        private readonly AdminBLL adminBLL;


        public AdminController(AdminBLL adminBLL, SelLangBLL selLangBLL, PermissionBLL permissionBLL)
        {
            this.adminBLL = adminBLL;
            this.selLangBLL = selLangBLL;
            this.permissionBLL = permissionBLL;
        }

        [HttpPost]
        public ApiResult AddAccount([FromBody]AdminUser adminUser)
        {
            var result = adminBLL.AddAccount(adminUser);
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
            var result = adminBLL.EditAccount(adminUser);
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
            var result = adminBLL.ChangePwdAccount(adminUser);
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

        public ApiResult<IEnumerable<FuncPermissionModel>> GetPermissionDatas()
        {
            var apiResult = new ApiResult<IEnumerable<FuncPermissionModel>>()
            {
                Data = permissionBLL.GetPermissionJSONData(),
                ErrorCode = 0
            };

            return apiResult;
        }

        public ApiResult<SpecialPasswordManagement> GetSpecialPasswordData(GetSpecialPasswordData para)
        {
            var result = new ApiResult<SpecialPasswordManagement>();
            try
            {
                result.Data = adminBLL.GetSpecialPasswordData(para.SiteId);
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        public ApiResult SetSpecialPasswordData(SetSpecialPasswordData para)
        {
            var result = adminBLL.SetSpecialPasswordData(para.SiteId, para.Password, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result > 0)
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
    }
}