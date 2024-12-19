using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.Withdrawal
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalSubmissionController : ControllerBase
    {
        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly SelLangBLL _selLangBll;
        private readonly NewAuthorizeBLL _authorizeBll;
        private readonly CustVerifyBLL _custVerifyBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly BankDetailBLL _bankDetailBll;

        #endregion

        #region Constructors

        public WithdrawalSubmissionController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _custVerifyBll = provider.GetRequiredService<CustVerifyBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
        }

        #endregion

        #region API Model

        public class GetCustBankListRequest
        {
            public int SiteId { set; get; }
            public string SabaId { set; get; }
            public string UserName { set; get; }
        }

        public class WithdrawalSubmissionRequest
        {
            public int CustId { set; get; }
            public ApplicationAccountTypeEnum PaymentId { set; get; }
            public string CryptoNetwork { get; set; }
            public int CryptoCurrencyId { set; get; }
            public bool IsExistBankCard { set; get; }
            /// <summary>
            /// 申請提款類型為Local Bank/EWallet才會有值
            /// </summary>
            public int SysId { get; set; }
            public string AccountNo { set; get; }
            public string AccountHolderName { set; get; }
            public decimal Amount { set; get; }
            public string Memo { set; get; }
            /// <summary>
            /// 參考匯率
            /// </summary>
            public decimal ExchangeRate { get; set; }
            /// <summary>
            /// 參考加密貨幣金額
            /// </summary>
            public decimal ExchangeAmount { get; set; }
        }

        #endregion

        #region Action

        /// <summary>
        /// WithdrawalSubmission/Verify 提交提款單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Submission > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 125, 1 })]
        [HttpPost]
        public async Task<ApiResult<object>> VerifyAsync(GetCustBankListRequest request)
        {

            var apiResult = new ApiResult<object>();
            var result = _custVerifyBll.CustVerify(request.SiteId, request.UserName, request.SabaId, out CustInfoDetail custInfo);

            if (result != ErrorCodeEnum.Success)
            {
                throw new AdminException(result);
            }

            if (custInfo != null)
            {
                if (custInfo.Status == (int)CustomerStatusEnum.Running)
                {
                    var custDetail = _custInfoBll.GetCustInfoWithCache(custInfo.CustId);
                    var availableBalance = _withdrawalBll.GetWithdrawalAvailableBalance(custInfo.CustId);
                    if (availableBalance <= 0)
                        return new ApiResult<object>
                        {
                            ErrorCode = -1,
                            Message = $"Available Balance is Insufficient. Available Balance={availableBalance}"
                        };
                    var AccountInfoList = await _custInfoBll.GetCustBankDetailAsync(custInfo.CustId);
                    var cryptoAccountInfoList = await _custInfoBll.GetCustCryptoInfoListAsync(custInfo.CustId);
                    var sysLocalBankList = _bankDetailBll.GetSysBankList()
                        .Where(x => (int)x.CurrencyId == custInfo.CurrencyId)
                        .Select(x => new { Value = x.SysId, Content = $"{x.BankCode}-{x.BankName}" });
                    apiResult.Data = new
                    {
                        CustId = custInfo.CustId,
                        CurrencyId = custInfo.CurrencyId,
                        AccountName = custDetail.AccountName,
                        EligibleWithdrawal = availableBalance,
                        ExistLocalBankList = AccountInfoList,
                        ExistCryptoList = cryptoAccountInfoList,
                        LocalBankSysList = sysLocalBankList,
                        //EWalletSysList = sysLocalBankList
                    };
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else if (custInfo.Status == (int)CustomerStatusEnum.Suspend)
                {
                    throw new AdminException(ErrorCodeEnum.CustStatusIsSuspend);
                }
                else
                {
                    throw new AdminException(ErrorCodeEnum.CustStatusIsClosed);
                }
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.CustNotFound);
            }

            return apiResult;
        }

        /// <summary>
        /// WithdrawalSubmission/Submit 提交提款單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Submission > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 125, 3 })]
        [HttpPost]
        public async Task<ApiResult> SubmitAsync(WithdrawalSubmissionRequest request)
        {
            var authData = _authorizeBll.GetAuthData();
            var result = await _withdrawalBll.CreateWithdrawalAsync(request.CustId,
                request.PaymentId,
                request.CryptoNetwork,
                request.CryptoCurrencyId,
                request.IsExistBankCard,
                request.SysId,
                request.AccountNo,
                request.AccountHolderName,
                request.Amount,
                request.ExchangeRate,
                request.ExchangeAmount,
                request.Memo,
                authData.UserName);

            var apiResult = new ApiResult();
            if (result.isSuccess)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = $"{_selLangBll.GetMsg("msg_InsertSuccessful")}, TransId = {result.TransId}";
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_InsertFailed");
            }
            return apiResult;
        }

        #endregion
    }
}
