using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace BackendSite.Controllers.Api
{

    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
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
                              ResponseSettingBLL responseSettingBLL)
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
        /// 後台登入
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "txtID": "admin",
        ///        "txtPW": "1q2w3e4r"
        ///     }
        ///
        /// </remarks>
        /// <param name="loginModel">登入參數</param>
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
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //   return apiResult;
            //}

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,adminUser.UserName ),
                    new Claim(ClaimTypes.NameIdentifier,adminUser.UserId.ToString())
                    //new Claim("LangId",? ),
                };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                 new ClaimsPrincipal(claimsIdentity));

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = Url.Action("Index", "Home")
            };
        }

        /// <summary>
        /// 登入檢查
        /// </summary>
        /// <returns></returns>
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
        /// 登出
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        public async Task<ApiResult> Signout()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = Url.RouteUrl("default", new { action = "Signin", controller = "Home" })
            };
        }

        /// <summary>
        /// 取得網站清單
        /// </summary>
        /// <returns></returns>
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

        #region Temp,沒再使用

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

        [HttpPost]
        public ApiResult GetDashBoardData(DashBoardPara para)
        {
            var CanSearchSite = new List<int>();

            _serverInfoBLL.GetSiteListByLoginUser().ToList().ForEach(x => CanSearchSite.Add(x.SiteId));
            var result = _dashBoardBLL.GetDashBoardData(para.SiteId, para.CurrencyId, para.BaseCurrencyId, CanSearchSite);

            return new ApiResult<DashBoardData>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
        }

        /// <summary>
        /// 取得主選單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResult GetMenu()
        {

            var data = _menuBLL.GetMenuData();

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