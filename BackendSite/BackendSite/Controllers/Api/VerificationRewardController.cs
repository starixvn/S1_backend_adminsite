using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
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
        private readonly VerificationRewardBLL _verificationRewardBll;
        public VerificationRewardController(VerificationRewardBLL verificationRewardBll)
        {
            _verificationRewardBll = verificationRewardBll;
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult<IEnumerable<VerificationRewardModel>>> Get(GetVerificationRewardRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<VerificationRewardModel>>
            {
                ErrorCode = 0, 
                Data = await _verificationRewardBll.GetVerificationRewardSetting(request.SiteId)
            };
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult> Set(SetVerificationRewardRequest req)
        {
            var updateOperator = User.Identity.Name;
            var result = await _verificationRewardBll.SetVerificationReward(req.SiteId, req.CurrencyId,
                req.VerificationType, req.PhoneReward, req.EmailReward, updateOperator);
            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult> UpdateStatus(UpdateVerificationRewardStatusReq req)
        {
            var updateOperator = User.Identity.Name;
            var result = await _verificationRewardBll.UpdateStatus(req.SiteId, req.CurrencyId, req.Status, updateOperator);
            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
        }
    }
}