using System;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitAdjust" })]
    public class AdjustWithdrawalLimitController : Controller
    {
        private readonly CustInfoBLL custInfoBLL;
        private readonly AdjustWithdrawalLimitBLL adjustWithdrawalLimitBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly CustInfoService custInfoService;
        private readonly Common common;

        public AdjustWithdrawalLimitController(CustInfoBLL custInfoBLL,
            AdjustWithdrawalLimitBLL adjustWithdrawalLimitBLL,
            AuthorizeBLL authorizeBLL,
            CustInfoService custInfoService, Common common)
        {
            this.custInfoBLL = custInfoBLL;
            this.adjustWithdrawalLimitBLL = adjustWithdrawalLimitBLL;
            this.authorizeBLL = authorizeBLL;
            this.custInfoService = custInfoService;
            this.common = common;
        }

        /// <summary>
        /// AdjustWithdrawalLimit/GetCustAndWalletInfoAsync 取得會員錢包資訊
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Adjustment > [Submit]
        /// 
        /// </remarks>
        /// <param name="SiteId"></param>
        /// <param name="UserName"></param>
        /// <param name="SabaId"></param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<dynamic> GetCustAndWalletInfoAsync([FromForm] int SiteId, [FromForm] string UserName, [FromForm] string SabaId)
        {
            var result = new ApiResult<dynamic>();
            try
            {
                var custId = (string.IsNullOrEmpty(SabaId)) ? null : common.TpIdToCustId(SabaId);
                var custInfo = custInfoBLL.GetCustInfoDetails(custId, UserName, SiteId);

                if (custInfo != null)
                {
                    var walletInfo = adjustWithdrawalLimitBLL.GetCustWalletInfo(custInfo.CustId);

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
                    result.ErrorCode = 0;
                }
                else
                {
                    result.ErrorCode = 21;
                    result.Message = "Player not found.";
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        /// <summary>
        /// AdjustWithdrawalLimit/GetAdjustWithdrawalLimitsLog 取得調整提款限額紀錄
        /// </summary>
        /// <remarks>
        ///
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Adjustment > ShowLog
        /// 
        /// </remarks>
        /// <param name="CustId"></param>
        /// <param name="PageNumber"></param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiListResult<AdjustWithdrawalLimit> GetAdjustWithdrawalLimitsLog([FromForm] int CustId, [FromForm] int PageNumber)
        {
            var PageSize = 10;

            var result = new ApiListResult<AdjustWithdrawalLimit>();
            try
            {
                result.Data = adjustWithdrawalLimitBLL.GetAdjustWithdrawalLimitsLog(CustId, PageNumber, PageSize, out int TotalPages, out int RowCount);

                result.PageNumber = PageNumber;
                result.RowCount = RowCount;
                result.TotalPages = TotalPages;
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        /// <summary>
        /// AdjustWithdrawalLimit/SetAdjustWithdrawalLimit 設定提款限額
        /// </summary>
        /// <remarks>
        ///
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Adjustment > Setting > [Submit]
        /// 
        /// </remarks>
        /// <param name="CustId"></param>
        /// <param name="adjustWithdrawalLimit"></param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult SetAdjustWithdrawalLimit([FromForm] int CustId, [FromForm] AdjustWithdrawalLimit adjustWithdrawalLimit)
        {
            var result = new ApiResult();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                var custInfo = custInfoBLL.GetCustInfoWithCache(CustId);
                adjustWithdrawalLimit.SiteName = custInfo.SiteName;
                adjustWithdrawalLimit.Operator = authData.UserName;
                adjustWithdrawalLimit.CustId = CustId;
                var resultCode = adjustWithdrawalLimitBLL.SetAdjustWithdrawalLimit(adjustWithdrawalLimit, custInfo.UserName, custInfo.CurrencyId, custInfo.SiteId, User.Identity.Name);
                if (resultCode == 21)
                {
                    result.ErrorCode = 21;
                    result.Message = "debit amount can not more then difference amount";
                }
                else
                {
                    result.ErrorCode = resultCode;
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        /// <summary>
        /// AdjustWithdrawalLimit/GetWalletInfo 取得錢包資訊
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Adjustment > ShowWalletInfo
        /// 
        /// </remarks>
        /// <param name="CustId"></param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<WalletInfo> GetWalletInfo([FromForm] int CustId)
        {
            var result = new ApiResult<WalletInfo>();
            try
            {
                result.Data = adjustWithdrawalLimitBLL.GetCustWalletInfo(CustId);
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }
    }
}