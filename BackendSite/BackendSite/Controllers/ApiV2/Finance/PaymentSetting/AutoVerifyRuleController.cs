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
    public class AutoVerifyRuleController : BaseController
    {
        private readonly IAutoVerifyRuleBLL _autoVerifyRuleBll;

        public AutoVerifyRuleController(IAutoVerifyRuleBLL autoVerifyRuleBll,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _autoVerifyRuleBll = autoVerifyRuleBll;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 89 })]
        public async Task<ApiResult<IEnumerable<AutoVerifyRuleDataListResponse>>> GetDataListAsync(AutoVerifyRuleDataListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<AutoVerifyRuleDataListResponse>>();
            try
            {
                var responseData = await _autoVerifyRuleBll.GetDataListAsync(request);
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
        public async Task<ApiResult> CreateAsync(AutoVerifyRuleCreateRequest request)
        {
            var apiResult = new ApiResult { ErrorCode = (int)ErrorCodeEnum.SystemException };
            try
            {
                await _autoVerifyRuleBll.CreateAutoVerifyRuleAsync(request, AuthData.UserName);
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
        public async Task<ApiResult> SetAsync(AutoVerifyRuleSetRequest request)
        {
            var apiResult = new ApiResult();

            try
            {
                await _autoVerifyRuleBll.SetAutoVerifyRuleAsync(request, AuthData.UserName);
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
        public async Task<ApiResult> DeleteAsync(AutoVerifyRuleDeleteRequest request)
        {
            var apiResult = new ApiResult();

            try
            {
                await _autoVerifyRuleBll.DeleteAutoVerifyRuleAsync(request, AuthData.UserName);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
    }
}
