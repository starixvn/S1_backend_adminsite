using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentBLL paymentBLL;
        private readonly CustInfoBLL custInfoBLL;
        private readonly TpBLL tpBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly int mainWalletType;

        public PaymentController(IConfiguration configuration, PaymentBLL paymentBLL, CustInfoBLL custInfoBLL, TpBLL tpBLL, AuthorizeBLL authorizeBLL)
        {
            this.paymentBLL = paymentBLL;
            this.custInfoBLL = custInfoBLL;
            this.tpBLL = tpBLL;
            this.authorizeBLL = authorizeBLL;
            mainWalletType = configuration.GetValue<int>("MainWalletType");
        }
        public class PaymentAdjustmentReq
        {
            public int CustId { get; set; }
            public int Option { get; set; }
            public decimal Amount { get; set; }
        }

        public class AdjustmentAndWithdrawalLimitReq : PaymentAdjustmentReq
        {
            public decimal WithdrawalLimitAmount { get; set; }
            public int WithdrawalLimitOption { get; set; }
        }

        [HttpPost]
        public async Task<ApiResult> PaymentAdjustmentAsync(PaymentAdjustmentReq paymentAdjustmentReq)
        {
            ApiResult apiResult = new ApiResult();
            AdjustmentInfo adjustmentInfo = new AdjustmentInfo();
            var custInfo = custInfoBLL.GetCustInfoWithCache(paymentAdjustmentReq.CustId);
            if (custInfo != null)
            {
                adjustmentInfo.CustId = custInfo.CustId;
                adjustmentInfo.CurrencyId = custInfo.CurrencyId;
                adjustmentInfo.UserName = custInfo.UserName;
                adjustmentInfo.SiteId = custInfo.SiteId;
                adjustmentInfo.Amount = paymentAdjustmentReq.Amount;
                adjustmentInfo.AdjustType = paymentAdjustmentReq.Option;

                string transId = null;
                if (adjustmentInfo.AdjustType == 2)
                {
                    var balance = custInfoBLL.GetBalance(custInfo.CustId);
                    if (balance < adjustmentInfo.Amount)
                    {
                        if (mainWalletType == 2)
                        {
                            var deAmount = adjustmentInfo.Amount - balance;
                            TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = deAmount, CustId = custInfo.CustId, direction = TpDirection.ToMain };

                            var acc = await tpBLL.TransferAsync(inTpTransfer);
                            if (acc.ErrorCode != 0 || acc.Data.Status != TransferStatus.Success)
                            {
                                apiResult.ErrorCode = 1001;
                                apiResult.Message = $"{balance.ToString("#,##0.##")}<{adjustmentInfo.Amount.ToString("#,##0.##")}";
                                return apiResult;
                            }

                            //balance = custInfoBLL.GetBalance(custInfo.CustId);
                            //if (balance < adjustmentInfo.Amount)
                            //{
                            //    apiResult.ErrorCode = 1001;
                            //    apiResult.Message = $"{balance.ToString("#,##0")}<{adjustmentInfo.Amount.ToString("#,##0")}";
                            //    return apiResult;
                            //}
                        }
                    }

                    balance = custInfoBLL.GetBalance(custInfo.CustId);

                    if (balance < adjustmentInfo.Amount)
                    {
                        apiResult.ErrorCode = 1002;
                        apiResult.Message = $"{balance.ToString("#,##0.##")} < {adjustmentInfo.Amount.ToString("#,##0.##")}";
                        return apiResult;
                    }
                }

                if (paymentBLL.PaymentAdjustment(adjustmentInfo, User.Identity.Name, out transId) > 0)
                {
                    if (mainWalletType == 2 && adjustmentInfo.AdjustType == 1)
                    {
                        var balance = custInfoBLL.GetBalance(custInfo.CustId);
                        TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = balance, CustId = custInfo.CustId, direction = TpDirection.ToTp };

                        var acc = await tpBLL.TransferAsync(inTpTransfer);
                        if (acc.ErrorCode == 0 && acc.Data.Status == TransferStatus.Success)
                        {
                            apiResult.ErrorCode = 0;
                            apiResult.Message = $"Success. ID is {transId}";
                        }
                    }
                    else
                    {
                        apiResult.ErrorCode = 0;
                        apiResult.Message = $"Success. ID is {transId}";
                    }

                }
            }

            return apiResult;
        }

        public class GetFinancialLogReq
        {
            public int CustId { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public int PageNumber { get; set; }
        }
        public ApiListResult<FinancialLog> GetFinancialLog(GetFinancialLogReq getFinancialLogReq)
        {
            int pageNumber = getFinancialLogReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var apiListResult = new ApiListResult<FinancialLog>();
            apiListResult.Data = paymentBLL.GetFinancialLog(getFinancialLogReq.CustId, getFinancialLogReq.StartDate, getFinancialLogReq.EndDate, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }
        [HttpPost]
        public async Task<ApiResult> AdjustmentAndWithdrawalLimitAsync(AdjustmentAndWithdrawalLimitReq adjustmentAndWithdrawalLimitReq)
        {
            ApiResult apiResult = new ApiResult();
            AdjustmentAndWithdrawalLimit adjustmentAndWithdrawalLimit = new AdjustmentAndWithdrawalLimit();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                var custInfo = custInfoBLL.GetCustInfoWithCache(adjustmentAndWithdrawalLimitReq.CustId);
                if (custInfo != null)
                {
                    adjustmentAndWithdrawalLimit.CustId = custInfo.CustId;
                    adjustmentAndWithdrawalLimit.CurrencyId = custInfo.CurrencyId;
                    adjustmentAndWithdrawalLimit.UserName = custInfo.UserName;
                    adjustmentAndWithdrawalLimit.SiteId = custInfo.SiteId;
                    adjustmentAndWithdrawalLimit.Amount = adjustmentAndWithdrawalLimitReq.Amount;
                    adjustmentAndWithdrawalLimit.AdjustType = adjustmentAndWithdrawalLimitReq.Option;
                    adjustmentAndWithdrawalLimit.SiteName = custInfo.SiteName;
                    adjustmentAndWithdrawalLimit.Operator = authData.UserName;
                    adjustmentAndWithdrawalLimit.WithdrawalLimitAmount = adjustmentAndWithdrawalLimitReq.WithdrawalLimitAmount;
                    adjustmentAndWithdrawalLimit.WithdrawalLimitAmountAdjustType = adjustmentAndWithdrawalLimitReq.WithdrawalLimitOption;



                    string transId = null;
                    if (adjustmentAndWithdrawalLimit.AdjustType == 2)
                    {
                        var balance = custInfoBLL.GetBalance(custInfo.CustId);
                        if (balance < adjustmentAndWithdrawalLimit.Amount)
                        {
                            if (mainWalletType == 2)
                            {
                                var deAmount = adjustmentAndWithdrawalLimit.Amount - balance;
                                TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = deAmount, CustId = custInfo.CustId, direction = TpDirection.ToMain };

                                var acc = await tpBLL.TransferAsync(inTpTransfer);
                                if (acc.ErrorCode != 0 || acc.Data.Status != TransferStatus.Success)
                                {
                                    apiResult.ErrorCode = 1001;
                                    apiResult.Message = $"{balance.ToString("#,##0.##")}<{adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}";
                                    return apiResult;
                                }
                            }
                        }

                        balance = custInfoBLL.GetBalance(custInfo.CustId);

                        if (balance < adjustmentAndWithdrawalLimit.Amount)
                        {
                            apiResult.ErrorCode = 1002;
                            apiResult.Message = $"{balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}";
                            return apiResult;
                        }
                    }

                    if (paymentBLL.AdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit, User.Identity.Name, out transId) > 0)
                    {
                        if (mainWalletType == 2 && adjustmentAndWithdrawalLimit.AdjustType == 1)
                        {
                            var balance = custInfoBLL.GetBalance(custInfo.CustId);
                            TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = balance, CustId = custInfo.CustId, direction = TpDirection.ToTp };

                            var acc = await tpBLL.TransferAsync(inTpTransfer);
                            if (acc.ErrorCode == 0 && acc.Data.Status == TransferStatus.Success)
                            {
                                apiResult.ErrorCode = 0;
                                apiResult.Message = $"Success. ID is {transId}";
                            }
                        }
                        else
                        {
                            apiResult.ErrorCode = 0;
                            apiResult.Message = $"Success. ID is {transId}";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.ToString();
            }

            return apiResult;
        }

        public class BonusCashBackInfoListDetail
        {
            public int errorCode { set; get; }
            public string custName { set; get; }
            public string massage { set; get; }
            public decimal WinlostAmountCountCalculation { set; get; }
            public decimal Percentage { set; get; }
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BalanceAdjustmentBatch" })]
        public async Task<List<ApiResult>> BalanceAdjustmentBatch([FromForm] IEnumerable<string> custList, [FromForm] int option, [FromForm] int withdrawalLimitOption, [FromForm] decimal withdrawalLimitAmount, [FromForm] decimal amount)
        {
            var apiResult = new List<ApiResult>();
            var authData = authorizeBLL.GetAuthData();
            foreach (string custUsername in custList)
            {
                var custInfoDetail = custInfoBLL.GetCustInfoDetailAsync(0, custUsername);
                if (custInfoDetail.Result != null)
                {
                    try
                    {
                        AdjustmentAndWithdrawalLimit adjustmentAndWithdrawalLimit = new AdjustmentAndWithdrawalLimit
                        {
                            CustId = custInfoDetail.Result.CustId,
                            CurrencyId = custInfoDetail.Result.CurrencyId,
                            UserName = custInfoDetail.Result.UserName,
                            SiteId = custInfoDetail.Result.SiteId,
                            Amount = amount,
                            AdjustType = option,
                            SiteName = custInfoDetail.Result.SiteName,
                            Operator = authData.UserName,
                            WithdrawalLimitAmount = withdrawalLimitAmount,
                            WithdrawalLimitAmountAdjustType = withdrawalLimitOption
                        };

                        string transId = null;
                        if (adjustmentAndWithdrawalLimit.AdjustType == 2)
                        {
                            var balance = custInfoBLL.GetBalance(custInfoDetail.Result.CustId);
                            if (balance < adjustmentAndWithdrawalLimit.Amount)
                            {
                                if (mainWalletType == 2)
                                {
                                    var deAmount = adjustmentAndWithdrawalLimit.Amount - balance;
                                    TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = deAmount, CustId = custInfoDetail.Result.CustId, direction = TpDirection.ToMain };

                                    var acc = await tpBLL.TransferAsync(inTpTransfer);
                                    if (acc.ErrorCode != 0 || acc.Data.Status != TransferStatus.Success)
                                    {
                                        apiResult.Add(new ApiResult() { ErrorCode = 1001, Message = $"Fail. Username:{custUsername}. {balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}" });
                                    }
                                }
                            }

                            balance = custInfoBLL.GetBalance(custInfoDetail.Result.CustId);

                            if (balance < adjustmentAndWithdrawalLimit.Amount)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username:{custUsername}. {balance.ToString("#,##0.##")} < {adjustmentAndWithdrawalLimit.Amount.ToString("#,##0.##")}" });
                                continue;
                            }
                        }

                        if (paymentBLL.AdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit, authData.UserName, out transId) > 0)
                        {
                            if (mainWalletType == 2 && adjustmentAndWithdrawalLimit.AdjustType == 1)
                            {
                                var balance = custInfoBLL.GetBalance(custInfoDetail.Result.CustId);
                                TpTransfer inTpTransfer = new TpTransfer() { TpId = 1, Amount = balance, CustId = custInfoDetail.Result.CustId, direction = TpDirection.ToTp };

                                var acc = await tpBLL.TransferAsync(inTpTransfer);
                                if (acc.ErrorCode == 0 && acc.Data.Status == TransferStatus.Success)
                                {
                                    apiResult.Add(new ApiResult() { ErrorCode = 1, Message = $"Success. Username:{custUsername}. ID is {transId}." });
                                }
                            }
                            else
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1, Message = $"Success. Username:{custUsername}. ID is {transId}." });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        apiResult.Add(new ApiResult() { ErrorCode = 999, Message = $"Fail. Username:{custUsername}. {ex}" });
                    }
                }
                else
                {
                    apiResult.Add(new ApiResult() { ErrorCode = -1, Message = $"Not found {custUsername}" });
                }
            }
            return apiResult;
        }
    }
}