using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BackendSite.Controllers.ApiV2.Finance.Adjustment
{

    //說明：

    //目的：該程式主要目的是限制Member的可提款限額，實際上異動的是RollOver數字。
    //公式：Balance-(Withdrawal Limit - RollOver)=可提款金額。
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalLimitAdjustmentController : ControllerBase
    {
        private readonly CustInfoBLL _custInfoBLL;
        private readonly AdjustWithdrawalLimitBLL _adjustWithdrawalLimitBLL;
        private readonly AuthorizeBLL _authorizeBLL;
        private readonly Common _common;

        public WithdrawalLimitAdjustmentController(IServiceProvider provider)
        {
            _custInfoBLL = provider.GetRequiredService<CustInfoBLL>();
            _adjustWithdrawalLimitBLL = provider.GetRequiredService<AdjustWithdrawalLimitBLL>(); ;
            _authorizeBLL = provider.GetRequiredService<AuthorizeBLL>(); ;
            _common = provider.GetRequiredService<Common>(); ;
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

        public class GetAdjustWithdrawalLimitsLogRequest : ApiRequestPageModel
        {
            /// <summary>
            /// 品牌ID
            /// </summary>
            public int SiteId { get; set; }
            /// <summary>
            /// 會員名稱
            /// </summary>
            public string UserName { get; set; }
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
            public int AdjustType { get; set; }

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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitAdjust" })]
        [HttpPost]
        //有新舊API，這支是新API，另一支新的API=api/AdjustWithdrawalLimit/GetCustAndWalletInfo
        public ApiResult<dynamic> GetCustAndWalletInfoAsync(GetCustAndWalletInfoRequest request)
        {
            var result = new ApiResult<dynamic>();
            try
            {
                if (string.IsNullOrWhiteSpace(request.SabaId))
                    request.SabaId = null;
                if (string.IsNullOrWhiteSpace(request.UserName))
                    request.UserName = null;

                var custId = (string.IsNullOrEmpty(request.SabaId)) ? null : _common.TpIdToCustId(request.SabaId);

                var custInfo = _custInfoBLL.GetCustInfoDetails(custId, request.UserName, request.SiteId);

                if (custInfo != null)
                {
                    var walletInfo = _adjustWithdrawalLimitBLL.GetCustWalletInfo(custInfo.CustId);

                    result.Data = new
                    {
                        CustId = custInfo.CustId,
                        SiteName = custInfo.SiteName,
                        UserName = custInfo.UserName,
                        Currency = custInfo.CurrencyId,
                        Balance = walletInfo.Balance,
                        differenceAmount = walletInfo.DifferenceAmount,
                        eligibleWithdrawalAmount = walletInfo.AvailableBalance
                    };
                    result.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    result.ErrorCode = (int)ErrorCodeEnum.CustNotFound;
                    result.Message = "Player not found.";
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = (int)ErrorCodeEnum.SystemException;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        /// <summary>
        /// WithdrawalLimitAdjustment/GetLog 取得調整提款限額紀錄
        /// </summary>
        /// <remarks>
        ///
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Adjustment > ShowLog
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitAdjust" })]
        [HttpPost]
        //有新舊API，這支是新API，另一支新的API=api/AdjustWithdrawalLimit/GetAdjustWithdrawalLimitsLog
        public ApiListResult<AdjustWithdrawalLimit> GetLog(GetAdjustWithdrawalLimitsLogRequest request)
        {

            var result = new ApiListResult<AdjustWithdrawalLimit>();

            var custInfo = _custInfoBLL.GetCustInfoDetails(null, request.UserName, request.SiteId);

            if (custInfo == null)
                throw new AdminException(ErrorCodeEnum.CustNotFound);

            result.Data = _adjustWithdrawalLimitBLL.GetAdjustWithdrawalLimitsLog(custInfo.CustId,
                request.PageNumber,
                request.PageSize,
                out int TotalPages,
                out int RowCount);

            result.PageNumber = request.PageNumber;
            result.RowCount = RowCount;
            result.TotalPages = TotalPages;
            result.ErrorCode = (int)ErrorCodeEnum.Success;

            return result;
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitAdjust" })]
        [HttpPost]
        //有新舊API，這支是新API，另一支新的API=api/AdjustWithdrawalLimit/SetAdjustWithdrawalLimit
        public ApiResult Submit(SetAdjustWithdrawalLimitRequest request)
        {
            var result = new ApiResult();
            try
            {
                var authData = _authorizeBLL.GetAuthData();
                var custInfo = _custInfoBLL.GetCustInfoWithCache(request.CustId);
                var adjustWithdrawalLimit = new AdjustWithdrawalLimit()
                {
                    CustId = custInfo.CustId,
                    AdjustType = request.AdjustType,
                    SiteName = custInfo.SiteName,
                    Amount = request.Amount,
                    Memo = request.Memo,
                    Operator = authData.UserName
                };
                var resultCode = _adjustWithdrawalLimitBLL.SetAdjustWithdrawalLimit(adjustWithdrawalLimit,
                    custInfo.UserName,
                    custInfo.CurrencyId,
                    custInfo.SiteId,
                    authData.UserName);

                if (resultCode == 21)
                {
                    result.ErrorCode = (int)ErrorCodeEnum.WithdrawalLimitAdjustmentNotBalance;
                    result.Message = "debit amount can not more then difference amount";
                }
                else if (result.ErrorCode == 22)
                {
                    result.ErrorCode = (int)ErrorCodeEnum.Fail;
                    result.Message = "Update Fail";
                }
                else
                {
                    result.ErrorCode = resultCode;
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = (int)ErrorCodeEnum.SystemException;
                result.RawMessage = ex.ToString();
            }

            return result;
        }
    }
}
