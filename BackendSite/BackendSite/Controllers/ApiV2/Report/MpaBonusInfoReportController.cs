using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MpaBonusInfoReportController : ControllerBase
    {
        #region Fields

        private readonly MpaBonusInfoBLL _mpaBonusInfoBll;
        private readonly ExcelBLL _excelBll;
        private readonly ReportExcelBLL _reportExcelBll;
        private readonly ServerInfoBLL _serverInfoBll;

        #endregion

        #region Constructor

        public MpaBonusInfoReportController(IServiceProvider provider)
        {
            _mpaBonusInfoBll = provider.GetRequiredService<MpaBonusInfoBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }

        #endregion

        #region API Model

        public class GetDataRequest : IOrderBy, IApiRequest
        {
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
            public string TimeZone
            {
                get
                {
                    var timezone = StartDate.Offset.ToString("hh\\:mm");
                    if (StartDate.Offset >= TimeSpan.Zero)
                    {
                        timezone = "+" + timezone;
                    }
                    else
                    {
                        timezone = "-" + timezone;
                    }

                    return timezone;
                }
            }
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
            public IEnumerable<int> BonusTypes { get; set; }
            public IEnumerable<int> BonusStatus { get; set; }
            public IEnumerable<int> ClaimStatuses { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            public BonusGroupByType GroupByType { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public int CurrencyId { get; set; }
            public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }
            public IEnumerable<MultiSiteRequestDto> MultiSiteRequest { get; set; }

            #region 分頁排序

            /// <summary>
            /// 頁數, -1 = export all
            /// </summary>
            [Required]
            public int PageNumber { get; set; } = 1;
            /// <summary>
            /// 每頁資料筆數
            /// </summary>
            public int PageSize { get; set; } = 25;
            /// <summary>
            /// 是否排序
            /// </summary>
            public bool OrderBy { get; set; }
            /// <summary>
            /// 排序欄位名稱
            /// </summary>
            public string OrderByProperty { get; set; }
            /// <summary>
            /// 排序類型:
            /// 0=升序
            /// 1=降序
            /// </summary>
            public OrderByDirection OrderByDirection { get; set; }

            public string GetRedisKey()
            {
                var jObject = JObject.Parse(JsonConvert.SerializeObject(this));
                //移除排序跟分頁
                jObject.Remove(nameof(PageSize));
                jObject.Remove(nameof(PageNumber));
                jObject.Remove(nameof(OrderBy));
                jObject.Remove(nameof(OrderByProperty));
                jObject.Remove(nameof(OrderByDirection));

                var key = new StringBuilder();
                foreach (var item in jObject.Properties())
                {
                    key.Append($"{item.Name}:{item.Value}:");
                }
                return key.ToString();
            }

            #endregion
        }

        public class GetDetailRequest : IOrderBy, IApiRequest
        {
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
            public string TimeZone
            {
                get
                {
                    var timezone = StartDate.Offset.ToString("hh\\:mm");
                    if (StartDate.Offset >= TimeSpan.Zero)
                    {
                        timezone = "+" + timezone;
                        return timezone;
                    }
                    else
                    {
                        timezone = "-" + timezone;
                        return timezone;
                    }
                }
            }
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
            public IEnumerable<int> ClaimStatuses { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public int CurrencyId { get; set; }
            public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }
            public IEnumerable<MultiSiteRequestDto> MultiSiteRequest { get; set; }

            #region 分頁排序

            /// <summary>
            /// 頁數, -1 = export all
            /// </summary>
            [Required]
            public int PageNumber { get; set; } = 1;
            /// <summary>
            /// 每頁資料筆數
            /// </summary>
            public int PageSize { get; set; } = 25;
            /// <summary>
            /// 是否排序
            /// </summary>
            public bool OrderBy { get; set; }
            /// <summary>
            /// 排序欄位名稱
            /// </summary>
            public string OrderByProperty { get; set; }
            /// <summary>
            /// 排序類型:
            /// 0=升序
            /// 1=降序
            /// </summary>
            public OrderByDirection OrderByDirection { get; set; }
            public string GetRedisKey()
            {
                var jObject = JObject.Parse(JsonConvert.SerializeObject(this));
                jObject[nameof(StartDate)] = StartDate.ToString("yyyyMMdd");
                jObject[nameof(EndDate)] = EndDate.ToString("yyyyMMdd");
                //移除排序跟分頁
                jObject.Remove(nameof(PageSize));
                jObject.Remove(nameof(PageNumber));
                jObject.Remove(nameof(OrderBy));
                jObject.Remove(nameof(OrderByProperty));
                jObject.Remove(nameof(OrderByDirection));

                var key = new StringBuilder();
                foreach (var item in jObject.Properties())
                {
                    key.Append($"{item.Name}:{item.Value}:");
                }
                return key.ToString();
            }

            #endregion
        }

        public class GetMarketingDataRequest : IApiRequest
        {
            private DateTimeOffset _comparedStartDate { get; set; }
            private DateTimeOffset _comparedendDate { get; set; }
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset ComparedStartDate
            {
                get => _comparedStartDate;
                set => _comparedStartDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset ComparedEndDate
            {
                get => _comparedendDate;
                set => _comparedendDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            public bool IsCompared { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public int CurrencyId { get; set; }
            public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }

            public DateTimeOffset GetEndDate()
            {
                //因為過去日期要查月底最後天，假設今天 12/1 去查  11/1 ~ 11/18 的資料，一律用 11/30的匯率 
                var endDate = EndDate.AddMonths(1).AddDays(EndDate.AddMonths(1).Day);
                return endDate;
            }
            public DateTimeOffset GetCompareEndDate()
            {
                //因為過去日期要查月底最後天，假設今天 12/1 去查  11/1 ~ 11/18 的資料，一律用 11/30的匯率 
                var endDate = ComparedEndDate.AddMonths(1).AddDays(ComparedEndDate.AddMonths(1).Day);
                return endDate;
            }

        }

        public class GetMarketingDetailRequest : IOrderBy
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public int SiteId { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }

            #region 分頁排序

            /// <summary>
            /// 頁數, -1 = export all
            /// </summary>
            [Required]
            public int PageNumber { get; set; } = 1;
            /// <summary>
            /// 每頁資料筆數
            /// </summary>
            public int PageSize { get; set; } = 25;
            /// <summary>
            /// 是否排序
            /// </summary>
            public bool OrderBy { get; set; }
            /// <summary>
            /// 排序欄位名稱
            /// </summary>
            public string OrderByProperty { get; set; }
            /// <summary>
            /// 排序類型:
            /// 0=升序
            /// 1=降序
            /// </summary>
            public OrderByDirection OrderByDirection { get; set; }
            public string GetRedisKey()
            {
                var jObject = JObject.Parse(JsonConvert.SerializeObject(this));
                jObject[nameof(StartDate)] = StartDate.ToString("yyyyMMdd");
                jObject[nameof(EndDate)] = EndDate.ToString("yyyyMMdd");
                //移除排序跟分頁
                jObject.Remove(nameof(PageSize));
                jObject.Remove(nameof(PageNumber));
                jObject.Remove(nameof(OrderBy));
                jObject.Remove(nameof(OrderByProperty));
                jObject.Remove(nameof(OrderByDirection));

                var key = new StringBuilder();
                foreach (var item in jObject.Properties())
                {
                    key.Append($"{item.Name}:{item.Value}:");
                }
                return key.ToString();
            }

            #endregion

        }

        #endregion

        #region MpaBonusInfo

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 72, 1 })]
        public ApiListResult<MpaBonusInfo> GetInfoData(GetDataRequest request)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            request.MultiSiteRequest = multiSiteRequest;
            var result = _mpaBonusInfoBll.GetMpaBonusInfoList(request, out var pageInfo);

            return new ApiListResult<MpaBonusInfo>(ErrorCodeEnum.Success, pageInfo, result);
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 72, 1 })]
        public MpaBonusInfoSummary GetInfoDataSummary(GetDataRequest request)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            request.MultiSiteRequest = multiSiteRequest;
            var result = _mpaBonusInfoBll.GetMpaBonusInfoListSummary(request);

            return result;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 72, 2 })]
        public IActionResult ExportInfoData(GetDataRequest request)
        {
            request.PageNumber = -1; //export all
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            request.MultiSiteRequest = multiSiteRequest;
            var data = _mpaBonusInfoBll.GetMpaBonusInfoList(request, out _);

            var stream = _excelBll.Export(data);
            return _excelBll.ExcelFile(stream, "MpaBonusInfoListReport");
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 72, 1 })]
        public ApiListResult<MpaBonusDetail> GetDetailData(GetDetailRequest request)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            request.MultiSiteRequest = multiSiteRequest;
            var result = _mpaBonusInfoBll.GetMpaBonusDetailList(request, out var pageInfo);

            return new ApiListResult<MpaBonusDetail>(ErrorCodeEnum.Success, pageInfo, result);
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 72, 2 })]
        public IActionResult ExportDetailData(GetDetailRequest request)
        {
            request.PageNumber = -1; //export all
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            request.MultiSiteRequest = multiSiteRequest;
            var data = _mpaBonusInfoBll.GetMpaBonusDetailList(request, out _);

            var stream = _excelBll.Export(data);
            return _excelBll.ExcelFile(stream, "MpaBonusDetailListReport");
        }

        #endregion

        #region MpaMarketingBonus

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 1 })]
        public ApiResult GetMpaMarketingBonus(GetMarketingDataRequest request)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            var result = _mpaBonusInfoBll.GetMpaMarketingBonus(multiSiteRequest, request).First();

            return new ApiResult<MpaMarketingBonusInfo>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 1 })]
        public ApiListResult<MpaMarketingBonusDetail> GetMpaMarketingBonusDetail(GetMarketingDetailRequest request)
        {
            var result = _mpaBonusInfoBll.GetMpaMarketingBonusDetail(request, out var pageInfo);

            return new ApiListResult<MpaMarketingBonusDetail>(ErrorCodeEnum.Success, pageInfo, result);
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 2 })]
        public IActionResult Export(GetMarketingDataRequest request)
        {

            #region Get Data

            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var multiSiteRequest = ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            var exportData = _mpaBonusInfoBll.GetMpaMarketingBonus(multiSiteRequest, new GetMarketingDataRequest
            {
                SiteId = request.SiteId,
                StartDate = request.StartDate.DateTime,
                EndDate = request.EndDate.DateTime,
                CurrencyIds = request.CurrencyIds,
                BaseCurrencyId = request.BaseCurrencyId,
                IsCompared = request.IsCompared,
                ComparedStartDate = request.ComparedStartDate,
                ComparedEndDate = request.ComparedEndDate
            }).First();

            var detailResult = _mpaBonusInfoBll.GetMpaMarketingBonusDetail(new GetMarketingDetailRequest
            {
                SiteId = request.SiteId,
                StartDate = request.StartDate.DateTime,
                EndDate = request.EndDate.DateTime,
                CurrencyIds = request.CurrencyIds,
                BaseCurrencyId = request.BaseCurrencyId,
                PageNumber = -1,
                PageSize = 0,
                OrderBy = true,
                OrderByProperty = "BonusCode",
                OrderByDirection = OrderByDirection.Ascending
            }, out _);

            #endregion

            #region Initial Param

            var summaryTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var summaryBonusCodeTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
            var detailTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();

            var summaryTitles = new Dictionary<string, TitleModel>();
            var summaryBonusCodeTitles = new Dictionary<string, TitleModel>();
            var detailTitles = new Dictionary<string, TitleModel>();

            var summaryContent = new List<Dictionary<string, dynamic>>();
            var summaryBonusCodeContent = new List<Dictionary<string, dynamic>>();
            var detailContent = new List<Dictionary<string, dynamic>>();

            #endregion

            #region Set Title(Title & Total Title & Merge Title & Not Summary Title)

            #region Titles To Merge

            var summaryMergeTitles = new Dictionary<string, MergeTitleModel>
            {
                {"FirstMergeTitle", new MergeTitleModel { Name = $"Bonus Amount Claimed, {request.StartDate.ToString("yyyy-MM-dd")} - {request.EndDate.ToString("yyyy-MM-dd")} , View in {Enum.GetName(typeof(CurrencyEnum), request.BaseCurrencyId)}", MergeRange = 4 }}
            };
            summaryTitlesToMerge.Add(summaryMergeTitles);

            var summaryBonusCodeMergeTitles = new Dictionary<string, MergeTitleModel>
            {
                {"FirstMergeTitle", new MergeTitleModel { Name = $"Bonus Amount Claimed, {request.StartDate.ToString("yyyy-MM-dd")} - {request.EndDate.ToString("yyyy-MM-dd")} , View in {Enum.GetName(typeof(CurrencyEnum), request.BaseCurrencyId)}", MergeRange = 6 }}
            };
            summaryBonusCodeTitlesToMerge.Add(summaryBonusCodeMergeTitles);

            var detailMergeTitles = new Dictionary<string, MergeTitleModel>
            {
                {"FirstMergeTitle", new MergeTitleModel { Name = $"Bonus Amount Claimed, {request.StartDate.ToString("yyyy-MM-dd")} - {request.EndDate.ToString("yyyy-MM-dd")} , View in {Enum.GetName(typeof(CurrencyEnum), request.BaseCurrencyId)}", MergeRange = 9 }}
            };
            detailTitlesToMerge.Add(detailMergeTitles);

            #endregion

            #region Title

            summaryTitles.Add("Currency", new TitleModel { Name = "Currency", SortId = 2 });
            summaryTitles.Add("TotalBonusAmountClaimed", new TitleModel { Name = "  Total Bonus Amount Claimed  ", SortId = 1 });
            if (request.IsCompared)
            {
                summaryTitles.Add("CompareTotalBonusAmountClaimed", new TitleModel { Name = "  Compare Total Bonus Amount Claimed  ", SortId = 3 });
                summaryTitles.Add("%Change", new TitleModel { Name = "  % Change  ", SortId = 4 });
            }

            summaryBonusCodeTitles.Add("BonusCode", new TitleModel { Name = "  Bonus Code  ", SortId = 1 });
            summaryBonusCodeTitles.Add("Currency", new TitleModel { Name = "  Currency  ", SortId = 2 });
            summaryBonusCodeTitles.Add("BonusName", new TitleModel { Name = "  Bonus Name  ", SortId = 3 });
            summaryBonusCodeTitles.Add("NoOfUsersClaimingBonus", new TitleModel { Name = "  No. of Users Claiming Bonus  ", SortId = 4 });
            summaryBonusCodeTitles.Add("TotalBonusAmountClaimed_Detail", new TitleModel { Name = "  Total Bonus Amount Claimed  ", SortId = 5 });
            summaryBonusCodeTitles.Add("TotalBonusAmountClaimed_Base", new TitleModel { Name = "  Total Bonus Amount Claimed (Base)  ", SortId = 6 });

            detailTitles.Add("BonusCode", new TitleModel { Name = "Bonus Code  ", SortId = 1 });
            detailTitles.Add("UserName", new TitleModel { Name = "  UserName  ", SortId = 2 });
            detailTitles.Add("Currency", new TitleModel { Name = "  Currency  ", SortId = 3 });
            detailTitles.Add("RegisteredDate", new TitleModel { Name = "  Registered Date  ", SortId = 4 });
            detailTitles.Add("SignupUrl", new TitleModel { Name = "  Signup Url  ", SortId = 5 });
            detailTitles.Add("AffCode", new TitleModel { Name = "  AFF Code  ", SortId = 6 });
            detailTitles.Add("TotalDepositAmount", new TitleModel { Name = "  Total Deposit Amount  ", SortId = 7 });
            detailTitles.Add("TotalWithdrawalAmount", new TitleModel { Name = "  Total Withdrawal Amount  ", SortId = 8 });
            detailTitles.Add("TotalBonusAmountClaimed", new TitleModel { Name = "  Total Bonus Amount Claimed  ", SortId = 9 });

            #endregion

            #region Total Title

            var totalTitle = new List<string>
            {

            };

            #endregion

            #region Not Summary Title

            var notSummaryTitles = new List<string>
            {
                "Key", "Currency","%Change","BonusCode","BonusName",
                "UserName","RegisteredDate","SignupUrl","AffCode","TotalDepositAmount",
                "TotalWithdrawalAmount","TotalBonusAmountClaimed_Detail"
            };

            #endregion

            #endregion

            #region Summary Content Add

            foreach (var item in exportData.GroupByCurrency)
            {

                #region Summary Find Define

                var getSummaryKey = $"{item.CurrencyId}";

                #endregion

                #region Summary Content Add

                var newItem = new Dictionary<string, dynamic>()
                {
                    { "Key", getSummaryKey },
                    { "Currency", item.Currency },
                    { "TotalBonusAmountClaimed", item.BonusAmountClaim }
                };
                if (request.IsCompared)
                {
                    newItem.Add("CompareTotalBonusAmountClaimed", item.ComparedBonusAmountClaim);
                    newItem.Add("%Change", $"{decimal.ToInt32(item.ChangeRate)}%");
                }
                summaryContent.Add(newItem);

                #endregion

            }

            foreach (var item in exportData.GroupByBonusCode)
            {
                var newItem = new Dictionary<string, dynamic>()
                {
                    { "Key", $"{item.GroupInfo.BonusCode}" },
                    { "BonusCode", item.GroupInfo.BonusCode },
                    { "Currency", "ALL" },
                    { "BonusName", item.GroupInfo.BonusName },
                    { "NoOfUsersClaimingBonus", item.GroupInfo.NumberOfBonusUser },
                    { "TotalBonusAmountClaimed_Detail", item.GroupInfo.BonusAmountClaim }
                };
                summaryBonusCodeContent.Add(newItem);

                foreach (var child in item.Children)
                {
                    var childItem = new Dictionary<string, dynamic>()
                    {
                        { "Key", $"{child.BonusCode}-{child.Currency}" },
                        { "BonusCode", child.BonusCode },
                        { "Currency", child.Currency },
                        { "BonusName", child.BonusName },
                        { "NoOfUsersClaimingBonus", child.NumberOfBonusUser },
                        { "TotalBonusAmountClaimed_Detail", child.BonusAmountClaim },
                    };
                    summaryBonusCodeContent.Add(childItem);
                }
            }

            foreach (var item in detailResult)
            {
                var newItem = new Dictionary<string, dynamic>()
                {
                    { "Key", $"{item.BonusCode}-{item.UserName}" },
                    { "BonusCode", item.BonusCode },
                    { "UserName", item.UserName },
                    { "Currency", item.Currency },
                    { "RegisteredDate", item.RegisteredDate.ToString(CultureInfo.InvariantCulture) },
                    { "SignupUrl", item.SignupUrl},
                    { "AffCode", item.AffCode.ToString() },
                    { "TotalDepositAmount", item.TotalDeposit },
                    { "TotalWithdrawalAmount", item.TotalWithdrawal },
                    { "TotalBonusAmountClaimed", item.BonusAmountClaim },
                };
                detailContent.Add(newItem);
            }

            #endregion

            var d = new List<ComplexReportExportDataToExcel>()
            {
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "SummaryData",
                    Content = summaryContent.OrderBy(x => x["Currency"]),
                    Titles = summaryTitles.ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = summaryTitlesToMerge,
                    NotSummaryTitles = notSummaryTitles
                },
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "SummaryBonusCode",
                    Content = summaryBonusCodeContent.OrderBy(x => x["BonusCode"]).ThenBy(x=>x["Currency"]),
                    Titles = summaryBonusCodeTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = summaryBonusCodeTitlesToMerge,
                    NotSummaryTitles = notSummaryTitles
                },
                new ComplexReportExportDataToExcel()
                {
                    SheetName = "DetailData",
                    Content = detailContent.OrderBy(x => x["BonusCode"]).ThenBy(x=>x["UserName"]),
                    Titles = detailTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = detailTitlesToMerge,
                    NotSummaryTitles = notSummaryTitles
                }
            };

            var stream = _reportExcelBll.ComplexReportExportToMultiSheet(d);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"MpaBonusInfoReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion
    }
}