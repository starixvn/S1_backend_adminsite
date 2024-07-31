using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DrawPrizeReportController : ControllerBase
    {
        #region Fields

        private readonly PrizeBLL _prizeBll;
        private readonly ExcelBLL _excelBll;
        private readonly Tools _tools;

        #endregion

        #region Constructor

        public DrawPrizeReportController(IServiceProvider provider)
        {
            _prizeBll = provider.GetRequiredService<PrizeBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _tools = provider.GetRequiredService<Tools>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
            public IEnumerable<string> EventNames { get; set; }
        }

        public class GetLogRequest : ApiRequestPageModel
        {
            public int EventId { get; set; }
            public int PrizeId { get; set; }
            public DateTimeOffset EventTime { get; set; }
            public string PrizeName { get; set; }
        }

        public class ExportRequest
        {
            public int SiteId { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public IEnumerable<string> EventNames { get; set; }
        }

        #endregion

        /// <summary>
        /// DrawPrizeReport/GetList 取得抽獎資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Draw Prize Report > [Submit] 
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 78, 1 })]
        [HttpPost]
        public ApiResult<DrawPrizeReportResult> GetList(GetListRequest request)
        {
            var apiResult = new ApiResult<DrawPrizeReportResult>();

            _tools.StringConvertDateTimeOffset(request.StartDate.ToString(), out DateTimeOffset _, out TimeSpan timeSpan);

            try
            {
                var param = new PrizeSelect()
                {
                    SelectSite = new List<int>() { request.SiteId },
                    CurrencyId = request.CurrencyIds,
                    StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = request.EndDate.ToString("yyyy-MM-dd"),
                    EventName = request.EventNames,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                };

                var prizeSearchResult = _prizeBll.PrizeSearch(param, timeSpan.Hours, out List<string> _);
                prizeSearchResult.PageNumber = request.PageNumber;
                apiResult.Data = prizeSearchResult;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// DrawPrizeReport/Export 匯出抽獎報告清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Draw Prize Report > [Submit] > [Export Excel] 
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 78, 2 })]
        [HttpPost]
        public IActionResult Export(ExportRequest request)
        {
            _tools.StringConvertDateTimeOffset(request.StartDate.ToString(), out DateTimeOffset _, out TimeSpan timeSpan);

            var exportData = _prizeBll.GetExportData(
                JsonConvert.SerializeObject(new List<int>() { request.SiteId }),
                request.StartDate.ToString("yyyy-MM-dd"),
                request.EndDate.ToString("yyyy-MM-dd"),
                JsonConvert.SerializeObject(request.CurrencyIds),
                JsonConvert.SerializeObject(request.EventNames),
                timeSpan.Hours);

            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "DrawPrizeReport");

        }

        /// <summary>
        /// DrawPrizeReport/GetLog 取得抽獎紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Draw Prize Report > [Submit] > [Detail] 
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 78, 1 })]
        [HttpPost]
        public ApiListResult<PrizeLogData> GetLog(GetLogRequest request)
        {
            var apiResult = new ApiListResult<PrizeLogData>();

            try
            {
                _tools.StringConvertDateTimeOffset(request.EventTime.ToString(), out _, out _);
                var param = new PrizeLogPara()
                {
                    EventId = request.EventId,
                    PrizeId = request.PrizeId,
                    EventTime = request.EventTime,
                    PrizeName = request.PrizeName,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                };

                var logResult = _prizeBll.GetPrizeLog(param);
                foreach (var data in logResult.PrizeLogData)
                {
                    data.DrawTime = new DateTimeOffset(data.DrawTime.DateTime, TimeSpan.Zero);
                }

                apiResult.Data = logResult.PrizeLogData;
                apiResult.PageNumber = request.PageNumber;
                apiResult.TotalPages = logResult.TotalPages;
                apiResult.RowCount = logResult.RowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
    }
}