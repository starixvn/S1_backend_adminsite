using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralReport" })]
    public class ReferralReportController : ControllerBase
    {
        #region Fields

        private readonly ReferralManagementBLL _referralManagementBll;

        #endregion

        #region Constructors

        public ReferralReportController(IServiceProvider provider)
        {
            _referralManagementBll = provider.GetRequiredService<ReferralManagementBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }
            public string UserName { get; set; }
            public int CurrencyId { get; set; }
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
                null,
                null,
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
        public ApiListResult<ReferralFriend> GetByCustId(GetByCustIdRequest request)
        {
            var apiResult = new ApiListResult<ReferralFriend>();
            var referralStatus = new Dictionary<int, string>()
            {
                {0,"Approved" },
                {1,"Running" },
                {2,"Expired" }
            };
            var startDate = request.StartDate != new DateTimeOffset() ? request.StartDate?.DateTime : null;
            var endDate = request.EndDate != new DateTimeOffset() ? request.EndDate?.DateTime : null;

            try
            {
                var pageNumber = request.PageNumber;

                var data = _referralManagementBll.GetRefFriendList(request.CustId,
                    startDate,
                    endDate,
                    request.PageSize,
                    request.PageNumber,
                    out var totalPages,
                    out var rowCount,true);

                foreach (var x in data)
                {
                    referralStatus.TryGetValue(x.Status, out var name);
                    x.StatusName = name;
                }
                apiResult.Data = data;
                apiResult.PageNumber = pageNumber;
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
    }
}