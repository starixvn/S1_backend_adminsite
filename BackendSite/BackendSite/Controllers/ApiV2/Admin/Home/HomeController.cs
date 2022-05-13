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
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendSite.Controllers.ApiV2.Admin.Home
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        #region Fields

        private readonly AuthorizeBLL _authorizeBll;
        private readonly Tools _tools;
        private readonly AdminBLL _adminBll;
        private readonly NewAuthorizeBLL _newAuthorizeBLL;

        #endregion

        #region Constructors

        public HomeController(IServiceProvider provider)
        {
            _authorizeBll = provider.GetService<AuthorizeBLL>();
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

            await HttpContext.SignOutAsync("newbackend");

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,adminUser.UserName ),
                    new Claim(ClaimTypes.NameIdentifier,adminUser.UserId.ToString())
                    //new Claim("LangId",? ),
                };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync("newbackend",
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

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

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

            var result = _adminBll.ChangePwd(new Service.Model.ChangePwd { OldPwd = changePwd.OldPwd, NewPwd = changePwd.NewPwd });

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

        #endregion
    }
}
