using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
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

        public class GetListRequest : ApiRequestPageModel
        {
            //TODO：應該要有時區可以轉換，但現在架構傳時區轉換沒有意義
            //private DateTimeOffset _startDate { get; set; }
            //private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CategoryIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }

            //TODO：應該要有時區可以轉換，但現在架構傳時區轉換沒有意義
            //public DateTimeOffset StartDate
            //{
            //    get => _startDate;
            //    set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            //}
            //public DateTimeOffset EndDate
            //{
            //    get => _endDate;
            //    set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            //}

            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }

        }

        public class SetExtraPointRequest
        {
            //TODO：應該要有時區可以轉換，但現在架構傳時區轉換沒有意義
            //private DateTimeOffset _activeDate { get; set; }
            public int SiteId { get; set; }
            public int CategoryId { get; set; }
            public string UserName { get; set; }
            public decimal ExtraPoint { get; set; }

            //TODO：應該要有時區可以轉換，但現在架構傳時區轉換沒有意義
            //public DateTimeOffset ActiveDate
            //{
            //    get => _activeDate;
            //    set => _activeDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            //}
            public DateTimeOffset ActiveDate { get; set; }
        }

        #endregion

        #region API 

        /// <summary>
        /// LeaderBoardAdjustment/GetList 取得排行榜清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        ///Func Path：Marketing > LeaderBoard ExtraPoint > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/LeaderBoardExtraPoint" })]
        [HttpPost]
        public ApiListResult<LeaderboardExtraPointList> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<LeaderboardExtraPointList>();

            var pageNumber = request.PageNumber;

            apiResult.Data = _bonusManageBll.GetLeaderboardExtraPointList(request.SiteIds,
                request.CategoryIds, request.CurrencyIds, request.StartDate.DateTime,
                request.EndDate.DateTime,request.PageNumber, request.PageSize, out var totalPages, out var rowCount);

            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// LeaderBoardAdjustment/SetExtraPoint 更新額外點數
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > LeaderBoard ExtraPoint > [Add ExtraPoint] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/LeaderBoardExtraPoint" })]
        [HttpPost]
        public ApiResult SetExtraPoint(SetExtraPointRequest request)
        {
            var apiResult = new ApiResult
            {
                Message = "Unknown Error"
            };
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _bonusManageBll.SetLeaderboardExtraPoint(request.ActiveDate.ToUnixTimeSeconds(), request.SiteId, request.CategoryId, request.UserName, request.ExtraPoint, userName);

            switch (result)
            {
                case 0:
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    apiResult.Message = "";
                    break;
                case -2:
                    throw new AdminException(ErrorCodeEnum.CustNotFound);
                    break;
                case -3:
                    apiResult.ErrorCode = result;
                    apiResult.Message = "Can only select the ActiveDate today or after today";
                    break;
                default:
                    throw new AdminException(ErrorCodeEnum.SystemException);
                    break;
            }
            return apiResult;
        }

        #endregion
    }
}