using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        public ApiListResult<TpDisableUser> GetTpDisableUser([FromForm] QueryTpDsiableUser query)
        {
            var apiResult = new ApiListResult<TpDisableUser>();

            var pageNumber = query.PageNumber;
            try
            {
                apiResult.Data = _riskBll.GetTpDisableUser(query.SiteIds, query.UserName, query.StartDate, query.EndDate, 
                                                           query.PageSize, query.PageNumber, out var totalPages, out var rowCount);
                apiResult.TotalPages = totalPages;
                apiResult.PageNumber = pageNumber;
                apiResult.RowCount = rowCount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpGet]
        [Route("{custId:int}")]
        public async Task<ApiResult<TpDisableUserForGameDetail>> GetTpDisableUserByCustId(int custId)
        {
            var apiResult = new ApiResult<TpDisableUserForGameDetail>();
            try
            {
                var tpList = await _gameCasinoBll.GetTpList();
                var userDisableGames = await _riskBll.GetTpDisableUserBuCustId(custId);

                if (userDisableGames.GameInfo != null && userDisableGames.GameInfo != "null")
                {
                    var disableGames = JsonConvert.DeserializeObject<List<DisableTpGame>>(userDisableGames.GameInfo);

                    foreach (var disableGame in disableGames)
                    {
                        foreach (var tp in tpList)
                        {
                            if (disableGame.TpId == tp.TpId)
                            {
                                tp.Checked = true;
                                var games = await _gameCasinoBll.GameCasinoByTPIdV3(tp.TpId);
                                tp.GameList = games.Select(a=> new TpGameCasino()
                                {
                                    GameId = a.GameId,
                                    Name = a.Name,
                                    Checked = disableGame.GameList != null && disableGame.GameList.Any(c=>c == a.GameId)
                                }).ToList();
                            }
                        }
                    }
                }
                
                var editInfo = new TpDisableUserForGameDetail()
                {
                    SiteId = userDisableGames.SiteId,
                    UserName = userDisableGames.UserName,
                    DisableGames = tpList.ToList()
                };
                apiResult.Data = editInfo;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;
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
                    //result: 1 (新增成功),2 (修改成功)
                    ErrorCode = (result == 1 || result == 2) ? 0 : result,
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
        public async Task<ApiResult> UpdateStatus([FromForm] DisableDisableUser disable)
        {
            try
            {
                var updateOperator = User.Identity.Name;
                var result = await _riskBll.UpdateStatus(disable.CustId, disable.Status, updateOperator);
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
        public async Task<ApiResult> Delete([FromForm] int custId)
        {
            try
            {
                var result = await _riskBll.DeleteTpDisableUser(custId);
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
                _logger.LogError(ex, ex.Message);
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            

            return apiResult;
        }

        [HttpGet]
        [Route("{tpId:int}")]
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
                _logger.LogError(ex, ex.Message);
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
    }
}