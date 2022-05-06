using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Controllers.ApiV2.Admin.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        #region Fields

        private readonly AdminUserBLL _adminUserBll;
        private readonly AdminGroupBLL _adminGroupBll;
        private readonly ServerInfoBLL _serverInfoBll;

        #endregion

        #region Constructors

        public UserManagementController(IServiceProvider provider)
        {
            _adminUserBll = provider.GetRequiredService<AdminUserBLL>();
            _adminGroupBll = provider.GetRequiredService<AdminGroupBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }

        #endregion

        #region API Model

        public class GetByUserIdRequest
        {
            public int UserId { get; set; }
        }

        public class UpdateSiteListByUserIdRequest : GetByUserIdRequest
        {
            public IEnumerable<int> SiteIdList { get; set; }
        }

        public class UpdateStatusRequest : GetByUserIdRequest
        {
            /// <summary>
            /// 狀態(0=Disable、1=Enable)
            /// </summary>
            public int Status { get; set; }
        }

        public class UpdateUserGroupByUserIdRequest : GetByUserIdRequest
        {
            public int GroupId { get; set; }
        }

        public class UpdatePasswordByUserIdRequest : GetByUserIdRequest
        {
            public string Password { get; set; }
        }

        public class CreateUserRequest
        {
            public string UserName { get; set; }
            public string UserPwd { get; set; }
            public IEnumerable<int> SiteIdList { get; set; }
            public int GroupId { get; set; }
        }

        #endregion

        #region Actions

        /// <summary>
        /// UserManagement/GetList 取得系統使用者清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > User Setting
        /// </remarks>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/UserAuthoritySetting" })]
        [HttpPost]
        public ApiListResult<AdminUserWithAllowSite> GetList(ApiRequestPageModel request)
        {
            var result = new List<AdminUserWithAllowSite>();
            var data = _adminUserBll.GetUserList(request.PageNumber, request.PageSize, out int totalPages, out int rowCount);
            var allSiteList = _serverInfoBll.GetSiteList();

            foreach (var user in data)
            {
                var newData = new AdminUserWithAllowSite();

                var assignSites = _adminUserBll.GetAssignSiteList(user.UserId).Select(x => x.SiteId);

                var allowSiteList = allSiteList.Where(o => assignSites.Contains(o.SiteId)).Select(t => new { t.SiteId, t.SiteName })
                   .ToDictionary(t => t.SiteId, t => t.SiteName);

                result.Add(new AdminUserWithAllowSite()
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Userpwd = user.Userpwd,
                    IsSuper = user.IsSuper,
                    Status = user.Status,
                    CreateTime = user.CreateTime,
                    GroupSEQ = user.GroupSEQ,
                    GroupName = user.GroupName,
                    AllowSiteList = allowSiteList
                });
            }

            if (!result.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            return new ApiListResult<AdminUserWithAllowSite>()
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount
            };
        }

        /// <summary>
        /// UserManagement/CreateUser 新增系統使用者
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > User Setting > [Add User] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/UserAuthoritySetting" })]
        [HttpPost]
        public ApiResult CreateUser(CreateUserRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<AdminUser>>();

            var newUserList = _adminUserBll.AddUser(request.UserName, request.UserPwd, request.SiteIdList, request.GroupId);
            if (newUserList != null)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }

            return apiResult;
        }

        /// <summary>
        /// UserManagement/UpdateStatus 更新系統使用者狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > User Setting > Enable > Click [V] or [ ]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/UserAuthoritySetting" })]
        [HttpPost]
        public ApiResult UpdateStatus(UpdateStatusRequest request)
        {
            var apiResult = new ApiResult();

            if (request.Status == 1 || request.Status == 0)
            {
                _adminUserBll.SetUserStatus(request.UserId, request.Status);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }

            return apiResult;
        }

        /// <summary>
        /// UserManagement/UpdateGroup 更新使用者權限群組
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > User Setting > Permission > Group > [Edit] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/UserAuthoritySetting" })]
        [HttpPost]
        public ApiResult UpdateGroup(UpdateUserGroupByUserIdRequest request)
        {
            var apiResult = new ApiResult();

            _adminUserBll.AssignGroup(request.UserId, request.GroupId);
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        /// <summary>
        /// UserManagement/GetAllowedSiteListByUserId 取得使用者可允許的品牌清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > User Setting > Permission > Site > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/UserAuthoritySetting" })]
        [HttpPost]
        public ApiResult<IEnumerable<Api.AdminUserController.DisplayList>> GetAllowedSiteListByUserId(GetByUserIdRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<Api.AdminUserController.DisplayList>>();

            var assignSites = _adminUserBll.GetAssignSiteList(request.UserId);
            var allSiteList = _serverInfoBll.GetSiteList();
            apiResult.Data = allSiteList.Select(x => new Api.AdminUserController.DisplayList() { Id = x.SiteId, DisplayText = x.SiteName, Selected = assignSites.Any(y => y.SiteId == x.SiteId) });
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// UserManagement/UpdateAllowedSiteList 更新使用者可允許的品牌清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > User Setting > Permission > Site > [Edit] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/UserAuthoritySetting" })]
        [HttpPost]
        public ApiResult UpdateAllowedSiteList(UpdateSiteListByUserIdRequest request)
        {
            var apiResult = new ApiResult();

            _adminUserBll.AssignSiteList(request.UserId, request.SiteIdList);
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        /// <summary>
        /// UserManagement/UpdatePassword 更新使用者密碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > User Setting > Password > [Password] > [Submit] 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/UserAuthoritySetting" })]
        [HttpPost]
        public ApiResult UpdatePassword(UpdatePasswordByUserIdRequest request)
        {
            var apiResult = new ApiResult();

            if (string.IsNullOrEmpty(request.Password))
            {
                apiResult.ErrorCode = 21;
                apiResult.Message = "password can not be null.";
            }
            else
            {
                _adminUserBll.SetUserPassword(request.UserId, request.Password);
                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }

        #endregion

    }
}
