using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ControllerBase
    {
        #region Fields

        private readonly SelLangBLL _selLangBLL;
        private readonly AuthorizeBLL _authorizeBLL;
        private readonly Tools _tools;
        private readonly ServerInfoBLL _serverInfoBLL;
        private readonly AdminBLL _adminBLL;
        private readonly HomeBLL _homeBLL;
        private readonly DashBoardBLL _dashBoardBLL;
        private readonly MenuBLL _menuBLL;
        private readonly ResponseSettingBLL _responseSettingBLL;
        private readonly IHostEnvironment _environment;

        #endregion

        #region Constructors

        public HomeController(SelLangBLL selLangBLL,
                              AuthorizeBLL authorizeBLL,
                              Tools tools,
                              ServerInfoBLL serverInfoBLL,
                              AdminBLL adminBLL,
                              HomeBLL homeBLL,
                              DashBoardBLL dashBoardBLL,
                              MenuBLL menuBLL,
                              ResponseSettingBLL responseSettingBLL,
                              IHostEnvironment environment)
        {
            _selLangBLL = selLangBLL;
            _authorizeBLL = authorizeBLL;
            _tools = tools;
            _serverInfoBLL = serverInfoBLL;
            _adminBLL = adminBLL;
            _homeBLL = homeBLL;
            _dashBoardBLL = dashBoardBLL;
            _menuBLL = menuBLL;
            _responseSettingBLL = responseSettingBLL;
            _environment = environment;
        }

        #endregion

        #region API Model

        public class LoginModel
        {
            /// <summary>
            /// 使用者帳號
            /// </summary>
            [Required]
            public string txtID { set; get; }

            /// <summary>
            /// 使用者密碼
            /// </summary>
            [Required]
            public string txtPW { set; get; }
            public string redirectUrl { set; get; }
        }

        public class DashBoardPara
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int BaseCurrencyId { get; set; }
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
        ///        "txtID": "admin",
        ///        "txtPW": "1q2w3e4r"
        ///     }
        ///
        /// </remarks>
        /// <param name="loginModel">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ApiResult> Signin([FromBody] LoginModel loginModel)
        {

            AdminUser adminUser = null;

            var userLoginResult = _authorizeBLL.UserLogin(loginModel.txtID,
                                                          loginModel.txtPW,
                                                          _tools.GetClientIP(HttpContext),
                                                          _tools.GetFromUrl(HttpContext),
                                                          Request.Headers["User-Agent"],
                                                          ref adminUser);

            if (userLoginResult != ErrorCodeEnum.Success)
            {
                throw new AdminException(userLoginResult);
            }

            //if (userLoginResult != 0)
            //{
            await HttpContext.SignOutAsync("backend");
            //   return apiResult;
            //}

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,adminUser.UserName ),
                    new Claim(ClaimTypes.NameIdentifier,adminUser.UserId.ToString())
                    //new Claim("LangId",? ),
                };

            var claimsIdentity = new ClaimsIdentity(claims, "backend");
            await HttpContext.SignInAsync("backend",
                                 new ClaimsPrincipal(claimsIdentity));

            if (string.IsNullOrWhiteSpace(loginModel.redirectUrl))
                loginModel.redirectUrl = Url.Action("Index", "Home");

            //For 前端查看swagger，只有在dev和local且帳號為admin
            if ((adminUser.UserName == "admin" || adminUser.UserName == "Admin")
                && (_environment.IsDevelopment() || _environment.IsEnvironment("Local")))
            {
                await HttpContext.SignOutAsync("backendV2");
                await HttpContext.SignOutAsync("brand");

                var claimsForSwagger = new List<Claim>
            {
                new Claim(ClaimTypes.Name,"admin" ),
                new Claim(ClaimTypes.NameIdentifier,"60".ToString())
                //new Claim("LangId",? ),
            };

                var claimsV2 = new List<Claim>
            {
                new Claim(ClaimTypes.Thumbprint, "1"),
                new Claim(ClaimTypes.NameIdentifier,"60".ToString())
                //new Claim("LangId",? ),
            };

                var claimsIdentityForSwagger = new ClaimsIdentity(claimsForSwagger, "backendV2");
                var claimsIdentityV2 = new ClaimsIdentity(claimsV2, "brand");
                await HttpContext.SignInAsync("backendV2",
                                     new ClaimsPrincipal(claimsIdentityForSwagger));
                await HttpContext.SignInAsync("brand",
                                     new ClaimsPrincipal(claimsIdentityV2));
            }

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = loginModel.redirectUrl
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
        public ApiResult CheckSignIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                return new ApiResult { ErrorCode = (int)ErrorCodeEnum.Success };
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

            await HttpContext.SignOutAsync("backend");

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = Url.RouteUrl("default", new { action = "Signin", controller = "Home" })
            };
        }

        /// <summary>
        /// Home/GetSiteList 取得品牌清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.
        /// 
        /// </remarks>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult GetSiteList()
        {
            var data = _serverInfoBLL.GetSiteList();

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
        public ApiResult ChangePwd([FromBody] ChangePwd changePwd)
        {

            var result = _adminBLL.ChangePwd(changePwd);
            var custMsg = string.Empty;

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

        #region Temp，沒再使用

        [HttpPost]
        public ApiResult<int> RegisteredCustomers([FromForm] int SiteId, [FromForm] long StartDate, [FromForm] long EndDate)
        {
            var result = _homeBLL.GetRegisteredCustomers(SiteId, StartDate, EndDate);
            var apiResult = new ApiResult<int>();

            if (result >= 0)
            {
                apiResult.ErrorCode = 0;
                apiResult.Data = result;
            }
            else
            {
                apiResult.ErrorCode = 999;
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<int> DepositCustomers([FromForm] int SiteId, [FromForm] long StartDate, [FromForm] long EndDate)
        {
            var result = _homeBLL.GetepositCustomers(SiteId, StartDate, EndDate);
            var apiResult = new ApiResult<int>();

            if (result >= 0)
            {
                apiResult.ErrorCode = 0;
                apiResult.Data = result;
            }
            else
            {
                apiResult.ErrorCode = 999;
            }

            return apiResult;
        }

        #endregion

        /// <summary>
        /// Home/GetDashBoardData 取得首頁儀錶板資訊資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Home > Dash Board > [Refresh]
        /// 
        /// </remarks>
        /// <param name="para">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult GetDashBoardData(DashBoardPara para)
        {
            var CanSearchSite = new List<int>();

            _serverInfoBLL.GetSiteListByLoginUser().ToList().ForEach(x => CanSearchSite.Add(x.SiteId));
            var result = _dashBoardBLL.GetDashBoardData(para.SiteId, para.CurrencyId, para.BaseCurrencyId, CanSearchSite, 8);

            return new ApiResult<DashBoardData>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
        }

        /// <summary>
        /// Home/GetMenu 取得主選單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Home > Signin > [Login] > [GetMenu].
        /// 確認登入成功後，打此API取得選單資料。
        /// 
        /// </remarks>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult GetMenu()
        {

            var data = _menuBLL.GetMenuData(false, out var errorCodeEnum);

            if (data.Count == 0)
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<MenuItem>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        #endregion

    }
}