using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositReferralReportController : ControllerBase
    {
        #region Fields

        private readonly DepositReferralReportBLL _depositReferralReportBll;
        private readonly ExcelBLL _excelBll;
        private readonly CustInfoBLL _custInfoBll;

        #endregion

        #region Constructors
        public DepositReferralReportController(DepositReferralReportBLL depositReferralReportBll,
            ExcelBLL excelBll,
            CustInfoBLL custInfoBll)
        {
            _depositReferralReportBll = depositReferralReportBll;
            _excelBll = excelBll;
            _custInfoBll = custInfoBll;
        }
        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public string UserName { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
        }

        public class GetDetailByCustIdRequest
        {
            public int CustId { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
        }

        public class ExportRequest
        {
            public int SiteId { get; set; }
            public string UserName { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
        }

        #endregion

        /// <summary>
        /// ReferralReport/GetList 取得推薦朋友資料報表清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Referral Report > [Submit] 
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 147, 1 })]
        [HttpPost]
        public async Task<ApiListResult<DepositReferralReportDao>> GetList(GetListRequest request)
        {
            var custId = 0;
            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                var custInfo = await _custInfoBll.GetCustInfosAsync(request.SiteId, new List<string>() { request.UserName });
                if (custInfo.Any())
                    custId = custInfo.First().CustId;
                else
                    custId = -1;
            }
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            var data = await _depositReferralReportBll.GetList(request.SiteId,
                request.CurrencyIds,
                startTime,
                endTime,
                custId,
                request.PageNumber,
                request.PageSize);

            return new ApiListResult<DepositReferralReportDao>
            {
                Data = data.Data,
                TotalPages = data.TotalPages,
                PageNumber = request.PageNumber,
                RowCount = data.RowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// ReferralReport/GetByCustId 取得指定客戶推薦朋友資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Referral Report > [Submit] > [Detail] 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 147, 1 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<DepositReferralReport_ReferralReferredDao>>> GetDetailByCustId(GetDetailByCustIdRequest request)
        {
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;

            return new ApiResult<IEnumerable<DepositReferralReport_ReferralReferredDao>>()
            {
                Data = await _depositReferralReportBll.GetDetailByCustId(new int[] { request.CustId }, startTime, endTime),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 147, 2 })]
        public async Task<IActionResult> Export(ExportRequest request)
        {
            var custId = 0;
            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                var custInfo = await _custInfoBll.GetCustInfosAsync(request.SiteId, new List<string>() { request.UserName });
                if (custInfo.Any())
                    custId = custInfo.First().CustId;
                else
                    custId = -1;
            }
            var timespan = request.StartDate.Offset;
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            var result = await _depositReferralReportBll.Export(request.SiteId,
                request.CurrencyIds,
                startTime,
                endTime,
                custId,
                timespan);

            var stream = _excelBll.ExportV2(
                new Dictionary<string, IEnumerable<DepositReferralReportForExportDao>>() { { "Referrer", result.ReferrerResult } },
                new Dictionary<string, IEnumerable<DepositReferralReport_ReferralReferredForExportDao>>() { { "Referred", result.ReferredResult } });

            return _excelBll.ExcelFile(stream, "NewReferralReport");
        }
    }
}
