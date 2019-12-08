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

namespace BackendSite.Controllers
{
    public class DepositController : Controller
    {
        private readonly DepositBLL depositBLL;
        private readonly SelLangBLL selLangBLL;

        public DepositController(DepositBLL depositBLL, SelLangBLL selLangBLL)
        {
            this.depositBLL = depositBLL;
            this.selLangBLL = selLangBLL;
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
            IEnumerable<DepositRequest> depositRequests = depositBLL.GetDepositRequestList();

            foreach (var item in depositRequests)
            {
                switch (item.DispositType)
                {
                    case (int)DispositType.OnlineBanking:
                        item.DispositTypeString = selLangBLL.GetResourceValue("lbl_OnlineBanking");
                        break;
                    case (int)DispositType.MobilePayment:
                        item.DispositTypeString = selLangBLL.GetResourceValue("lbl_MobilePayment");
                        break;
                    case (int)DispositType.WireTransfer:
                        item.DispositTypeString = selLangBLL.GetResourceValue("lbl_WireTransfer");
                        break;
                }

                switch (item.Status)
                {
                    case (int)DepositStatus.Draft:
                        item.StatusString = selLangBLL.GetMsg("msg_Draft");
                        break;
                    case (int)DepositStatus.Pending:
                        item.StatusString = selLangBLL.GetMsg("msg_Pending");
                        break;
                    case (int)DepositStatus.Paid:
                        item.StatusString = selLangBLL.GetMsg("msg_Paid");
                        break;
                    case (int)DepositStatus.Approved:
                        item.StatusString = selLangBLL.GetMsg("msg_Approved");
                        break;
                    case (int)DepositStatus.Rejected:
                        item.StatusString = selLangBLL.GetMsg("msg_Rejected");
                        break;
                    case (int)DepositStatus.Canceled:
                        item.StatusString = selLangBLL.GetMsg("msg_Canceled");
                        break;
                    case (int)DepositStatus.Expired:
                        item.StatusString = selLangBLL.GetMsg("msg_Expired");
                        break;
                    case (int)DepositStatus.Error:
                        item.StatusString = selLangBLL.GetMsg("msg_Error");
                        break;
                }
            }

            return View(depositRequests.ToList());
        }

        public IActionResult DepositRequest(string transId)
        {
            DepositRequest depositRequest = depositBLL.GetDepositRequest(transId);
            IEnumerable<Comments> comments = depositBLL.GetDepositRequestComment(transId);

            switch (depositRequest.DispositType)
            {
                case (int)DispositType.OnlineBanking:
                    depositRequest.DispositTypeString = selLangBLL.GetResourceValue("lbl_OnlineBanking");
                    break;
                case (int)DispositType.MobilePayment:
                    depositRequest.DispositTypeString = selLangBLL.GetResourceValue("lbl_MobilePayment");
                    break;
                case (int)DispositType.WireTransfer:
                    depositRequest.DispositTypeString = selLangBLL.GetResourceValue("lbl_WireTransfer");
                    break;
            }

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
            depositRequest.Comments = comments;
            return View(depositRequest);
        }

        [HttpPost]
        public ApiResult DepositConfirm([FromBody]DepositReq depositReq)
        {
            var apiResult = new ApiResult(selLangBLL);
            //var apiResult = new ApiResult();
            var depositConfirmReuslt = 99;

            if (depositReq.Action == 1)
            {
                if (depositReq.ActualDeposit > 0)
                     depositConfirmReuslt = depositBLL.DepositApproved(depositReq.TransId, depositReq.ActualDeposit, depositReq.Comment);
            }
            else if (depositReq.Action == 2)
            {
                 depositConfirmReuslt = depositBLL.DepositRejectd(depositReq.TransId, depositReq.ActualDeposit, depositReq.Comment);
            }
            else
            {
                 depositConfirmReuslt = depositBLL.DepositDraft(depositReq.TransId, depositReq.ActualDeposit, depositReq.Comment);
            }

            if (depositConfirmReuslt !=0)
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
