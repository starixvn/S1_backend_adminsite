using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class RebateReportController : ControllerBase
    {
        #region Fields

        private readonly PaymentBLL _paymentBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructors

        public RebateReportController(IServiceProvider provider)
        {
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class GetListForVipRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
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
            public string UserName { get; set; }
            public IEnumerable<int> CategoryIds { get; set; }
            public IEnumerable<VipRebatePayoutStatusEnum> PayoutStatuses { get; set; }
            public int BaseCurrencyId { get; set; }
        }

        public class ExportForVipRequest
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
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
            public string UserName { get; set; }
            public IEnumerable<int> CategoryIds { get; set; }
            public IEnumerable<VipRebatePayoutStatusEnum> PayoutStatuses { get; set; }
            public int BaseCurrencyId { get; set; }
        }

        public class GetVipRebateDetailRequest
        {
            public int CustId { get; set; }
            public int TpCategoryId { get; set; }
            public string RefId { get; set; }
        }

        #endregion

        /// <summary>
        /// RebateReport/GetListForVip 取得VIP返水資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Marketing > Vip Rebate Report > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 75, 1 })]
        [HttpPost]
        public ApiResult<VipRebateReportResult> GetListForVip(GetListForVipRequest request)
        {
            var apiResult = new ApiResult<VipRebateReportResult>();
            try
            {
                var currencyAmount = new List<string>();
                var searchResult = _paymentBll.GetVipRebateInfoList(request.SiteIds,
                    request.CurrencyIds,
                    request.StartDate.DateTime,
                    request.EndDate.DateTime,
                    string.IsNullOrWhiteSpace(request.UserName) ? null : request.UserName,
                    request.CategoryIds, request.BaseCurrencyId,
                    request.PayoutStatuses,
                    request.PageSize, request.PageNumber, ref currencyAmount);

                foreach (var data in searchResult.VipRebateInfoList)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                }

                apiResult.Data = searchResult;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// RebateReport/ExportForVip 匯出VIP返水資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Marketing > Vip Rebate Report > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 75, 2 })]
        [HttpPost]
        public IActionResult ExportForVip(ExportForVipRequest request)
        {
            var searchResult = _paymentBll.GetVipRebateInfoListExport(request.SiteIds,
                    request.CurrencyIds,
                    request.StartDate.DateTime,
                    request.EndDate.DateTime,
                    string.IsNullOrWhiteSpace(request.UserName) ? null : request.UserName,
                    request.CategoryIds, request.BaseCurrencyId,request.PayoutStatuses);

            foreach (var data in searchResult.VipRebateInfoList)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            }

            var stream = _excelBll.ExportV2(searchResult.VipRebateInfoList);
            return _excelBll.ExcelFile(stream, "VipRebateReport");
        }

        /// <summary>
        /// RebateReport/GetVipRebateDetail 取得指定客戶VIP回扣資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Rebate Report > [Submit] > [Detail]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 75, 1 })]
        [HttpPost]
        public ApiResult<IEnumerable<VipRebateDetail>> GetVipRebateDetail(GetVipRebateDetailRequest request)
        {
            return new ApiResult<IEnumerable<VipRebateDetail>>
            {
                Data = _paymentBll.GetVipRebateDetail(request.CustId, request.TpCategoryId, request.RefId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

    }
}