using System.ComponentModel.DataAnnotations;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
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
    public class TpFailRecordReportController : ControllerBase
    {

        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly ExcelBLL _excelBll;
        private readonly ServerInfoBLL _serverInfoBll;

        #endregion

        #region Constructor

        public TpFailRecordReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();

        }

        #endregion

        #region Model

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startTime { get; set; }
            private DateTimeOffset _endTime { get; set; }

            /// <summary>
            /// 品牌ID
            /// </summary>
            public IEnumerable<int> SiteIds { get; set; }

            /// <summary>
            /// 幣別
            /// </summary>
            public IEnumerable<int> CurrencyIds { get; set; }

            /// <summary>
            /// 三方ID
            /// </summary>
            public IEnumerable<int> TpIds { get; set; }

            /// <summary>
            /// 交易時間起
            /// </summary>
            public DateTimeOffset StartTime
            {
                get => _startTime;
                set => _startTime = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }

            /// <summary>
            /// 交易時間迄
            /// </summary>
            public DateTimeOffset EndTime
            {
                get => _endTime;
                set => _endTime = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }

            /// <summary>
            /// 用戶
            /// </summary>
            public string UserName { get; set; }

            /// <summary>
            /// 交易單號
            /// </summary>
            public string TransId { get; set; }
        }

        public class ExportRequest
        {
            private DateTimeOffset _startTime { get; set; }
            private DateTimeOffset _endTime { get; set; }
            /// <summary>
            /// 品牌ID
            /// </summary>
            public IEnumerable<int> SiteIds { get; set; }

            /// <summary>
            /// 幣別
            /// </summary>
            public IEnumerable<int> CurrencyIds { get; set; }

            /// <summary>
            /// 三方ID
            /// </summary>
            public IEnumerable<int> TpIds { get; set; }

            /// <summary>
            /// 交易時間起
            /// </summary>
            public DateTimeOffset StartTime
            {
                get => _startTime;
                set => _startTime = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }

            /// <summary>
            /// 交易時間迄
            /// </summary>
            public DateTimeOffset EndTime
            {
                get => _endTime;
                set => _endTime = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }

            /// <summary>
            /// 用戶
            /// </summary>
            public string UserName { get; set; }

            /// <summary>
            /// 交易單號
            /// </summary>
            public string TransId { get; set; }

            ///// <summary>
            ///// 匯出資料顯示的時區(-12~14)
            ///// </summary>
            //[Range(-12, 14)]
            //public int TimeZone { get; set; }
        }

        #endregion

        #region TpFailRocordReport

        /// <summary>
        /// TpFailRecordReport/GetList 取得資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Bet Failure Record > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TpFailRecordReport" })]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 61, 1 })]
        public ApiListResult<TpFailRecordReport> GetList(GetListRequest request)
        {
            var siteInfoList = _serverInfoBll.GetSiteList();
            var currencyIdList = new List<int>();
            var siteList = new List<int>();

            if (request.SiteIds.Contains(0))
            {
                siteList.AddRange(siteInfoList.Select(x => x.SiteId));
            }
            else
            {
                siteList = request.SiteIds.ToList();
            }

            if (request.CurrencyIds == null)
            {
                foreach (var site in siteInfoList)
                {
                    currencyIdList.AddRange(site.ICurrencyList);
                }
            }
            else
                currencyIdList = request.CurrencyIds.ToList();

            if (string.IsNullOrWhiteSpace(request.TransId))
                request.TransId = null;

            if (string.IsNullOrWhiteSpace(request.UserName))
                request.UserName = null;

            var data =
                _reportBll.GetTpFailRecordReport(new GetTpFailRecordReportInput
                {
                    SiteIds = siteList,
                    CurrencyIds = currencyIdList,
                    TpIds = request.TpIds,
                    FromTransTime = request.StartTime.DateTime,
                    ToTransTime = request.EndTime.DateTime,
                    UserName = request.UserName,
                    TransId = request.TransId,
                    PageSize = request.PageSize,
                    StartIndex = (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize)
                }, out int totalPages, out int rowCount);

            if (!data.Any())
                throw new AdminException(ErrorCodeEnum.NoData);

            return new ApiListResult<TpFailRecordReport>
            {
                Data = data,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                TotalPages = totalPages,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// TpFailRecordReport/Export 匯出 Excel
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Bet Failure Record > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>Excel File</returns>
        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TpFailRecordReport" })]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 61, 2 })]
        public IActionResult Export(ExportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TransId))
                request.TransId = null;

            if (string.IsNullOrWhiteSpace(request.UserName))
                request.UserName = null;

            return ExportTpFailRecordReport(request.SiteIds,
                request.CurrencyIds,
                request.TpIds,
                request.StartTime.DateTime,
                request.EndTime.DateTime,
                request.UserName,
                request.TransId);
        }

        #endregion

        #region Private Methods

        private FileContentResult ExportTpFailRecordReport(IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            IEnumerable<int> tpIds,
            DateTime fromTransTime,
            DateTime toTransTime,
            string userName,
            string transId)
        {
            var exportData =
                _reportBll.GetTpFailRecordReportForExport(new GetTpFailRecordReportInput
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    TpIds = tpIds,
                    FromTransTime = fromTransTime,
                    ToTransTime = toTransTime,
                    UserName = userName,
                    TransId = transId
                });

            var resultData = exportData.Select(x => new TpFailRecordForExport
            {
                SiteName = x.SiteName,
                User = x.UserName,
                CurrencyName = x.CurrencyName,
                TransTime = x.TransTime.ToOffset(TimeSpan.FromHours(8)),
                Vendor = x.TpName,
                TransId = x.TransId,
                Info = x.Info
            });

            var stream = _excelBll.ExportV2(resultData);

            return _excelBll.ExcelFile(stream, "TpFailedRecordReport");
        }

        #endregion
    }
}
