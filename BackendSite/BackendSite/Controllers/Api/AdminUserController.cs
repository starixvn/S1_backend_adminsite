using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Attributes;

namespace BackendSite.Controllers.Api
{
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/UserAuthoritySetting" })]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminUserController : Controller
    {
        private readonly AdminUserBLL adminUserBLL;
        private readonly AdminGroupBLL adminGroupBLL;
        private readonly ServerInfoBLL serverInfoBLL;
        public AdminUserController(AdminUserBLL adminUserBLL, ServerInfoBLL serverInfoBLL, AdminGroupBLL adminGroupBLL)
        {
            this.adminUserBLL = adminUserBLL;
            this.serverInfoBLL = serverInfoBLL;
            this.adminGroupBLL = adminGroupBLL;
        }

        public ApiResult<IEnumerable<AdminUser>> GetUserList()
        {
            var apiResult = new ApiResult<IEnumerable<AdminUser>>();
            try
            {
                apiResult.Data = adminUserBLL.GetUserList();
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        public ApiResult<IEnumerable<DisplayList>> GetSiteList([FromForm]int UserId)
        {
            var apiResult = new ApiResult<IEnumerable<DisplayList>>();
            try
            {
                var assignSites = adminUserBLL.GetAssignSiteList(UserId);
                var allSiteList = serverInfoBLL.GetSiteList();
                apiResult.Data = allSiteList.Select(x => new DisplayList() { Id = x.SiteId, DisplayText = x.SiteName, Selected = assignSites.Any(y => y.SiteId == x.SiteId) });
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        public ApiResult<IEnumerable<DisplayList>> GetAllSiteList()
        {
            var apiResult = new ApiResult<IEnumerable<DisplayList>>();
            try
            {
                var allSiteList = serverInfoBLL.GetSiteList();
                apiResult.Data = allSiteList.Select(x => new DisplayList() { Id = x.SiteId, DisplayText = x.SiteName });
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        public ApiResult AssignSiteList([FromForm]int UserId, [FromForm]IEnumerable<int> SiteIdList)
        {
            var apiResult = new ApiResult();
            try
            {
                adminUserBLL.AssignSiteList(UserId, SiteIdList);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        public ApiResult<IEnumerable<DisplayList>> GetAllGroupList()
        {
            var apiResult = new ApiResult<IEnumerable<DisplayList>>();
            try
            {
                var groupList = adminGroupBLL.GetGroupList();
                apiResult.Data = groupList.Select(g => new DisplayList() { Id = (int)g.GroupSEQ, DisplayText = g.GroupName });
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }

        public ApiResult AssignGroup([FromForm]int UserId, [FromForm]int? GroupSEQ)
        {
            var apiResult = new ApiResult();
            try
            {
                adminUserBLL.AssignGroup(UserId, GroupSEQ);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        public ApiResult UpdatePassword([FromForm]int UserId, [FromForm]string Password)
        {
            var apiResult = new ApiResult();
            try
            {
                if (string.IsNullOrEmpty(Password))
                {
                    apiResult.ErrorCode = 21;
                    apiResult.Message = "password can not be null.";
                }
                else
                {
                    adminUserBLL.SetUserPassword(UserId, Password);
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

        public ApiResult UpdateStatus([FromForm]int UserId, [FromForm]int Status)
        {
            var apiResult = new ApiResult();
            try
            {
                if (Status == 1 || Status == 0)
                {
                    adminUserBLL.SetUserStatus(UserId, Status);
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

        public ApiResult<IEnumerable<AdminUser>> AddUser([FromForm]string UserName, [FromForm]string Userpwd, [FromForm]int[] SiteList, [FromForm]int? GroupSEQ)
        {
            var apiResult = new ApiResult<IEnumerable<AdminUser>>();
            try
            {
                var newUserList = adminUserBLL.AddUser(UserName, Userpwd, SiteList, GroupSEQ);
                if (newUserList != null)
                {
                    apiResult.Data = adminUserBLL.GetUserList();
                    apiResult.ErrorCode = 0;
                }
                else
                {
                    apiResult.ErrorCode = 21;
                    apiResult.Message = "something went wrong.";
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

        public class DisplayList
        {
            public int Id { get; set; }
            public string DisplayText { get; set; }
            public bool Selected { get; set; }
        }
    }
}