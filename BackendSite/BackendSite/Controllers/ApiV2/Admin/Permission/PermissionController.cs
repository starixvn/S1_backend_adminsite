using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Admin.Permission
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        #region Fields

        private readonly AuthorizeBLL _authorizeBll;
        private readonly AdminBLL _adminBll;
        private readonly PermissionBLL _permissionBll;
        private readonly MenuBLL _menuBll;
        private readonly ServerInfoBLL _serverInfoBll;

        #endregion

        #region Constructor

        public PermissionController(IServiceProvider provider)
        {
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _adminBll = provider.GetRequiredService<AdminBLL>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _menuBll = provider.GetRequiredService<MenuBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }

        #endregion

        /// <summary>
        /// Permission/GetAllPermission 取得所有的權限
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        [HttpPost]
        public ApiResult GetAllPermission()
        {
            var authInfo = _authorizeBll.GetAuthData();

            if (authInfo == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies"
                };
            }

            var loginUser = _adminBll.GetUser(authInfo.UserId);

            if (loginUser == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies.LoginUser is null."
                };
            }

            var permissionData = _permissionBll.GetPermissionJSONData();

            if (!permissionData.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            return new ApiResult<IEnumerable<FuncPermissionModel>>
            {
                Data = permissionData,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

        }

        /// <summary>
        /// Permission/GetAllMenu 取得主選單所有資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        [HttpPost]
        public ApiResult GetAllMenu()
        {
            var data = _menuBll.GetMenuData(true, out var errorCodeEnum);

            if (errorCodeEnum == ErrorCodeEnum.Fail)
            {
                return new ApiResult
                {
                    ErrorCode = (int)errorCodeEnum,
                    Message = "Missing cookies"
                };
            }

            if (data.Count == 0)
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            return new ApiResult<IEnumerable<MenuItem>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Permission/GetUserPermission 取得使用者的權限
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        [HttpPost]
        public ApiResult GetUserPermission()
        {
            var authInfo = _authorizeBll.GetAuthData();

            if (authInfo == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies"
                };
            }

            var loginUser = _adminBll.GetUser(authInfo.UserId);

            if (loginUser == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies.LoginUser is null."
                };
            }

            if (loginUser.GroupSEQ == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies.LoginUser's Group is null."
                };
            }

            var permissions = _permissionBll.GetUserPermission((int)loginUser.GroupSEQ);

            return new ApiResult<IEnumerable<GroupPermisison>>
            {
                Data = permissions,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Permission/GetUserMenu 取得使用者主選單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        [HttpPost]
        public ApiResult GetUserMenu()
        {
            var data = _menuBll.GetMenuData(false, out var errorCodeEnum);

            if (errorCodeEnum == ErrorCodeEnum.Fail)
            {
                return new ApiResult
                {
                    ErrorCode = (int)errorCodeEnum,
                    Message = "Missing Cookie."
                };
            }

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            return new ApiResult<IEnumerable<MenuItem>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Permission/GetUserSiteList 取得使用者可使用的品牌清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.
        /// 每個程式在需要選取品牌條件時，都需要確認有哪些品牌可以選擇。
        /// 
        /// </remarks>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<IEnumerable<ServerInfo>> GetUserSiteList()
        {
            var apiResult = new ApiResult<IEnumerable<ServerInfo>>();
            try
            {
                apiResult.Data = _serverInfoBll.GetSiteListByLoginUser();
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
    }
}