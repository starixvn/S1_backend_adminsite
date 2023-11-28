using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.SystemFunc
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        #region Fields
        private readonly NewAuthorizeBLL _newAuthorizeBLL;
        private readonly AdminBLL _adminBll;
        private readonly ServerInfoBLL _serverInfoBLL;
        private readonly RebateManageBLL _rebateManageBLL;
        private readonly List<int> _RewardCurrencyList = new List<int> { 2, 4, 15, 51 };  //商城目前只開放給MYR、THB、IDR使用
        #endregion

        #region Constructors
        public CommonController(IServiceProvider provider)
        {
            _newAuthorizeBLL = provider.GetService<NewAuthorizeBLL>();
            _adminBll = provider.GetService<AdminBLL>();
            _serverInfoBLL = provider.GetService<ServerInfoBLL>();
            _rebateManageBLL = provider.GetService<RebateManageBLL>();
        }
        #endregion

        #region API Model

        public class CryptoAmountCalculationRequest
        {
            public decimal CryptoAmount { get; set; }
            public decimal ExchangeRate { get; set; }
            public CurrencyEnum CurrencyId { get; set; }
        }

        public class CryptoAmountCalculationResponse
        {
            public decimal Amount { get; set; }
        }

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
        public ApiResult<object> AdminUserCurrencyList()
        {
            var apiResult = new ApiResult<object>();
            var normalCurrencyList = Enumerable.Empty<int>();
            var AllSiteCurrencyList = Enumerable.Empty<int>();
            var vipCurrencyList = Enumerable.Empty<int>();
            var allSiteEnableCurrencyList = Enumerable.Empty<int>();

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
                        normalCurrencyList = brandData.ICurrencyList;
                        AllSiteCurrencyList = siteList.SelectMany(r => r.ICurrencyList).Distinct();
                        if (brandData != null)
                            normalCurrencyList = brandData.ICurrencyList;
                    }
                    else
                    {
                        var siteList = _newAuthorizeBLL.GetAssignSiteListV2(authInfo.UserId);
                        var brandData = siteList.Find(x => x.SiteId == brand);
                        var resultList = new List<IEnumerable<int>>();

                        siteList.ForEach(x =>
                        {
                            var jsonArray = JsonConvert.DeserializeObject<IEnumerable<int>>(x.CurrencyList);
                            resultList.Add(jsonArray);
                        });
                        if (brandData != null)
                        {
                            normalCurrencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(brandData.CurrencyList);
                            AllSiteCurrencyList = resultList.SelectMany(r => r).Distinct();
                        }
                            
                    }

                    vipCurrencyList = _rebateManageBLL.GetRebatePeriodList(brand).Select(x => x.CurrencyId).Distinct();
                    allSiteEnableCurrencyList = _serverInfoBLL.GetSiteList().ToList().SelectMany(r => r.ICurrencyList).Distinct();
                }
                else
                {
                    throw new AdminException(ErrorCodeEnum.Fail);
                }
                apiResult.Data = new
                {
                    SiteCurrencyList = normalCurrencyList,
                    AllSiteCurrencyList = AllSiteCurrencyList,
                    RewardsCurrencyList = normalCurrencyList.Intersect(_RewardCurrencyList),
                    VipCurrencyList = normalCurrencyList.Intersect(vipCurrencyList),
                    AllSiteEnableCurrencyList = allSiteEnableCurrencyList
                };
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public async Task<ApiResult> CheckQrCodeImg(IFormFile file)
        {
            var result = await QrCodeExtension.DecodeFromImage(file);

            return string.IsNullOrEmpty(result)
                ? new ApiResult((int)ErrorCodeEnum.Fail, "Incorrect QR code image.")
                : new ApiResult((int)ErrorCodeEnum.Success, string.Empty);
        }

        [HttpPost]
        public ApiResult<CryptoAmountCalculationResponse> CryptoAmountCalculation(CryptoAmountCalculationRequest request)
        {
            var tmpAmount = request.CryptoAmount / request.ExchangeRate;
            var amount = request.CurrencyId is CurrencyEnum.VND or CurrencyEnum.INR
                ? Math.Floor(tmpAmount)
                : Math.Floor(tmpAmount * 100) / 100; // VND、INR以外保留小數兩位

            return new ApiResult<CryptoAmountCalculationResponse>
            {
                Data = new CryptoAmountCalculationResponse
                {
                    Amount = amount
                },
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
    }
}
