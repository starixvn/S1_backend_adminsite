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

namespace BackendSite.Controllers
{
    public class DepositController : Controller
    {
        private readonly DepositBLL depositBLL;

        public DepositController(DepositBLL depositBLL)
        {
            this.depositBLL = depositBLL;
        }

        public class DepositReq
        {
            public decimal ActualDeposit { set; get; }
            public string Command { set; get; }
            public string TransId { set; get; }
            public int Action { set; get; }
        }

        public IActionResult DepositRequest()
        {
            IEnumerable<DepositRequest> depositRequests = depositBLL.GetDepositRequestList();

            return View(depositRequests.ToList());
        }

        public IActionResult DepositApprove(string transId)
        {
            DepositRequest depositRequest = depositBLL.GetDepositRequest(transId);
            IEnumerable<Comments> comments = depositBLL.GetDepositRequestComment(transId);
            depositRequest.Comments = comments;
            return View(depositRequest);
        }

        [HttpPost]
        public ApiResult DepositConfirm([FromBody]DepositReq depositReq)
        {
            //var apiResult = new ApiResult(selLangBLL);
            var apiResult = new ApiResult();
            var depositConfirmReuslt = 99;

            if (depositReq.Action == 1)
            {
                if (depositReq.ActualDeposit > 0)
                     depositConfirmReuslt = depositBLL.DepositApproved(depositReq.TransId, depositReq.ActualDeposit, depositReq.Command);
            }
            else if (depositReq.Action == 2)
            {
                 depositConfirmReuslt = depositBLL.DepositRejectd(depositReq.TransId, depositReq.ActualDeposit, depositReq.Command);
            }
            else
            {
                 depositConfirmReuslt = depositBLL.DepositDraft(depositReq.TransId, depositReq.ActualDeposit, depositReq.Command);
            }

            if (depositConfirmReuslt !=0)
            {
                apiResult.ErrorCode = 1101;
                apiResult.Message = "Update Failed!";
                //apiResult.Message = selLangBLL.GetMsg("msg_DepositProcessLogin_15");
            }
            else
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = "Update Scuessed!";
            }

            return apiResult;
        }
    }
}
