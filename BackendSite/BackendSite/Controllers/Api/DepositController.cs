using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize]
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
        private readonly int mainWalletType;

        public DepositController(IConfiguration configuration, DepositBLL depositBLL, SelLangBLL selLangBLL, MessageBLL messageBLL, PendingFundBLL pendingFundBLL, CustInfoBLL custInfoBLL, BankDetailBLL bankDetailBLL)
        {
            this.depositBLL = depositBLL;
            this.selLangBLL = selLangBLL;
            this.messageBLL = messageBLL;
            this.pendingFundBLL = pendingFundBLL;
            this.custInfoBLL = custInfoBLL;
            this.bankDetailBLL = bankDetailBLL;
            mainWalletType = configuration.GetValue<int>("MainWalletType");
        }
        public class DepositListReq
        {
            public IEnumerable<int> SelectSite { get; set; }
            public int PageNumber { get; set; }
            public string DepositId { get; set; }
            public string UserName { get; set; }
            public IEnumerable<int> DepositType { get; set; }
            public IEnumerable<int> CurrencyId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
            public IEnumerable<int> status { get; set; }
            public IEnumerable<int> LevelStatus { get; set; }
        }

        [HttpPost]
        public ApiListResult<DepositRequest> GetDepositRequestList(DepositListReq depositListReq)
        {
            var apiListResult = new ApiListResult<DepositRequest>();
            int pageNumber = depositListReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var depositRequestList = depositBLL.GetDepositRequestList(depositListReq.SelectSite, depositListReq.DepositType, depositListReq.startDate, depositListReq.endDate, depositListReq.DepositId, depositListReq.UserName, depositListReq.CurrencyId, depositListReq.status, depositListReq.LevelStatus,10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.Data = depositRequestList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }

        public class DepositListReq1
        {
            public int PageNumber { get; set; }
            public int CustId { get; set; }
            public string TransId { get; set; }
            public int? Status { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
        }

        [HttpPost]
        public ApiListResult<DepositRequest> GetDepositListByCust(DepositListReq1 depositListReq)
        {
            var apiListResult = new ApiListResult<DepositRequest>();
            int pageNumber = depositListReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var depositRequestList = depositBLL.GetDepositListByCust(depositListReq.CustId, depositListReq.TransId, depositListReq.Status, depositListReq.startDate, depositListReq.endDate, 10, ref pageNumber, out totalPages, out rowCount);
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
            try
            {
                var depositRequest = depositBLL.GetDepositRequest(depositReq.TransId);

                apiResult.Data = depositRequest;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
            }

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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList", "f_dp_execute" })]
        [HttpPost]
        public async Task<ApiResult> DepositConfirmAsync(DepositReq1 depositReq)
        {
            var apiResult = new ApiResult(selLangBLL);
            var depositConfirmReuslt = -1;

            if (depositReq.Action == (int)DepositStatus.Approved)
            {
                if (depositReq.ActualDeposit > 0)
                    depositConfirmReuslt = depositBLL.DepositApproved(depositReq.TransId, depositReq.ActualDeposit, depositReq.Comment, User.Identity.Name);

                if (depositConfirmReuslt > 0)
                {
                    ApiResult<TransferResult> tmp = new ApiResult<TransferResult>();
                    if (mainWalletType == 2)
                    {
                        PendingFund pendingFund = new PendingFund();
                        pendingFund.CustId = depositConfirmReuslt;
                        tmp = await Process(pendingFund);
                    }
                    else
                    {
                        tmp.ErrorCode = 0;
                    }

                    if (tmp.ErrorCode != 0)
                    {
                        depositConfirmReuslt = -4;
                    }
                    else
                    {
                        var depositRequest = depositBLL.GetDepositRequest(depositReq.TransId);
                        var userLang = custInfoBLL.GetCustInfoWithCache(depositRequest.CustId).UserLang;
                        // Send message to player
                        string sender = selLangBLL.GetResourceValue("lbl_MessageDepoistWithdrawalSender", userLang);
                        string title = selLangBLL.GetResourceValue("lbl_DepositApprovedTitle", userLang);
                        title = string.Format(title, depositReq.TransId);
                        string content = selLangBLL.GetResourceValue("lbl_DepositApprovedContent", userLang);
                        content = string.Format(content, depositReq.TransId);
                        messageBLL.SendMessageToPlayer(sender, title, content, depositRequest.CustId);
                    }
                }
            }
            else if (depositReq.Action == (int)DepositStatus.Rejected)
            {
                depositConfirmReuslt = depositBLL.DepositRejectd(depositReq.TransId, depositReq.ActualDeposit, depositReq.Comment, User.Identity.Name);

                if (depositConfirmReuslt > 0)
                {
                    var depositRequest = depositBLL.GetDepositRequest(depositReq.TransId);
                    var userLang = custInfoBLL.GetCustInfoWithCache(depositRequest.CustId).UserLang;
                    // Send message to player
                    string sender = selLangBLL.GetResourceValue("lbl_MessageDepoistWithdrawalSender", userLang);
                    string title = selLangBLL.GetResourceValue("lbl_DepositRejectTitle", userLang);
                    title = string.Format(title, depositReq.TransId);
                    string content = selLangBLL.GetResourceValue("lbl_DepositRejectContent", userLang);
                    content = string.Format(content, depositReq.TransId);
                    messageBLL.SendMessageToPlayer(sender, title, content, depositRequest.CustId);
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
        public async Task<ApiResult<TransferResult>> Process(PendingFund pendingFund)
        {
            return await pendingFundBLL.ProcessAsync(pendingFund);
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        public ApiResult<IEnumerable<DepositLimitCurrencyInfo>> GetDepositLimitCurrencyList(DepositLimit depositLimit)
        {
            var apiResult = new ApiResult<IEnumerable<DepositLimitCurrencyInfo>>();
            var depositLimitCurrencyList = depositBLL.GetDepositLimitCurrencyList(depositLimit.SiteId, depositLimit.DepositType);
            apiResult.Data = depositLimitCurrencyList;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        public ApiResult<IEnumerable<DepositLimitInfo>> GetDepositLimitInfos(DepositLimit depositLimit)
        {
            var apiResult = new ApiResult<IEnumerable<DepositLimitInfo>>();
            var depositLimitInfos = depositBLL.GetDepositLimitInfos(depositLimit.SiteId, depositLimit.DepositType, depositLimit.CurrencyId);
            apiResult.Data = depositLimitInfos;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        public ApiResult AddDepositLimitInfo(DepositLimitInfo depositLimitInfo)
        {
            var result = depositBLL.AddDepositLimit(depositLimitInfo);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        public ApiResult EditDepositLimitInfo(DepositLimitInfo depositLimitInfo)
        {
            var result = depositBLL.EditDepositLimit(depositLimitInfo);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        [HttpPost]
        public ApiResult DeleteDepositLimitInfo(DepositLimitInfo depositLimitInfo)
        {
            var result = depositBLL.DeleteDepositLimit(depositLimitInfo);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        public class BatchRejectedDepositList
        {
            public IEnumerable<string> TransId { get; set; }
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList", "f_dp_execute" })]
        [HttpPost]
        public List<ApiResult> BatchRejectedDeposit(BatchRejectedDepositList batchRejectedDepositList)
        {
            var apiResult = new List<ApiResult>();
            var depositConfirmReuslt = -1;
            var Comment = $"{ User.Identity.Name} Rejected Deposit Multiple";

            foreach (string _transId in batchRejectedDepositList.TransId)
            {
                var aaa = _transId;
                depositConfirmReuslt = depositBLL.DepositRejectd(_transId, 0, Comment, User.Identity.Name);

                if (depositConfirmReuslt > 0)
                {
                    var depositRequest = depositBLL.GetDepositRequest(_transId);
                    var userLang = custInfoBLL.GetCustInfoWithCache(depositRequest.CustId).UserLang;
                    // Send message to player
                    string sender = selLangBLL.GetResourceValue("lbl_MessageDepoistWithdrawalSender", userLang);
                    string title = selLangBLL.GetResourceValue("lbl_DepositRejectTitle", userLang);
                    title = string.Format(title, _transId);
                    string content = selLangBLL.GetResourceValue("lbl_DepositRejectContent", userLang);
                    content = string.Format(content, _transId);
                    messageBLL.SendMessageToPlayer(sender, title, content, depositRequest.CustId);
                }
                if (depositConfirmReuslt == -4)
                {
                    apiResult.Add(new ApiResult(selLangBLL) { ErrorCode = 1102, Message = $"{_transId} {selLangBLL.GetMsg("msg_UpdateFailedtoSB")}" });
                }
                else if (depositConfirmReuslt == 0)
                {
                    apiResult.Add(new ApiResult(selLangBLL) { ErrorCode = 1101, Message = $"{_transId} {selLangBLL.GetMsg("msg_UpdateFailed")}" });
                }
                else
                {
                    apiResult.Add(new ApiResult(selLangBLL) { ErrorCode = 0, Message = $"{_transId} {selLangBLL.GetMsg("msg_Updatesuccessful")}" });
                }
            }
            return apiResult;
        }


    }
}