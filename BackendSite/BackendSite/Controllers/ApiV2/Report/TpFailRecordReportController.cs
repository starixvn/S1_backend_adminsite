using System;
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

        #endregion
        
        #region Constructor

        public TpFailRecordReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region Model

        public class GetListReq : ApiRequestPageModel
        {
            private DateTimeOffset _startTime { get; set; }
            private DateTimeOffset _endTime { get; set; }

            /// <summary>
            /// 品牌ID
            /// </summary>
            public int SiteId { get; set; }

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

        public class ExportReq
        {
            private DateTimeOffset _startTime { get; set; }
            private DateTimeOffset _endTime { get; set; }

            /// <summary>
            /// 品牌ID
            /// </summary>
            public int SiteId { get; set; }

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
        /// <param name="req">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TpFailRecordReport" })]
        public ApiListResult<object> GetList(GetListReq req)
        {
            var data =
                _reportBll.GetTpFailRecordReport(new GetTpFailRecordReportInput
                {
                    SiteId = req.SiteId,
                    TpId = req.TpId,
                    FromTransTime = req.StartTime.DateTime,
                    ToTransTime = req.EndTime.DateTime,
                    UserName = req.UserName,
                    TransId = req.TransId,
                    PageSize = req.PageSize,
                    StartIndex = (Math.Abs(req.PageNumber) - 1) * Math.Abs(req.PageSize)
                }, out int totalPages, out int rowCount);

            if (!data.Any())
                throw new AdminException(ErrorCodeEnum.NoData);
            
            return new ApiListResult<object>
            {
                Data = data,
                PageNumber = req.PageNumber,
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
        public IActionResult ExportV1(int siteId,
            int tpId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            string userName,
            string transId,
            int timezone)
        {
            return ExportTpFailRecordReport(siteId,
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
        /// <param name="req">參數</param>
        /// <returns>Excel File</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TpFailRecordReport" })]
        public IActionResult Export(ExportReq req)
        {
            return ExportTpFailRecordReport(req.SiteId,
                req.TpId,
                req.StartTime.DateTime,
                req.EndTime.DateTime,
                req.UserName,
                req.TransId,
                req.TimeZone);
        }

        #endregion

        #region Private Methods

        private FileContentResult ExportTpFailRecordReport(int siteId,
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
                    SiteId = siteId,
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
