using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
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
        private readonly VerificationRewardBLL _verificationRewardBll;
        public VerificationRewardController(VerificationRewardBLL verificationRewardBll)
        {
            _verificationRewardBll = verificationRewardBll;
        }
        
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

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult> Set(SetVerificationRewardRequest request)
        {
            var updateOperator = User.Identity.Name;
            var result = await _verificationRewardBll.SetVerificationReward(request.SiteId, request.CurrencyId,
                request.VerificationType, request.PhoneReward, request.EmailReward, updateOperator);
 
            var apiResult = new ApiResult()
            {
                ErrorCode = (result == 1 || result == 2) ? (int) ErrorCodeEnum.Success : (int) ErrorCodeEnum.UpdateFailed,
                Message = ErrorCodeEnum.UpdateFailed.GetDescription()
            };
            return apiResult;
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult> UpdateStatus(UpdateVerificationRewardStatusReq request)
        {
            var updateOperator = User.Identity.Name;
            var result = await _verificationRewardBll.UpdateStatus(request.SiteId, request.CurrencyId, request.Status, updateOperator);
            var apiResult = new ApiResult()
            {
                ErrorCode = result == 1 ? (int) ErrorCodeEnum.Success : (int) ErrorCodeEnum.UpdateFailed,
                Message = ErrorCodeEnum.UpdateFailed.GetDescription()
            };
            return apiResult;
        }
    }
}