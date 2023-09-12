using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MpaMarketingDashboardController : ControllerBase
    {
        #region Fields

        private readonly MpaMarketingDashboardBLL _mpaMarketingDashboardBll;
        private readonly ExcelBLL _excelBll;
        private readonly ServerInfoBLL _serverInfoBll;

        #endregion

        #region Constructor

        public MpaMarketingDashboardController(IServiceProvider provider)
        {
            _mpaMarketingDashboardBll = provider.GetRequiredService<MpaMarketingDashboardBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }

        #endregion

        #region PerformanceHighlight

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 1 })]
        public ApiResult<PerformanceHighlightData> GetPerformanceHighlightData(GetDataRequest request)
        {
            var apiResult = new ApiResult<PerformanceHighlightData>();

            try
            {
                var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
                var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);

                request.MultiSiteRequest = multiSiteRequest;
                var data = _mpaMarketingDashboardBll.GetPerformanceHighlightData(request).First();
                apiResult.Data = data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 1 })]
        public ApiResult<IEnumerable<PerformanceHighlightData>> GetPerformanceHighlightDataForMultiSites(GetDataRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<PerformanceHighlightData>>();

            try
            {
                var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
                var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);

                request.MultiSiteRequest = multiSiteRequest;
                var data = _mpaMarketingDashboardBll.GetPerformanceHighlightData(request);
                apiResult.Data = data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }


        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 2 })]
        public IActionResult GetPerformanceHighlightDataExport(GetDataRequestExport input)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)input).ConvertToMultiSiteParam(userServerInfo);
            var request = new GetDataRequest
            {
                MultiSiteRequest = multiSiteRequest,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                SiteId = input.SiteId,
                CurrencyIds = input.CurrencyIds,
                StartDateCompared = new DateTimeOffset(),
                EndDateCompared = new DateTimeOffset(),
                BaseCurrencyId = input.BaseCurrencyId,
                IsCompared = false
            };

            var data = _mpaMarketingDashboardBll.GetPerformanceHighlightDataExport(request);
            var stream = _excelBll.ExportV2(data);
            return _excelBll.ExcelFileDateRange(stream, request.StartDate.Date, request.EndDate.Date, "Business Performance Highlight");
        }

        #endregion

        #region UserActivities

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 1 })]
        public ApiListResult<UserActivitiesResult> GetUserActivitiesData(GetUserActivitiesDataRequest request)
        {
            var apiResult = new ApiListResult<UserActivitiesResult>();

            try
            {
                var data = _mpaMarketingDashboardBll.GetUserActivitiesData(request);
                apiResult.Data = data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 2 })]
        public IActionResult GetRegistrationDetailsExport(GetRegistrationDetailsRequest request)
        {
            var data = _mpaMarketingDashboardBll.GetRegistrationDetails(request);
            var stream = _excelBll.ExportV2(data);
            return _excelBll.ExcelFileDateRange(stream, request.StartDate.Date, request.EndDate.Date, "Registration Details");
        }

        #endregion

        #region RevenueAvg

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 1 })]
        public ApiListResult<RevenueAvgData> GetRevenueAvgData(GetRevenueAvgDataRequest request)
        {
            var apiResult = new ApiListResult<RevenueAvgData>();

            try
            {
                var data = _mpaMarketingDashboardBll.GetRevenueAvgData(request);
                apiResult.Data = data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
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