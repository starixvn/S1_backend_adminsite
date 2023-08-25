using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        #region Fields

        private readonly VideoManagementBLL _videoManagementBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructor

        public VideoController(IServiceProvider provider)
        {
            _videoManagementBll = provider.GetRequiredService<VideoManagementBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public IEnumerable<int> LangIds { get; set; }
        }

        public class SetRequest
        {
            public int LangId { get; set; }
            public string Title { get; set; }
            public string Link { get; set; }
        }

        #endregion

        #region Action

        /// <summary>
        /// Video/GetList 取得最新精采影片清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 68, 1 })]
        public ApiListResult<VideoData> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<VideoData>();

            var langList = request.LangIds.Select(landId => ((LangType)landId).ToString()).ToList();

            var param = new GetVideoDataPara
            {
                SiteId = request.SiteId,
                CurrencyIds = request.CurrencyIds,
                LangList = langList,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var result = _videoManagementBll.GetVideoData(param);
            apiResult.Data = result.VideoDatas;
            apiResult.PageNumber = request.PageNumber;
            apiResult.TotalPages = result.TotalPages;
            apiResult.RowCount = result.RowCount;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// Video/Set 更新最新精采影片資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > GameCasinoManagement
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 68, 3 })]
        public ApiResult Update(ApiRequestWithLog<SetRequest> request)
        {
            var apiResult = new ApiResult();
            try
            {
                var param = new ApiRequestWithLog<SetVideoPara>
                {
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    Before = new SetVideoPara
                    {
                        LangId = ((LangType)request.Before.LangId).ToString(),
                        Title = request.Before.Title,
                        Link = request.Before.Link
                    },
                    After = new SetVideoPara
                    {
                        LangId = ((LangType)request.After.LangId).ToString(),
                        Title = request.After.Title,
                        Link = request.After.Link
                    }
                };
                var result = _videoManagementBll.SetVideoInfo(new ParamsWithLog<SetVideoPara>(param, User.Identity.Name));

                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                    apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
                }
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
