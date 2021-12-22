using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TpFailRecordReportController : Controller
    {

        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly ReportExcelBLL _reportExcelBll;

        #endregion


        #region Constructor

        public TpFailRecordReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
        }

        #endregion

        #region Model

        public class GetTpFailRecordReportReq
        {
            public int SiteId { get; set; }

            public int TpId { get; set; }

            public long FromTransTime { get; set; }

            public long ToTransTime { get; set; }

            public string UserName { get; set; }

            public string TransId { get; set; }

            public int PageSize { get; set; }

            public int PageNumber { get; set; }
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
        /// <param name="req">參數:
        ///     SiteId 
        ///     TpId
        ///     FromTransTime: 交易時間起(Unix)
        ///     ToTransTime: 交易時間迄(Unix)
        ///     UserName: 用戶
        ///     TransId: 交易單號
        ///     PageSize: 分頁筆數
        ///     PageNumber: 頁數
        /// </param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/TpFailRecordReport" })]
        public ApiListResult<object> GetList(GetTpFailRecordReportReq req)
        {
            var data =
                _reportBll.GetTpFailRecordReport(new GetTpFailRecordReportInput
                {
                    SiteId = req.SiteId,
                    TpId = req.TpId,
                    FromTransTime = req.FromTransTime,
                    ToTransTime = req.ToTransTime,
                    UserName = req.UserName,
                    TransId = req.TransId,
                    PageSize = req.PageSize,
                    StartIndex = (Math.Abs(req.PageNumber) - 1) * Math.Abs(req.PageSize)
                }, out int totalPages, out int rowCount);

            if (!data.Any())
                throw new AdminException(ErrorCodeEnum.NoData);

            var serverData = _reportBll.GetServer();
            var siteNameMapper =
                serverData.ToDictionary(x => x.SiteId, x => x.SiteName);
            var result = data.ToList();
            result.ForEach(x => x.SiteName = siteNameMapper[x.SiteId]);
            
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
        /// TpFailRecordReport/Export 匯出 Excel
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Bet Failure Record > [Submit]
        /// </remarks>
        /// <param name="siteId">SiteId</param>
        /// <param name="tpId">TpId</param>
        /// <param name="fromTransTime">交易時間起(Unix)</param>
        /// <param name="toTransTime">交易時間迄(Unix)</param>
        /// <param name="userName">用戶</param>
        /// <param name="transId">交易單號</param>
        /// <param name="timezone">匯出資料顯示的時區(-12~14)</param>
        /// <returns>Excel File</returns>
        [HttpPost]
        public IActionResult Export(int siteId,
            int tpId,
            long fromTransTime,
            long toTransTime,
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

            var serverData = _reportBll.GetServer();
            var siteNameMapper = 
                serverData.ToDictionary(x => x.SiteId, x => x.SiteName);
            
            var titles = new Dictionary<string, Api.ReportController.TitleModel>
            {
                { "Site", new Api.ReportController.TitleModel { Name = "Site", SortId = 1 } },
                { "User", new Api.ReportController.TitleModel { Name = "User", SortId = 2 } },
                { "TransTime", new Api.ReportController.TitleModel { Name = "TransTime", SortId = 3 } },
                { "Vendor", new Api.ReportController.TitleModel { Name = "Vendor", SortId = 4 } },
                { "TransId", new Api.ReportController.TitleModel { Name = "TransId", SortId = 5 } },
                { "Info", new Api.ReportController.TitleModel { Name = "Info", SortId = 6 } }
            };
            
            var content = exportData.Select(x =>
                new Dictionary<string, dynamic>
                {
                    {"Site", siteNameMapper[x.SiteId]},
                    {"User", x.UserName},
                    {"TransTime", x.TransTime.AddHours(timezone).ToString("MM/dd/yyyy HH:mm:ss")},
                    {"Vendor", x.TpName},
                    {"TransId", x.TransId},
                    {"Info", x.Info}
                });
            
            var excels = new List<GeneralReportExportDataToExcel>()
            {
                new GeneralReportExportDataToExcel()
                {
                    SheetName = "Tp Failed Record Report",
                    Content = content,
                    Titles = titles
                        .OrderBy(x=> x.Value.SortId)
                        .ToDictionary(x => x.Key, x => x.Value.Name)
                },
            };

            var stream = _reportExcelBll.GeneralReportExportToMultiSheet(excels);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"TpFailedRecordReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        #endregion
    }
}
