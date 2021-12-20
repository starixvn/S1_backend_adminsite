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

            public DateTime FromTransTime { get; set; }

            public DateTime ToTransTime { get; set; }

            public string UserName { get; set; }

            public string TransId { get; set; }

            public int PageSize { get; set; }

            public int StartIndex { get; set; }
            
            public int TimeZone { get; set; }
        }

        #endregion

        #region TpFailRocordReport

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/TpFailRecordReport" })]
        public ApiResult GetList(GetTpFailRecordReportReq req)
        {
            if (!TimeZoneValidation(req.TimeZone))
                return new ApiResult<object>
                {
                    Message = "Invalid TimeZone",
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                };
            
            var data = _reportBll.GetTpFailRecordReport(new GetTpFailRecordReportInput
            {
                SiteId = req.SiteId,
                TpId = req.TpId,
                FromTransTime = ToUnixTimeStamp(req.FromTransTime, req.TimeZone),
                ToTransTime = ToUnixTimeStamp(req.ToTransTime, req.TimeZone),
                UserName = req.UserName,
                TransId = req.TransId,
                PageSize = req.PageSize,
                StartIndex = req.StartIndex
            });

            if (!data.Result.Any())
                throw new AdminException(ErrorCodeEnum.NoData);

            return new ApiResult<object>
            {
                Data = new
                {
                    Result = data.Result,
                    RowCount = data.RowCount
                },
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public IActionResult Export(GetTpFailRecordReportReq req)
        {
            #region Get Data

            var exportData = _reportBll.GetTpFailRecordReport(new GetTpFailRecordReportInput
            {
                SiteId = req.SiteId,
                TpId = req.TpId,
                FromTransTime = ToUnixTimeStamp(req.FromTransTime, req.TimeZone),
                ToTransTime = ToUnixTimeStamp(req.ToTransTime, req.TimeZone),
                UserName = req.UserName,
                TransId = req.TransId,
                PageSize = req.PageSize,
                StartIndex = req.StartIndex
            });

            var serverData = _reportBll.GetServer();
            var siteNameMapper = 
                serverData.ToDictionary(x => x.SiteId, x => x.SiteName);

            #endregion

            #region Set Title

            var titles = new Dictionary<string, Api.ReportController.TitleModel>
            {
                { "SiteName", new Api.ReportController.TitleModel { Name = "Site Name", SortId = 1 } },
                { "User", new Api.ReportController.TitleModel { Name = "User", SortId = 2 } },
                { "TransTime", new Api.ReportController.TitleModel { Name = "Trans Time(GMT " + req.TimeZone + ")", SortId = 3 } },
                { "Vendor", new Api.ReportController.TitleModel { Name = "Vendor", SortId = 4 } },
                { "TransId", new Api.ReportController.TitleModel { Name = "Trans Id", SortId = 5 } },
                { "Info", new Api.ReportController.TitleModel { Name = "Info", SortId = 6 } }
            };

            #endregion

            var content = exportData.Result.Select(x =>
                new Dictionary<string, object>
                {
                    {"SiteName", siteNameMapper[x.SiteId]},
                    {"User", x.UserName},
                    {"TransTime", x.TransTime.AddHours(req.TimeZone).ToString("yyyy-MM-dd HH:mm:ss")},
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

        #region Private Method

        private bool TimeZoneValidation(int timeZone)
            => timeZone <= 14 && timeZone >= -12;

        private long ToUnixTimeStamp(DateTime dateTime, int timeZone)
        {
            var datetimeStr = $"{dateTime:yyyy-MM-dd} {TransferToTimeZoneFormat(timeZone)}";

            return DateTimeOffset.Parse(datetimeStr).ToUnixTimeSeconds();
        }

        private string TransferToTimeZoneFormat(int timeZone)
            => (Math.Sign(timeZone) >= 0 ? "+" : "-") + Math.Abs(timeZone).ToString("D2") + ":00";


        #endregion
    }
}
