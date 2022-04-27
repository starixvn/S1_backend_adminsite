using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class VerificationRewardController : Controller
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

        #region Action

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult<VerificationRewardModel>> Get(GetVerificationRewardRequest request)
        {
            var apiResult = new ApiResult<VerificationRewardModel>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = await _verificationRewardBll.GetVerificationReward(request.SiteId, request.CurrencyId)
            };
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult> Set(SetVerificationRewardRequest request)
        {
            var updateOperator = User.Identity.Name;
            var result = await _verificationRewardBll.SetVerificationReward(request.SiteId, request.CurrencyId,
                (int)request.VerificationType, request.PhoneReward, request.EmailReward, updateOperator);

            if (result != 1 || result != 2)
            {
                throw new AdminException(ErrorCodeEnum.UpdateFailed);
            }

            return new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Success };
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult> UpdateStatus(UpdateVerificationRewardStatusReq request)
        {
            var updateOperator = User.Identity.Name;
            var result = await _verificationRewardBll.UpdateStatus(request.SiteId, request.CurrencyId, (int)request.Status, updateOperator);

            if (result != 1)
            {
                throw new AdminException(ErrorCodeEnum.UpdateFailed);
            }

            return new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Success };
        }

        #endregion
    }
}