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
using Newtonsoft.Json;

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

        #region API Model

        public class GetAddRolePermissionReq
        {
            public int SiteId { get; set; }
        }
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

        public class PermissionReq
        {
            public int Role { get; set; }
            public int SiteId { get; set; }
        }

        public class GetRoleListReq
        {
            public string RoleName { get; set; }
            public int SiteId { get; set; }
        }

        public class SetRolePermissionReq
        {
            public string Name { get; set; }
            public string Mark { get; set; }
            public int? Role { get; set; }
            public List<int> CurrencyList { get; set; }
            public List<SetPermission> Permission { get; set; }
            public int SiteId { get; set; }
        }

        public class AdminUserReq
        {
            public int UserId { get; set; }
            public int SiteId { get; set; }
        }

        public class GetAdminUserListReq
        {
            public string UserName { get; set; }
            public int SiteId { get; set; }
        }

        //public class SetPermission
        //{
        //    public int DisPlayKey { get; set; }
        //    public List<int> PermissionIdList { get; set; }
        //}

        public class SetAdminUserReq : AdminUserReq
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public int Role { get; set; }
            public List<int> CurrencyList { get; set; }
            public List<SetPermission> Permission { get; set; }
        }

        public class SetAdminUserPasswordReq : AdminUserReq
        {
            public string Password { get; set; }
        }

        public class SetAdminUserStatusReq : AdminUserReq
        {
            public int Status { get; set; }
        }
        public class GetAddAdminUserCurrencyReq
        {
            public int SiteId { get; set; }
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

        #region RolePermission
        [HttpPost]
        public ApiResult GetAddRolePermission(GetAddRolePermissionReq request)
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

            var loginUser = _adminBll.GetUserV2(authInfo.UserId,request.SiteId);

            if (loginUser == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies.LoginUser is null."
                };
            }

            var result = _permissionBll.GetAddRolePermission(loginUser);

            if (!result.Permission.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            return new ApiResult<NewPermission>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult GetUpdateRolePermission(PermissionReq request)
        {
            var authInfo = _authorizeBll.GetAuthData();
            var adminRole = _permissionBll.GetAdminRole().ToList();

            if (authInfo == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies"
                };
            }

            var loginUser = _adminBll.GetUserV2(authInfo.UserId, request.SiteId);
            var roleData = adminRole.Find(x => x.Role == request.Role && x.SiteId == request.SiteId);
            if (roleData == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Role is not exists"
                };
            }

            if (!loginUser.IsSuper && roleData.UserId != authInfo.UserId)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "You can not update this role"
                };
            }

            var result = _permissionBll.GetUpdateRolePermission(request.Role,loginUser);
            result.Name = roleData.RoleName;
            result.Mark = roleData.Remark;
            result.SelectCurrencyList = JsonConvert.DeserializeObject<List<int>>(roleData.CurrencyList);

            return new ApiResult<RoleData>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult<IEnumerable<RoleList>> GetRoleList(GetRoleListReq request)
        {
            var authInfo = _authorizeBll.GetAuthData();
            if (authInfo == null)
            {
                return new ApiResult<IEnumerable<RoleList>>
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies"
                };
            }

            var loginUser = _adminBll.GetUserV2(authInfo.UserId, request.SiteId);

            if (loginUser == null)
            {
                return new ApiResult<IEnumerable<RoleList>>
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies.LoginUser is null."
                };
            }
            var result = _permissionBll.GetRoleList(request.RoleName, request.SiteId).ToList();

            if (!loginUser.IsSuper)
            {
                result.RemoveAll(x => x.UserId != loginUser.UserId);
            }
            return new ApiResult<IEnumerable<RoleList>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult AddRolePermission(SetRolePermissionReq request)
        {
            var result = _permissionBll.GetAdminRole().ToList();

            var authInfo = _authorizeBll.GetAuthData();
            if (result.Exists(x => x.RoleName == request.Name))
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Role name is exists"
                };
            }
            if (_permissionBll.AddAdminRole(request.SiteId, request.Name, request.Mark, request.CurrencyList, request.Permission, authInfo.UserId) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Add role fail"
                };
            }
        }

        [HttpPost]
        public ApiResult DeleteRolePermission(PermissionReq request)
        {
            var result = _permissionBll.GetAdminRole().ToList();
            if (!result.Exists(x => x.Role == request.Role))
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Role is not exists"
                };
            }
            var allUser = _adminBll.GetAllUserV2();

            _permissionBll.DeleteAdminRole(request.Role, allUser);

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult UpdateRolePermission(SetRolePermissionReq request)
        {
            var result = _permissionBll.GetAdminRole().ToList();

            if (result.Exists(x => x.RoleName == request.Name && x.Role != request.Role) )
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.UpdateFailed,
                    Message = "Role name is exists"
                };
            }
            if (!result.Exists(x => x.Role == request.Role))
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.UpdateFailed,
                    Message = "Role is not exists"
                };
            }
            if (_permissionBll.UpdateAdminRole(request.Role.Value,request.Name, request.Mark, request.CurrencyList, request.Permission) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.UpdateFailed,
                    Message = "Update role fail"
                };
            }
        }

        #endregion

        #region AdminUser
        [HttpPost]
        public ApiResult GetAddAdminUser(PermissionReq request)
        {
            var authInfo = _authorizeBll.GetAuthData();
            var adminRole = _permissionBll.GetAdminRole().ToList();

            if (authInfo == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies"
                };
            }

            var loginUser = _adminBll.GetUserV2(authInfo.UserId, request.SiteId);
            var roleData = adminRole.Find(x => x.Role == request.Role && x.SiteId == request.SiteId);
            if (roleData == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Role is not exists"
                };
            }

            var result = _permissionBll.GetAddAdminUser(request.Role, loginUser);

            return new ApiResult<AddUserPremission>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult GetUpdateAdminUser(AdminUserReq request)
        {
            var authInfo = _authorizeBll.GetAuthData();
            var adminRole = _permissionBll.GetAdminRole().ToList();

            if (authInfo == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies"
                };
            }
            var User = _adminBll.GetUserV2(request.UserId, request.SiteId);
            var loginUser = _adminBll.GetUserV2(authInfo.UserId, request.SiteId);

            if (User == null)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "User is not exists"
                };
            }
            var roleList = JsonConvert.DeserializeObject<List<int>>(User.Role);

            var result = _permissionBll.GetUpdateAdminUser(roleList.Count > 0 ? roleList.FirstOrDefault() : 0, request.UserId, request.SiteId, loginUser);
            result.UserName = User.UserName;
            result.Password = User.Userpwd;
            if (roleList.Count > 0)
            {
                result.Role = roleList.FirstOrDefault();
            }
            result.SelectCurrencyList = JsonConvert.DeserializeObject<List<int>>(User.CurrencyList);

            return new ApiResult<UpdateUserPermission>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult<IEnumerable<UserList>> GetAdminUserList(GetAdminUserListReq request)
        {
            var authInfo = _authorizeBll.GetAuthData();
            if (authInfo == null)
            {
                return new ApiResult<IEnumerable<UserList>>
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies"
                };
            }
            var result = _permissionBll.GetAdminUserList(request.UserName, request.SiteId).ToList();
            return new ApiResult<IEnumerable<UserList>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult<IEnumerable<int>> GetAddAdminUserCurrency(GetAddAdminUserCurrencyReq request)
        {
            var authInfo = _authorizeBll.GetAuthData();
            if (authInfo == null)
            {
                return new ApiResult<IEnumerable<int>>
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies"
                };
            }

            var loginUser = _adminBll.GetUserV2(authInfo.UserId, request.SiteId);

            if (loginUser == null)
            {
                return new ApiResult<IEnumerable<int>>
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Missing cookies.LoginUser is null."
                };
            }

            var result = _permissionBll.GetAddAdminUserCurrency(loginUser).ToList();

            return new ApiResult<IEnumerable<int>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult AddAdminUser(SetAdminUserReq request)
        {
            var result = _permissionBll.GetAdminRole().ToList();

            var authInfo = _authorizeBll.GetAuthData();

            if (_permissionBll.AddAdminUser(request.SiteId, request.Role, request.UserName, request.Password, request.CurrencyList, request.Permission) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Add role fail"
                };
            }
        }

        [HttpPost]
        public ApiResult DeleteAdminUser(AdminUserReq request)
        {
            _permissionBll.DeleteAdminUser(request.UserId);

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult UpdateAdminUser(SetAdminUserReq request)
        {
            if (_permissionBll.UpdateAdminUser(request.SiteId, request.UserId, request.Role, request.UserName, request.Password, request.CurrencyList, request.Permission) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.UpdateFailed,
                    Message = "Update user fail"
                };
            }
        }

        [HttpPost]

        public ApiResult SetAdminUserPassword(SetAdminUserPasswordReq request)
        {
            if (_permissionBll.SetAdminUserPassword(request.UserId, request.Password) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.UpdateFailed,
                    Message = "Set user password fail"
                };
            }
        }

        [HttpPost]
        public ApiResult SetAdminUserStatus(SetAdminUserStatusReq request)
        {
            if (_permissionBll.SetAdminUserStatus(request.UserId, request.Status) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.UpdateFailed,
                    Message = "Set user status fail"
                };
            }
        }
        #endregion
    }
}