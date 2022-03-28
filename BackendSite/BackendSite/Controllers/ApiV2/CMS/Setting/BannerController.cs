using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BannerController : ControllerBase
    {
        #region Fields

        private readonly BannerManageBLL _bannerManageBll;
        private readonly AuthorizeBLL _authorizeBll;

        #endregion

        #region Constructor

        public BannerController(IServiceProvider provider)
        {
            _bannerManageBll = provider.GetRequiredService<BannerManageBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
        }

        #endregion

        #region API Model

        public class SetRequest
        {
            private DateTimeOffset? _startTime;
            private DateTimeOffset? _endTime;
            private DateTimeOffset? _sportBattleDateTime;

            public bool isAdd { get; set; }
            public int SiteId { get; set; }
            public string BannerName { get; set; }
            public BannerTypeEnum BannerType { get; set; }
            public BannerActionModeEnum ActionMode { get; set; }
            public ShowStatusEnum ShowStatus { get; set; }
            public DateTimeOffset? StartTime
            {
                get => _startTime;
                set
                {
                    if (value != null)
                        _startTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset? EndTime
            {
                get => _endTime;
                set
                {
                    if (value != null)
                        _endTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public string BonusCode { get; set; }
            public int? MatchId { get; set; }
            public int? TpId { get; set; }
            public string GameId { get; set; }
            public string UrlRouter { get; set; }

            public DateTimeOffset? SportBattleDateTime
            {
                get => _sportBattleDateTime;
                set
                {
                    if (value != null)
                        _sportBattleDateTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public int? SportCategoryId { get; set; }
            public int? SportLeaguesId { get; set; }
            public IEnumerable<BannerSportTeamsDetails> SportTeamsId { get; set; }
            public IEnumerable<BannerDetails> Specific { get; set; }
            public int Sort { get; set; }
            public string SiteName { get; set; }

        }

        public class GetBannerRequest
        {
            public int SiteId { get; set; }
            public string BannerName { get; set; }
            public int CurrencyId { get; set; }
        }

        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<BannerTypeEnum> BannerTypes { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public IEnumerable<BannerStatusEnum> Statuses { get; set; }
        }

        public class UpdateStatusRequest
        {
            public int SiteId { get; set; }
            public string BannerName { get; set; }
            public BannerStatusEnum Status { get; set; }
        }
        public class GetHotEventListReq : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<BannerStatusEnum> Statuses { get; set; }
        }
        public class GetHotEventreq
        {
            public int SiteId { get; set; }
            public string BannerName { get; set; }
        }

        #endregion

        #region API

        /// <summary>
        /// Banner/GetBanner 取得指定橫幅
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management > Select [Available Sites] > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/BannerManage" })]
        [HttpPost]
        public ApiResult<BannerInfo> GetBanner(GetBannerRequest request)
        {
            var apiResult = new ApiResult<BannerInfo>();
            try
            {
                var result = _bannerManageBll.GetBanner(request.SiteId, request.BannerName , request.CurrencyId);
                if (result != null)
                {
                    apiResult.Data = result;
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "The Banner Detail No Data";
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }
            return apiResult;
        }

        /// <summary>
        /// Banner/GetList 取得橫幅清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management > Select [Available Sites]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiListResult<BannerInfoList> GetList(GetListRequest request)
        {
            var apiListResult = new ApiListResult<BannerInfoList>();
            try
            {
                var bannerTypeList = request.BannerTypes.Select(x => (int)x);
                var statusList = request.Statuses.Select(x => (int)x);
                var currencyIds = request.CurrencyIds.Select(x => x);

                var data = _bannerManageBll.GetBannerList(request.SiteIds,
                    bannerTypeList,
                    statusList,
                    currencyIds,
                    request.PageNumber,
                    request.PageSize,
                    out var totalPages, out var rowCount);

                apiListResult.Data = data;
                apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiListResult.TotalPages = totalPages;
                apiListResult.RowCount = rowCount;
                apiListResult.PageNumber = request.PageNumber;
            }
            catch (Exception ex)
            {
                apiListResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiListResult.Message = ex.Message;
                apiListResult.RawMessage = ex.ToString();
            }
            return apiListResult;
        }

        /// <summary>
        /// Banner/SetBanner 新增/更新橫幅
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management  > [Edit] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult SetBanner(SetRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var data = new BannerInfo
                {
                    SiteId = request.SiteId,
                    BannerName = request.BannerName,
                    BannerType = (int)request.BannerType,
                    ActionMode = (int)request.ActionMode,
                    ShowStatus = (int)request.ShowStatus,
                    UnixStartTime = request.StartTime?.ToUnixTimeSeconds(),
                    UnixEndTime = request.EndTime?.ToUnixTimeSeconds(),
                    BonusCode = request.BonusCode,
                    MatchId = request.MatchId,
                    TpId = request.TpId,
                    GameId = request.GameId,
                    UrlRouter = request.UrlRouter,
                    UnixSportBattleDateTime = request.SportBattleDateTime?.ToUnixTimeSeconds(),
                    SportCategoryId = request.SportCategoryId,
                    SportLeaguesId = request.SportLeaguesId,
                    SportTeamsId = request.SportTeamsId,
                    Specific = request.Specific,
                    Sort = request.Sort,
                    SiteName = request.SiteName
                };

                var user = _authorizeBll.GetAuthData().UserName;
                var errorCode = _bannerManageBll.SetBanner(data, user);

                if (request.isAdd)
                {
                    apiResult.ErrorCode = errorCode == 1 ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.Fail;
                }
                else
                {
                    apiResult.ErrorCode = errorCode == 2 ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.Fail;
                    if (errorCode == 1)
                    {
                        apiResult.Message = "The Banner Name setting already exists which with same Site.";
                    }
                }
            }
            catch (MySqlException sqlException)
            {
                apiResult.ErrorCode = sqlException.Number;
                apiResult.Message = sqlException.Message;
                apiResult.RawMessage = sqlException.ToString();
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        /// <summary>
        /// Banner/UpdateStatus 更新橫幅狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management > Select [Available Sites] > [Status] 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult UpdateStatus(UpdateStatusRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var user = _authorizeBll.GetAuthData().UserName;
                apiResult.ErrorCode = _bannerManageBll.UpdateBannerStatus(request.SiteId, request.BannerName, (int)request.Status, user);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        /// <summary>
        /// Banner/GetHotEventList
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Hot Event Management
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/HotEventManagement" })]
        [HttpPost]
        public ApiListResult<HotEventList> GetHotEventList(GetHotEventListReq req)
        {
            var apiResult = new ApiListResult<HotEventList>();
            try
            {
                var statusList = req.Statuses.Select(x => (int)x);


                apiResult.Data = _bannerManageBll.GetHotEventList(req.SiteIds,
                    statusList,
                    req.PageNumber, req.PageSize, out var totalPages, out var rowCount);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.TotalPages = totalPages;
                apiResult.RowCount = rowCount;
                apiResult.PageNumber = req.PageNumber;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }
            return apiResult;
        }

        /// <summary>
        /// Banner/GetHotEvent 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Hot Event Management > [Edit]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/HotEventManagement" })]
        [HttpPost]
        public ApiResult<BannerInfo> GetHotEvent(GetHotEventreq req)
        {
            var apiResult = new ApiResult<BannerInfo>();
            try
            {
                var result = _bannerManageBll.GetBanner(req.SiteId, req.BannerName, 0);
                if (result != null)
                {
                    apiResult.Data = result;
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.NoData;
                    apiResult.Message = "The Hot Event Detail No Data";
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }
            return apiResult;
        }
        #endregion
    }
}
