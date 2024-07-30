using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class LeaderBoardReportController : ControllerBase
    {
        private readonly LeaderBoardBLL _leaderBoardBll;
        private readonly ExcelBLL _excelBll;
        private readonly ServerInfoBLL _serverInfoBll;

        public LeaderBoardReportController(IServiceProvider provider)
        {
            _leaderBoardBll = provider.GetRequiredService<LeaderBoardBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 143, 1 })]
        public ApiListResult<LeaderboardReportResponse> GetLeaderboardReportBySiteIds(GetLeaderboardReportRequest request)
        {
            var apiResult = new ApiListResult<LeaderboardReportResponse>();
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            try
            {
                var data = _leaderBoardBll.GetLeaderboardReportBySiteIds(request);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 143, 2 })]
        public IActionResult GetLeaderboardReportExport(GetLeaderboardReportRequest request)
        {
            var data = _leaderBoardBll.ExportLeaderboardReport(request);

            var stream = _excelBll.ExportV2(data);
            return _excelBll.ExcelFile(stream, "LeaderboardReport");
        }

    }
}