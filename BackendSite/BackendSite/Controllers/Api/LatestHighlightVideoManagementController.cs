using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/LatestHighlightVideoManagement" })]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LatestHighlightVideoManagementController : Controller
    {
        private readonly VideoManagementBLL highlightVideoManagementBLL;

        public LatestHighlightVideoManagementController(VideoManagementBLL highlightVideoManagementBLL)
        {
            this.highlightVideoManagementBLL = highlightVideoManagementBLL;
        }

        [HttpPost]
        public ApiListResult<VideoData> GetLatestHighlightVideoData(GetVideoDataPara para)
        {
            var apiResult = new ApiListResult<VideoData>();
            try
            {
                var retult = highlightVideoManagementBLL.GetVideoData(para);
                apiResult.Data = retult.VideoDatas;
                apiResult.PageNumber = retult.PageNumber;
                apiResult.TotalPages = retult.TotalPages;
                apiResult.RowCount = retult.RowCount;
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
        public ApiResult SetLatestHighlightVideo(SetVideoPara para)
        {
            var apiResult = new ApiResult();
            try
            {
                // 給 null 未來移除舊API
                highlightVideoManagementBLL.SetVideoInfo(null);
                apiResult.ErrorCode = 0;
                highlightVideoManagementBLL.AddLog("Marketing/LatestHighlightVideoManagement", ActionType.Update, JsonConvert.SerializeObject(para), User.Identity.Name);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<GetSiteLangData>> GetSiteLangData(GetSiteLangPara para)
        {
            var apiResult = new ApiResult<IEnumerable<GetSiteLangData>>();
            try
            {
                var data = highlightVideoManagementBLL.GetSiteLangData(para.SiteId);
                apiResult.Data = data;
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
        public ApiResult InsertLatestHighlightVideo(InsertVideoPara para)
        {
            var apiResult = new ApiResult();
            try
            {              
                highlightVideoManagementBLL.InsertLatestHighlightVideo(para, User.Identity.Name);
                apiResult.ErrorCode = 0;
                highlightVideoManagementBLL.AddLog("Marketing/LatestHighlightVideoManagement", ActionType.Create, JsonConvert.SerializeObject(para), User.Identity.Name);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
    }
}
