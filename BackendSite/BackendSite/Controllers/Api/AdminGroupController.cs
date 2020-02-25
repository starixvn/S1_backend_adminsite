using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminGroupController : Controller
    {
        private readonly SelLangBLL selLangBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly AdminGroupBLL adminGroupBLL;

        public AdminGroupController(AdminGroupBLL adminGroupBLL, SelLangBLL selLangBLL, AuthorizeBLL authorizeBLL)
        {
            this.adminGroupBLL = adminGroupBLL;
            this.selLangBLL = selLangBLL;
            this.authorizeBLL = authorizeBLL;
        }

        [HttpPost]
        public ApiListResult<AdminGroup> GetGroupList()
        {
            var apiResult = new ApiListResult<AdminGroup>();
            var result = adminGroupBLL.GroupList();
            apiResult.Data = result;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        public ApiResult AddGroup(GroupWithPermission GroupInfo)
        {
            var apiResult = new ApiListResult<AdminGroup>();
            GroupInfo.GroupSEQ = null;
            if (adminGroupBLL.CheckGroupIsExists(GroupInfo.GroupName, GroupInfo.GroupSEQ))
            {
                apiResult.ErrorCode = 21;
                apiResult.Message = "Group Name is already taken.";
            }
            else
            {
                try
                {
                    adminGroupBLL.AddGroup(GroupInfo);
                    var result = adminGroupBLL.GroupList();
                    apiResult.Data = result;
                    apiResult.ErrorCode = 0;
                }
                catch (Exception ex)
                {
                    apiResult.ErrorCode = 999;
                    apiResult.Message = "something went wrong.";
                }
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult DeleteGroup(AdminGroup GroupInfo)
        {
            var apiResult = new ApiResult();
            try
            {
                if (GroupInfo.GroupSEQ == null && GroupInfo.GroupSEQ == 0)
                {
                    apiResult.ErrorCode = 21;
                    apiResult.Message = "wrong web-api parameter";
                }
                else
                {
                    adminGroupBLL.DeleteGroup((int)GroupInfo.GroupSEQ);
                    apiResult.ErrorCode = 0;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }
            return apiResult;
        }

        public ApiResult GetEditGroup(AdminGroup GroupInfo)
        {
            var apiResult = new ApiListResult<FuncPermissionModel>();
            try
            {
                apiResult.Data = adminGroupBLL.GetGroupPermissions((int)GroupInfo.GroupSEQ);
                apiResult.ErrorCode = 0;
            }
            catch (Exception)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong";
            }

            return apiResult;
        }

        public ApiResult UpdateGroup(GroupWithPermission GroupInfo)
        {
            var apiResult = new ApiResult();
            if (GroupInfo.GroupSEQ == null || GroupInfo.GroupSEQ == 0)
            {
                apiResult.ErrorCode = 21;
                apiResult.Message = "wrong web-api parameter";
            }
            else
            {
                try
                {
                    adminGroupBLL.UpdateGroup(GroupInfo);
                    apiResult.ErrorCode = 0;
                }
                catch (Exception ex)
                {
                    apiResult.ErrorCode = 999;
                    apiResult.Message = "something went wrong";
                }
            }

            return apiResult;
        }
    }
}
