using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class ReferralController : ControllerBase
    {
        private readonly ReferralManagementDecorator _managementDecorator;
        public ReferralController(ReferralManagementDecorator managementDecorator)
        {
            _managementDecorator = managementDecorator;
        }
        
        #region API Model

        public class GetListRequest
        {
            public int SiteId { get; set; }
        }
        
        public class GetRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        public class SetStatusRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }

            public ReferralStatusEnum Status { get; set; }
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
                Data = _managementDecorator.GetReferralBySiteId(request.SiteId)
            };
            return apiResult;
        }
        
        /// <summary>
        /// ReferralManagement/Get 取得推薦資料內容
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Referral Management > [Search]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        [HttpPost]
        public async Task<ApiResult<ReferralDetail>> Get(GetRequest request)
        {
            var apiResult = new ApiResult<ReferralDetail>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = await _managementDecorator.GetReferralDetail(request.SiteId, request.CurrencyId)
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
        public async Task<ApiResult> UpdateStatus(SetStatusRequest request)
        {
            _managementDecorator.Operator = User.Identity.Name;
            var result = await _managementDecorator.SetReferralStatusAsync(request.SiteId, request.CurrencyId, (int)request.Status);
            var apiResult = new ApiResult
            {
                ErrorCode = result > 0 ? (int)ErrorCodeEnum.Success : result
            };
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
        /// <param name="setReferral"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        [HttpPost]
        public async Task<ApiResult> Set(SetReferralRequest setReferral)
        {
            _managementDecorator.Operator = User.Identity.Name;
            var result = await _managementDecorator.SetReferralAsync(setReferral);
            var apiResult = new ApiResult
            {
                ErrorCode = (result == 1 || result == 2) ? (int)ErrorCodeEnum.Success : result
            };
            return apiResult;
        }

        #endregion
    }
}