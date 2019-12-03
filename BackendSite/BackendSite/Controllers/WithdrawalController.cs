using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;

namespace BackendSite.Controllers
{
    public class WithdrawalController : Controller
    {
        private readonly WithdrawalBLL withdrawalBLL;
        private readonly SelLangBLL selLangBLL;

        public WithdrawalController(WithdrawalBLL withdrawBLL, SelLangBLL selLangBLL)
        {
            this.withdrawalBLL = withdrawBLL;
            this.selLangBLL = selLangBLL;
        }

        public class WithdrawalReq
        {
            public string Command { set; get; }
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
            withdrawalRequest.Comments = comments;
            return View(withdrawalRequest);
        }

        [HttpPost]
        public ApiResult WithdrawalConfirm([FromBody]WithdrawalReq withdrawalReq)
        {
            var apiResult = new ApiResult(selLangBLL);
            //var apiResult = new ApiResult();
            var withdrawConfirmReuslt = 99;

            if (withdrawalReq.Action == 1)
            {
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalApproved(withdrawalReq.TransId, withdrawalReq.Command);
            }
            else if (withdrawalReq.Action == 2)
            {
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalRejectd(withdrawalReq.TransId, withdrawalReq.Command);
            }
            else
            {
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalDraft(withdrawalReq.TransId, withdrawalReq.Command);
            }

            if (withdrawConfirmReuslt != 0)
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