using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize]
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
            public int TpId { get; set; }

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
            public int TpId { get; set; }

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

            /// <summary>
            /// 匯出資料顯示的時區(-12~14)
            /// </summary>
            [Range(-12, 14)]
            public int TimeZone { get; set; }
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TpFailRecordReport" })]
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
                    TpId = request.TpId,
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
        /// TpFailRecordReport/ExportV1 匯出 Excel
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Bet Failure Record > [Submit]
        /// </remarks>
        /// <param name="siteId">SiteId</param>
        /// <param name="tpId">TpId</param>
        /// <param name="startTime">交易時間起</param>
        /// <param name="endTime">交易時間迄</param>
        /// <param name="userName">用戶</param>
        /// <param name="transId">交易單號</param>
        /// <param name="timezone">匯出資料顯示的時區(-12~14)</param>
        /// <returns>Excel File</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TpFailRecordReport" })]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult ExportV1(string siteIds,
            int tpId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            string userName,
            string transId,
            int timezone)
        {
            var siteInfoList = _serverInfoBll.GetSiteList();

            var currencyIdList = new List<int>();

            foreach (var site in siteInfoList)
            {
                currencyIdList.AddRange(site.ICurrencyList);
            }
            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(siteIds);

            if (siteList.Contains(0))
            {
                siteList = siteInfoList.Select(x => x.SiteId);
            }

            if (string.IsNullOrWhiteSpace(transId))
                transId = null;

            if (string.IsNullOrWhiteSpace(userName))
                userName = null;

            return ExportTpFailRecordReport(siteList,
                currencyIdList,
                tpId,
                new DateTimeOffset(startTime.DateTime, TimeSpan.Zero).DateTime,
                new DateTimeOffset(endTime.DateTime, TimeSpan.Zero).DateTime,
                userName,
                transId,
                timezone);
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TpFailRecordReport" })]
        public IActionResult Export(ExportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TransId))
                request.TransId = null;

            if (string.IsNullOrWhiteSpace(request.UserName))
                request.UserName = null;

            return ExportTpFailRecordReport(request.SiteIds,
                request.CurrencyIds,
                request.TpId,
                request.StartTime.DateTime,
                request.EndTime.DateTime,
                request.UserName,
                request.TransId,
                request.TimeZone);
        }

        #endregion

        #region Private Methods

        private FileContentResult ExportTpFailRecordReport(IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            int tpId,
            DateTime fromTransTime,
            DateTime toTransTime,
            string userName,
            string transId,
            int timezone)
        {
            var exportData =
                _reportBll.GetTpFailRecordReportForExport(new GetTpFailRecordReportInput
                {
                    SiteIds = siteIds,
                    CurrencyIds = currencyIds,
                    TpId = tpId,
                    FromTransTime = fromTransTime,
                    ToTransTime = toTransTime,
                    UserName = userName,
                    TransId = transId
                });

            var resultData = exportData.Select(x => new TpFailRecordForExport
            {
                SiteName = x.SiteName,
                User = x.UserName,
                TransTime = x.TransTime.ToOffset(TimeSpan.FromHours(timezone)),
                Vendor = x.TpName,
                TransId = x.TransId,
                Info = x.Info
            });

            var stream = _excelBll.Export(resultData);

            return _excelBll.ExcelFile(stream, "TpFailedRecordReport");
        }

        #endregion
    }
}
