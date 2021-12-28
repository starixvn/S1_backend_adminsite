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
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class UserOnlineReportController : ControllerBase
    {
        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly ReportExcelBLL _reportExcelBll;
        private readonly ResponseSettingBLL _responseSettingBll;

        #endregion

        #region Constructor

        public UserOnlineReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
            _responseSettingBll = provider.GetRequiredService<ResponseSettingBLL>();
        }

        #endregion

        #region Model

        public class GetUserOnlineReportReq
        {
            public IEnumerable<int> SiteIds { get; set; }
            public string SelectDate { get; set; }
            public int TimeZone { get; set; }
        }

        #endregion
        
        #region UserOnlineReport

        /// <summary>
        /// UserOnlineReport/Get 取得使用者線上報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > User Online Report > [Submit]
        /// </remarks>
        /// <param name="request">參數,TimeZone(-12~14)</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/UserOnlineReport" })]
        public ApiResult Get(GetUserOnlineReportReq request)
        {
            if (request.TimeZone <= 14 && request.TimeZone >= -12)
            {
                var timeZone = TransferToTimeZoneFormat(request.TimeZone);
                var data = _reportBll.GetUserOnlineReport(JsonConvert.SerializeObject(request.SiteIds), request.SelectDate , timeZone);

                if (!data.Any())
                {
                    throw new AdminException(ErrorCodeEnum.NoData);
                }

                return new ApiResult<IEnumerable<UserOnlineReportModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
            
            return new ApiResult<IEnumerable<UserOnlineReportModel>>
            {
                Message ="Invalid TimeZone",
                ErrorCode = (int)ErrorCodeEnum.Fail,
            };
        }

        /// <summary>
        /// Report/Export 匯出使用者線上報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > User Online Report > [Export Excel]
        /// </remarks>
        /// <param name="req">TimeZone(-12~14)</param>
        /// <returns>檔案</returns>
        [HttpPost]
        public IActionResult Export(GetUserOnlineReportReq req)
        {
            #region Get Data
        
            var timeZone = TransferToTimeZoneFormat(req.TimeZone);
            var exportData = _reportBll.GetUserOnlineReport(JsonConvert.SerializeObject(req.SiteIds), req.SelectDate,timeZone);
        
            var totalMaxCount = exportData.GroupBy(x => new {x.LocalTime})
                .Select(x => x.Sum(y => y.MaxCount)).Max();
        
            var serverData = _reportBll.GetServer();
        
            #endregion
        
            #region Initial Param
        
            var content = new List<Dictionary<string, dynamic>>();
            var titles = new Dictionary<string, Api.ReportController.TitleModel>();
            var titlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
        
            #endregion
        
            #region Set Title(Merge Title & Title & Total Title & Not Summary Title)
        
            #region Merge Title
        
            var mergeTitles = new Dictionary<string, MergeTitleModel>
            {
                {"Date", new MergeTitleModel {Name = req.SelectDate, MergeRange = 1}},
                {"OnlineCount", new MergeTitleModel {Name = "Online Count", MergeRange = req.SiteIds.Count() + 1}}
            };

            titlesToMerge.Add(mergeTitles);
        
            #endregion
        
            #region Title
        
            titles.Add("LocalTime", new Api.ReportController.TitleModel { Name = "Local Time(GMT "+ req.TimeZone + ")", SortId = 1 });
            titles.Add("TotalOnline", new Api.ReportController.TitleModel { Name = "Total Online", SortId = 200 });
        
            foreach (var site in req.SiteIds)
            {
                var siteName = serverData.Where(x => x.SiteId == site).Select(x => x.SiteName).FirstOrDefault();
                var onlinePropName = $"Online-{siteName}";
                var onlineSortId = 100 + site;
                titles.Add(onlinePropName, new Api.ReportController.TitleModel { Name = siteName, SortId = onlineSortId });
            }
        
            #endregion
        
            var totalTitle = new List<string>
            {
                "TotalOnline",
            };
        
            var notSummaryTitles = new List<string>
            {
                "Key", "LocalTime",
                "TotalOnline"
            };
        
            #endregion
        
            foreach (var item in exportData)
            {
                var onlinePropName = $"Online-{item.SiteName}";
                var getKey = $"{item.LocalTime}";
                var getItem = content.FirstOrDefault(x =>
                    x.Any(y => y.Key == "Key" && (string)y.Value == getKey)
                );
        
                if (getItem != null)
                {
                    getItem.TryGetValue("TotalOnline", out var totalCountData);
                    getItem["TotalOnline"] = (int)totalCountData + item.MaxCount;
        
                    if (getItem.ContainsKey(onlinePropName))
                    {
                        getItem.TryGetValue(onlinePropName, out var maxCountData);
                        getItem[onlinePropName] = (int)maxCountData + item.MaxCount;
                    }
                    else
                    {
                        getItem.Add(onlinePropName, item.MaxCount);
                    }
        
                    if (getItem["TotalOnline"] == totalMaxCount)
                    {
                        getItem["MaxCount"] = true;
                    }
        
                }
                else
                {
                    var newItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", getKey },
                        { "LocalTime", item.LocalTime },
        
                        { onlinePropName,  item.MaxCount },
                        { "TotalOnline", item.MaxCount }
                    };
        
                    content.Add(newItem);
                }
            }
        
            var d = new List<ComplexReportExportDataToExcel>()
            {
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "User Online Report",
                    Content = content,
                    Titles = titles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = titlesToMerge,
                    NotSummaryTitles = notSummaryTitles,
                    IsSummary = false,
                    IsTotalTitleMax = true
                },
            };
        
            var stream = _reportExcelBll.ComplexReportExportToMultiSheet(d);
        
            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"UserOnlineReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion

        #region Private Method

        private string TransferToTimeZoneFormat(int timeZone)
        {
            if (Math.Sign(timeZone) > 0 || Math.Sign(timeZone) == 0)
            {
                return string.Format("+" + Math.Abs(timeZone).ToString("D2") + ":00");
            }
            else 
            {
                return string.Format("-" + Math.Abs(timeZone).ToString("D2") + ":00");
            }
        }

        #endregion
    }
}