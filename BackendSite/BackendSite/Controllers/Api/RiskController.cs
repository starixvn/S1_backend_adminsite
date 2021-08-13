using System.Threading.Tasks;
using Amazon.S3.Util;
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
    public class RiskController : Controller
    {
        private readonly RiskBLL _riskBll;

        public RiskController(RiskBLL riskBll)
        {
            _riskBll = riskBll;
        }

        [HttpPost]
        public async Task<ApiResult> TpBanningUser(TpDisableUser tpDisableUser)
        {
            tpDisableUser.UpdateOperator = User.Identity.Name;
            var result = await _riskBll.DisableUserForGame(tpDisableUser);
            return new ApiResult
            {
                ErrorCode = result,
            };
        }
    }
}