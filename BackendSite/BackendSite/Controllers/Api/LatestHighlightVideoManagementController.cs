using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/LatestHighlightVideoManagement" })]
    public class LatestHighlightVideoManagementController : Controller
    {
        private readonly LatestHighlightVideoManagementBLL highlightVideoManagementBLL;

        public LatestHighlightVideoManagementController(LatestHighlightVideoManagementBLL highlightVideoManagementBLL)
        {
            this.highlightVideoManagementBLL = highlightVideoManagementBLL;
        }

        [HttpPost]
        public ApiListResult<LatestHighlightVideoData> GetLatestHighlightVideoData(GetLatestHighlightVideoDataPara para)
        {
            var apiResult = new ApiListResult<LatestHighlightVideoData>();
            try
            {
                var retult = highlightVideoManagementBLL.GetLatestHighlightVideoData(para);
                apiResult.Data = retult.LatestHighlightVideoDatas;
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
        public ApiResult SetLatestHighlightVideo(SetLatestHighlightVideoPara para)
        {
            var apiResult = new ApiResult();
            try
            {
                highlightVideoManagementBLL.SetLatestHighlightVideo(para, User.Identity.Name);
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
        public ApiResult InsertLatestHighlightVideo(InsrtLatestHighlightVideoPara para)
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
