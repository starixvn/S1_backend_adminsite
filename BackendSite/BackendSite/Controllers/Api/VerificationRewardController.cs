using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
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
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationReward" })]
        public async Task<ApiResult<IEnumerable<VerificationRewardModel>>> Get(int siteId)
        {
            var apiResult = new ApiResult<IEnumerable<VerificationRewardModel>>();
            var reward = await _verificationRewardBll.GetVerificationRewardSetting(siteId);
            return apiResult;
        }
    }
}