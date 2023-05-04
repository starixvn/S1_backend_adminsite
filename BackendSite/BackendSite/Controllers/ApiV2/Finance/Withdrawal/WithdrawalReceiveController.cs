using BackendSite.Attributes;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.Withdrawal
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalReceiveController : BaseController
    {
        public class WithdrawalReceiveReq
        {
            public string TransId { get; set; }
        }

        private readonly IWithdrawalReceiveBLL _withdrawalReceiveBll;

        public WithdrawalReceiveController(IWithdrawalReceiveBLL withdrawalReceiveBll,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _withdrawalReceiveBll = withdrawalReceiveBll;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 3 })]
        public async Task<ApiResult> LockWithdrawalAsync(WithdrawalReceiveReq req)
        {
            var apiResult = new ApiResult();

            try
            {
                var lockResult = await _withdrawalReceiveBll.LockWithdrawalTicketAsync(req.TransId, AuthData.UserName, AuthData.SiteId);
                if (lockResult.Success)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = $"Withdrawal TransId Has Been {lockResult.Value} Lock";
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 3 })]
        public async Task<ApiResult> UnLockWithdrawalAsync(WithdrawalReceiveReq req)
        {
            var apiResult = new ApiResult();

            try
            {
                var _ = await _withdrawalReceiveBll.UnLockWithdrawalTicketAsync(req.TransId, AuthData.SiteId, AuthData.UserName);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 83 })]
        public async Task<ApiResult> SetLockWithdrawalUserAsync(WithdrawalReceiveReq req)
        {
            var apiResult = new ApiResult();

            try
            {
                var setLockUserResult = await _withdrawalReceiveBll.OverrideLockWithdrawalTicketUserAsync(req.TransId, AuthData.UserName, AuthData.SiteId);
                if (setLockUserResult)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                }

            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public async Task<ApiResult> LockWithdrawalExistsAsync(WithdrawalReceiveReq req)
        {
            var apiResult = new ApiResult();
            try
            {
                var exists = await _withdrawalReceiveBll.LockWithdrawalTicketExistsAsync(req.TransId);
                apiResult.ErrorCode = exists
                    ? (int)ErrorCodeEnum.Fail
                    : (int)ErrorCodeEnum.Success;
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
