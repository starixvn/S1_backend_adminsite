using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class ReferralReportController : ControllerBase
    {
        #region Fields

        private readonly IReferralManagementBLL _referralManagementBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructors
        public ReferralReportController(IReferralManagementBLL referralManagementBll, ExcelBLL excelBll)
        {
            _referralManagementBll = referralManagementBll;
            _excelBll = excelBll;
        }
        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset? _startDate;
            private DateTimeOffset? _endDate;
            public IEnumerable<int> SiteIds { get; set; }
            public string UserName { get; set; }
            public int CurrencyId { get; set; }
            public DateTimeOffset? StartDate
            {
                get => _startDate;
                set
                {
                    if (value != null)
                        _startDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset? EndDate
            {
                get => _endDate;
                set
                {
                    if (value != null)
                        _endDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
        }

        public class GetByCustIdRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;
            public int CustId { get; set; }
            public DateTimeOffset? StartDate
            {
                get => _startDate;
                set
                {
                    if (value != null)
                        _startDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset? EndDate
            {
                get => _endDate;
                set
                {
                    if (value != null)
                        _endDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
        }

        public class ExportRequest
        {
            private DateTimeOffset? _startDate;
            private DateTimeOffset? _endDate;
            public IEnumerable<int> SiteIds { get; set; }
            public string UserName { get; set; }
            public int CurrencyId { get; set; }
            public DateTimeOffset? StartDate
            {
                get => _startDate;
                set
                {
                    if (value != null)
                        _startDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }

            public DateTimeOffset? EndDate
            {
                get => _endDate;
                set
                {
                    if (value != null)
                        _endDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }

            public int Timezone { get; set; }
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
        [HttpPost]
        public ApiListResult<ReferralCustList> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<ReferralCustList>();

            var pageNumber = request.PageNumber;
            apiResult.Data = _referralManagementBll.GetReferralCustList(request.SiteIds,
                request.UserName,
                request.CurrencyId,
                request.StartDate?.DateTime,
                request.EndDate?.DateTime,
                request.PageSize,
                ref pageNumber, out var totalPages, out var rowCount);
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
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
        [HttpPost]
        public ApiListResult<ReferralFriendRresponse> GetByCustId(GetByCustIdRequest request)
        {
            var apiResult = new ApiListResult<ReferralFriendRresponse>();
            var startDate = request.StartDate?.DateTime;
            var endDate = request.EndDate?.DateTime;

            try
            {
                var data = _referralManagementBll.GetRefFriendList(
                    request.CustId, startDate, endDate, request.PageSize, request.PageNumber, 
                    out var totalPages, out var rowCount);
                apiResult.Data = data;
                apiResult.PageNumber = request.PageNumber;
                apiResult.TotalPages = totalPages;
                apiResult.RowCount = rowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }

        /// <summary>
        /// ReferralReport/Export 匯出推薦朋友資料報表清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Referral Report > [Submit] > [Export] 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Export(ExportRequest request)
        {
            var exportData = _referralManagementBll.GetReferralCustListExport(
                JsonConvert.SerializeObject(request.SiteIds),
                request.UserName,
                request.CurrencyId,
                request.StartDate?.ToUniversalTime().DateTime,
                request.EndDate?.ToUniversalTime().DateTime,
                request.Timezone);

            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "ReferralReport");
        }
    }
}