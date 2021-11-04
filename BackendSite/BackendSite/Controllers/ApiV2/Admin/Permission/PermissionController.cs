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
        private readonly AdminUserService _adminUserService;
        private readonly PermissionBLL _permissionBll;
        private readonly MenuBLL _menuBll;

        #endregion

        #region Constructor

        public PermissionController(IServiceProvider provider)
        {
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _adminUserService = provider.GetRequiredService<AdminUserService>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _menuBll = provider.GetRequiredService<MenuBLL>();
        }
        
        #endregion
        
        /// <summary>
        /// 取得使用者的權限
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        [HttpPost]
        public ApiResult GetUserPermission()
        {
            var authInfo = _authorizeBll.GetAuthData();
            if (authInfo != null)
            {
                var loginUser = _adminUserService.GetUser(authInfo.UserId);
                if (loginUser.GroupSEQ != null)
                { 
                    var permissions = _permissionBll.GetUserPermission((int) loginUser.GroupSEQ).ToList();
                
                    return new ApiResult<IEnumerable<GroupPermisison>>
                    {
                        Data = permissions,
                        ErrorCode = (int)ErrorCodeEnum.Success
                    };
                }
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            
            return new ApiResult<IEnumerable<GroupPermisison>>
            {
                Message = "Missing cookies",
                ErrorCode = (int)ErrorCodeEnum.Fail
            };
        }
        
        /// <summary>
        /// Home/GetMenu 取得主選單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// 確認登入成功後，打此API取得選單資料。
        /// </remarks>
        [HttpPost]
        public ApiResult GetMenu()
        {
            var data = _menuBll.GetMenuData();

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
    }
}