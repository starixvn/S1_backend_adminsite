using System.ComponentModel.DataAnnotations;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        #region Fields

        private readonly ServerInfoBLL _serverInfoBll;
        private readonly DashBoardBLL _dashBoardBll;

        #endregion

        #region Constructor

        public DashBoardController(IServiceProvider provider)
        {
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _dashBoardBll = provider.GetRequiredService<DashBoardBLL>();
        }

        #endregion

        #region API Model

        public class GetDataRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int BaseCurrencyId { get; set; }

            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }

        public class GetGameRequest : GetDataRequest
        {
            public IEnumerable<int> TpId { get; set; }
            public IEnumerable<int> TpCategory { get; set; }
            public IEnumerable<int> TpGameType{ get; set; }
            public IEnumerable<int> BetStatus { get; set; } //待確認
        }

        #endregion

        /// <summary>
        /// DashBoard/GetData 取得首頁儀錶板資訊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Home > Dash Board > [Refresh]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 1, 1 })]
        [HttpPost]
        public ApiResult<DashBoardData> GetData(GetDataRequest request)
        {
            var canSearchSite = new List<int>();

            if (request.TimeZone > 14 || request.TimeZone < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            _serverInfoBll.GetSiteListByLoginUser().ToList().ForEach(x => canSearchSite.Add(x.SiteId));
            var result = _dashBoardBll.GetDashBoardData(request.SiteId, request.CurrencyId, request.BaseCurrencyId, canSearchSite, request.TimeZone);

            return new ApiResult<DashBoardData>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
        }

        /// <summary>
        /// DashBoard/GetData 取得儀錶板遊戲資訊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Home > Dash Board > [Refresh]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 1, 1 })]
        [HttpPost]
        public ApiResult<DashBoardData> GetGameData(GetDataRequest request)
        {
            var canSearchSite = new List<int>();

            if (request.TimeZone > 14 || request.TimeZone < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            //var result = _dashBoardBll.GetGameData(request.SiteId, request.CurrencyId, request.BaseCurrencyId, canSearchSite, request.TimeZone);

            return new ApiResult<DashBoardData>
            {
                Data = null,
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
        }
    }
}