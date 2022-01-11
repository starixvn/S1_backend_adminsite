using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Controllers.Api;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    public class NewsManagementController : ControllerBase
    {
        #region Fields
        
        private readonly NewsBLL _newsBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly ILogger<BonusManageController> _logger;

        #endregion

        #region Constructors

        public NewsManagementController(IServiceProvider provider)
        {
            _newsBll = provider.GetRequiredService<NewsBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _logger = provider.GetRequiredService<ILogger<BonusManageController>>();
        }

        #endregion

        #region API Model

        public class GetSiteListByLoginUserReq
        {
            public string SiteId { get; set; }
        } 
        
        public class GetNewsReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SelectSite { get; set; }
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
            public int PageSize { get; set; }
            public int PageNumber { get; set; }
            public int Status { get; set; }
        }

        public class GetNewsByIdReq
        {
            public int NewsId { get; set; }
        }
        
        public class SetNewsReq
        {
            public bool IsAdd { get; set; }
            public int? SiteId { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string ImageUrl { get; set; }
            public int? Status { get; set; }
            public long? UnixPostTime { get; set; }
            public string LastUpdateUser { get; set; }
            public int? Sort { get; set; }
            public int? NewsId { get; set; }
        }

        public class SetStatusReq
        {
            public int NewsId { get; set; }
            public int Status { get; set; }
        }
        #endregion
        
        /// <summary>
        /// NewsManagement/GetSiteListByLoginUser 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  News Management 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<IEnumerable<dynamic>> GetSiteListByLoginUser(GetSiteListByLoginUserReq req)
        {
            var apiResult = new ApiResult<IEnumerable<dynamic>>();
            try
            {
                apiResult.Data = _serverInfoBll.GetSiteListByLoginUser()?.Select(x =>
                    new { x.SiteName, x.SiteId, CurrencyList = x.ICurrencyList, LangList = x.ILangList });
                if (req.SiteId != null && apiResult.Data != null && apiResult.Data.Any())
                {
                    apiResult.Data = apiResult.Data.Where(s => s.SiteId == req.SiteId);
                }

                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }
        
        /// <summary>
        /// NewsManagement/GetNewsList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  News Management  > [Search]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiListResult<News> GetNewsList(GetNewsReq req)
        {
            var apiResult = new ApiListResult<News>();
            try
            {
                var searchResult = _newsBll.GetNewsList(req.SelectSite, req.StartDate.DateTime, req.EndDate.DateTime, req.PageSize, req.PageNumber, req.Status);
                apiResult.Data = searchResult.NewsList;
                apiResult.PageNumber = searchResult.PageNumber;
                apiResult.TotalPages = searchResult.TotalPages;
                apiResult.RowCount = searchResult.RowCount;
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        /// <summary>
        /// NewsManagement/GetNewsById 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  News Management  > [Edit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<News> GetNewsById(GetNewsByIdReq req)
        {
            var result = new ApiResult<News>();

            try
            {
                result.Data = _newsBll.GetNewsById(req.NewsId);
                result.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                result.ErrorCode = (int) ErrorCodeEnum.SystemException;
                result.RawMessage = ex.Message;
            }

            return result;
        }
        
        /// <summary>
        /// NewsManagement/SetNews 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  News Management  > [Edit] > [submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult SetNews(SetNewsReq req)
        {
            var result = new ApiResult();

            try
            {
                if (!req.IsAdd)
                {
                    req.LastUpdateUser = _authorizeBll.GetAuthData().UserName;
                }

                var setReq = new News()
                {
                    NewsId = req.NewsId,
                    SiteId = req.SiteId,
                    Title = req.Title,
                    Content = req.Content,
                    ImageURL = req.ImageUrl,
                    Status = req.Status,
                    UnixPostTime = req.UnixPostTime,
                    LastUpdateUser = req.LastUpdateUser,
                    Sort= req.Sort
                };
                
                var newsId = _newsBll.SetNews(req.IsAdd, setReq);
                if (req.IsAdd)
                {
                    result.ErrorCode = newsId != null && newsId != 0 ? (int) ErrorCodeEnum.Success : (int) ErrorCodeEnum.Fail;
                }
                else
                {
                    result.ErrorCode = (int) ErrorCodeEnum.Success;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                result.ErrorCode = (int) ErrorCodeEnum.SystemException;
                result.RawMessage = ex.Message;
            }

            return result;
        }
        
        /// <summary>
        /// NewsManagement/SetStatus 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  News Management  > [Status] 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<dynamic> SetStatus(SetStatusReq req)
        {
            var result = new ApiResult<dynamic>();
            try
            {
                _newsBll.SetStatus(req.NewsId, req.Status, _authorizeBll.GetAuthData().UserName);
                result.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                result.ErrorCode = (int) ErrorCodeEnum.SystemException;
                result.RawMessage = ex.Message;
            }

            return result;
        }
    }
}