using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Controllers.Api;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Member.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TpDisableController : ControllerBase
    {
        #region Fields

        private readonly RiskBLL _riskBll;
        private readonly GameCasinoBLL _gameCasinoBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ILogger<RiskController> _logger;

        #endregion

        #region Constructor

        public TpDisableController(IServiceProvider provider)
        {
            _riskBll = provider.GetRequiredService<RiskBLL>();
            _gameCasinoBll = provider.GetRequiredService<GameCasinoBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _logger = provider.GetRequiredService<ILogger<RiskController>>();
        }

        #endregion

        #region Model

        public class QueryTpDisableUserReq : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public string UserName { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        public class TpDisableUserForGameDetailReq
        {
            public int CustId { get; set; }
        }
        public class DeleteReq
        {
            public int CustId { get; set; }
        }

        #endregion

        /// <summary>
        /// TpDisable/GetTpDisableUser 取得第三方被禁止的用戶資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
        [HttpPost]
        public ApiListResult<TpDisableUser> GetTpDisableUserList(QueryTpDisableUserReq req)
        {
            var apiResult = new ApiListResult<TpDisableUser>();

            var pageNumber = req.PageNumber;
            try
            {
                apiResult.Data = _riskBll.GetTpDisableUser(
                    req.SiteIds,
                    req.UserName,
                    req.StartDate,
                    req.EndDate,
                    req.PageSize,
                    req.PageNumber,
                    out var totalPages,
                    out var rowCount);
                apiResult.TotalPages = totalPages;
                apiResult.PageNumber = pageNumber;
                apiResult.RowCount = rowCount;
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// TpDisable/GetTpDisableUserByCustId 取得指定第三方被禁止的會員資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit] > Click[Edit]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
        [HttpPost]
        public async Task<ApiResult<TpDisableUserForGameDetail>> GetTpDisableUserByCustId(TpDisableUserForGameDetailReq req)
        {
            var apiResult = new ApiResult<TpDisableUserForGameDetail>();
            try
            {
                var tpList = await _gameCasinoBll.GetTpList();
                var userDisableGames = await _riskBll.GetTpDisableUserBuCustId(req.CustId);

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
                                tp.GameList = games.Select(a => new TpGameCasino()
                                {
                                    GameId = a.GameId,
                                    Name = a.Name,
                                    Checked = disableGame.GameList != null && disableGame.GameList.Any(c => c == a.GameId)
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
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }

        /// <summary>
        /// TpDisable/TpDisableUserForGame 更新會員第三方被禁止的資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit] > Click[Edit] > [Submit]
        /// </remarks>
        /// <param name="disableGames"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
        [HttpPost]
        public async Task<ApiResult> TpDisableUserForGame(DisableUser disableGames)
        {
            var apiResult = new ApiResult();
            try
            {
                var updateOperator = User.Identity.Name;
                var customer = await _custInfoBll.GetCustId(disableGames.UserName, disableGames.SiteId);
                if (customer == null)
                {
                    apiResult.ErrorCode = (int)ErrorMessageCodeEnum.NoData; ;
                    apiResult.Message = "User Not Found";
                    return apiResult;
                }

                var result = await _riskBll.DisableUserForGame(customer.CustId, customer.SiteId,
                    disableGames.DisableTpGame, updateOperator);
                return new ApiResult
                {
                    //result: 1 (新增成功),2 (修改成功)
                    ErrorCode = (result == 1 || result == 2) ? (int)ErrorMessageCodeEnum.Success : result,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResult
                {
                    ErrorCode = (int)ErrorMessageCodeEnum.SystemException,
                    Message = ex.Message,
                };
            }
        }

        /// <summary>
        /// TpDisable/UpdateStatus 更新會員第三方被禁止的資料狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit] >Change[Status]
        /// </remarks>
        /// <param name="disable"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
        [HttpPost]
        public async Task<ApiResult> UpdateStatus(DisableDisableUser disable)
        {
            try
            {
                var updateOperator = User.Identity.Name;
                var result = await _riskBll.UpdateStatus(disable.CustId, disable.Status, updateOperator);
                return new ApiResult
                {
                    ErrorCode = result == 1 ? (int)ErrorMessageCodeEnum.Success : result,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResult
                {
                    ErrorCode = (int)ErrorMessageCodeEnum.SystemException,
                    Message = ex.Message,
                };
            }
        }

        /// <summary>
        /// TpDisable/Delete 刪除會員第三方被禁止的資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Disable User For Game > [Submit] >Click [Delete]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
        [HttpPost]
        public async Task<ApiResult> Delete(DeleteReq req)
        {
            try
            {
                var result = await _riskBll.DeleteTpDisableUser(req.CustId);
                return new ApiResult
                {
                    ErrorCode = result == 1 ? (int)ErrorMessageCodeEnum.Success : result,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResult
                {
                    ErrorCode = (int)ErrorMessageCodeEnum.SystemException,
                    Message = ex.Message,
                };
            }
        }

    }
}