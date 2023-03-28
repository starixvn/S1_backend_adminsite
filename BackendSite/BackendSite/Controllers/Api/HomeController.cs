using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;


namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ControllerBase
    {
        #region Fields

        private readonly AuthorizeBLL _authorizeBLL;
        private readonly Tools _tools;
        private readonly ServerInfoBLL _serverInfoBLL;
        private readonly AdminBLL _adminBLL;
        private readonly MenuBLL _menuBLL;

        #endregion

        #region Constructors

        public HomeController(AuthorizeBLL authorizeBLL,
                              Tools tools,
                              ServerInfoBLL serverInfoBLL,
                              AdminBLL adminBLL,
                              MenuBLL menuBLL)
        {
            _authorizeBLL = authorizeBLL;
            _tools = tools;
            _serverInfoBLL = serverInfoBLL;
            _adminBLL = adminBLL;
            _menuBLL = menuBLL;
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

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = Url.Action("Index", "Home")
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