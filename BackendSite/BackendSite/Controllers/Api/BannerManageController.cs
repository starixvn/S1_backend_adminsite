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
using BackendSite.Controllers.ApiV2.CMS.Setting;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/BannerManage" })]
    [ApiExplorerSettings(IgnoreApi = true)]
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
        public ApiResult<IEnumerable<BannerInfoList>> GetBannerList([FromForm] int SiteId, [FromForm] int BannerTypeId, [FromForm] int Status)
        {
            var apiResult = new ApiResult<IEnumerable<BannerInfoList>>();
            try
            {
                var siteList = new List<int>(); siteList.Add(SiteId);
                var bannerTypeList = new List<int>(); siteList.Add(SiteId);
                var statusList = new List<int>(); siteList.Add(SiteId);

                if (BannerTypeId > 0)
                {
                    bannerTypeList.Add(BannerTypeId);
                }
                else
                {
                    bannerTypeList.Add(1);
                    bannerTypeList.Add(2);
                    bannerTypeList.Add(3);
                    bannerTypeList.Add(4);
                    bannerTypeList.Add(5);
                    bannerTypeList.Add(6);
                }

                if (Status == 1)
                {
                    statusList.Add(1);
                }
                else if (Status == 2)
                {
                    statusList.Add(0);
                }
                else
                {
                    statusList.Add(0); statusList.Add(1);
                }

                apiResult.Data = bannerManageBLL.GetBannerList(siteList,
                    bannerTypeList, statusList,
                    1, 5000, out _, out _);
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