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
    public class AutoWithdrawalRuleController : BaseController
    {
        private readonly IAutoWithdrawalRuleBLL _autoWithdrawalRuleBll;

        public AutoWithdrawalRuleController(IAutoWithdrawalRuleBLL autoWithdrawalRuleBll,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _autoWithdrawalRuleBll = autoWithdrawalRuleBll;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 82 })]
        public async Task<ApiResult<IEnumerable<AutoWithdrawalRuleDataListResponse>>> GetDataListAsync(AutoWithdrawalRuleDataListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<AutoWithdrawalRuleDataListResponse>>();
            try
            {
                var responseData = await _autoWithdrawalRuleBll.GetDataListAsync(request);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 82 })]
        public async Task<ApiResult> CreateAsync(AutoWithdrawalRuleCreateRequest request)
        {
            var apiResult = new ApiResult() { ErrorCode = (int)ErrorCodeEnum.SystemException };
            try
            {
                await _autoWithdrawalRuleBll.CreateAutoWithdrawalRuleAsync(request, AuthData.UserName);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 82 })]
        public async Task<ApiResult> SetAsync(AutoWithdrawalRuleSetRequest request)
        {
            var apiResult = new ApiResult();

            try
            {
                await _autoWithdrawalRuleBll.SetAutoWithdrawalRuleAsync(request, AuthData.UserName);
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
        public async Task<ApiResult> DeleteAsync(AutoWithdrawalRuleDeleteRequest request)
        {
            var apiResult = new ApiResult();

            try
            {
                await _autoWithdrawalRuleBll.DeleteAutoWithdrawalRuleAsync(request, AuthData.UserName);
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
                Data = _autoWithdrawalRuleBll.GetWithdrawalOption(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
    }
}
