using BackendSite.Service.BLL;
using BackendSite.Service.Library;
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
        private readonly PermissionBLL _permissionBll;
        private readonly Cryptor _cryptor;
        private readonly NewAuthorizeBLL _authorizeBll;

        #endregion

        #region Constructors

        public ThirdPartyManagementController(IServiceProvider provider)
        {
            _paymentManageBll = provider.GetRequiredService<PaymentManageBLL>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _cryptor = provider.GetRequiredService<Cryptor>();
            _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
        }

        #endregion

        #region API Model

        public class GetBankListForSettingRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int ChannelId { get; set; }
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

        public class SetPayListLoginInfoRequest
        {
            public int SiteId { get; set; }
            public int PayId { get; set; }
            public string LoginUrl { get; set; }
            public string LoginAccount { get; set; }
            public string LoginPassword { get; set; }
            public string TransferPin { get; set; }
            public string LoginPin { get; set; }
            public string CorporateId { get; set; }
        }

        public class UpdateBankInfoByPayIdRequest
        {
            public int SiteId { get; set; }
            public int PayId { get; set; }

            public int SysId { get; set; }
            public int ChannelId { get; set; }

            /// <summary>
            /// 狀態(1=Enable、2=Disable)
            /// </summary>
            public BankInfoPayStatusEnum Status { get; set; }
        }

        public class SetCryptoInfoStatusRequest
        {
            public CryptoCurrencyEnum CurrencyId { get; set; }
            public NetworkTypeEnum Network { get; set; }
            public GeneralStatusEnum Status { get; set; }
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
            var authData = _authorizeBll.GetAuthData();
            var success = _paymentManageBll.UpdatePaySetting(request, authData.UserName);

            return new ApiResult
            {
                ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }

        [HttpPost]
        public ApiResult SetPaySettingStatus(UpdatePaySettingStatusParams request)
        {
            var authData = _authorizeBll.GetAuthData();
            var success = _paymentManageBll.UpdatePaySettingStatus(request, authData.UserName);

            return new ApiResult
            {
                ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }

        [HttpPost]
        public ApiResult SetPaySettingChargeFee(UpdatePaySettingChargeFeeParams request)
        {
            var authData = _authorizeBll.GetAuthData();
            var success = _paymentManageBll.UpdatePaySettingChargeFee(request, authData.UserName);

            return new ApiResult
            {
                ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }

        [HttpPost]
        public ApiResult GetPayList(GetPayListRequest request)
        {
            var result = _paymentManageBll.GetPayList(request.SiteId)
                .Where(x => x.PayStatus == PayListStatusEnum.Enable);

            if (_permissionBll.CheckPermission(39, new[] { 15 }))
            {
                foreach (var d in result)
                {
                    d.LoginAccount = _cryptor.Decrypt(d.LoginAccount);
                    d.LoginPassword = _cryptor.Decrypt(d.LoginPassword);
                    d.TransferPin = _cryptor.Decrypt(d.TransferPin);
                    d.LoginPin = _cryptor.Decrypt(d.LoginPin);
                    d.CorporateId = _cryptor.Decrypt(d.CorporateId);
                }
            }

            return new ApiResult<IEnumerable<PayListInfo>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult SetPayListStatus(SetPayListStatusRequest request)
        {
            var authData = _authorizeBll.GetAuthData();
            var success = _paymentManageBll.SetPayListStatus(request.SiteId, request.PayId, request.Status, authData.UserName);

            return new ApiResult
            {
                ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }

        [HttpPost]
        public ApiResult SetPayListLoginInfo(SetPayListLoginInfoRequest request)
        {
            var authData = _authorizeBll.GetAuthData();
            var success = _paymentManageBll.SetPayListLoginInfo(new SetPayListLoginInfoParams
            {
                SiteId = request.SiteId,
                PayId = request.PayId,
                LoginUrl = request.LoginUrl,
                LoginAccount = _cryptor.Encrypt(request.LoginAccount),
                LoginPassword = _cryptor.Encrypt(request.LoginPassword),
                LoginPin = _cryptor.Encrypt(request.LoginPin),
                TransferPin = _cryptor.Encrypt(request.TransferPin),
                CorporateId = _cryptor.Encrypt(request.CorporateId),
                Operator = authData.UserName
            });

            return new ApiResult
            {
                ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }

        [HttpPost]
        public ApiResult GetBankInfoList(GetBankListForSettingRequest request)
        {
            var result =
                _paymentManageBll.GetBankInfoPayForSetting(request.SiteId, request.CurrencyId, request.ChannelId);

            return new ApiResult<IEnumerable<BankInfoPayForSetting>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult SetBankStatus(UpdateBankInfoByPayIdRequest request)
        {
            var authData = _authorizeBll.GetAuthData();
            var result = _paymentManageBll
                .UpdateBankInfo(request.SiteId, request.PayId, request.SysId, request.Status, request.ChannelId, authData.UserName);

            return new ApiResult
            {
                ErrorCode = (int)(result ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }

        [HttpPost]
        public ApiResult GetCryptoInfoList()
        {
            var authData = _authorizeBll.GetAuthData();
            var result = _paymentManageBll.GetCryptoInfoList(authData.SiteId);

            return new ApiResult<IEnumerable<CryptoInfo>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult SetCryptoInfoStatus(SetCryptoInfoStatusRequest request)
        {
            var authData = _authorizeBll.GetAuthData();
            var result = _paymentManageBll
                .SetCryptoInfoStatus(authData.SiteId, request.CurrencyId, request.Network.ToString(), request.Status, authData.UserName);

            return new ApiResult
            {
                ErrorCode = (int)(result ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
            };
        }

        #endregion

    }
}
