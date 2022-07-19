using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
        public async Task<ApiResult> Set(SetVerificationRewardRequest request)
        {
            
            var updateOperator = User.Identity.Name;
            var oldData = await _verificationRewardBll.GetVerificationReward(request.SiteId, request.CurrencyId);
            var result = await _verificationRewardBll.SetVerificationReward(request.SiteId, request.CurrencyId,
                (int)request.VerificationType, request.PhoneReward, request.EmailReward, updateOperator);

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
                        VerificationType = ((VerificationTypeEnum) oldData.VerificationType).ToString(),
                        oldData.EmailReward,
                        oldData.PhoneReward
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        VerificationType = request.VerificationType.ToString(),
                        request.EmailReward,
                        request.PhoneReward
                    })
                });
            }
            
            return success
                ? new ApiResult((int) ErrorCodeEnum.Success, string.Empty)
                : new ApiResult((int) ErrorCodeEnum.UpdateFailed, ErrorCodeEnum.UpdateFailed.GetDescription());
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
        public async Task<ApiResult> UpdateStatus(UpdateVerificationRewardStatusReq request)
        {
            var updateOperator = User.Identity.Name;
            var result = await _verificationRewardBll.UpdateStatus(request.SiteId, request.CurrencyId,(int)request.Status, updateOperator);
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
                        Status = (request.Status == VerificationRewardStatusEnum.Enable
                            ? VerificationRewardStatusEnum.Disable
                            : VerificationRewardStatusEnum.Enable).ToString()
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        Status = request.Status.ToString()
                    })
                });
            }
            
            return success
                ? new ApiResult((int) ErrorCodeEnum.Success, string.Empty)
                : new ApiResult((int) ErrorCodeEnum.UpdateFailed, ErrorCodeEnum.UpdateFailed.GetDescription());
        }
        #endregion
    }
}