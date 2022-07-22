using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/GroupAuthoritySetting" })]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AdminGroupController : Controller
    {
        private readonly AdminGroupBLL adminGroupBLL;
        private readonly PermissionBLL permissionBLL;

        public AdminGroupController(AdminGroupBLL adminGroupBLL, PermissionBLL permissionBLL)
        {
            this.adminGroupBLL = adminGroupBLL;
            this.permissionBLL = permissionBLL;
        }

        [HttpPost]
        public ApiResult<IEnumerable<AdminGroup>> GetGroupList()
        {
            var apiResult = new ApiResult<IEnumerable<AdminGroup>>
            {
                Data = adminGroupBLL.GetGroupList(1, 1000, out _, out _),
                ErrorCode = 0
            };
            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<AdminGroup>> AddGroup(GroupWithPermission GroupInfo)
        {
            var apiResult = new ApiResult<IEnumerable<AdminGroup>>();
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
                    var result = adminGroupBLL.GetGroupList(1, 1000, out _, out _);
                    apiResult.Data = result;
                    apiResult.ErrorCode = 0;
                }
                catch (Exception ex)
                {
                    apiResult.ErrorCode = 999;
                    apiResult.Message = "something went wrong.";
                    apiResult.RawMessage = ex.Message;
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
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }

        public ApiResult<IEnumerable<FuncPermissionModel>> GetEditGroup(AdminGroup GroupInfo)
        {
            var apiResult = new ApiResult<IEnumerable<FuncPermissionModel>>();
            try
            {
                apiResult.Data = permissionBLL.GetGroupPermissionModel((int)GroupInfo.GroupSEQ);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong";
                apiResult.RawMessage = ex.Message;
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
                    apiResult.RawMessage = ex.Message;
                }
            }

            return apiResult;
        }
    }
}
