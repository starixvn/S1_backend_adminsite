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
            return View();
        }

        public IActionResult WithdrawalRequest(string transId)
        {
            WithdrawalRequest withdrawalRequest = withdrawalBLL.GetWithdrawalRequest(transId);
            IEnumerable<Comments> comments = withdrawalBLL.GetWithdrawalRequestComment(transId);

            foreach (var item in comments)
            {
                switch (item.Action)
                {
                    case (int)WithdrawalStatus.Pending:
                        item.ActionString = selLangBLL.GetMsg("msg_Pending");
                        break;
                    case (int)WithdrawalStatus.OnHold:
                        item.ActionString = selLangBLL.GetMsg("msg_OnHold");
                        break;
                    case (int)WithdrawalStatus.Verify:
                        item.ActionString = selLangBLL.GetMsg("msg_Verify");
                        break;
                    case (int)WithdrawalStatus.Process:
                        item.ActionString = selLangBLL.GetMsg("msg_Processing");
                        break;
                    case (int)WithdrawalStatus.Cancelled:
                        item.ActionString = selLangBLL.GetMsg("msg_Cancelled");
                        break;
                    case (int)WithdrawalStatus.Approved:
                        item.ActionString = selLangBLL.GetMsg("msg_Approved");
                        break;
                    case (int)WithdrawalStatus.Rejected:
                        item.ActionString = selLangBLL.GetMsg("msg_Rejected");
                        break;
                    case (int)WithdrawalStatus.Error:
                        item.ActionString = selLangBLL.GetMsg("msg_Error");
                        break;
                }
            }

            //withdrawalRequest.Comments = comments;
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
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalVerify(withdrawalReq.TransId, withdrawalReq.Comment);
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