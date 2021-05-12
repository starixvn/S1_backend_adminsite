using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BannerManage" })]
    public class BannerManageController : Controller
    {
        private readonly CustInfoBLL custInfoBLL;
        private readonly BannerManageBLL bannerManageBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly CustInfoService custInfoService;
        private readonly Common common;

        public BannerManageController(CustInfoBLL custInfoBLL, BannerManageBLL bannerManageBLL, AuthorizeBLL authorizeBLL, CustInfoService custInfoService, Common common)
        {
            this.custInfoBLL = custInfoBLL;
            this.bannerManageBLL = bannerManageBLL;
            this.authorizeBLL = authorizeBLL;
            this.custInfoService = custInfoService;
            this.common = common;
        }

        [HttpPost]
        public ApiResult SetBanner([FromForm] BannerInfo bannerInfo)
        {
            var apiResult = new ApiResult();
            try
            {
                var user = authorizeBLL.GetAuthData().UserName;
                var errorCode = bannerManageBLL.SetBanner(bannerInfo, user);
                apiResult.ErrorCode = errorCode;
                apiResult.Message = errorCode == 1 ? "The Banner Name setting already exists which with same Site." : "";
            }
            catch (MySqlException sqlException)
            {
                apiResult.ErrorCode = sqlException.Number;
                apiResult.Message = sqlException.Message;
                apiResult.RawMessage = sqlException.ToString();
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }
        [HttpPost]
        public ApiResult<IEnumerable<BannerInfoList>> GetBannerList([FromForm] int SiteId, [FromForm] int BannerTypeId)
        {
            var apiResult = new ApiResult<IEnumerable<BannerInfoList>>();
            try
            {
                apiResult.Data = bannerManageBLL.GetBannerList(SiteId, BannerTypeId);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }
            return apiResult;
        }
        [HttpPost]
        public ApiResult<BannerInfo> GetBanner([FromForm] int SiteId, [FromForm] string BannerName)
        {
            var apiResult = new ApiResult<BannerInfo>();
            try
            {
                var result = bannerManageBLL.GetBanner(SiteId, BannerName);
                if (result != null)
                {
                    apiResult.Data = result;
                    apiResult.ErrorCode = 0;
                }
                else
                {
                    apiResult.ErrorCode = -1;
                    apiResult.Message = "The Banner Detail No Data";
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }
            return apiResult;
        }
        [HttpPost]
        public ApiResult UpdateBannerStatus([FromForm] int SiteId, [FromForm] string BannerName, [FromForm] int Status)
        {
            var apiResult = new ApiResult();
            try
            {
                var User = authorizeBLL.GetAuthData().UserName;
                apiResult.ErrorCode = bannerManageBLL.UpdateBannerStatus(SiteId, BannerName, Status, User);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }


    }
}