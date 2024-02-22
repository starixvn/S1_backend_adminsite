using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Marketing.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class LeaderBoardManagementController : ControllerBase
    {
        #region Fields

        private readonly ExcelBLL _excelBll;
        private readonly LeaderBoardBLL _leaderBoardBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ILogger<LeaderBoardAdjustmentController> _logger;

        #endregion

        #region Constructor

        public LeaderBoardManagementController(IServiceProvider provider)
        {
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _leaderBoardBll = provider.GetRequiredService<LeaderBoardBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _logger = provider.GetRequiredService<ILogger<LeaderBoardAdjustmentController>>();
        }

        #endregion

        #region API

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 151, 1 })]
        public ApiResult<IEnumerable<LeaderboardRankingBonusList>> GetLeaderboardRankingBonus(LeaderboardRankingBonusReq request)
        {
            var apiResult = new ApiResult<IEnumerable<LeaderboardRankingBonusList>>();

            var data = _leaderBoardBll.GetLeaderboardRankingBonus(JsonConvert.SerializeObject(request.SiteIds));
            var groupedData = data
                .GroupBy(item => new { item.SiteId, item.SiteName, item.Currency, item.CurrencyId })
                .Select(group => new LeaderboardRankingBonusList
                {
                    SiteId = group.Key.SiteId,
                    SiteName = group.Key.SiteName,
                    Currency = group.Key.Currency,
                    CurrencyId = group.Key.CurrencyId,
                    Data = group.Select(item => new LeaderboardRankingBonusDetail
                    {
                        Ranking = item.Ranking,
                        RankingName = $"TOP {item.Ranking}",
                        MinPoint = item.MinPoint,
                        Bonus = item.Bonus
                    })
                });
            
            apiResult.Data = groupedData;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 151, 3 })]
        public ApiResult SetLeaderboardRankingBonus(ApiRequestWithLog<SetLeaderboardRankingBonusReq> req)
        {
            var apiResult = new ApiResult();

            try
            {
                var authData = _authorizeBll.GetAuthData();
                var result = _leaderBoardBll.SetLeaderboardRankingBonus(new ParamsWithLog<SetLeaderboardRankingBonusReq>(req, authData.UserName));
                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 151, 1 })]
        public ApiResult<IEnumerable<LeaderboardPointCalculation>> GetLeaderboardPointCalculation(LeaderboardRankingBonusReq request)
        {
            var apiResult = new ApiResult<IEnumerable<LeaderboardPointCalculation>>();

            var date = DateTime.UtcNow.ToString("yyyyMM");
            var data = _leaderBoardBll.GetLeaderboardPointCalculation(JsonConvert.SerializeObject(request.SiteIds) ,date);
            
            apiResult.Data = data;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 151, 3 })]
        public ApiResult SetLeaderboardPointCalculation(ApiRequestWithLog<SetLeaderboardPointCalculationReq> req)
        {
            var apiResult = new ApiResult();

            try
            {
                var authData = _authorizeBll.GetAuthData();
                var date = DateTime.UtcNow.ToString("yyyyMM");
                var result = _leaderBoardBll.SetLeaderboardPointCalculation
                    (new ParamsWithLog<SetLeaderboardPointCalculationReq>(req, authData.UserName) ,date);
                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }
        
        
        #endregion
    }
}


