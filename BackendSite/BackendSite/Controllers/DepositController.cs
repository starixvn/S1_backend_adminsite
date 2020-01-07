using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;

namespace BackendSite.Controllers
{
    [Authorize(Roles = "Admin, Finance")]
    public class DepositController : Controller
    {
        private readonly DepositBLL depositBLL;
        private readonly SelLangBLL selLangBLL;
        private readonly PendingFundBLL pendingFundBLL;
        private readonly MessageBLL messageBLL;
        public DepositController(DepositBLL depositBLL, SelLangBLL selLangBLL, PendingFundBLL pendingFundBLL, MessageBLL messageBLL)
        {
            this.depositBLL = depositBLL;
            this.selLangBLL = selLangBLL;
            this.pendingFundBLL = pendingFundBLL;
            this.messageBLL = messageBLL;
        }

        public class DepositReq
        {
            public decimal ActualDeposit { set; get; }
            public string Comment { set; get; }
            public string TransId { set; get; }
            public int Action { set; get; }
        }

        public IActionResult DepositList()
        {
            return View();
        }

        public IActionResult DepositRequest(string transId)
        {
            //DepositRequest depositRequest = depositBLL.GetDepositRequest(transId);
            //IEnumerable<Comments> comments = depositBLL.GetDepositRequestComment(transId);

            //switch (depositRequest.DepositType)
            //{
            //    case (int)DepositType.OnlineBanking:
            //        depositRequest.DepositTypeString = selLangBLL.GetResourceValue("lbl_OnlineBanking");
            //        break;
            //    case (int)DepositType.MobilePayment:
            //        depositRequest.DepositTypeString = selLangBLL.GetResourceValue("lbl_MobilePayment");
            //        break;
            //    case (int)DepositType.WireTransfer:
            //        depositRequest.DepositTypeString = selLangBLL.GetResourceValue("lbl_WireTransfer");
            //        break;
            //}

            //foreach (var item in comments)
            //{
            //    switch (item.Action)
            //    {
            //        case (int)DepositStatus.Draft:
            //            item.ActionString = selLangBLL.GetMsg("msg_Draft");
            //            break;
            //        case (int)DepositStatus.Pending:
            //            item.ActionString = selLangBLL.GetMsg("msg_Pending");
            //            break;
            //        case (int)DepositStatus.Paid:
            //            item.ActionString = selLangBLL.GetMsg("msg_Paid");
            //            break;
            //        case (int)DepositStatus.Approved:
            //            item.ActionString = selLangBLL.GetMsg("msg_Approved");
            //            break;
            //        case (int)DepositStatus.Rejected:
            //            item.ActionString = selLangBLL.GetMsg("msg_Rejected");
            //            break;
            //        case (int)DepositStatus.Canceled:
            //            item.ActionString = selLangBLL.GetMsg("msg_Canceled");
            //            break;
            //        case (int)DepositStatus.Expired:
            //            item.ActionString = selLangBLL.GetMsg("msg_Expired");
            //            break;
            //        case (int)DepositStatus.Error:
            //            item.ActionString = selLangBLL.GetMsg("msg_Error");
            //            break;
            //    }
            //}
            //depositRequest.Comments = comments;
            return View();
        }

        [HttpPost]
        public ApiResult DepositConfirm([FromBody]DepositReq depositReq)
        {
            var apiResult = new ApiResult(selLangBLL);
            var depositConfirmReuslt = -1;

            if (depositReq.Action == 1)
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
            else if (depositReq.Action == 2)
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
            else
            {
                depositConfirmReuslt = depositBLL.DepositDraft(depositReq.TransId, depositReq.ActualDeposit, depositReq.Comment, User.Identity.Name);
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

        [HttpGet]
        public IActionResult GetDepositReceiptPic(string transId)
        {
            var depositReceipt = depositBLL.GetDepositReceipt(transId);
            if (depositReceipt == null)
            {
                return StatusCode(404);
            }
            var image = depositReceipt.DepositPic;
            return File(image, "image/jpeg");
        }
    }
}
