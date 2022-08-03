using System;
using System.Collections.Generic;
using System.Linq;
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

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        #region Fields

        private readonly NewsBLL _newsBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly ILogger<BonusManageController> _logger;

        #endregion

        #region Constructors

        public NewsController(IServiceProvider provider)
        {
            _newsBll = provider.GetRequiredService<NewsBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _logger = provider.GetRequiredService<ILogger<BonusManageController>>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
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
            public IEnumerable<NewsStatusEnum> Status { get; set; }
        }

        public class GetByIdRequest
        {
            public int NewsId { get; set; }
        }

        public class SetRequest
        {
            private DateTimeOffset? _postTime;
            public bool IsAdd { get; set; }
            public int? SiteId { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string ImageUrl { get; set; }
            public int? Status { get; set; }
            public DateTimeOffset? PostTime
            {
                get => _postTime;
                set
                {
                    if (value != null)
                        _postTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public string LastUpdateUser { get; set; }
            public int? Sort { get; set; }
            public int? NewsId { get; set; }
        }

        public class SetStatusRequest
        {
            public int NewsId { get; set; }
            public NewsStatusEnum Status { get; set; }
        }
        #endregion

        /// <summary>
        /// News/GetSiteListByLoginUser 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  News Management 
        /// </remarks>
        /// <param name="req"></param>
        //[HttpPost]
        //public ApiResult<IEnumerable<dynamic>> GetSiteListByLoginUser(GetSiteListByLoginUserReq req)
        //{
        //    var apiResult = new ApiResult<IEnumerable<dynamic>>();
        //    try
        //    {
        //        apiResult.Data = _serverInfoBll.GetSiteListByLoginUser()?.Select(x =>
        //            new { x.SiteName, x.SiteId, CurrencyList = x.ICurrencyList, LangList = x.ILangList });
        //        if (req.SiteId != null && apiResult.Data != null && apiResult.Data.Any())
        //        {
        //            apiResult.Data = apiResult.Data.Where(s => s.SiteId == req.SiteId);
        //        }

        //        apiResult.ErrorCode = 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, MethodExtension.GetCurrentMethod());
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
        //        apiResult.Message = "something went wrong.";
        //        apiResult.RawMessage = ex.Message;
        //    }

        //    return apiResult;
        //}

        /// <summary>
        /// News/GetList 取得新聞清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  News Management  > [Search]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/NewsManage" })]
        [HttpPost]
        public ApiListResult<News> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<News>();

            var statusList = request.Status.Select(x => (int)x);

            var searchResult = _newsBll.GetNewsList(request.SiteIds,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.PageSize,
                request.PageNumber,
                statusList);
            apiResult.Data = searchResult.NewsList;
            apiResult.PageNumber = searchResult.PageNumber;
            apiResult.TotalPages = searchResult.TotalPages;
            apiResult.RowCount = searchResult.RowCount;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// News/GetById 取得指定新聞
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  News Management  > [Submit] > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/NewsManage" })]
        [HttpPost]
        public ApiResult<News> GetById(GetByIdRequest request)
        {
            var result = new ApiResult<News>();

            result.Data = _newsBll.GetNewsById(request.NewsId);
            result.ErrorCode = (int)ErrorCodeEnum.Success;

            return result;
        }

        /// <summary>
        /// News/Set 新增或更新指定新聞
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  News Management  > [Edit] > [submit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/NewsManage" })]
        [HttpPost]
        public ApiResult Set(SetRequest request)
        {
            var result = new ApiResult();

            if (!request.IsAdd)
            {
                request.LastUpdateUser = _authorizeBll.GetAuthData().UserName;
            }

            var setReq = new News()
            {
                NewsId = request.NewsId,
                SiteId = request.SiteId,
                Title = request.Title,
                Content = request.Content,
                ImageURL = request.ImageUrl,
                Status = request.Status,
                PostTime = request.PostTime,
                LastUpdateUser = request.LastUpdateUser,
                Sort = request.Sort
            };

            var newsId = _newsBll.SetNews(request.IsAdd, setReq);

            if (request.IsAdd)
            {
                result.ErrorCode = newsId != null && newsId != 0 ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.Fail;
            }
            else
            {
                result.ErrorCode = (int)ErrorCodeEnum.Success;
            }

            return result;
        }

        /// <summary>
        /// News/SetStatus 更新新聞狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > News Management > [Submit] > [Status] 
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/NewsManage" })]
        [HttpPost]
        public ApiResult<dynamic> SetStatus(SetStatusRequest request)
        {
            var result = new ApiResult<dynamic>();

            _newsBll.SetStatus(request.NewsId, (int)request.Status, _authorizeBll.GetAuthData().UserName);
            result.ErrorCode = (int)ErrorCodeEnum.Success;

            return result;
        }
    }
}