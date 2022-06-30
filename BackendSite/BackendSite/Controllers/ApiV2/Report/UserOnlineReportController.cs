using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class UserOnlineReportController : ControllerBase
    {
        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly ReportExcelBLL _reportExcelBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public UserOnlineReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest
        {
            public IEnumerable<int> SiteIds { get; set; }

            public string SelectDate { get; set; }

            [Range(-12, 14)]
            public int TimeZone { get; set; }
        }

        public class TitleModel
        {
            public string Name { get; set; }
            public int SortId { get; set; }
        }

        #endregion

        #region UserOnlineReport

        /// <summary>
        /// UserOnlineReport/GetList 取得使用者線上報表資料
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
        public ApiResult<IEnumerable<UserOnlineReportModel>> GetList(GetListRequest request)
        {
            if (request.TimeZone > 14 || request.TimeZone < -12)
                throw new AdminException(ErrorCodeEnum.Fail, "Invalid TimeZone");

            var data = _reportBll.GetUserOnlineReport(JsonConvert.SerializeObject(request.SiteIds),
                request.SelectDate,
                request.TimeZone);

            if (!data.Any())
                throw new AdminException(ErrorCodeEnum.NoData);

            return new ApiResult<IEnumerable<UserOnlineReportModel>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Report/Export 匯出使用者線上報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > User Online Report > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="request">TimeZone(-12~14)</param>
        /// <returns>檔案</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/UserOnlineReport" })]
        public IActionResult Export(GetListRequest request)
        {
            #region Get Data

            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(JsonConvert.SerializeObject(request.SiteIds));

            var exportData = _reportBll.GetUserOnlineReport(JsonConvert.SerializeObject(request.SiteIds), request.SelectDate, 8);

            var totalMaxCount = exportData.GroupBy(x => new
            {
                x.LocalTime
            }).Select(x => x.Sum(y => y.MaxCount)).Max();

            var serverData = _reportBll.GetServer();

            #endregion

            #region Initial Param

            var content = new List<Dictionary<string, dynamic>>();
            var titles = new Dictionary<string, TitleModel>();
            var titlesToMerge = new List<Dictionary<string, MergeTitleModel>>();

            #endregion

            #region Set Title(Merge Title & Title & Total Title & Not Summary Title)

            #region IsRemoveOld Logic

            var isRemoveOld = exportData.Where(x => x.DeviceType == (short)DeviceId.Old)
                .All(x => x.MaxCount == 0);
            var deviceIdCount = Enum.GetNames(typeof(DeviceId)).Length;
            var deviceIdList = Enum.GetValues(typeof(DeviceId)).Cast<DeviceId>().ToList();

            if (isRemoveOld)
            {
                deviceIdCount -= 1;
                deviceIdList.Remove(DeviceId.Old);
            }

            #endregion

            #region Merge Title

            // row 1
            var mergeTitles = new Dictionary<string, MergeTitleModel>
            {
                {"Date", new MergeTitleModel {Name = request.SelectDate, MergeRange = 1}},
                {"OnlineCount", new MergeTitleModel {Name = "Online Count", MergeRange =  (siteList.Count() * (deviceIdCount +1 )) }}
            };

            // row 2
            var mergeDeviceTitles = new Dictionary<string, MergeTitleModel>
            {
                {"Site", new MergeTitleModel {Name = "Site", MergeRange = 1}}
            };

            foreach (var site in siteList)
            {
                var siteName = serverData.Where(x => x.SiteId == site).Select(x => x.SiteName).FirstOrDefault();
                var onlinePropName = $"Online-{siteName}";
                mergeDeviceTitles.Add(onlinePropName, new MergeTitleModel { Name = siteName, MergeRange = deviceIdCount + 1 });
            }

            titlesToMerge.Add(mergeTitles);
            titlesToMerge.Add(mergeDeviceTitles);

            #endregion

            #region Title

            titles.Add("LocalTime", new TitleModel { Name = "Local Time(GMT +8)", SortId = 1 });
            titles.Add("TotalOnline", new TitleModel { Name = "Total Online", SortId = 200 });

            foreach (var site in siteList)
            {
                var siteName = serverData.Where(x => x.SiteId == site).Select(x => x.SiteName).FirstOrDefault();
                foreach (var id in deviceIdList)
                {
                    var onlinePropName = $"Online-{siteName}-{(short)id}";
                    var onlineSortId = 100 + (site * 10) + (int)id;
                    titles.Add(onlinePropName, new TitleModel { Name = id.ToString(), SortId = onlineSortId });
                }
                var onlinePropNameSummary = $"Online-{siteName}-Summary";
                var onlineSummarySortId = 100 + (site * 10) + 5;
                titles.Add(onlinePropNameSummary, new TitleModel { Name = "Site Total", SortId = onlineSummarySortId });
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
                var onlinePropName = $"Online-{item.SiteName}-{item.DeviceType}";
                var onlineSitePropName = $"Online-{item.SiteName}-Summary";

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

                    if (getItem.ContainsKey(onlineSitePropName))
                    {
                        getItem.TryGetValue(onlineSitePropName, out var maxCountData);
                        getItem[onlineSitePropName] = (int)maxCountData + item.MaxCount;
                    }
                    else
                    {
                        getItem.Add(onlineSitePropName, item.MaxCount);
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
                        { onlineSitePropName,  item.MaxCount },
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

            return _excelBll.ExcelFile(stream, "UserOnlineReport");
        }

        #endregion
    }
}