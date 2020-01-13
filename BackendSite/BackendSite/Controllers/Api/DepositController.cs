using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DepositController : ControllerBase
    {
        private readonly DepositBLL depositBLL;
        private readonly SelLangBLL selLangBLL;
        private readonly MessageBLL messageBLL;
        private readonly PendingFundBLL pendingFundBLL;
        private readonly CustInfoBLL custInfoBLL;
        private readonly BankDetailBLL bankDetailBLL;

        public DepositController(DepositBLL depositBLL, SelLangBLL selLangBLL, MessageBLL messageBLL, PendingFundBLL pendingFundBLL, CustInfoBLL custInfoBLL, BankDetailBLL bankDetailBLL)
        {
            this.depositBLL = depositBLL;
            this.selLangBLL = selLangBLL;
            this.messageBLL = messageBLL;
            this.pendingFundBLL = pendingFundBLL;
            this.custInfoBLL = custInfoBLL;
            this.bankDetailBLL = bankDetailBLL;
        }
        public class DepositListReq
        {
            public int PageNumber { get; set; }
            public string DepositId { get; set; }
            public string UserName { get; set; }
            public int? DepositType { get; set; }
            public int? CurrencyId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
            public int? status { get; set; }
        }

        [HttpPost]
        public ApiListResult<DepositRequest> GetDepositRequestList(DepositListReq depositListReq)
        {
            var apiListResult = new ApiListResult<DepositRequest>();
            int pageNumber = depositListReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var depositRequestList = depositBLL.GetDepositRequestList(depositListReq.DepositType, depositListReq.startDate, depositListReq.endDate, depositListReq.DepositId, depositListReq.UserName, depositListReq.CurrencyId, depositListReq.status, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.Data = depositRequestList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }

        public class DepositReq
        {
            public string TransId { get; set; }
        }
        [HttpPost]
        public ApiResult<DepositRequest> GetDepositRequest(DepositReq depositReq)
        {
            var apiResult = new ApiResult<DepositRequest>();
            var depositRequest = depositBLL.GetDepositRequest(depositReq.TransId);

            apiResult.Data = depositRequest;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<Comments>> GetDepositRequestComment(DepositReq depositReq)
        {
            var apiResult = new ApiResult<IEnumerable<Comments>>();
            var comments = depositBLL.GetDepositRequestComment(depositReq.TransId);
            apiResult.Data = comments;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        public class DepositReq1 : DepositReq
        {
            public decimal ActualDeposit { set; get; }
            public string Comment { set; get; }
            public int Action { set; get; }
        }

        [HttpPost]
        public ApiResult DepositConfirm(DepositReq1 depositReq)
        {
            var apiResult = new ApiResult(selLangBLL);
            var depositConfirmReuslt = -1;

            if (depositReq.Action == (int)DepositStatus.Approved)
            {
                if (depositReq.ActualDeposit > 0)
                    depositConfirmReuslt = depositBLL.DepositApproved(depositReq.TransId, depositReq.ActualDeposit, depositReq.Comment, User.Identity.Name);

                if (depositConfirmReuslt > 0)
                {
                    PendingFund pendingFund = new PendingFund();
                    pendingFund.CustId = depositConfirmReuslt;
                    Task<bool> tmp = Process(pendingFund);

                    if (!tmp.Result)
                    {
                        depositConfirmReuslt = -4;
                    }
                    else
                    {
                        // Send message to player
                        string result = selLangBLL.GetResourceValue("lbl_Approved");
                        string sender = selLangBLL.GetResourceValue("lbl_MessageDepoistWithdrawalSender");
                        string title = selLangBLL.GetResourceValue("lbl_MessageDepositTitle");
                        title = string.Format(title, depositReq.TransId, result);
                        string content = selLangBLL.GetResourceValue("lbl_MessageDepositContent");
                        content = string.Format(content, depositReq.TransId, result);
                        messageBLL.SendMessageToPlayer(sender, title, content, depositConfirmReuslt);
                    }
                }
            }
            else if (depositReq.Action == (int)DepositStatus.Rejected)
            {
                depositConfirmReuslt = depositBLL.DepositRejectd(depositReq.TransId, depositReq.ActualDeposit, depositReq.Comment, User.Identity.Name);

                if (depositConfirmReuslt > 0)
                {
                    // Send message to player
                    string result = selLangBLL.GetResourceValue("lbl_Rejected");
                    string sender = selLangBLL.GetResourceValue("lbl_MessageDepoistWithdrawalSender");
                    string title = selLangBLL.GetResourceValue("lbl_MessageDepositTitle");
                    title = string.Format(title, depositReq.TransId, result);
                    string content = selLangBLL.GetResourceValue("lbl_MessageDepositContent");
                    content = string.Format(content, depositReq.TransId, result);
                    messageBLL.SendMessageToPlayer(sender, title, content, depositConfirmReuslt);
                }
            }
            else if (depositReq.Action == (int)DepositStatus.Draft)
            {
                depositConfirmReuslt = depositBLL.DepositDraft(depositReq.TransId, depositReq.ActualDeposit, depositReq.Comment, User.Identity.Name);
            }
            else if (depositReq.Action == (int)DepositStatus.OnHold)
            {
                depositConfirmReuslt = depositBLL.DepositOnHold(depositReq.TransId, depositReq.ActualDeposit, depositReq.Comment, User.Identity.Name);
            }
            else if (depositReq.Action == (int)DepositStatus.Pending)
            {
                depositConfirmReuslt = depositBLL.DepositPending(depositReq.TransId, depositReq.ActualDeposit, depositReq.Comment, User.Identity.Name);
            }

            if (depositConfirmReuslt == -4)
            {
                apiResult.ErrorCode = 1102;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailedtoSB");
            }
            else if (depositConfirmReuslt == 0)
            {
                apiResult.ErrorCode = 1101;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            else
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }

            return apiResult;
        }
        public async Task<bool> Process(PendingFund pendingFund)
        {
            bool result = await pendingFundBLL.ProcessAsync(pendingFund);
            return result;
        }

        public class PaymentAdjustmentReq
        {
            public int CustId { get; set; }
            public int Option { get; set; }
            public int BankId { get; set; }
            public decimal Amount { get; set; }
        }

        [HttpPost]
        public ApiResult PaymentAdjustment(PaymentAdjustmentReq paymentAdjustmentReq)
        {
            ApiResult apiResult = new ApiResult();
            DepositRequest depositRequest = new DepositRequest();
            var custInfo = custInfoBLL.GetCustInfoWithCache(paymentAdjustmentReq.CustId);
            var bankInfo = bankDetailBLL.GetBankInfoOff().Where(x => x.BankId == paymentAdjustmentReq.BankId).FirstOrDefault();
            if (bankInfo != null && custInfo != null)
            {
                depositRequest.CustId = custInfo.CustId;
                depositRequest.CurrencyId = custInfo.CurrencyId;
                depositRequest.UserName = custInfo.UserName;
                depositRequest.SiteId = custInfo.SiteId;
                depositRequest.Amount = paymentAdjustmentReq.Amount * (paymentAdjustmentReq.Option == 2 ? -1 : 1);
                DepositInfo3_1 depositInfo3_1 = new DepositInfo3_1();
                depositInfo3_1.AccountName = bankInfo.AccountName;
                depositInfo3_1.AccountNo = bankInfo.AccountNo;
                depositInfo3_1.CurrencyId = custInfo.CurrencyId;
                depositInfo3_1.SysId = bankInfo.SysId;
                depositRequest.DepositInfo = Newtonsoft.Json.JsonConvert.SerializeObject(depositInfo3_1);

                string transId = null;

                if (depositBLL.PaymentAdjustment(depositRequest, out transId) > 0)
                {
                    apiResult.ErrorCode = 0;
                    apiResult.Message = $"Success. ID is {transId}";
                }
            }

            return apiResult;
        }
    }
}