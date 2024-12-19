using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.Adjustment
{
    //說明：
    //目的：該程式主要目的是限制Member的可提款限額，實際上異動的是RollOver數字。
    //公式：Balance-(Withdrawal Limit - RollOver)=可提款金額。
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalLimitAdjustmentController : ControllerBase
    {
        private readonly CustInfoBLL _custInfoBLL;
        private readonly AdjustWithdrawalLimitBLL _adjustWithdrawalLimitBLL;
        private readonly NewAuthorizeBLL _authorizeBLL;
        private readonly CustVerifyBLL _custVerifyBll;
        private readonly BankFinancialBLL _bankFinancialBll;

        public WithdrawalLimitAdjustmentController(IServiceProvider provider)
        {
            _custInfoBLL = provider.GetRequiredService<CustInfoBLL>();
            _custVerifyBll = provider.GetRequiredService<CustVerifyBLL>();
            _adjustWithdrawalLimitBLL = provider.GetRequiredService<AdjustWithdrawalLimitBLL>();
            _authorizeBLL = provider.GetRequiredService<NewAuthorizeBLL>();
            _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
        }

        #region API Model

        public class GetCustAndWalletInfoRequest
        {
            /// <summary>
            /// 品牌ID
            /// </summary>
            public int SiteId { get; set; }

            /// <summary>
            /// 會員名稱
            /// </summary>
            public string UserName { get; set; }

            /// <summary>
            /// 沙巴ID
            /// </summary>
            public string SabaId { get; set; }
        }

        public class SetAdjustWithdrawalLimitRequest
        {
            /// <summary>
            /// 會員ID
            /// </summary>
            public int CustId { get; set; }

            /// <summary>
            /// 調整型態 1=Debit, 2=Credit
            /// </summary>
            public WithdrawalLimitAdjustmentAdjustTypeEnum AdjustType { get; set; }

            public WithdrawalLimitAdjustmentReasonEnum ReasonId { get; set; }

            /// <summary>
            /// 調整金額
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// 備註
            /// </summary>
            public string Memo { get; set; }
        }

        #endregion

        /// <summary>
        /// WithdrawalLimitAdjustment/GetCustAndWalletInfo 取得會員和錢包資訊
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Adjustment > [Submit]
        /// 
        /// Finance > Withdrawal Limit Adjustment > Cust Info [Refresh]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<object> GetCustAndWalletInfoAsync(GetCustAndWalletInfoRequest request)
        {
            var errorCode = _custVerifyBll.CustVerify(request.SiteId, request.UserName, request.SabaId,
                out CustInfoDetail custInfo);

            if (errorCode != ErrorCodeEnum.Success)
            {
                throw new AdminException(errorCode);
            }

            var walletInfo = _adjustWithdrawalLimitBLL.GetCustWalletInfo(custInfo.CustId);
            var status = (CustomerStatusEnum)custInfo.Status;

            return status switch
            {
                CustomerStatusEnum.Running
                    => new ApiResult<object>
                    {
                        Data = new
                        {
                            SiteId = custInfo.SiteId,
                            SiteName = custInfo.SiteName,
                            CustId = custInfo.CustId,
                            UserName = custInfo.UserName,
                            Currency = custInfo.CurrencyId,
                            Balance = walletInfo.Balance,
                            differenceAmount = walletInfo.DifferenceAmount,
                            eligibleWithdrawalAmount = walletInfo.AvailableBalance
                        },
                        ErrorCode = (int)ErrorCodeEnum.Success
                    },

                CustomerStatusEnum.Suspend
                    => throw new AdminException(ErrorCodeEnum.CustStatusIsSuspend),

                _ => throw new AdminException(ErrorCodeEnum.CustStatusIsClosed)
            };
        }

        /// <summary>
        /// WithdrawalLimitAdjustment/Submit 提交提款限額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Adjustment > Setting > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult Submit(SetAdjustWithdrawalLimitRequest request)
        {
            var authData = _authorizeBLL.GetAuthData();
            var custInfo = _custInfoBLL.GetCustInfoWithCache(request.CustId);

            var (success, msg) = _adjustWithdrawalLimitBLL
                .SetAdjustWithdrawalLimitV2(new SetAdjustWithdrawalLimitParams
                {
                    CustId = request.CustId,
                    SiteId = custInfo.SiteId,
                    CurrencyId = custInfo.CurrencyId,
                    Amount = request.Amount,
                    Memo = request.Memo,
                    AdjustType = request.AdjustType,
                    ReasonId = request.ReasonId,
                    UserName = custInfo.UserName,
                    Operator = authData.UserName
                });


            return new ApiResult
            {
                ErrorCode = (int)(success ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail),
                Message = msg
            };
        }

        [HttpPost]
        public ApiResult<IEnumerable<SelectListItem>> GetReasonList()
        {
            var result =
                _bankFinancialBll.GetReason("Finance/WithdrawalLimitAdjustment");
            var data =
                result.Select(x => new SelectListItem
                {
                    Value = x.ReasonId,
                    Name = x.Description
                });
            var apiResult =
                new ApiResult<IEnumerable<SelectListItem>>((int)ErrorCodeEnum.Success, string.Empty, data);

            return apiResult;
        }
    }
}