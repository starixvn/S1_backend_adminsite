using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MySql.Data.MySqlClient;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BannerController : ControllerBase
    {
        #region Fields

        private readonly BannerManageBLL _bannerManageBll;
        private readonly ServerInfoBLL _serverInfoBll;

        #endregion

        #region Constructor

        public BannerController(IServiceProvider provider)
        {
            _bannerManageBll = provider.GetRequiredService<BannerManageBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel , IApiRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public IEnumerable<BannerTypeEnum> BannerTypes { get; set; }
            public IEnumerable<BannerStatusEnum> Statuses { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }
        }

        #endregion

        #region API

        /// <summary>
        /// Banner/GetBanner 取得指定橫幅
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management > Select [Available Sites] > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult<BannerInfo> GetBanner(GetBannerRequest request)
        {
            var apiResult = new ApiResult<BannerInfo>();
            try
            {
                var result = _bannerManageBll.GetBanner(request.SiteId, request.BannerName, request.CurrencyId);
                if (result != null)
                {
                    apiResult.Data = result;
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
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
        /// Banner/GetList 取得橫幅清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management > Select [Available Sites]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiListResult<BannerInfoList> GetList(GetListRequest request)
        {
            var apiListResult = new ApiListResult<BannerInfoList>();
            try
            {
                var bannerTypeList = request.BannerTypes.Select(x => (int)x);
                var statusList = request.Statuses.Select(x => (int)x);
                var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
                ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);

                var data = _bannerManageBll.GetBannerList(
                    request.SiteIds,
                    bannerTypeList,
                    statusList,
                    new List<int> { request.CurrencyId },
                    request.PageNumber,
                    request.PageSize,
                    out var totalPages, out var rowCount);

                apiListResult.Data = data;
                apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiListResult.TotalPages = totalPages;
                apiListResult.RowCount = rowCount;
                apiListResult.PageNumber = request.PageNumber;
            }
            catch (Exception ex)
            {
                apiListResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiListResult.Message = ex.Message;
                apiListResult.RawMessage = ex.ToString();
            }
            return apiListResult;
        }

        /// <summary>
        /// Banner/SetBanner 新增/更新橫幅
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management  > [Edit] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult SetBanner(ApiRequestWithLog<SetRequest> request)
        {
            var apiResult = new ApiResult();

            var urlRouter = new Dictionary<string, string>()
            {
                {"0_1","eSport"},
                {"0_2","live-casino"},
                {"2_2","liveCasino"},
                {"3_2","liveCasino"},
                {"0_3","referral"},
                {"0_4","leaderBoard"},
                {"0_5","angPow"},
                {"0_6","lottery"},
                {"0_7","slots"},
                {"2_7","casino"},
                {"3_7","casino"},
                {"0_8","game"},
                {"2_8","games"},
                {"3_8","games"},
                {"0_9","fishing"},
                {"0_10","promotion"},
                {"0_11","vip"},
                {"0_12","deposit"},
                {"0_13","rewards"}
            };

            var router = string.IsNullOrEmpty(request.After.UrlRouter) ?
                null : urlRouter.ContainsKey($"{request.SiteId}_{request.After.UrlRouter}") ?
                    urlRouter[$"{request.SiteId}_{request.After.UrlRouter}"] : urlRouter[$"0_{request.After.UrlRouter}"];
            request.After.UrlRouter = router;

            var beforeRouter = string.IsNullOrEmpty(request.Before.UrlRouter) ?
                null : urlRouter.ContainsKey($"{request.SiteId}_{request.Before.UrlRouter}") ?
                    urlRouter[$"{request.SiteId}_{request.Before.UrlRouter}"] : urlRouter[$"0_{request.Before.UrlRouter}"];
            request.Before.UrlRouter = beforeRouter;

            try
            {
                var errorCode = _bannerManageBll.SetBanner(new ParamsWithLog<SetRequest>(request, User.Identity.Name));
                apiResult.ErrorCode = errorCode >= 0 ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.Fail;

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
        /// Banner/UpdateStatus 更新橫幅狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management > Select [Available Sites] > [Status] 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult UpdateStatus(ApiRequestWithLog<UpdateStatusRequest> request)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = _bannerManageBll.UpdateBannerStatus(new ParamsWithLog<UpdateStatusRequest>(request, User.Identity.Name)) >= 0;

                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
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
        /// Banner/UpdateStatus 更新橫幅狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing >  Banner Management > Delete
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult DeleteBanner(ApiRequestWithLog<GetBannerRequest> request)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = _bannerManageBll.DeleteBanner(
                    new ParamsWithLog<GetBannerRequest>(request, User.Identity.Name)) >= 0;

                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
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


        #endregion
    }
}
