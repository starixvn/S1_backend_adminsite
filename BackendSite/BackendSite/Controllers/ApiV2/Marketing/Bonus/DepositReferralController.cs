using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositReferralController : ControllerBase
    {
        private readonly IReferralManagementBLL _managementDecorator;
        public DepositReferralController(IReferralManagementBLL managementDecorator)
        {
            _managementDecorator = managementDecorator;
        }

        /// <summary>
        /// ReferralManagement/GetList 取得推薦資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Referral Management > [Search]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 148, 1 })]
        [HttpPost]
        public ApiResult<IEnumerable<NewReferralManagementDto>> GetList(GetReferralListRequest request)
        {
            var data = _managementDecorator.GetReferralBySiteId(request.SiteId);
            var apiResult = new ApiResult<IEnumerable<NewReferralManagementDto>>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data?.Select(x => new NewReferralManagementDto(x))
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 148, 1 })]
        [HttpPost]
        public async Task<ApiResult<NewReferralDetailDto>> Get(GetReferralRequest request)
        {
            var data = await _managementDecorator.GetReferralDetail(request.SiteId, request.CurrencyId);
            data.SpecificList = data.SpecificList.Where(x => x.Type == ReferralTypeEnum.DepositReferral);
            var apiResult = new ApiResult<NewReferralDetailDto>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data != null ? new NewReferralDetailDto(data) : null
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 148, 12 })]
        [HttpPost]
        public async Task<ApiResult> UpdateStatus(SetReferralStatusRequest request)
        {
            _managementDecorator.Operator = User.Identity.Name;
            var result = await _managementDecorator.SetReferralStatusAsync(request.SiteId, request.CurrencyId, request.Status);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 148, 3 })]
        [HttpPost]
        public async Task<ApiResult> Set(SetNewReferralRequest setReferral)
        {
            _managementDecorator.Operator = User.Identity.Name;
            var result = await _managementDecorator.SetReferralAsync(ReferralTypeEnum.DepositReferral, new SetReferralRequest(setReferral));
            var apiResult = new ApiResult
            {
                ErrorCode = (result == 1 || result == 2) ? (int)ErrorCodeEnum.Success : result
            };
            return apiResult;
        }

    }
}
