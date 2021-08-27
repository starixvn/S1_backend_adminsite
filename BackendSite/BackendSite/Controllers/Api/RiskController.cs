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
        private readonly GameCasinoBLL _gameCasinoBLL;
        private readonly ILogger<RiskController> _logger;

        public RiskController(RiskBLL riskBll, GameCasinoBLL gameCasinoBll, ILogger<RiskController> logger)
        {
            _riskBll = riskBll;
            _gameCasinoBLL = gameCasinoBll;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ApiResult> TpDisableUserForGame([FromForm] DisableUser tpDisableUser)
        {
            try
            {
                tpDisableUser.UpdateOperator = User.Identity.Name;
                var result = await _riskBll.DisableUserForGame(tpDisableUser);
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

        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/Risk" })]
        public async Task<ApiResult<IEnumerable<RiskTpListInfo>>> GetTpList()
        {
            var apiResult = new ApiResult<IEnumerable<RiskTpListInfo>>();
            try
            {
                {
                    apiResult.Data = await _gameCasinoBLL.GetTpList();
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
                var gameCasinoData = await _gameCasinoBLL.GameCasinoByTPIdV3(tpId);
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