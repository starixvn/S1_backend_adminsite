using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WithdrawalController : ControllerBase
    {
        private readonly WithdrawalBLL withdrawalBLL;
        private readonly SelLangBLL selLangBLL;
        private readonly MessageBLL messageBLL;

        public WithdrawalController(WithdrawalBLL withdrawalBLL, SelLangBLL selLangBLL, MessageBLL messageBLL)
        {
            this.withdrawalBLL = withdrawalBLL;
            this.selLangBLL = selLangBLL;
            this.messageBLL = messageBLL;
        }

        public class WithdrawalListReq
        {
            public int PageNumber { get; set; }
            public string WithdrawalId { get; set; }
            public string UserName { get; set; }
            public int? withdrawalType { get; set; }
            public int? CurrencyId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
            public int? status { get; set; }
        }

        [HttpPost]
        public ApiListResult<WithdrawalRequest> GetWithdrawalRequestList(WithdrawalListReq withdrawalListReq)
        {
            var apiListResult = new ApiListResult<WithdrawalRequest>();
            int pageNumber = withdrawalListReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var withdrawalRequestList = withdrawalBLL.GetWithdrawalRequestList(withdrawalListReq.withdrawalType, withdrawalListReq.startDate, withdrawalListReq.endDate, withdrawalListReq.WithdrawalId, withdrawalListReq.UserName, withdrawalListReq.CurrencyId, withdrawalListReq.status, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.Data = withdrawalRequestList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }
        public class WithdrawalReq
        {
            public string TransId { get; set; }
        }
        [HttpPost]
        public ApiResult<WithdrawalRequest> GetWithdrawalRequest(WithdrawalReq withdrawalReq)
        {
            var apiResult = new ApiResult<WithdrawalRequest>();
            var withdrawalRequest = withdrawalBLL.GetWithdrawalRequest(withdrawalReq.TransId);
            apiResult.Data = withdrawalRequest;
            apiResult.ErrorCode = 0;
            return apiResult;
        }
        [HttpPost]
        public ApiResult<IEnumerable<Comments>> GetWithdrawalRequestComment(WithdrawalReq withdrawalReq)
        {
            var apiResult = new ApiResult<IEnumerable<Comments>>();
            var comments = withdrawalBLL.GetWithdrawalRequestComment(withdrawalReq.TransId);
            apiResult.Data = comments;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        public class WithdrawalReq2: WithdrawalReq
        {
            public string Comment { set; get; }
            public int Action { set; get; }
        }

        [HttpPost]
        public ApiResult WithdrawalConfirm(WithdrawalReq2 withdrawalReq)
        {
            var apiResult = new ApiResult(selLangBLL);
            //var apiResult = new ApiResult();
            var withdrawConfirmReuslt = -1;

            if (withdrawalReq.Action == (int)WithdrawalStatus.Approved)
            {
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalApproved(withdrawalReq.TransId, withdrawalReq.Comment, User.Identity.Name);

                if (withdrawConfirmReuslt > 0)
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
            else if (withdrawalReq.Action == (int)WithdrawalStatus.Rejected)
            {
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalRejectd(withdrawalReq.TransId, withdrawalReq.Comment, User.Identity.Name);

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
            else if (withdrawalReq.Action == (int)WithdrawalStatus.Verify)
            {
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalVerify(withdrawalReq.TransId, withdrawalReq.Comment, User.Identity.Name);
            }
            else if (withdrawalReq.Action == (int)WithdrawalStatus.OnHold)
            {
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalOnHold(withdrawalReq.TransId, withdrawalReq.Comment, User.Identity.Name);
            }
            else if (withdrawalReq.Action == (int)WithdrawalStatus.Draft)
            {
                withdrawConfirmReuslt = withdrawalBLL.WithdrawalDraft(withdrawalReq.TransId, withdrawalReq.Comment, User.Identity.Name);
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