using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositReferralReportController : ControllerBase
    {
        #region Fields

        private readonly DepositReferralReportBLL _depositReferralReportBll;

        #endregion

        #region Constructors
        public DepositReferralReportController(DepositReferralReportBLL depositReferralReportBll)
        {
            _depositReferralReportBll = depositReferralReportBll;
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
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            var data = await _depositReferralReportBll.GetList(request.SiteId,
                request.CurrencyIds,
                startTime,
                endTime,
                request.UserName,
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
            var apiResult = new ApiListResult<DepositReferralReport_ReferralReferredDao>();

            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;

            return new ApiResult<IEnumerable<DepositReferralReport_ReferralReferredDao>>()
            {
                Data = await _depositReferralReportBll.GetDetailByCustId(request.CustId, startTime, endTime),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
    }
}
