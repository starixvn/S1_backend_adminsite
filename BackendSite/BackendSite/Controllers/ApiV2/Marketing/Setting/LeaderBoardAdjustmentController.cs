using System;
using System.Collections.Generic;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Marketing.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class LeaderBoardAdjustmentController : ControllerBase
    {
        #region Fields

        private readonly BonusManageBLL _bonusManageBll;
        private readonly AuthorizeBLL _authorizeBll;

        #endregion

        #region Constructor

        public LeaderBoardAdjustmentController(IServiceProvider provider)
        {
            _bonusManageBll = provider.GetRequiredService<BonusManageBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
        }

        #endregion

        #region Model

        public class LeaderboardExtraPointReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public int CategoryId { get; set; }
            public int Currency { get; set; }
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
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        public class SetLeaderboardExtraPointReq
        {
            public long ActiveDate { get; set; }
            public int SiteId { get; set; }
            public int CategoryId { get; set; }
            public string UserName { get; set; }
            public decimal ExtraPoint { get; set; }
        }
        
        #endregion
        
        #region API 
        
        /// <summary>
        /// LeaderBoardAdjustment/SetExtraPoint 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > LeaderBoard ExtraPoint > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiListResult<LeaderboardExtraPointList> GetList(LeaderboardExtraPointReq request)
        {
            var apiResult = new ApiListResult<LeaderboardExtraPointList>();

            var pageNumber = request.PageNumber;
            var pageSize = 10;
            if (request.PageSize > 0)
                pageSize = request.PageSize;
            
            apiResult.Data = _bonusManageBll.GetLeaderboardExtraPointList(request.SiteIds,
                request.CategoryId, request.Currency, request.StartDate.DateTime,
                request.EndDate.DateTime, pageSize, ref pageNumber, out var totalPages, out var rowCount);
            
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = (int) ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// LeaderBoardAdjustment/SetExtraPoint 取得清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > LeaderBoard ExtraPoint > [add]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult SetLeaderboardExtraPoint(SetLeaderboardExtraPointReq request)
        {
            var apiResult = new ApiResult
            {
                Message = "Unknown Error"
            };
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _bonusManageBll.SetLeaderboardExtraPoint(request.ActiveDate, request.SiteId, request.CategoryId, request.UserName, request.ExtraPoint, userName);
            
            switch (result)
            {
                case 0:
                    apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                    apiResult.Message = "";
                    break;
                case -2:
                    apiResult.ErrorCode = result;
                    apiResult.Message = "User Not Found";
                    break;
                case -3:
                    apiResult.ErrorCode = result;
                    apiResult.Message = "Can only select the ActiveDate today or after today";
                    break;
                default:
                    apiResult.ErrorCode = result;
                    apiResult.Message = "Unknown Error";
                    break;
            }
            return apiResult;
        }
        
        #endregion
    }
}