using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
            public string txtID { set; get; }
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
        /// 登入
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Signin([FromBody] LoginModel loginModel)
        {
            //var apiResult = new ApiResult(_selLangBLL);
            try
            {
                AdminUser adminUser = null;

                var userLoginResult = _authorizeBLL.UserLogin(loginModel.txtID,
                                                              loginModel.txtPW,
                                                              _tools.GetClientIP(HttpContext),
                                                              _tools.GetFromUrl(HttpContext),
                                                              Request.Headers["User-Agent"],
                                                              ref adminUser);

                if (userLoginResult != Service.Model.enums.ErrorMessageCodeEnum.Success)
                {
                    return StatusCode(200,
                                      _responseSettingBLL.SetResponse(userLoginResult));
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

                return StatusCode(200, _responseSettingBLL.SetResponse(userLoginResult,
                                                                                 custMessage: Url.Action("Index", "Home")));
            }
            catch (Exception ex)
            {
                return StatusCode(500, _responseSettingBLL.SetResponse(ex));
            }

        }

        /// <summary>
        /// 登入檢查
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CheckSignIn()
        {
            var result = new ApiResult();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    return StatusCode(200,
                                      _responseSettingBLL.SetResponse(Service.Model.enums.ErrorMessageCodeEnum.Success, custMessage: "Logining."));
                }
                else
                {
                    return StatusCode(200,
                                      _responseSettingBLL.SetResponse(Service.Model.enums.ErrorMessageCodeEnum.Fail, custMessage: "Not logining."));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                                  _responseSettingBLL.SetResponse(ex));
            }
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Signout()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return StatusCode(200,
                              _responseSettingBLL.SetResponse(Service.Model.enums.ErrorMessageCodeEnum.Success, custMessage: Url.RouteUrl("default", new { action = "Signin", controller = "Home" })));
        }

        /// <summary>
        /// 取得網站清單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetSiteList()
        {
            try
            {
                var data = _serverInfoBLL.GetSiteList();

                if (data.Count() == 0)
                {
                    return StatusCode(200,
                                      _responseSettingBLL.SetResponse<IEnumerable<ServerInfo>>(Service.Model.enums.ErrorMessageCodeEnum.NoData,
                                                                                               data));
                }
                else
                {
                    return StatusCode(200,
                                      _responseSettingBLL.SetResponse<IEnumerable<ServerInfo>>(Service.Model.enums.ErrorMessageCodeEnum.Success,
                                                                                               data));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                                  _responseSettingBLL.SetResponse(ex));
            }
        }

        [HttpPost]
        public IActionResult ChangePwd([FromBody] ChangePwd changePwd)
        {
            try
            {
                var result = _adminBLL.ChangePwd(changePwd);
                string custMsg = string.Empty;

                if (result == Service.Model.enums.ErrorMessageCodeEnum.Success)
                {
                    custMsg = "Update Successful.";
                }
                else if (result == Service.Model.enums.ErrorMessageCodeEnum.Fail)
                {
                    custMsg = "Update Fail.";
                }

                return StatusCode(200,
                                  _responseSettingBLL.SetResponse(result, custMessage: custMsg));


            }
            catch (Exception ex)
            {
                return StatusCode(500,
                                 _responseSettingBLL.SetResponse(ex));
            }
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
        public ApiResult<DashBoardData> GetDashBoardData(DashBoardPara para)
        {
            var apiResult = new ApiResult<DashBoardData>();
            List<int> CanSearchSite = new List<int>();
            try
            {
                _serverInfoBLL.GetSiteListByLoginUser().ToList().ForEach(x => CanSearchSite.Add(x.SiteId));
                var result = _dashBoardBLL.GetDashBoardData(para.SiteId, para.CurrencyId, para.BaseCurrencyId, CanSearchSite);

                apiResult.Data = result;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.RawMessage = ex.ToString();
            }


            return apiResult;
        }

        /// <summary>
        /// 取得主選單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetMenu()
        {
            try
            {
                var data = _menuBLL.GetMenuData();

                if (data.Count() == 0)
                {
                    return StatusCode(200,
                  _responseSettingBLL.SetResponse<IEnumerable<MenuItem>>(Service.Model.enums.ErrorMessageCodeEnum.NoData,
                                                                         data,
                                                                         custMessage: "No data."));
                }
                else
                {
                    return StatusCode(200,
                  _responseSettingBLL.SetResponse<IEnumerable<MenuItem>>(Service.Model.enums.ErrorMessageCodeEnum.Success,
                                                                         data));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                                  _responseSettingBLL.SetResponse(ex));
            }
        }

        #endregion

    }
}