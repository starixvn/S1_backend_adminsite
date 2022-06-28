using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendSite.Controllers.ApiV2.Admin.Home
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        #region Fields

        private readonly Tools _tools;
        private readonly AdminBLL _adminBll;
        private readonly NewAuthorizeBLL _newAuthorizeBLL;

        #endregion

        #region Constructors

        public HomeController(IServiceProvider provider)
        {
            _tools = provider.GetService<Tools>();
            _adminBll = provider.GetService<AdminBLL>();
            _newAuthorizeBLL = provider.GetService<NewAuthorizeBLL>();
        }


        #endregion

        #region API Model

        public class LoginRequest
        {
            /// <summary>
            /// 使用者帳號
            /// </summary>
            [Required]
            public string UserName { set; get; }

            /// <summary>
            /// 使用者密碼
            /// </summary>
            [Required]
            public string UserPwd { set; get; }
        }

        public class ChangePwdReqest
        {
            /// <summary>
            /// 舊密碼
            /// </summary>
            public string OldPwd { set; get; }
            /// <summary>
            /// 新密碼
            /// </summary>
            public string NewPwd { set; get; }
        }

        public class ChangeBrandRequest
        {
            public int SiteId { get; set; }
        }
        #endregion

        #region Actions

        /// <summary>
        /// Home/Signin 後台登入
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Home > Signin > [Login]
        /// 
        /// Sample request:
        ///
        ///     {
        ///        "userName": "admin",
        ///        "userPwd": "1q2w3e4r"
        ///     }
        ///
        /// </remarks>
        /// <param name="loginModel">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ApiResult<string>> Signin(LoginRequest loginModel)
        {

            NewAdminUser adminUser = null;

            var userLoginResult = _newAuthorizeBLL.UserLogin(loginModel.UserName,
                                                          loginModel.UserPwd,
                                                          _tools.GetClientIP(HttpContext),
                                                          _tools.GetFromUrl(HttpContext),
                                                          Request.Headers["User-Agent"],
                                                          ref adminUser);

            if (userLoginResult != ErrorCodeEnum.Success)
            {
                throw new AdminException(userLoginResult);
            }

            var siteList = _newAuthorizeBLL.GetAssignSiteListV2(adminUser.UserId);

            if(siteList.Count == 0)
            {
                throw new AdminException(ErrorCodeEnum.AccountError);
            }


            _newAuthorizeBLL.SetLoginTime(adminUser.UserId);

            await HttpContext.SignOutAsync("backendV2");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,adminUser.UserName ),
                new Claim(ClaimTypes.NameIdentifier,adminUser.UserId.ToString()),
                new Claim(ClaimTypes.Thumbprint, siteList.FirstOrDefault().SiteId.ToString())
                //new Claim("LangId",? ),
            };

            var claimsIdentity = new ClaimsIdentity(claims, "backendV2");
            await HttpContext.SignInAsync("backendV2",
                                 new ClaimsPrincipal(claimsIdentity));

            return new ApiResult<string>
            {
                Data = adminUser.UserName,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Home/CheckSignIn 登入檢查
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.每次Load首頁時確認是否為已登入狀態。
        /// 
        /// </remarks>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<string> CheckSignIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                return new ApiResult<string> { Data = User.Identity.Name, ErrorCode = (int)ErrorCodeEnum.Success };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }
        }

        /// <summary>
        /// Home/Signout 登出
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Home > User > [Logout]
        /// 
        /// </remarks>
        /// <returns>結果</returns>
        [HttpPost]
        public async Task<ApiResult> Signout()
        {

            await HttpContext.SignOutAsync("backendV2");

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Home/ChangePwd 使用者密碼變更
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Home > User > Change Password > [Confirm]
        /// 
        /// </remarks>
        /// <param name="changePwd">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult ChangePwd(ChangePwdReqest changePwd)
        {
            var authInfo = _newAuthorizeBLL.GetAuthData();
            var result = _adminBll.ChangePwdV2(authInfo.UserId, changePwd.OldPwd,changePwd.NewPwd);

            if (result != ErrorCodeEnum.Success)
            {
                if (result == ErrorCodeEnum.Fail)
                    throw new AdminException(result, "Update Fail.");
                else
                    throw new AdminException(result);
            }

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = "Update Success."
            };
        }

        /// <summary>
        /// Home/GetUserData 取得LoginUser的資訊
        /// </summary>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<AdminUserData> GetUserData()
        {
            if (User.Identity.IsAuthenticated)
            {
                var authInfo = _newAuthorizeBLL.GetAuthData();
                var data = _adminBll.GetUserV2(authInfo.UserId);
                return new ApiResult<AdminUserData> { Data = data, ErrorCode = (int)ErrorCodeEnum.Success };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }
        }

        /// <summary>
        /// Home/CheckBrand 從cookie 取得brand
        /// </summary>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<int> CheckBrand()
        {
            var brand = _newAuthorizeBLL.CheckBrand();
            if (User.Identity.IsAuthenticated)
            {
                return new ApiResult<int> { Data = brand, ErrorCode = (int)ErrorCodeEnum.Success };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }
        }

        /// <summary>
        /// Home/ChangeBrand 異動brand回傳新的cookie
        /// </summary>
        /// <returns>結果</returns>
        [HttpPost]
        public async Task<ApiResult<int>> ChangeBrand(ChangeBrandRequest request)
        {
            var authInfo = _newAuthorizeBLL.GetAuthData();
            await HttpContext.SignOutAsync("backendV2");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Thumbprint, request.SiteId.ToString()),
                new Claim(ClaimTypes.NameIdentifier,authInfo.UserId.ToString()),
                new Claim(ClaimTypes.Name,authInfo.UserName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "backendV2");

            await HttpContext.SignInAsync("backendV2",
                                 new ClaimsPrincipal(claimsIdentity));

            return new ApiResult<int>
            {
                Data = request.SiteId,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion
    }
}
