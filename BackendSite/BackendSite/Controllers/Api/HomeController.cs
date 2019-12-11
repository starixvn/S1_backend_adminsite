using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        public HomeController(SelLangBLL selLangBLL, AuthorizeBLL authorizeBLL, Tools tools)
        {
            this.selLangBLL = selLangBLL;
            this.authorizeBLL = authorizeBLL;
            this.tools = tools;
        }
        public class LoginModel
        {
            public string txtID { set; get; }
            public string txtPW { set; get; }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ApiResult> Signin([FromBody]LoginModel loginModel)
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

            List<string> roleList = JsonConvert.DeserializeObject<List<string>>(adminUser.Role);
            foreach (var item in roleList)
            {
                claims.Add(new Claim(ClaimTypes.Role, item.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            apiResult.Message = Url.Action("Index", "Home");
            apiResult.ErrorCode = 0;
            return apiResult;
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
    }
}