using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BonusPayoutController : Controller
    {

        #region Fields
        private readonly AuthorizeBLL _authorizeBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly string _bonusApiUrl;
        private readonly CourierAdapter _courierAdapter;
        #endregion

        #region Constructors
        public BonusPayoutController(IServiceProvider provider, IConfiguration configuration)
        {
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _bonusApiUrl = configuration.GetValue<string>("BonusService");
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
        }
        #endregion

        #region API Model
        public class BonusPayoutReq
        {
            public int CustId { get; set; }
            public decimal Amount { get; set; }
            public string Memo { set; get; }
            public string BounsCode { get; set; }
            public int RollOver { get; set; }
            public int PromotionType { get; set; }
        }
        #endregion

        #region Action
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BonusPayout" })]
        [HttpPost]
        public async Task<ApiResult> SetBonusPayout(BonusPayoutReq bonusPayoutReq)
        {
            var apiResult = new ApiResult();
            string bonusUrl = $"{_bonusApiUrl}SetAdjustBonus";
            try
            {
                var authData = _authorizeBll.GetAuthData();
                var custInfo = _custInfoBll.GetCustInfoWithCache(bonusPayoutReq.CustId);

                var setAdjustBonusReq = new SetAdjustBonusReq()
                {
                    CustId = custInfo.CustId,
                    BonusCode = bonusPayoutReq.BounsCode,
                    Memo = bonusPayoutReq.Memo,
                    Operator = authData.UserName,
                    Amount = bonusPayoutReq.Amount,
                    PromotionType = bonusPayoutReq.PromotionType
                };
                var apiOnlineResult =
                    await _courierAdapter.MyPostAsync<ApiResult, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);
                if (apiOnlineResult.ErrorCode == 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    apiResult.Message = $"Success.";
                }
                else
                {
                    apiResult.ErrorCode = apiOnlineResult.ErrorCode;
                    apiResult.Message = apiOnlineResult.Message;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.ToString();
            }
            return apiResult;
        }
        #endregion
    }
}
