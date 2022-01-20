using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/NewsManage" })]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class NewsController : Controller
    {
        private readonly NewsBLL newsBLL;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly AuthorizeBLL authorizeBLL;


        public NewsController(NewsBLL newsBLL, ServerInfoBLL serverInfoBLL, AuthorizeBLL authorizeBLL)
        {
            this.newsBLL = newsBLL;
            this.serverInfoBLL = serverInfoBLL;
            this.authorizeBLL = authorizeBLL;
        }

        [HttpPost]
        public ApiResult<IEnumerable<dynamic>> GetSiteListByLoginUser()
        {
            var apiResult = new ApiResult<IEnumerable<dynamic>>();
            try
            {
                apiResult.Data = serverInfoBLL.GetSiteListByLoginUser()?.Select(x => new { x.SiteName, x.SiteId, LangList = x.ILangList });
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        public class GetNewsPara
        {
            public IEnumerable<int> SelectSite { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public int PageSize { get; set; }
            public int PageNumber { get; set; }
            public int Status { get; set; }
        }

        [HttpPost]
        public ApiListResult<News> GetNewsList(GetNewsPara para)
        {
            var apiResult = new ApiListResult<News>();
            try
            {
                var startTime = DateTimeExtensions.UnixToDateTime(para.StartDate);
                var endTime = DateTimeExtensions.UnixToDateTime(para.EndDate);

                var statusList = new List<int>();

                if (para.Status == 0)
                {
                    statusList.Add(0); statusList.Add(1);
                }
                else
                {
                    statusList.Add(para.Status);
                }

                var searchResult = newsBLL.GetNewsList(para.SelectSite, startTime, endTime, para.PageSize, para.PageNumber, statusList);

                apiResult.Data = searchResult.NewsList;
                apiResult.PageNumber = searchResult.PageNumber;
                apiResult.TotalPages = searchResult.TotalPages;
                apiResult.RowCount = searchResult.RowCount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<News> GetNewsById([FromForm] int NewsId)
        {
            var result = new ApiResult<News>();

            try
            {
                var serverInfo = serverInfoBLL.GetServerInfo();
                result.Data = newsBLL.GetNewsById(NewsId);
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.Message;
            }

            return result;
        }

        [HttpPost]
        public ApiResult SetNews([FromForm] bool IsAdd, [FromForm] News newsDatas)
        {
            var result = new ApiResult();

            try
            {
                if (!IsAdd)
                {
                    newsDatas.LastUpdateUser = authorizeBLL.GetAuthData().UserName;
                }

                var newsId = newsBLL.SetNews(IsAdd, newsDatas);
                if (IsAdd)
                {
                    result.ErrorCode = newsId != null && newsId != 0 ? 0 : -1;
                }
                else
                {
                    result.ErrorCode = 0;
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.Message;
            }

            return result;
        }

        [HttpPost]
        public ApiResult<dynamic> SetStatus([FromForm] int NewsId, [FromForm] int Status)
        {
            var result = new ApiResult<dynamic>();
            try
            {
                newsBLL.SetStatus(NewsId, Status, authorizeBLL.GetAuthData().UserName);
                var data = newsBLL.GetNewsById(NewsId);
                result.Data = new { data.LastUpdateUser, data.LastUpdateTime };
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.Message;
            }

            return result;
        }
    }
}
