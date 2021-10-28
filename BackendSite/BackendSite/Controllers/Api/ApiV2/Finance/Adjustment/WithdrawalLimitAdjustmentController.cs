using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.ApiV2.Finance.Adjustment
{

    //說明：

    //目的：該程式主要目的是限制Member的可提款限額，實際上異動的是RollOver數字。
    //公式：Balance-(Withdrawal Limit - RollOver)=可提款金額。
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitAdjust" })]
    public class WithdrawalLimitAdjustmentController : ControllerBase
    {
        private readonly CustInfoBLL _custInfoBLL;
        private readonly AdjustWithdrawalLimitBLL _adjustWithdrawalLimitBLL;
        private readonly AuthorizeBLL _authorizeBLL;
        private readonly Common _common;

        public WithdrawalLimitAdjustmentController(CustInfoBLL custInfoBLL,
            AdjustWithdrawalLimitBLL adjustWithdrawalLimitBLL,
            AuthorizeBLL authorizeBLL,
            Common common)
        {
            _custInfoBLL = custInfoBLL;
            _adjustWithdrawalLimitBLL = adjustWithdrawalLimitBLL;
            _authorizeBLL = authorizeBLL;
            _common = common;
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
            /// 會員ID
            /// </summary>
            public int CustId { get; set; }
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
        /// WithdrawalLimitAdjustment/GetCustAndWalletInfoAsync 取得會員錢包資訊
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
        //有新舊API，這支是新API，另一支新的API=api/AdjustWithdrawalLimit/GetCustAndWalletInfo
        public ApiResult<dynamic> GetCustAndWalletInfoAsync(GetCustAndWalletInfoRequest request)
        {
            var result = new ApiResult<dynamic>();
            try
            {
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
                    result.ErrorCode = (int)ErrorMessageCodeEnum.Success;
                }
                else
                {
                    result.ErrorCode = (int)ErrorMessageCodeEnum.CustNotFound;
                    result.Message = "Player not found.";
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = (int)ErrorMessageCodeEnum.SystemException;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        /// <summary>
        /// WithdrawalLimitAdjustment/GetAdjustWithdrawalLimitsLog 取得調整提款限額紀錄
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
        [HttpPost]
        //有新舊API，這支是新API，另一支新的API=api/AdjustWithdrawalLimit/GetAdjustWithdrawalLimitsLog
        public ApiListResult<AdjustWithdrawalLimit> GetAdjustWithdrawalLimitsLog(GetAdjustWithdrawalLimitsLogRequest request)
        {

            var result = new ApiListResult<AdjustWithdrawalLimit>();
            try
            {
                result.Data = _adjustWithdrawalLimitBLL.GetAdjustWithdrawalLimitsLog(request.CustId,
                    request.PageNumber,
                    request.PageSize,
                    out int TotalPages,
                    out int RowCount);

                result.PageNumber = request.PageNumber;
                result.RowCount = RowCount;
                result.TotalPages = TotalPages;
                result.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            }
            catch (Exception ex)
            {
                result.ErrorCode = (int)ErrorMessageCodeEnum.SystemException;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        /// <summary>
        /// WithdrawalLimitAdjustment/SetAdjustWithdrawalLimit 設定提款限額
        /// </summary>
        /// <remarks>
        ///
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Adjustment > Setting > [Submit]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>結果</returns>
        [HttpPost]
        //有新舊API，這支是新API，另一支新的API=api/AdjustWithdrawalLimit/SetAdjustWithdrawalLimit
        public ApiResult SetAdjustWithdrawalLimit(SetAdjustWithdrawalLimitRequest request)
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
                    result.ErrorCode = (int)ErrorMessageCodeEnum.WithdrawalLimitAdjustmentNotBalance;
                    result.Message = "debit amount can not more then difference amount";
                }
                else if (result.ErrorCode == 22)
                {
                    result.ErrorCode = (int)ErrorMessageCodeEnum.Fail;
                    result.Message = "Update Fail";
                }
                else
                {
                    result.ErrorCode = resultCode;
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = (int)ErrorMessageCodeEnum.SystemException;
                result.RawMessage = ex.ToString();
            }

            return result;
        }
    }
}
