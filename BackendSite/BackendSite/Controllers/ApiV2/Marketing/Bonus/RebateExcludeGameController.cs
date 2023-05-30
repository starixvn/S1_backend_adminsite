using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus;

[Authorize(AuthenticationSchemes = "backendV2")]
[Route("apiV2/[controller]/[action]")]
[ApiController]
public class RebateExcludeGameController : ControllerBase
{
    #region Fields

    private readonly ServerInfoBLL _serverInfoBll;
    private readonly RebateManageBLL _rebateManageBll;
    private readonly NewAuthorizeBLL _newAuthorizeBll;
    private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
    private readonly ILogger<RebateExcludeGameController> _logger;

    #endregion

    #region Constructor

    public RebateExcludeGameController(IServiceProvider provider)
    {
        _serverInfoBll = provider.GetService<ServerInfoBLL>();
        _rebateManageBll = provider.GetRequiredService<RebateManageBLL>();
        _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
        _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
        _logger = provider.GetRequiredService<ILogger<RebateExcludeGameController>>();
    }

    #endregion

    #region Model

    public class GetListRequest
    {
        public int SiteId { get; set; }
    }
    public class GetByCurrencyIdRequest
    {
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }
    }

    public class SetRequest
    {
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }

        public IEnumerable<ExcludeGame> ExcludeGameList { get; set; }
    }

    public class ExcludeGame
    {
        public int TpId { get; set; }
        public int GameType { get; set; }
        public int CategoryId { get; set; }
        public string GameId { get; set; }
        public string AppId { get; set; }
    }

    #endregion

    /// <summary>
    /// RebateExcludeGame/GetList
    /// </summary>
    /// <remarks>
    /// Http Method：POST
    /// 
    /// Func Path：
    /// </remarks>
    /// <param name="req"></param>
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 145, 1 })]
    [HttpPost]
    public ApiResult<IEnumerable<int>> GetList(GetListRequest req)
    {
        return new ApiResult<IEnumerable<int>>()
        {
            Data = _serverInfoBll.GetServerInfoWithSiteId(req.SiteId).ICurrencyList,
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }

    /// <summary>
    /// RebateExcludeGame/GetByCurrencyId
    /// </summary>
    /// <remarks>
    /// Http Method：POST
    /// 
    /// Func Path：
    /// </remarks>
    /// <param name="req"></param>
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 145, 1 })]
    [HttpPost]
    public async Task<ApiResult<IEnumerable<RebateExcludeGame>>> GetByCurrencyId(GetByCurrencyIdRequest req)
    {
        return new ApiResult<IEnumerable<RebateExcludeGame>>()
        {
            Data = await _rebateManageBll.GetRebateExcludeGameByCurrencyIdAsync(req.SiteId, req.CurrencyId),
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }

    /// <summary>
    /// RebateExcludeGame/Set
    /// </summary>
    /// <remarks>
    /// Http Method：POST
    /// 
    /// Func Path：
    /// </remarks>
    /// <param name="disableGames"></param>
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 145, 3 })]
    [HttpPost]
    public async Task<ApiResult> Set(SetRequest req)
    {
        var apiResult = new ApiResult();
        var authData = _newAuthorizeBll.GetAuthData();
        var updateOperator = authData.UserName;

        var beforeDetailInfo = await _rebateManageBll.GetRebateExcludeGameByCurrencyIdAsync(req.SiteId, req.CurrencyId);

        if (await _rebateManageBll.SetRebateExcludeGameByCurrencyIdAsync(
            req.SiteId,
            req.CurrencyId,
            req.ExcludeGameList, updateOperator))
        {
            var afterDetailInfo = await _rebateManageBll.GetRebateExcludeGameByCurrencyIdAsync(req.SiteId, req.CurrencyId);

            _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
            {
                Operator = updateOperator,
                SiteId = req.SiteId,
                CurrencyId = req.CurrencyId,
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.RebateExcludeGame,
                BeforeDetailInfo = beforeDetailInfo.Any() ? JsonConvert.SerializeObject(beforeDetailInfo) : null,
                AfterDetailInfo = afterDetailInfo.Any() ? JsonConvert.SerializeObject(afterDetailInfo) : null,
                RefTransId = string.Empty
            });

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
        }

        return new ApiResult
        {
            ErrorCode = (int)ErrorCodeEnum.UpdateFailed,
        };
    }

}
