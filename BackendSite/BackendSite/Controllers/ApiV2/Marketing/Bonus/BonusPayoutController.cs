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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BonusPayoutController : ControllerBase
    {
        #region Fields
        private readonly AuthorizeBLL _authorizeBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        private readonly string _bonusApiUrl;
        private readonly CourierAdapter _courierAdapter;
        #endregion

        #region Constructors
        public BonusPayoutController(IServiceProvider provider, IConfiguration configuration)
        {
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
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
            public PromotionTypeEnum PromotionType { get; set; }
        }
        #endregion

        /// <summary>
        /// BonusPayout/SetBonusPayout 後台手動領取Bonus
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance  > BonusPayout > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BonusPayout" })]
        [HttpPost]
        public async Task<ApiResult> SetBonusPayout(BonusPayoutReq request)
        {
            var apiResult = new ApiResult();
            string bonusUrl = $"{_bonusApiUrl}SetAdjustBonus";
            try
            {
                var authData = _authorizeBll.GetAuthData();
                var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);

                var detailInfo = new
                {
                    UserName = custInfo.UserName,
                    BonusCode = request.BounsCode,
                    Memo = request.Memo,
                    Amount = request.Amount,
                    PromotionType = request.PromotionType.ToString()
                };
                
                var logResult =
                    _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                    {
                        Operator = authData.UserName,
                        SiteId = custInfo.SiteId,
                        CurrencyId = custInfo.CurrencyId,
                        ActionType = ActionType.Create,
                        MenuEnum = AdminAdjustmentLogMenuEnum.IssueBonusManagement,
                        BeforeDetailInfo = null,
                        AfterDetailInfo = JsonConvert.SerializeObject(detailInfo),
                        RefTransId = string.Empty
                    });
                
                if (!logResult)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Insert log error.";
                    return apiResult;
                }
                
                var setAdjustBonusReq = new SetAdjustBonusReq()
                {
                    CustId = custInfo.CustId,
                    BonusCode = request.BounsCode,
                    Memo = request.Memo,
                    Operator = authData.UserName,
                    Amount = request.Amount,
                    PromotionType = (int) request.PromotionType
                };
                var apiOnlineResult =
                    await _courierAdapter.MyPostAsync<ApiResult, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);
                if (apiOnlineResult.ErrorCode == (int)ErrorCodeEnum.Success)
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
    }
}
