using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Marketing.VerificationReward
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class VerificationRewardController : ControllerBase
    {
        #region Fields
        private readonly VerificationRewardBLL _verificationRewardBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        #endregion

        #region Constructor
        public VerificationRewardController(IServiceProvider provider)
        {
            _verificationRewardBll = provider.GetService<VerificationRewardBLL>();
            _adjustmentLogReportBll = provider.GetService<AdjustmentLogReportBLL>();
        }
        #endregion

        #region Marketing > VerificationReward

        /// <summary>
        /// VerificationReward/Get 取得驗證獎勵設定
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > VerificationReward > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<VerificationRewardModel>>> Get(GetVerificationRewardRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<VerificationRewardModel>>
            {
                ErrorCode = 0,
                Data = await _verificationRewardBll.GetVerificationRewardBySiteId(request.SiteId)
            };

            return apiResult;
        }

        /// <summary>
        /// VerificationReward/Set 設定驗證獎勵
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > VerificationReward > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public async Task<ApiResult> Set(ApiRequestWithLog<SetVerificationRewardParams> request)
        {

            var updateOperator = User.Identity.Name;
            var success = await _verificationRewardBll.SetVerificationReward(request.SiteId, request.CurrencyId,
                request.After, updateOperator);

            if (success)
            {
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = updateOperator,
                    ActionType = ActionType.Update,
                    CurrencyId = request.CurrencyId,
                    SiteId = request.SiteId,
                    MenuEnum = AdminAdjustmentLogMenuEnum.VerificationRewardSetting,
                    BeforeDetailInfo = request.Before != null ? JsonConvert.SerializeObject(new
                    {
                        VerificationType = request.Before.VerificationType.ToString(),
                        request.Before.EmailReward,
                        request.Before.PhoneReward,
                        request.Before.MinDeposit
                    }) : null,
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        VerificationType = request.After.VerificationType.ToString(),
                        request.After.EmailReward,
                        request.After.PhoneReward,
                        request.After.MinDeposit
                    })
                });
            }

            return success
                ? new ApiResult((int)ErrorCodeEnum.Success, string.Empty)
                : new ApiResult((int)ErrorCodeEnum.UpdateFailed, ErrorCodeEnum.UpdateFailed.GetDescription());
        }

        /// <summary>
        /// VerificationReward/UpdateStatus 變更狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > VerificationReward > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public async Task<ApiResult> UpdateStatus(ApiRequestWithLog<UpdateVerificationRewardStatusParams> request)
        {
            var updateOperator = User.Identity.Name;
            var result = await _verificationRewardBll.UpdateStatus(request.SiteId, request.CurrencyId, (int)request.After.Status, updateOperator);
            var success = result == 1 || result == 2;
            if (success)
            {
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = updateOperator,
                    ActionType = ActionType.Update,
                    CurrencyId = request.CurrencyId,
                    SiteId = request.SiteId,
                    MenuEnum = AdminAdjustmentLogMenuEnum.VerificationRewardSetting,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        Status = request.Before.Status.ToString()
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        Status = request.After.Status.ToString()
                    })
                });
            }

            return success
                ? new ApiResult((int)ErrorCodeEnum.Success, string.Empty)
                : new ApiResult((int)ErrorCodeEnum.UpdateFailed, ErrorCodeEnum.UpdateFailed.GetDescription());
        }
        #endregion
    }
}