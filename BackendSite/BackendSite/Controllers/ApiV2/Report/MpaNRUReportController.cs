using System.Text;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BackendSite.Attributes;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MpaNRUReportController : ControllerBase
    {
        #region Fields
        
        private readonly MpaNRUBLL _mpaNRUBll;
        private readonly ExcelBLL _excelBll;
        private readonly ReportExcelBLL _reportExcelBll;

        #endregion

        #region Constructor

        public MpaNRUReportController(IServiceProvider provider)
        {
            _mpaNRUBll = provider.GetRequiredService<MpaNRUBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
        }

        #endregion

        #region API Model

        public class IsComparedRequest
        {
            private DateTimeOffset _comparedStartDate { get; set; }
            private DateTimeOffset _comparedendDate { get; set; }
            [JsonProperty("CompareFromDate")]
            public DateTimeOffset ComparedStartDate
            {
                get => _comparedStartDate;
                set => _comparedStartDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            [JsonProperty("CompareToDate")]
            public DateTimeOffset ComparedEndDate
            {
                get => _comparedendDate;
                set => _comparedendDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public bool IsCompared { get; set; }
        }
        
        public class GetMarketingNRUDataRequest : IsComparedRequest
        {
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
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
        }

        public class GetMarketingNRUBonusDetailRequest : IOrderBy
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
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 1 })]
        public ApiResult GetMpaNRUReport(GetMarketingNRUDataRequest request)
        {
            var result = _mpaNRUBll.GetMpaMarketingNRU(request);
            
            return new ApiResult<NRUReportData>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 1 })]
        public ApiResult GetMpaNRUBonusReport(GetMarketingNRUDataRequest request)
        {
            var result = _mpaNRUBll.GetMpaMarketingNRUBonus(request);
            
            return new ApiResult<List<MpaMarketingNRUBonus>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 1 })]
        public ApiListResult<MpaMarketingNRUBonusDetail> GetMpaNRUBonusDetailReport(GetMarketingNRUBonusDetailRequest request)
        {
            var result = _mpaNRUBll.GetMpaMarketingNRUBonusDetail(request, out var pageInfo);
            
            return new ApiListResult<MpaMarketingNRUBonusDetail>(ErrorCodeEnum.Success, pageInfo, result);
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 140, 2 })]
        public IActionResult Export(GetMarketingNRUDataRequest request)
        {
            #region Get Data
            
            var sumResult = _mpaNRUBll.GetMpaMarketingNRU(request);
            
            var bonusResult = _mpaNRUBll.GetMpaMarketingNRUBonus(request);

            var bonusDetailRequest = new GetMarketingNRUBonusDetailRequest
            {
                SiteId = request.SiteId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CurrencyIds = request.CurrencyIds,
                BaseCurrencyId = request.BaseCurrencyId,
                PageNumber = -1,
                OrderBy = false,
                OrderByProperty = "UserName",
                OrderByDirection = OrderByDirection.Ascending
            };
            var bonusDetailResult = _mpaNRUBll.GetMpaMarketingNRUBonusDetail(bonusDetailRequest, out _);
            
            #endregion

            #region Initial Param
            
            var summaryTitlesToMerge = new List<Dictionary<string, MergeTitleModel>>();
           
            var summaryTitles = new Dictionary<string, Api.ReportController.TitleModel>();
            var summarySignupUrlTitles = new Dictionary<string, Api.ReportController.TitleModel>();
            var summaryBonusCodeTitles = new Dictionary<string, Api.ReportController.TitleModel>();
            var summaryBonusCodeDetailTitles = new Dictionary<string, Api.ReportController.TitleModel>();

            var summaryContent = new List<Dictionary<string, dynamic>>();
            var summarySignupUrlContent = new List<Dictionary<string, dynamic>>();
            var summaryBonusCodeContent = new List<Dictionary<string, dynamic>>();
            var summaryBonusCodeDetailContent = new List<Dictionary<string, dynamic>>();

            #endregion

            #region Set Title(Title & Total Title & Merge Title & Not Summary Title)

            #region Titles To Merge
            
            var mergeTitles = new Dictionary<string, MergeTitleModel>
            {
                {"FirstMergeTitle", new MergeTitleModel { Name = $"Newly Registered Users, {request.StartDate.ToString("yyyy-MM-dd")} - {request.EndDate.ToString("yyyy-MM-dd")} , View in {Enum.GetName(typeof(CurrencyEnum), request.BaseCurrencyId)}", MergeRange = 9 }}
            };
            summaryTitlesToMerge.Add(mergeTitles);

            #endregion

            #region Title
            
            summaryTitles.Add("Total", new Api.ReportController.TitleModel { Name = " Total ", SortId = 1 });
            summaryTitles.Add("SumNRU", new Api.ReportController.TitleModel { Name = " NRU ", SortId = 2 });
            if (request.IsCompared)
            {
                summaryTitles.Add("CompareNRU", new Api.ReportController.TitleModel { Name = " Compare NRU ", SortId = 3 });
                summaryTitles.Add("%Change", new Api.ReportController.TitleModel { Name = "  % Change  ", SortId = 4 });
            }
           
            summarySignupUrlTitles.Add("SignupUrl", new Api.ReportController.TitleModel { Name = " Signup Url ", SortId = 1 });
            summarySignupUrlTitles.Add("NRU", new Api.ReportController.TitleModel { Name = " NRU ", SortId = 2 });
            summarySignupUrlTitles.Add("Deposit", new Api.ReportController.TitleModel { Name = " Deposit ", SortId = 3 });
            summarySignupUrlTitles.Add("FirstDepositAmount", new Api.ReportController.TitleModel { Name = "First Deposit Amount", SortId = 4 });
            summarySignupUrlTitles.Add("TotalDepositAmount", new Api.ReportController.TitleModel { Name = "Total Deposit Amount", SortId = 5 });

            summaryBonusCodeTitles.Add("BonusCode", new Api.ReportController.TitleModel { Name = "Bonus Code ", SortId = 1 });
            summaryBonusCodeTitles.Add("BonusName", new Api.ReportController.TitleModel { Name = " Bonus Name ", SortId = 2 });
            summaryBonusCodeTitles.Add("NRU", new Api.ReportController.TitleModel { Name = "  NRU  ", SortId = 3 });
            summaryBonusCodeTitles.Add("Deposit", new Api.ReportController.TitleModel { Name = "Deposit", SortId = 4 });
            summaryBonusCodeTitles.Add("FirstDepositAmount", new Api.ReportController.TitleModel { Name = " First Deposit Amount ", SortId = 5 });
            summaryBonusCodeTitles.Add("TotalDepositAmount", new Api.ReportController.TitleModel { Name = " Total Deposit Amount ", SortId = 6 });
            
            summaryBonusCodeDetailTitles.Add("BonusCode", new Api.ReportController.TitleModel { Name = " Bonus Code ", SortId = 1 });
            summaryBonusCodeDetailTitles.Add("UserName", new Api.ReportController.TitleModel { Name = " User Name ", SortId = 2 });
            summaryBonusCodeDetailTitles.Add("Currency", new Api.ReportController.TitleModel { Name = " Currency ", SortId = 3 });
            summaryBonusCodeDetailTitles.Add("RegisteredDate", new Api.ReportController.TitleModel { Name = " Registered Date ", SortId = 4 });
            summaryBonusCodeDetailTitles.Add("SignupUrl", new Api.ReportController.TitleModel { Name = " Signup Url ", SortId = 5 });
            summaryBonusCodeDetailTitles.Add("AffCode", new Api.ReportController.TitleModel { Name = " AFF Code ", SortId = 6 });
            summaryBonusCodeDetailTitles.Add("Deposit", new Api.ReportController.TitleModel { Name = "Deposit", SortId = 7 });
            summaryBonusCodeDetailTitles.Add("FirstDepositAmount", new Api.ReportController.TitleModel { Name = " First Deposit Amount ", SortId = 8 });
            summaryBonusCodeDetailTitles.Add("TotalDepositAmount", new Api.ReportController.TitleModel { Name = " Total Deposit Amount ", SortId = 9 });

            #endregion

            #region Total Title

            var totalTitle = new List<string>
            {
                
            };
            
            #endregion
            
            #region Not Summary Title

            var notSummaryTitles = new List<string>
            {
                "Key","SignupUrl"
            };

            #endregion

            #endregion

            #region Summary Content Add
            
            var newItem = new Dictionary<string, dynamic>()
            {
                { "Total", "NRU" },
                { "SumNRU", sumResult.SummaryData.NewlyRegisterUsers }
            };
            if (request.IsCompared)
            {
                newItem.Add("CompareNRU", sumResult.SummaryData.CompareNewlyRegisterUsers);
                newItem.Add("%Change", $"{decimal.ToInt32(sumResult.SummaryData.ChangeRate)}%");
            }
            summaryContent.Add(newItem);
            
            foreach (var item in sumResult.SignUpDataSum)
            {
                if (item.Children != null)
                {
                    foreach (var child in item.Children)
                    {
                        var childItem = new Dictionary<string, dynamic>()
                        {
                            { "Key", $"{child.SignUpSource}" },
                            { "SignupUrl", child.SignUpSource },
                            { "NRU", child.NewlyRegisterUsers },
                            { "Deposit", child.DepositUsers },
                            { "FirstDepositAmount", child.FirstDepositAmount },
                            { "TotalDepositAmount", child.TotalDepositAmount }
                        };
                        summarySignupUrlContent.Add(childItem);
                    }
                }
                else
                {
                    var newItem2 = new Dictionary<string, dynamic>()
                    {
                        { "Key", $"{item.SignUpSource}" },
                        { "SignupUrl", item.SignUpSource },
                        { "NRU", item.NewlyRegisterUsers },
                        { "Deposit", item.DepositUsers },
                        { "FirstDepositAmount", item.FirstDepositAmount },
                        { "TotalDepositAmount", item.TotalDepositAmount }
                    };
                    summarySignupUrlContent.Add(newItem2);
                }
            }
            
            foreach (var item in bonusResult)
            {
                var newItem3 = new Dictionary<string, dynamic>()
                {
                    { "Key", item.BonusCode },
                    { "BonusCode", item.BonusCode },
                    { "BonusName", item.BonusName },
                    { "NRU", item.NRUCount },
                    { "Deposit", item.DepositCount },
                    { "FirstDepositAmount", item.FirstDeposit },
                    { "TotalDepositAmount", item.TotalDeposit }
                };
                summaryBonusCodeContent.Add(newItem3);
            }
            
            foreach (var item in bonusDetailResult)
            {
                var newItem4 = new Dictionary<string, dynamic>()
                {
                    { "Key", $"{item.BonusCode}-{item.UserName}" },
                    { "BonusCode", item.BonusCode },
                    { "UserName", item.UserName },
                    { "Currency", item.Currency },
                    { "RegisteredDate", item.RegTime.ToString(CultureInfo.InvariantCulture) },
                    { "SignupUrl", item.SignupUrl },
                    { "AffCode", item.AffCode },
                    { "Deposit", item.IsDeposit? "YES" : "NO" },
                    { "FirstDepositAmount", item.FirstDeposit },
                    { "TotalDepositAmount", item.TotalDeposit }
                };
                summaryBonusCodeDetailContent.Add(newItem4);
            }
            
            #endregion
            
            var d = new List<ComplexReportExportDataToExcel>()
            {
                new()
                {
                    SheetName = "SummaryData",
                    Content = summaryContent,
                    Titles = summaryTitles.ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = summaryTitlesToMerge,
                    IsSummary = false
                },
                new()
                {
                    SheetName = "SummarySignupUrlData",
                    Content = summarySignupUrlContent.OrderBy(x => x["SignupUrl"]),
                    Titles = summarySignupUrlTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = summaryTitlesToMerge,
                    NotSummaryTitles = notSummaryTitles
                },
                new()
                {
                    SheetName = "SummaryBonusCodeData",
                    Content = summaryBonusCodeContent.OrderBy(x => x["BonusCode"]),
                    Titles = summaryBonusCodeTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = summaryTitlesToMerge,
                    IsSummary = false
                },
                new()
                {
                    SheetName = "SummaryBonusCodeDetailData",
                    Content = summaryBonusCodeDetailContent.OrderBy(x => x["BonusCode"]),
                    Titles = summaryBonusCodeDetailTitles.OrderBy(x=> x.Value.SortId).ToDictionary(x=>x.Key,x=>x.Value.Name),
                    TotalTitles = totalTitle,
                    TitlesToMerge = summaryTitlesToMerge,
                    IsSummary = false
                }
            };

            var stream = _reportExcelBll.ComplexReportExportToMultiSheet(d);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"MpaNRUInfoReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

    }
}