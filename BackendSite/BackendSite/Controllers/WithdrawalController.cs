using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
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
        public IActionResult WithdrawalList()
        {
            return View();
        }

        //public IActionResult WithdrawalRequest(string transId)
        //{
        //    WithdrawalRequest withdrawalRequest = withdrawalBLL.GetWithdrawalRequest(transId);
        //    IEnumerable<Comments> comments = withdrawalBLL.GetWithdrawalRequestComment(transId);

        //    foreach (var item in comments)
        //    {
        //        switch (item.Action)
        //        {
        //            case (int)WithdrawalStatus.Pending:
        //                item.ActionString = selLangBLL.GetMsg("msg_Pending");
        //                break;
        //            case (int)WithdrawalStatus.OnHold:
        //                item.ActionString = selLangBLL.GetMsg("msg_OnHold");
        //                break;
        //            case (int)WithdrawalStatus.Verify:
        //                item.ActionString = selLangBLL.GetMsg("msg_Verify");
        //                break;
        //            case (int)WithdrawalStatus.Process:
        //                item.ActionString = selLangBLL.GetMsg("msg_Processing");
        //                break;
        //            case (int)WithdrawalStatus.Cancelled:
        //                item.ActionString = selLangBLL.GetMsg("msg_Cancelled");
        //                break;
        //            case (int)WithdrawalStatus.Approved:
        //                item.ActionString = selLangBLL.GetMsg("msg_Approved");
        //                break;
        //            case (int)WithdrawalStatus.Rejected:
        //                item.ActionString = selLangBLL.GetMsg("msg_Rejected");
        //                break;
        //            case (int)WithdrawalStatus.Error:
        //                item.ActionString = selLangBLL.GetMsg("msg_Error");
        //                break;
        //        }
        //    }

        //    //withdrawalRequest.Comments = comments;
        //    return View(withdrawalRequest);
        //}
    }
}