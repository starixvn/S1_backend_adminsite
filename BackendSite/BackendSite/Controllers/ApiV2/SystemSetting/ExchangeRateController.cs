using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.SystemSetting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class ExchangeRateController : ControllerBase
    {
        private readonly ExchangeUSDRateBLL _ExchangeUSDRateBLL;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBLL;
        private readonly NewAuthorizeBLL _authorizeBll;

        public ExchangeRateController(IServiceProvider provider)
        {
            _ExchangeUSDRateBLL = provider.GetRequiredService<ExchangeUSDRateBLL>();
            _adjustmentLogReportBLL = provider.GetRequiredService<AdjustmentLogReportBLL>();
            _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
        }

        /// <summary>
        /// System Setting > Exchange  Rate 取得指定幣別對美元的匯率
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：System Setting > Exchange Rate [Submit]
        /// </remarks>
        /// <param name="request">
        /// </param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 129, 1 })]
        public ApiResult<GetExchangeUSDRateResponse> GetUSD(GetExchangeUSDRateRequest request)
        {
            var data = _ExchangeUSDRateBLL.GetExchangeUSDRate(request.CurrencyId);

            return new ApiResult<GetExchangeUSDRateResponse>((int)ErrorCodeEnum.Success, "Success", data);
        }

        /// <summary>
        /// System Setting > Exchange  Rate 設定指定幣別對美元的匯率
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：System Setting > Exchange Rate [Submit]
        /// </remarks>
        /// <param name="request">
        /// </param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 129, 3 })]
        public ApiResult SetUSD(SetExchangeUSDRateRequest request)
        {
            var oldUSDRate = _ExchangeUSDRateBLL.GetExchangeUSDRate(request.CurrencyId);
            var result = _ExchangeUSDRateBLL.SetExchangeUSDRate(request);

            if (result)
            {
                _adjustmentLogReportBLL.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = _authorizeBll.GetAuthData().UserName,
                    SiteId = _authorizeBll.CheckBrand(),
                    CurrencyId = request.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.SetCurrencyUSDRate,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new { request.CurrencyId, USDRate = oldUSDRate }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new { request.CurrencyId, USDRate = request.USDRate }),
                    RefTransId = string.Empty
                });
                return new ApiResult()
                {
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }

            return new ApiResult()
            {
                ErrorCode = (int)ErrorCodeEnum.Fail
            };
        }
    }
}
