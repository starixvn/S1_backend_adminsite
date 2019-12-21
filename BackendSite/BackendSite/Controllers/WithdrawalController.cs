using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;

namespace BackendSite.Controllers
{
    [Authorize(Roles = "Admin, Finance")]
    public class WithdrawalController : Controller
    {
        private readonly WithdrawalBLL withdrawalBLL;
        private readonly SelLangBLL selLangBLL;
        private readonly MessageBLL messageBLL;

        public WithdrawalController(WithdrawalBLL withdrawBLL, SelLangBLL selLangBLL, MessageBLL messageBLL)
        {
            this.withdrawalBLL = withdrawBLL;
            this.selLangBLL = selLangBLL;
            this.messageBLL = messageBLL;
        }

        public class WithdrawalReq
        {
            public string Comment { set; get; }
            public string TransId { set; get; }
            public int Action { set; get; }
        }

        public IActionResult WithdrawalList()
        {
            IEnumerable<WithdrawalRequest> withdrawRequests = withdrawalBLL.GetWithdrawalRequestList();

            return View(withdrawRequests.ToList());
        }

        public IActionResult WithdrawalRequest(string transId)
        {
            WithdrawalRequest withdrawalRequest = withdrawalBLL.GetWithdrawalRequest(transId);
            IEnumerable<Comments> comments = withdrawalBLL.GetWithdrawalRequestComment(transId);

            foreach (var item in comments)
            {
                switch (item.Action)
                {
                    case (int)DepositStatus.Draft:
                        item.ActionString = selLangBLL.GetMsg("msg_Draft");
                        break;
                    case (int)DepositStatus.Pending:
                        item.ActionString = selLangBLL.GetMsg("msg_Pending");
                        break;
                    case (int)DepositStatus.Paid:
                        item.ActionString = selLangBLL.GetMsg("msg_Paid");
                        break;
                    case (int)DepositStatus.Approved:
                        item.ActionString = selLangBLL.GetMsg("msg_Approved");
                        break;
                    case (int)DepositStatus.Rejected:
                        item.ActionString = selLangBLL.GetMsg("msg_Rejected");
                        break;
                    case (int)DepositStatus.Canceled:
                        item.ActionString = selLangBLL.GetMsg("msg_Canceled");
                        break;
                    case (int)DepositStatus.Expired:
                        item.ActionString = selLangBLL.GetMsg("msg_Expired");
                        break;
                    case (int)DepositStatus.Error:
                        item.ActionString = selLangBLL.GetMsg("msg_Error");
                        break;
                }
            }

            withdrawalRequest.Comments = comments;
            return View(withdrawalRequest);
        }

        [HttpPost]
        public ApiResult WithdrawalConfirm([FromBody]WithdrawalReq withdrawalReq)
        {
            var apiResult = new ApiResult(selLangBLL);
            //var apiResult = new ApiResult();
            var withdrawConfirmReuslt = -1;

            if (withdrawalReq.Action == 1)
            {
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalApproved(withdrawalReq.TransId, withdrawalReq.Comment);

                if(withdrawConfirmReuslt > 0)
                {
                    // Send message to player
                    string result = selLangBLL.GetResourceValue("lbl_Approved");
                    string sender = selLangBLL.GetResourceValue("lbl_MessageDepoistWithdrawalSender");
                    string title = selLangBLL.GetResourceValue("lbl_MessageWithdrawalTitle");
                    title = string.Format(title, withdrawalReq.TransId, result);
                    string content = selLangBLL.GetResourceValue("lbl_MessageWithdrawalContent");
                    content = string.Format(content, withdrawalReq.TransId, result);
                    messageBLL.SendMessageToPlayer(sender, title, content, withdrawConfirmReuslt);
                }
            }
            else if (withdrawalReq.Action == 2)
            {
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalRejectd(withdrawalReq.TransId, withdrawalReq.Comment);

                if (withdrawConfirmReuslt > 0)
                {
                    // Send message to player
                    string result = selLangBLL.GetResourceValue("lbl_Rejected");
                    string sender = selLangBLL.GetResourceValue("lbl_MessageDepoistWithdrawalSender");
                    string title = selLangBLL.GetResourceValue("lbl_MessageWithdrawalTitle");
                    title = string.Format(title, withdrawalReq.TransId, result);
                    string content = selLangBLL.GetResourceValue("lbl_MessageWithdrawalContent");
                    content = string.Format(content, withdrawalReq.TransId, result);
                    messageBLL.SendMessageToPlayer(sender, title, content, withdrawConfirmReuslt);
                }
            }
            else
            {
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalDraft(withdrawalReq.TransId, withdrawalReq.Comment);
            }

            if (withdrawConfirmReuslt <= 0)
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
    }
}