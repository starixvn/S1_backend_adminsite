using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using BackendSite.ExceptionHandling;
using BackendSite.Service.Model.enums;
using System.Linq;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.SystemFunc
{
    [Authorize(AuthenticationSchemes = "backendV2,brand")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        #region Fields
        private readonly NewAuthorizeBLL _newAuthorizeBLL;
        private readonly AdminBLL _adminBll;
        private readonly ServerInfoBLL _serverInfoBLL;
        #endregion

        #region Constructors
        public CommonController(IServiceProvider provider)
        {
            _newAuthorizeBLL = provider.GetService<NewAuthorizeBLL>();
            _adminBll = provider.GetService<AdminBLL>();
            _serverInfoBLL = provider.GetService<ServerInfoBLL>();
        }
        #endregion

        #region API Model
        #endregion

        /// <summary>
        /// Common/AdminUserCurrencyList 取得可使用的Currency清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST 
        /// 
        /// </remarks>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<IEnumerable<int>> AdminUserCurrencyList()
        {
            var apiResult = new ApiResult<IEnumerable<int>>();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var authInfo = _newAuthorizeBLL.GetAuthData();
                    var userData = _adminBll.GetUserV2(authInfo.UserId);
                    var brand = _newAuthorizeBLL.CheckBrand();
                    if (userData.IsSuper)
                    {
                        var siteList = _serverInfoBLL.GetSiteList().ToList();
                        var brandData = siteList.Find(x => x.SiteId == brand);
                        if (brandData != null)
                        {
                            apiResult.Data = brandData.ICurrencyList;
                            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                        }
                    }
                    else
                    {
                        var siteList = _newAuthorizeBLL.GetAssignSiteListV2(authInfo.UserId);
                        var brandData = siteList.Find(x => x.SiteId == brand);
                        if(brandData != null)
                        {
                            apiResult.Data = JsonConvert.DeserializeObject<IEnumerable<int>>(brandData.CurrencyList);
                            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                        }
                    }        
                }
                else
                {
                    throw new AdminException(ErrorCodeEnum.Fail);
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }
    }
}
