using BackendSite.Attributes;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MySql.Data.MySqlClient;

namespace BackendSite.Controllers.ApiV2.Finance.PaymentSetting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class AutoPaymentRuleController : BaseController
    {
        private readonly IAutoPaymentRuleBLL _autoPaymentRuleBll;

        public AutoPaymentRuleController(IAutoPaymentRuleBLL autoPaymentRuleBll,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _autoPaymentRuleBll = autoPaymentRuleBll;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 89 })]
        public async Task<ApiResult<IEnumerable<AutoPaymentRuleDataListResponse>>> GetDataListAsync(AutoPaymentRuleDataListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<AutoPaymentRuleDataListResponse>>();
            try
            {
                var responseData = await _autoPaymentRuleBll.GetDataListAsync(request);
                apiResult.Data = responseData;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 89 })]
        public async Task<ApiResult> CreateAsync(AutoPaymentRuleCreateRequest request)
        {
            var apiResult = new ApiResult { ErrorCode = (int)ErrorCodeEnum.SystemException };
            try
            {
                await _autoPaymentRuleBll.CreateAutoPaymentRuleAsync(request, AuthData.UserName);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (MySqlException ex) when (ex?.Number == 1062)
            {
                apiResult.Message = "Duplicate Currency Setting";
            }
            catch (Exception ex)
            {
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 89 })]
        public async Task<ApiResult> SetAsync(AutoPaymentRuleSetRequest request)
        {
            var apiResult = new ApiResult();

            try
            {
                await _autoPaymentRuleBll.SetAutoPaymentRuleAsync(request, AuthData.UserName);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 89 })]
        public async Task<ApiResult> DeleteAsync(AutoPaymentRuleDeleteRequest request)
        {
            var apiResult = new ApiResult();

            try
            {
                await _autoPaymentRuleBll.DeleteAutoPaymentRuleAsync(request, AuthData.UserName);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 82 })]
        public ApiResult<IEnumerable<WithdrawalOptionDto>> WithdrawalTypeAsync()
        {
            return new ApiResult<IEnumerable<WithdrawalOptionDto>>
            {
                Data = _autoPaymentRuleBll.GetWithdrawalOption(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
    }
}
