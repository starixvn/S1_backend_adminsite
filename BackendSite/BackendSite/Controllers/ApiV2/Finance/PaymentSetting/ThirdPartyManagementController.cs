using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.PaymentSetting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class ThirdPartyManagementController : ControllerBase
    {
        #region Fields

        private readonly PaymentManageBLL _paymentManageBll;

        #endregion

        #region Constructors

        public ThirdPartyManagementController(IServiceProvider provider)
        {
            _paymentManageBll = provider.GetRequiredService<PaymentManageBLL>();
        }

        #endregion

        #region API Model

        public class GetBankListByPayIdRequest
        {
            public int SiteId { get; set; }
            public int PayId { get; set; }
        }
        
        public class GetPayListRequest
        {
            public int SiteId { get; set; }
        }
        
        public class SetPayListStatusRequest
        {
            public int SiteId { get; set; }
            public int PayId { get; set; }
            public PayListStatusEnum Status { get; set; }
        }
        
        public class UpdateBankInfoByPayIdRequest
        {
            public int SiteId { get; set; }
            public int PayId { get; set; }

            public int SysId { get; set; }

            /// <summary>
            /// 狀態(1=Enable、2=Disable)
            /// </summary>
            public BankInfoPayStatusEnum Status { get; set; }
        }

        #endregion

        #region Actions

        [HttpPost]
        public ApiResult GetPaySettingList(GetPaySettingListRequest request)
        {
            var result = _paymentManageBll.GetPaySettingList(new GetPaySettingListParams(request),
                request.PageNumber, request.PageSize, out var pageInfo);

            return new ApiListResult<PaySetting>(ErrorCodeEnum.Success, pageInfo, result);
        }

        [HttpPost]
        public ApiResult SetPaySetting(UpdatePaySettingParams request)
        {
            var success = _paymentManageBll.UpdatePaySetting(request, User.Identity.Name);
            
            return new ApiResult
            {
                ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }

        [HttpPost]
        public ApiResult SetPaySettingStatus(UpdatePaySettingStatusParams request)
        {
            var success = _paymentManageBll.UpdatePaySettingStatus(request, User.Identity.Name);
            
            return new ApiResult
            {
                ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }
        
        [HttpPost]
        public ApiResult GetPayList(GetPayListRequest request)
        {
            var result = _paymentManageBll.GetPayList(request.SiteId);

            return new ApiResult<IEnumerable<PayListInfo>>
            {
                Data = result,
                ErrorCode = (int) ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult SetPayListStatus(SetPayListStatusRequest request)
        {
            var success = _paymentManageBll.SetPayListStatus(request.SiteId, request.PayId, request.Status);
            
            return new ApiResult
            {
                ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }

        [HttpPost]
        public ApiResult GetBankInfoList(GetBankListByPayIdRequest request)
        {
            var result =
                _paymentManageBll.GetBankSysInfoList(request.SiteId, request.PayId);

            return new ApiResult<IEnumerable<PayListBankInfoSys>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult SetBankStatus(UpdateBankInfoByPayIdRequest request)
        {
            var user = User.Identity.Name;
            var result = _paymentManageBll
                .UpdateBankInfo(request.SiteId, request.PayId, request.SysId, request.Status, user);

            return new ApiResult
            {
                ErrorCode = (int)(result ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }

        #endregion

    }
}
