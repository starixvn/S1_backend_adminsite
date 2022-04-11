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
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
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

        private readonly MySqlClient _mySqlClient;

        private readonly ServiceCollection _serviceCollection;
        private IServiceProvider _serviceProvider;
        #endregion

        #region Constructors

        public HomeController(IServiceProvider provider)
        {
            _authorizeBll = provider.GetService<AuthorizeBLL>();
            _tools = provider.GetService<Tools>();
            _adminBll = provider.GetService<AdminBLL>();

            _mySqlClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
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

            AdminUser adminUser = null;

            var userLoginResult = _authorizeBll.UserLogin(loginModel.UserName,
                                                          loginModel.UserPwd,
                                                          _tools.GetClientIP(HttpContext),
                                                          _tools.GetFromUrl(HttpContext),
                                                          Request.Headers["User-Agent"],
                                                          ref adminUser);

            if (userLoginResult != ErrorCodeEnum.Success)
            {
                throw new AdminException(userLoginResult);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,adminUser.UserName ),
                    new Claim(ClaimTypes.NameIdentifier,adminUser.UserId.ToString())
                    //new Claim("LangId",? ),
                };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
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

        #region MenuTest

        public class MenuTestModel
        {
            public int MId { get; set; }

            public string MName { get; set; }

            public IEnumerable<FuncViewModel> FuncDatas { get; set; }
            public IEnumerable<MenuTestModel> SecondLayerDatas { get; set; }
        }

        public class FuncViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Sort { get; set; }
            public Dictionary<int, string> Permission { get; set; }
        }


        public class ModuleModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Layer { get; set; }
            public int HeirId { get; set; }
            public int Sort { get; set; }
            public int Status { get; set; }
        }

        public class FuncModel
        {
            public int Id { get; set; }
            public int ModuleId { get; set; }
            public string Name { get; set; }
            public int Sort { get; set; }
            public string Permission { get; set; }
            public int Status { get; set; }
        }

        public class PermissionModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Sort { get; set; }
            public int Status { get; set; }
        }

        /// <summary>
        /// Home/MenuTest 新menu測試
        /// </summary>
        /// </remarks>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<List<MenuTestModel>> MenuTest()
        {
            var result = new List<MenuTestModel>();

            //Get Data
            string moduleSqlScript = "SELECT * FROM EttaTestModule";
            var moduleDatas = _mySqlClient.Query<ModuleModel>(moduleSqlScript);
            string funcSqlScript = "SELECT Id,"
                                 + "       ModuleId,"
                                 + "       Name,"
                                 + "       Sort,"
                                 + "       UsedPermission->> '$.Permission' AS Permission,"
                                 + "       Status"
                                 + "  FROM EttaTestFunc";
            var funcnDatas = _mySqlClient.Query<FuncModel>(funcSqlScript);
            string permissionSqlScript = "SELECT * FROM EttaTestPermission";
            var permissionDatas = _mySqlClient.Query<PermissionModel>(permissionSqlScript);

            var firstLayerDatas = moduleDatas.Where(x => x.Layer == 0); //第一層
            var secondLayerDatas = moduleDatas.Where(x => x.Layer == 1).Select(x => x.HeirId); //第二層

            //Process
            foreach (var moduleData in firstLayerDatas)
            {
                var FuncDatas = new List<FuncViewModel>();
                var SecondLayerDatas = new List<MenuTestModel>();
                var SecondFuncDatas = new List<FuncViewModel>();
                var funcViewModel = new FuncViewModel();

                var thisModuleFuncDatas = funcnDatas.Where(x => x.ModuleId == moduleData.Id);
                foreach (var funcData in thisModuleFuncDatas)
                {
                    var permissionData = new Dictionary<int, string>();
                    var sourcePermissionData = JsonConvert.DeserializeObject<IEnumerable<int>>(funcData.Permission);
                    foreach (var permission in sourcePermissionData)
                    {
                        permissionData[permission] = permissionDatas.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                    }
                    FuncDatas.Add(new FuncViewModel()
                    {
                        Id = funcData.Id,
                        Name = funcData.Name,
                        Permission = permissionData
                    });
                }

                if (secondLayerDatas.Contains(moduleData.Id))
                {
                    foreach (var secondModule in moduleDatas.Where(x => x.HeirId == moduleData.Id))
                    {
                        var SecondFuncData = new List<FuncViewModel>();
                        foreach (var funcData in funcnDatas.Where(x => x.ModuleId == secondModule.Id))
                        {
                            var permissionData = new Dictionary<int, string>();
                            var sourcePermissionData = JsonConvert.DeserializeObject<IEnumerable<int>>(funcData.Permission);
                            foreach (var permission in sourcePermissionData)
                            {
                                permissionData[permission] = permissionDatas.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                            }
                            SecondFuncData.Add(new FuncViewModel()
                            {
                                Id = funcData.Id,
                                Name = funcData.Name,
                                Permission = permissionData
                            });
                        }
                        SecondLayerDatas.Add(new MenuTestModel()
                        {
                            MId = secondModule.Id,
                            MName = secondModule.Name,
                            FuncDatas = SecondFuncData
                        });
                    }
                }

                result.Add(new MenuTestModel()
                {
                    MId = moduleData.Id,
                    MName = moduleData.Name,
                    FuncDatas = FuncDatas,
                    SecondLayerDatas = SecondLayerDatas
                });
            }
            return new ApiResult<List<MenuTestModel>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion

        #endregion
    }
}
