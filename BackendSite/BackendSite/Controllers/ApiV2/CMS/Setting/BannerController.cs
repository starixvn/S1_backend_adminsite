using System;
using System.Collections.Generic;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BannerController : Controller
    {
        #region Fields
        
        private readonly BannerManageBLL _bannerManageBll;
        private readonly AuthorizeBLL _authorizeBll;

        #endregion

        #region Constructor

        public BannerController(IServiceProvider provider)
        {
            _bannerManageBll = provider.GetRequiredService<BannerManageBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
        }

        #endregion

        #region API Model

        public class SetBannerReq
        {
            public int SiteId { get; set; }
            public string BannerName { get; set; }
            public int BannerType { get; set; }
            public int ActionMode { get; set; }
            public int ShowStatus { get; set; }
            public long? UnixStartTime { get; set; }
            public long? UnixEndTime { get; set; }
            public string BonusCode { get; set; }
            public int? MatchId { get; set; }
            public int? TpId { get; set; }
            public string GameId { get; set; }
            public string UrlRouter { get; set; }
            public long? UnixSportBattleDateTime { get; set; }
            public int? SportCategoryId { get; set; }
            public int? SportLeaguesId { get; set; }
            public IEnumerable<BannerSportTeamsDetails> SportTeamsId { get; set; }
            public IEnumerable<BannerDetails> Specific { get; set; }
            public int Sort { get; set; }
            public string SiteName { get; set; }

        }

        public class GetBannerReq
        {
            public int SiteId { get; set; }
            public string BannerName { get; set; }
        }

        public class GetBannerListReq
        {
            public int SiteId { get; set; }
            public int BannerTypeId { get; set; }
            public int Status { get; set; }
        }

        public class UpdateBannerStatusReq
        {
            public int SiteId { get; set; }
            public string BannerName { get; set; }
            public int Status { get; set; }
        }
        
        #endregion

        #region API

        /// <summary>
        /// Banner/GetBanner 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management > [Edit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<BannerInfo> GetBanner(GetBannerReq req)
        {
            var apiResult = new ApiResult<BannerInfo>();
            try
            {
                var result = _bannerManageBll.GetBanner(req.SiteId, req.BannerName);
                if (result != null)
                {
                    apiResult.Data = result;
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success ;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "The Banner Detail No Data";
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }
            return apiResult;
        }
        
        /// <summary>
        /// Banner/GetBannerList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<IEnumerable<BannerInfoList>> GetBannerList(GetBannerListReq req)
        {
            var apiResult = new ApiResult<IEnumerable<BannerInfoList>>();
            try
            {
                apiResult.Data = _bannerManageBll.GetBannerList(req.SiteId, req.BannerTypeId, req.Status);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success ;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }
            return apiResult;
        }
        
        /// <summary>
        /// Banner/SetBanner 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management  > [Edit] > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult SetBanner(SetBannerReq req)
        {
            var apiResult = new ApiResult();
            try
            {
                var user = _authorizeBll.GetAuthData().UserName;
                var errorCode = _bannerManageBll.SetBanner(req, user);
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
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        /// <summary>
        /// Banner/UpdateBannerStatus 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management  > [Status] 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult UpdateBannerStatus(UpdateBannerStatusReq req)
        {
            var apiResult = new ApiResult();
            try
            {
                var user = _authorizeBll.GetAuthData().UserName;
                apiResult.ErrorCode = _bannerManageBll.UpdateBannerStatus(req.SiteId, req.BannerName, req.Status, user);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }
       
        
        #endregion
    }
}