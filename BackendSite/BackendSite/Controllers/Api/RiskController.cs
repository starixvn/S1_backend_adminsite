using System;
using System.Collections.Generic;
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
        private readonly GameCasinoBLL _gameCasinoBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ILogger<RiskController> _logger;

        public RiskController(RiskBLL riskBll, GameCasinoBLL gameCasinoBll, CustInfoBLL custInfoBll, ILogger<RiskController> logger)
        {
            _riskBll = riskBll;
            _gameCasinoBll = gameCasinoBll;
            _custInfoBll = custInfoBll;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ApiResult> TpDisableUserForGame([FromForm] DisableUser disableGames)
        {
            var apiResult = new ApiResult();
            try
            {
                var updateOperator = User.Identity.Name;
                var customer = await _custInfoBll.GetCustId(disableGames.UserName, disableGames.SiteId);
                if (customer == null)
                {
                    apiResult.ErrorCode = -2;
                    apiResult.Message = "User Not Found";
                    return apiResult;
                }

                var result = await _riskBll.DisableUserForGame(customer.CustId, customer.SiteId,
                    disableGames.DisableTpGame, updateOperator);
                return new ApiResult
                {
                    ErrorCode = result == 1 ? 0 : result,
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

        [HttpGet]
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/Risk" })]
        public async Task<ApiResult<IEnumerable<RiskTpListInfo>>> GetTpList()
        {
            var apiResult = new ApiResult<IEnumerable<RiskTpListInfo>>();
            try
            {
                {
                    apiResult.Data = await _gameCasinoBll.GetTpList();
                    apiResult.ErrorCode = 0;
                };
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            

            return apiResult;
        }

        [HttpGet]
        [Route("{tpId:int}")]
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/GameCasinoManagement" })]
        public async Task<ApiResult<IEnumerable<RiskGameCasinoData>>> GameCasinoByTpIdV3(int tpId)
        {
            var apiResult = new ApiResult<IEnumerable<RiskGameCasinoData>>();
            try
            {
                var gameCasinoData = await _gameCasinoBll.GameCasinoByTPIdV3(tpId);
                apiResult.Data = gameCasinoData;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
    }
}