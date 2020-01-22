using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly int mainWalletType;

        public PaymentController(IConfiguration configuration, PaymentBLL paymentBLL, CustInfoBLL custInfoBLL, TpBLL tpBLL)
        {
            this.paymentBLL = paymentBLL;
            this.custInfoBLL = custInfoBLL;
            this.tpBLL = tpBLL;
            mainWalletType = configuration.GetValue<int>("MainWalletType");
        }
        public class PaymentAdjustmentReq
        {
            public int CustId { get; set; }
            public int Option { get; set; }
            public decimal Amount { get; set; }
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
    }
}