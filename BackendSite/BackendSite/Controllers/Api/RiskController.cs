using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RiskController : ControllerBase
    {
        private readonly RiskBLL _riskBll;
        private readonly GameCasinoBLL _gameCasinoBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ILogger<RiskController> _logger;

        public RiskController(RiskBLL riskBll, 
            GameCasinoBLL gameCasinoBll, 
            CustInfoBLL custInfoBll, 
            ILogger<RiskController> logger)
        {
            _riskBll = riskBll;
            _gameCasinoBll = gameCasinoBll;
            _custInfoBll = custInfoBll;
            _logger = logger;
        }

        /// <summary>
        /// Risk/GetTpDisableUser 取得第三方被禁止的用戶資料清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit]
        /// 
        /// </remarks>
        /// <param name="query"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
        [HttpPost]
        public ApiListResult<TpDisableUser> GetTpDisableUser([FromForm] QueryTpDsiableUser query)
        {
            var apiResult = new ApiListResult<TpDisableUser>();

            var pageNumber = query.PageNumber;
            try
            {
                var startTime = DateTimeExtensions.UnixToDateTime(query.StartDate);
                var endTime = DateTimeExtensions.UnixToDateTime( query.EndDate);
                
                apiResult.Data = _riskBll.GetTpDisableUser(query.SiteIds,
                    query.UserName,
                    startTime,
                    endTime, 
                    query.PageSize,
                    query.PageNumber,
                    out var totalPages,
                    out var rowCount);

                foreach (var data in apiResult.Data)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                }
                
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

        /// <summary>
        /// Risk/GetTpDisableUserByCustId 取得指定第三方被禁止的會員資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit] > Click[Edit]
        /// 
        /// </remarks>
        /// <param name="custId"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
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

        /// <summary>
        /// Risk/TpDisableUserForGame 更新會員第三方被禁止的資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit] > Click[Edit] > [Submit]
        /// 
        /// </remarks>
        /// <param name="disableGames"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
        [HttpPost]
        public async Task<ApiResult> TpDisableUserForGame([FromForm] DisableUser disableGames)
        {
            var apiResult = new ApiResult();
            try
            {
                var updateOperator = User.Identity.Name;
                var customer = await _custInfoBll.GetCustId(null,disableGames.UserName, disableGames.SiteId);
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

        /// <summary>
        /// Risk/UpdateStatus 更新會員第三方被禁止的資料狀態
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit] >Change[Status]
        /// 
        /// </remarks>
        /// <param name="disable"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
        [HttpPost]
        public async Task<ApiResult> UpdateStatus(ApiRequestWithLog<DisableDisableUser> req)
        {
            try
            {
                var updateOperator = User.Identity.Name;
                var result = await _riskBll.UpdateStatus(new ParamsWithLog<DisableDisableUser>(req, updateOperator));
                return new ApiResult
                {
                    ErrorCode = result ? 0 : 1,
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

        /// <summary>
        /// Risk/Delete 刪除會員第三方被禁止的資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit] >Click [Delete]
        /// 
        /// </remarks>
        /// <param name="custId"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
        [HttpPost]
        public async Task<ApiResult> Delete(ApiRequestWithLog<DeleteDisableUser> req)
        {
            try
            {
                var result = await _riskBll.DeleteTpDisableUser(
                    new ParamsWithLog<DeleteDisableUser>(req, User.Identity.Name));
                return new ApiResult
                {
                    ErrorCode = result ? 0 : 1
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

        /// <summary>
        /// Risk/GetTpList 取得Slot第三方品牌
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit] > Click[Edit] > Show Slot Tp List
        /// 
        /// </remarks>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
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

        /// <summary>
        /// Risk/GameCasinoByTpIdV3 取得Slot第三方品牌的遊戲
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit] > Click[Edit] > Slot Tp List > Check [Tp]
        /// 
        /// </remarks>
        /// <param name="tpId"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
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