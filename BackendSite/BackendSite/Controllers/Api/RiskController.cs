using System;
using System.Threading.Tasks;
using Amazon.S3.Util;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RiskController : Controller
    {
        private readonly RiskBLL _riskBll;
        private readonly ILogger<RiskController> _logger;

        public RiskController(RiskBLL riskBll, ILogger<RiskController> logger)
        {
            _riskBll = riskBll;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ApiResult> TpDisableUserForGame(TpDisableUser tpDisableUser)
        {
            try
            {
                tpDisableUser.UpdateOperator = User.Identity.Name;
                var result = await _riskBll.DisableUserForGame(tpDisableUser);
                return new ApiResult
                {
                    ErrorCode = result,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResult
                {
                    ErrorCode = 999,
                    Message = ex.Message,
                };
            }
            
        }
    }
}