using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class LatestHighlightVideoController : ControllerBase
    {
        #region Fields

        private readonly GameCasinoManageBLL _gameCasinoManageBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly LatestHighlightVideoManagementBLL _highlightVideoManagementBll;

        #endregion

        #region Constructor

        public LatestHighlightVideoController(IServiceProvider provider)
        {
            _gameCasinoManageBll = provider.GetRequiredService<GameCasinoManageBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _highlightVideoManagementBll = provider.GetRequiredService<LatestHighlightVideoManagementBLL>();
        }

        #endregion


        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }
            /// <summary>
            /// 日期-起
            /// </summary>
            /// <remarks>
            /// 單位為年月日，不會到小時，故不會有時區概念。
            /// 資料的欄位紀錄目前都是 +0 的日期(年月日)
            /// </remarks>
            public DateTimeOffset StartDate { get; set; }
            /// <summary>
            /// 日期-迄
            /// </summary>
            /// <remarks>
            /// 單位為年月日，不會到小時，故不會有時區概念。
            /// 資料的欄位紀錄目前都是 +0 的日期(年月日)
            /// </remarks>
            public DateTimeOffset EndDate { get; set; }
            public int Lang { get; set; }
        }

        public class SetRequest
        {
            /// <summary>
            /// 日期
            /// </summary>
            /// <remarks>
            /// 單位為年月日，不會到小時，故不會有時區概念。
            /// 資料的欄位紀錄目前都是 +0 的日期(年月日)
            /// </remarks>
            public DateTimeOffset Date { get; set; }
            public string Title { get; set; }
            public string Link { get; set; }
            public int VideoId { get; set; }
        }

        public class CreateRequest
        {
            public int SiteId { get; set; }
            public DateTimeOffset Date { get; set; }
            public IEnumerable<VideoDetail> Detail { get; set; }
        }

        public class VideoDetail
        {
            public string LangId { get; set; }
            public string Title { get; set; }
            public string Link { get; set; }
        }

        #endregion

        #region Action

        /// <summary>
        /// LatestHighlightVideo/GetList 取得最新精采影片清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/LatestHighlightVideoManagement" })]
        [HttpPost]
        public ApiListResult<LatestHighlightVideoData> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<LatestHighlightVideoData>();

            var param = new GetLatestHighlightVideoDataPara
            {
                SelectSite = request.SiteIds,
                StartDate = request.StartDate.DateTime.ToString(),
                EndDate = request.EndDate.DateTime.ToString(),
                Lang = request.Lang,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var result = _highlightVideoManagementBll.GetLatestHighlightVideoData(param);
            apiResult.Data = result.LatestHighlightVideoDatas;
            apiResult.PageNumber = result.PageNumber;
            apiResult.TotalPages = result.TotalPages;
            apiResult.RowCount = result.RowCount;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// LatestHighlightVideo/Set 更新最新精采影片資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/LatestHighlightVideoManagement" })]
        [HttpPost]
        public ApiResult Update(SetRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var param = new SetLatestHighlightVideoPara()
                {
                    VideoId = request.VideoId,
                    Date = request.Date.DateTime.ToString(),
                    Title = request.Title,
                    Link = request.Link,
                };

                _highlightVideoManagementBll.SetLatestHighlightVideo(param, User.Identity.Name);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                _highlightVideoManagementBll.AddLog("Marketing/LatestHighlightVideoManagement", ActionType.Update, JsonConvert.SerializeObject(request), User.Identity.Name);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }

        /// <summary>
        /// LatestHighlightVideo/Create 新增最新精采影片清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/LatestHighlightVideoManagement" })]
        [HttpPost]
        public ApiResult Create(CreateRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var param = new InsrtLatestHighlightVideoPara()
                {
                    SiteId = request.SiteId,
                    Date = request.Date.DateTime.ToString(),
                    Detail = JsonConvert.SerializeObject(request.Detail)
                };

                _highlightVideoManagementBll.InsertLatestHighlightVideo(param, User.Identity.Name);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

                _highlightVideoManagementBll.AddLog("Marketing/LatestHighlightVideoManagement", ActionType.Create, JsonConvert.SerializeObject(request), User.Identity.Name);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }

        #endregion

    }
}
