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

        #endregion

        #region Constructor

        public MpaBonusInfoReportController(IServiceProvider provider)
        {
            _mpaBonusInfoBll = provider.GetRequiredService<MpaBonusInfoBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class GetDataRequest : IOrderBy
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
            public IEnumerable<int> BonusTypes { get; set; }
            public IEnumerable<int> BonusStatus { get; set; }
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
            public IEnumerable<int> ClaimStatuses { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            public bool IsGroupByDate { get; set; }
            
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
        
        public class GetDetailRequest : IOrderBy
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public int SiteId { get; set; }
            public string BonusCode { get; set; }
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
            public IEnumerable<int> ClaimStatuses { get; set; }
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

        /// <summary>
        /// MpaBonusInfo/GetData
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiListResult<MpaBonusInfo> GetInfoData(GetDataRequest request)
        {
            var result = _mpaBonusInfoBll.GetMpaBonusInfoList(request, out var pageInfo);
            
            return new ApiListResult<MpaBonusInfo>(ErrorCodeEnum.Success, pageInfo, result);
        }
        
        [HttpPost]
        public IActionResult ExportInfoData(GetDataRequest request)
        {
            request.PageNumber = -1; //export all
            var data = _mpaBonusInfoBll.GetMpaBonusInfoList(request, out _);

            var stream = _excelBll.Export(data);
            return _excelBll.ExcelFile(stream, "MpaBonusInfoListReport");
        }
        
        /// <summary>
        /// MpaBonusInfo/GetDetail
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiListResult<MpaBonusDetail> GetDetailData(GetDetailRequest request)
        {
            var result = _mpaBonusInfoBll.GetMpaBonusDetailList(request, out var pageInfo);
            
            return new ApiListResult<MpaBonusDetail>(ErrorCodeEnum.Success, pageInfo, result);
        }
        
        [HttpPost]
        public IActionResult ExportDetailData(GetDetailRequest request)
        {
            request.PageNumber = -1; //export all
            var data = _mpaBonusInfoBll.GetMpaBonusDetailList(request, out _);

            var stream = _excelBll.Export(data);
            return _excelBll.ExcelFile(stream, "MpaBonusDetailListReport");
        }
    }
}