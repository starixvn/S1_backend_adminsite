using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    public class SiteIdModel
    {
        public int SiteId { get; set; }
    }

    public class PointLevelModel
    {
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }
    }

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MarketingController : ControllerBase
    {
        private readonly SelLangBLL selLangBLL;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly MarketingBLL marketingBLL;

        public MarketingController(SelLangBLL selLangBLL, ServerInfoBLL serverInfoBLL, MarketingBLL marketingBLL)
        {
            this.selLangBLL = selLangBLL;
            this.serverInfoBLL = serverInfoBLL;
            this.marketingBLL = marketingBLL;
        }

        [HttpGet]
        public ApiResult<IEnumerable<ServerInfo>> GetAllSites()
        {
            var apiResult = new ApiResult<IEnumerable<ServerInfo>>();
            var serverInfos = serverInfoBLL.GetSiteList();
            apiResult.Data = serverInfos;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<int>> GetSiteCurrencyIds(SiteIdModel siteIdModel)
        {
            var apiResult = new ApiResult<IEnumerable<int>>();
            var serverInfo = serverInfoBLL.GetServerInfoWithSiteId(siteIdModel.SiteId);
            apiResult.Data = serverInfo.ICurrencyList;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        public ApiResult<IEnumerable<PointLevelInfo>> GetPointLevelInfos(PointLevelModel pointLevelModel)
        {
            var apiResult = new ApiResult<IEnumerable<PointLevelInfo>>();
            var pointLevelInfos = marketingBLL.GetPointLevelInfos(pointLevelModel.SiteId, pointLevelModel.CurrencyId);
            apiResult.Data = pointLevelInfos;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        public ApiResult AddPointLevelInfo(PointLevelInfo pointLevelInfo)
        {
            var result = marketingBLL.AddPointLevel(pointLevelInfo);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        public ApiResult EditPointLevelInfo(PointLevelInfo pointLevelInfo)
        {
            var result = marketingBLL.EditPointLevel(pointLevelInfo);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        [HttpPost]
        public ApiResult DeletePointLevelInfo(PointLevelInfo pointLevelInfo)
        {
            var result = marketingBLL.DeletePointLevel(pointLevelInfo);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
    }
}