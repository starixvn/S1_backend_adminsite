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
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly SelLangBLL selLangBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly Tools tools;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly AdminBLL adminBLL;
        private readonly HomeBLL homeBLL;
        private readonly DashBoardBLL dashBoardBLL;

        public HomeController(SelLangBLL selLangBLL, AuthorizeBLL authorizeBLL, Tools tools, ServerInfoBLL serverInfoBLL, AdminBLL adminBLL, HomeBLL homeBLL, DashBoardBLL dashBoardBLL)
        {
            this.selLangBLL = selLangBLL;
            this.authorizeBLL = authorizeBLL;
            this.tools = tools;
            this.serverInfoBLL = serverInfoBLL;
            this.adminBLL = adminBLL;
            this.homeBLL = homeBLL;
            this.dashBoardBLL = dashBoardBLL;
        }
        public class LoginModel
        {
            public string txtID { set; get; }
            public string txtPW { set; get; }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ApiResult> Signin([FromBody] LoginModel loginModel)
        {
            var apiResult = new ApiResult(selLangBLL);
            string userName = loginModel.txtID;
            string userpwd = loginModel.txtPW;
            AdminUser adminUser = null;

            var userLoginResult = authorizeBLL.UserLogin(userName, userpwd, tools.GetClientIP(HttpContext), tools.GetFromUrl(HttpContext), Request.Headers["User-Agent"], ref adminUser);

            if (userLoginResult == -1)
            {
                apiResult.ErrorCode = 1101;
                apiResult.Message = selLangBLL.GetMsg("msg_LoginFailed");
            }
            else if (userLoginResult == -2)
            {
                apiResult.ErrorCode = 1001;
                apiResult.Message = selLangBLL.GetMsg("msg_LoginFailed");
            }
            else if (userLoginResult == -3)
            {
                apiResult.ErrorCode = 1102;
                apiResult.Message = selLangBLL.GetMsg("msg_ACLocked");
            }
            else if (userLoginResult != 0)
            {
                apiResult.ErrorCode = 9000;
            }

            if (userLoginResult != 0)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return apiResult;
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,adminUser.UserName ),
                    new Claim(ClaimTypes.NameIdentifier,adminUser.UserId.ToString())
                    //new Claim("LangId",? ),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            apiResult.Message = Url.Action("Index", "Home");
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        public ApiResult CheckSignIn()
        {
            var result = new ApiResult();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    result.ErrorCode = 0;
                }
                else
                {
                    result.ErrorCode = -1;
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        [HttpPost]
        public async Task<ApiResult> Signout()
        {
            var apiResult = new ApiResult(selLangBLL);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            apiResult.Message = Url.RouteUrl("default", new { action = "Signin", controller = "Home" });
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<ServerInfo>> GetSiteList()
        {
            var apiResult = new ApiResult<IEnumerable<ServerInfo>>(selLangBLL);
            apiResult.Data = serverInfoBLL.GetSiteList();
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        [HttpPost]
        public ApiResult ChangePwd([FromBody] ChangePwd changePwd)
        {
            int response = 0;
            bool result = adminBLL.ChangePwd(changePwd, out response);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                if (response == 8)
                {
                    apiResult.ErrorCode = 8;
                    apiResult.Message = selLangBLL.GetMsg("msg_OldpwdNotMatch");
                }
                else
                {
                    apiResult.ErrorCode = 9;
                    apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
                }
            }
            return apiResult;
        }

        [HttpPost]
        public ApiResult<int> RegisteredCustomers([FromForm] int SiteId, [FromForm] long StartDate, [FromForm] long EndDate)
        {
            var result = homeBLL.GetRegisteredCustomers(SiteId, StartDate, EndDate);
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
            var result = homeBLL.GetepositCustomers(SiteId, StartDate, EndDate);
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
        public class DashBoardPara
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int BaseCurrencyId { get; set; }
        }
        [HttpPost]
        public ApiResult<DashBoardData> GetDashBoardData(DashBoardPara para)
        {
            var apiResult = new ApiResult<DashBoardData>();
            dashBoardBLL.GetDashBoardData(para.SiteId,para.CurrencyId,para.BaseCurrencyId);

            return apiResult;
        }
    }
}