using System.ComponentModel.DataAnnotations;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        #region Fields

        private readonly ServerInfoBLL _serverInfoBll;
        private readonly DashBoardBLL _dashBoardBll;

        #endregion

        #region Constructor

        public DashBoardController(IServiceProvider provider)
        {
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _dashBoardBll = provider.GetRequiredService<DashBoardBLL>();
        }

        #endregion

        #region API Model

        public class GetMemBerDataRequest
        {
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }

        public class GetNewMemBerListRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }
        
        #endregion

        #region DashBoardMember

        /// <summary>
        /// DashBoard/GetData 取得首頁儀錶板資訊資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Home > Dash Board > [Refresh]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 1, 1 })]
        [HttpPost]
        public ApiResult<DashBoardMemBerData> GetMemBerData(GetMemBerDataRequest request)
        {
            if (request.TimeZone is > 14 or < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");
            
            var result = _dashBoardBll.GetDashBoardMemberData(request.SiteId, request.CurrencyIds, request.TimeZone);

            return new ApiResult<DashBoardMemBerData>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
        }
        
        [HttpPost]
        public ApiListResult<NewMemberList> GetNewMemBerList(GetNewMemBerListRequest request)
        {
            if (request.TimeZone is > 14 or < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");
            
            var localTime = DateTime.UtcNow.AddHours(request.TimeZone);
            var startTime = localTime.Date;
            var endTime = startTime.AddDays(1).AddMilliseconds(-1);
           
            var result = _dashBoardBll.GetNewMemberList(
                request.SiteId, 
                request.CurrencyIds,
                startTime,
                endTime,
                request.PageSize,
                request.PageNumber,
                out var totalPages,
                out var rowCount );

            return new ApiListResult<NewMemberList>
            {
                Data = result,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        #endregion
        
        
    }
}