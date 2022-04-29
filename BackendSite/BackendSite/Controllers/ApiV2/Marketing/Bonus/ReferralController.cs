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

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class ReferralController : ControllerBase
    {
        #region Fields

        private readonly ServerInfoBLL _serverInfoBll;
        private readonly ReferralManagementBLL _referralManagementBll;
        private readonly ReportBLL _reportBll;

        #endregion

        #region Constructors

        public ReferralController(IServiceProvider provider)
        {
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _referralManagementBll = provider.GetRequiredService<ReferralManagementBLL>();
            _reportBll = provider.GetRequiredService<ReportBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest
        {
            public int SiteId { get; set; }
        }

        public class SetStatusRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }

            public ReferralStatusEnum Status { get; set; }
        }

        public class SetRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int PeriodDays { get; set; }
            public decimal BonusAmount { get; set; }
            public decimal VaildBet { get; set; }
            public decimal Deposit { get; set; }
        }

        #endregion

        #region Action

        /// <summary>
        /// ReferralManagement/GetList 取得推薦資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Referral Management > [Search]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        [HttpPost]
        public ApiResult<IEnumerable<ReferralList>> GetList(GetListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<ReferralList>>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = _referralManagementBll.GetReferralBySiteId(request.SiteId)
            };
            return apiResult;

        }

        /// <summary>
        /// ReferralManagement/UpdateStatus 更新推薦資料狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Referral Management > [Search] > [Status]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        [HttpPost]
        public ApiResult UpdateStatus(SetStatusRequest request)
        {
            var apiResult = new ApiResult();

            var referralList = _referralManagementBll.GetReferralBySiteId(request.SiteId).ToList();
            var referral = referralList.FirstOrDefault(x => x.CurrencyId == request.CurrencyId);

            if (referral == null)
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }

            _referralManagementBll.SetReferralStatus(request.SiteId, request.CurrencyId, (int)request.Status);

            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// ReferralManagement/Set 新增或更新推薦資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Referral Management > [Search] > [Edit] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        [HttpPost]
        public ApiResult Set(SetRequest request)
        {
            var apiResult = new ApiResult();
            var referral = new ReferralManagement()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                PeriodDays = request.PeriodDays.ToString(),
                BonusAmount = request.BonusAmount.ToString(),
                VaildBet = request.BonusAmount.ToString(),
                Deposit = request.Deposit.ToString()
            };
            var result = _referralManagementBll.SetReferral(referral);

            apiResult.ErrorCode = (result == 1 || result == 2) ? (int)ErrorCodeEnum.Success : result;
            return apiResult;
        }

        #endregion
    }
}