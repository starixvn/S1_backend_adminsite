using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Marketing.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class VerificationRewardController : ControllerBase
    {
        #region Fields
        private readonly VerificationRewardBLL _verificationRewardBll;
        #endregion
        
        #region Constructor
        public VerificationRewardController(VerificationRewardBLL verificationRewardBll)
        {
            _verificationRewardBll = verificationRewardBll;
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult<VerificationRewardModel>> Get(GetVerificationRewardRequest request)
        {
            var apiResult = new ApiResult<VerificationRewardModel>
            {
                ErrorCode = 0, 
                Data = await _verificationRewardBll.GetVerificationReward(request.SiteId, request.CurrencyId)
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult<VerificationRewardModel>> Set(SetVerificationRewardRequest request)
        {
            var apiResult = new ApiResult<VerificationRewardModel>();
            var updateOperator = User.Identity.Name;
            var result = await _verificationRewardBll.SetVerificationReward(request.SiteId, request.CurrencyId,
                request.VerificationType, request.PhoneReward, request.EmailReward, updateOperator);
            
            if (result == 1 || result == 2)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                apiResult.Data = await _verificationRewardBll.GetVerificationReward(request.SiteId, request.CurrencyId);
            }
            else
            {
                apiResult.ErrorCode = result;
            }
            return apiResult;
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult> UpdateStatus(UpdateVerificationRewardStatusReq request)
        {
            var apiResult = new ApiResult<VerificationRewardModel>();
            var updateOperator = User.Identity.Name;
            var result = await _verificationRewardBll.UpdateStatus(request.SiteId, request.CurrencyId, request.Status, updateOperator);
            if (result == 1)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                apiResult.Data = await _verificationRewardBll.GetVerificationReward(request.SiteId, request.CurrencyId);
            }
            else
            {
                apiResult.ErrorCode = result;
            }

            return apiResult;
        }
        #endregion
    }
}