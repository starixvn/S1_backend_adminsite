using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.AdminMenu;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Admin.Permission
{
    [Authorize(AuthenticationSchemes = "backendV2")]
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

        #region API Model

        public class GetUserPermissionResponseModel
        {
            public bool IsSuper { get; set; }
            public IEnumerable<GroupPermisison> PermissionDatas { get; set; }
        }

        public class GetMenuRequestModel
        {
            /// <summary>
            /// 語系
            /// </summary>
            public string Lang { get; set; }
        }

        #endregion

        /// <summary>
        /// Permission/GetAllPermission 取得所有的權限
        /// </summary>
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
        /// 
        /// Http Method：POST
        /// 
        /// Sample request:(舊選單)
        ///
        ///     {
        ///        "lang": null
        ///     }
        ///
        /// Sample request:(新選單)
        ///
        ///     {
        ///        "lang": ""
        ///     }
        ///     
        ///     {
        ///        "lang": "en"
        ///     }
        /// </remarks>
        [HttpPost]
        public async Task<ApiResult> GetAllMenuAsync(GetMenuRequestModel request)
        {
            if (request.Lang == null)
            {
                //old Menu
                var data = _menuBll.GetMenuData(true, out var errorCodeEnum);

                if (errorCodeEnum == ErrorCodeEnum.Fail)
                {
                    return new ApiResult
                    {
                        ErrorCode = (int)errorCodeEnum,
                        Message = "Missing cookies"
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
            else
            {
                //New Menu
                var data = await _adminBll.GetAllAdminMenuAsync(request.Lang);

                if (!data.Any())
                {
                    throw new AdminException(ErrorCodeEnum.NoData);
                }

                return new ApiResult<List<AdminMenuModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// Permission/GetAllSiteList 取得所有品牌清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.Save in cache.
        /// 
        /// </remarks>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult GetAllSiteList()
        {
            var data = _serverInfoBll.GetSiteList();

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<ServerInfo>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
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
            IEnumerable<GroupPermisison> result = null;
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

            if (loginUser.IsSuper)
            {
                result = _permissionBll.GetAllPermission();
            }
            else
            {
                result = _permissionBll.GetUserPermission((int)loginUser.GroupSEQ);
            }

            if (!result.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            return new ApiResult<GetUserPermissionResponseModel>
            {
                Data = new GetUserPermissionResponseModel()
                {
                    IsSuper = loginUser.IsSuper,
                    PermissionDatas = result
                },
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Permission/GetUserMenu 取得使用者主選單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Sample request:(舊選單)
        ///
        ///     {
        ///        "lang": null
        ///     }
        ///
        /// Sample request:(新選單)
        ///
        ///     {
        ///        "lang": ""
        ///     }
        ///     
        ///     {
        ///        "lang": "en"
        ///     }
        /// </remarks>
        [HttpPost]
        public async Task<ApiResult> GetUserMenuAsync(GetMenuRequestModel request)
        {
            if (request.Lang == null)
            {
                //old menu
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
            else
            {
                //new menu(only for super)
                //New Menu
                var data = await _adminBll.GetAllAdminMenuAsync(request.Lang);

                if (!data.Any())
                {
                    throw new AdminException(ErrorCodeEnum.NoData);
                }

                return new ApiResult<List<AdminMenuModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
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