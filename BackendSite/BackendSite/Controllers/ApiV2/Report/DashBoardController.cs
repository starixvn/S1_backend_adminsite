using System.ComponentModel.DataAnnotations;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
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

        public class GetMemBerDataRequest
        {
            public int SiteId { get; set; }
            public int BaseCurrencyId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }

            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }
        
        public class GetGameRequest
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            public IEnumerable<int> TpIds { get; set; }
            public IEnumerable<int> Categories { get; set; }
            public IEnumerable<int> GameTypes { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }

        public class GetGameTopListRequest : ApiRequestPageModel
        {
            public DashboardGameTopDataTypeEnum Type { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            public IEnumerable<int> TpIds { get; set; }
            public IEnumerable<int> Categories { get; set; }
            public IEnumerable<int> GameTypes { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
            public string OrderByProperty { get; set; }
            public bool IsDesc { get; set; }
        }

        public class GetNewMemBerListRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }
        
        public class GetTopDepositorsListRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
            public DashboardOrderByPropertyEnum OrderByProperty { get; set; }  = DashboardOrderByPropertyEnum.Amount ;
        }
        
        public class GetTopWithdrawalListRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
            public DashboardOrderByPropertyEnum OrderByProperty { get; set; } = DashboardOrderByPropertyEnum.Amount ;
        }
        
        public class GetTopBonusClaimedListRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
            public DashboardOrderByPropertyEnum OrderByProperty { get; set; } = DashboardOrderByPropertyEnum.Amount ;
        }
        
        public class GetFinancialDataRequest
        {
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }

        #endregion

        #region DashBoardMember

        /// <summary>
        /// DashBoard/GetMemBerData 取得首頁儀錶板MemBer資訊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Home > Dash Board > [MemBer]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 1, 1 })]
        [HttpPost]
        public ApiResult<DashBoardMemBerData> GetMemBerData(GetMemBerDataRequest request)
        {
            if (request.TimeZone is > 14 or < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            var result = _dashBoardBll.GetDashBoardMemberData(request.SiteId, request.CurrencyIds, request.TimeZone);

            return new ApiResult<DashBoardMemBerData>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
        }

        /// <summary>
        /// DashBoard/GetNewMemBerList 取得首頁儀錶板MemBer_NewMemBer資訊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Home > Dash Board > [MemBer] > [NewMemBer]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiListResult<NewMemberList> GetNewMemBerList(GetNewMemBerListRequest request)
        {
            if (request.TimeZone is > 14 or < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            var localTime = DateTime.UtcNow.AddHours(request.TimeZone);
            var startTime = localTime.Date;
            var endTime = startTime.AddDays(1).AddMilliseconds(-1);

            var result = _dashBoardBll.GetNewMemberList(
                request.SiteId,
                request.CurrencyIds,
                startTime,
                endTime,
                request.PageSize,
                request.PageNumber,
                out var totalPages,
                out var rowCount);

            return new ApiListResult<NewMemberList>
            {
                Data = result,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion

        #region DashBoardFinancial

        /// <summary>
        /// DashBoard/GetFinancialData 取得首頁儀錶板Financial資訊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Home > Dash Board > [Financial]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 1, 1 })]
        [HttpPost]
        public ApiResult<DashBoardFinancialData> GetFinancialData(GetFinancialDataRequest request)
        {
            if (request.TimeZone is > 14 or < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            var result = _dashBoardBll.GetDashBoardFinancialData(request.SiteId, request.CurrencyIds, request.BaseCurrencyId, request.TimeZone);

            return new ApiResult<DashBoardFinancialData>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
        }
        
        /// <summary>
        /// DashBoard/GetTopDepositorsList 取得首頁儀錶板Financial資訊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Home > Dash Board > [Financial] > [Top Depositors]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiListResult<TopDepositorsList> GetTopDepositorsList(GetTopDepositorsListRequest request)
        {
            if (request.TimeZone is > 14 or < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            var localTime = DateTime.UtcNow.AddHours(request.TimeZone);
            var startTime = localTime.Date;
            var endTime = startTime.AddDays(1).AddMilliseconds(-1);
            
            var result = _dashBoardBll.GetTopDepositorsList(
                request.SiteId,
                request.CurrencyIds,
                request.BaseCurrencyId,
                startTime,
                endTime,
                request.OrderByProperty,
                request.PageSize,
                request.PageNumber,
                out var totalPages,
                out var rowCount);

            return new ApiListResult<TopDepositorsList>
            {
                Data = result,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        /// <summary>
        /// DashBoard/GetTopWithdrawalList 取得首頁儀錶板Financial資訊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Home > Dash Board > [Financial] > [Top Withdrawal]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiListResult<TopWithdrawalList> GetTopWithdrawalList(GetTopWithdrawalListRequest request)
        {
            if (request.TimeZone is > 14 or < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            var localTime = DateTime.UtcNow.AddHours(request.TimeZone);
            var startTime = localTime.Date;
            var endTime = startTime.AddDays(1).AddMilliseconds(-1);
            
            var result = _dashBoardBll.GetTopWithdrawalList(
                request.SiteId,
                request.CurrencyIds,
                request.BaseCurrencyId,
                startTime,
                endTime,
                request.OrderByProperty,
                request.PageSize,
                request.PageNumber,
                out var totalPages,
                out var rowCount);

            return new ApiListResult<TopWithdrawalList>
            {
                Data = result,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        /// <summary>
        /// DashBoard/GetTopBonusClaimedList 取得首頁儀錶板Financial資訊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Home > Dash Board > [Financial] > [Top Bonus Claimed]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiListResult<TopBonusClaimedList> GetTopBonusClaimedList(GetTopBonusClaimedListRequest request)
        {
            if (request.TimeZone is > 14 or < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            var localTime = DateTime.UtcNow.AddHours(request.TimeZone);
            var startTime = localTime.Date;
            var endTime = startTime.AddDays(1).AddMilliseconds(-1);
            
            var result = _dashBoardBll.GetTopBonusClaimedList(
                request.SiteId,
                request.CurrencyIds,
                request.BaseCurrencyId,
                startTime,
                endTime,
                request.OrderByProperty,
                request.PageSize,
                request.PageNumber,
                out var totalPages,
                out var rowCount);

            return new ApiListResult<TopBonusClaimedList>
            {
                Data = result,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        #endregion

        #region DashBoardGame

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
        public ApiResult<DashBoardGameDto> GetGameData(GetGameRequest request)
        {
            if (request.TimeZone > 14 || request.TimeZone < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            var result = _dashBoardBll.GetGame(request.SiteIds,
                request.CurrencyIds, 
                request.BaseCurrencyId,
                request.TpIds,
                request.Categories,
                request.GameTypes,
                request.TimeZone);

            return new ApiResult<DashBoardGameDto>
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
        public ApiResult<Dictionary<string,List<DashBoardGameOverAllDto>>> GetGameOverAllData(GetGameRequest request)
        {
            if (request.TimeZone > 14 || request.TimeZone < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            var result = _dashBoardBll.GetGameOverall(request.SiteIds,
                request.CurrencyIds,
                request.BaseCurrencyId,
                request.TpIds,
                request.Categories,
                request.GameTypes,
                request.TimeZone);

            return new ApiResult<Dictionary<string, List<DashBoardGameOverAllDto>>>
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
        public ApiListResult<DashBoardGameTopDto> GetGameTopTwentyData(GetGameTopListRequest request)
        {

            var result = _dashBoardBll.GetGameTopTwenty(
                request.Type,
                request.SiteIds,
                request.CurrencyIds,
                request.BaseCurrencyId,
                request.TpIds,
                request.Categories,
                request.GameTypes,
                request.OrderByProperty,
                request.IsDesc,
                request.TimeZone,
                request.PageSize,
                request.PageNumber,
                out var totalPages,
                out var rowCount);

            return new ApiListResult<DashBoardGameTopDto>
            {
                Data = result,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion
    }
}