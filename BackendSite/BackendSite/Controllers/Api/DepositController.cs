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
using Newtonsoft.Json;
using BackendSite.ExceptionHandling;

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
        private readonly ExcelBLL excelBLL;
        private readonly int mainWalletType;


        public DepositController(IConfiguration configuration, DepositBLL depositBLL, SelLangBLL selLangBLL, MessageBLL messageBLL, PendingFundBLL pendingFundBLL, CustInfoBLL custInfoBLL, BankDetailBLL bankDetailBLL, ExcelBLL excelBLL)
        {
            this.depositBLL = depositBLL;
            this.selLangBLL = selLangBLL;
            this.messageBLL = messageBLL;
            this.pendingFundBLL = pendingFundBLL;
            this.custInfoBLL = custInfoBLL;
            this.bankDetailBLL = bankDetailBLL;
            this.excelBLL = excelBLL;
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
        public ApiListResultCurrencyAmount<DepositRequest> GetDepositRequestList(DepositListReq depositListReq)
        {
            var apiListResult = new ApiListResultCurrencyAmount<DepositRequest>();
            int pageNumber = depositListReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            List<string> CurrencyAmount = new List<string>();
            var depositRequestList = depositBLL.GetDepositRequestList(depositListReq.SelectSite, depositListReq.DepositType, depositListReq.startDate, depositListReq.endDate, depositListReq.DepositId, depositListReq.UserName, depositListReq.CurrencyId, depositListReq.status, depositListReq.LevelStatus, 10, ref pageNumber, out totalPages, out rowCount, ref CurrencyAmount);
            apiListResult.Data = depositRequestList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            apiListResult.CurrencyAmount = CurrencyAmount;

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

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use：
        /// CSD > Customer Administration > CustInfo > Deposit Info
        /// </remarks>
        /// <param name="depositListReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult GetDepositListByCust(DepositListReq1 depositListReq)
        {
            int pageNumber = depositListReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            decimal totalAmount = 0;
            var data = depositBLL.GetDepositListByCust(depositListReq.CustId,
                                                       depositListReq.TransId,
                                                       depositListReq.Status,
                                                       depositListReq.startDate,
                                                       depositListReq.endDate,
                                                       10,
                                                       ref pageNumber,
                                                       out totalPages,
                                                       out rowCount,
                                                       out totalAmount);

            return new ApiListResultTotalAmount<DepositRequest>
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                RowCount = rowCount,
                TotalAmount = totalAmount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

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
                        string sender = selLangBLL.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
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
                    string sender = selLangBLL.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositSubmission" })]
        public ApiResult DepositSubmission([FromForm] DepositDetailSubmission deposit)
        {
            var depositDetails = JsonConvert.DeserializeObject<DepositDetailInfo>(deposit.DepositDetails);

            var result = depositBLL.CreateDeposit(depositDetails, deposit.Receipt);
            var apiResult = new ApiResult();
            if (result == 1)
            {
                apiResult.ErrorCode = 1;
                apiResult.Message = selLangBLL.GetMsg("msg_InsertSuccessful");
            }
            else if (result == -1)
            {
                apiResult.ErrorCode = -1;
                apiResult.Message = selLangBLL.GetMsg("msg_UserNotFound");
            }
            else
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_InsertFailed");
            }
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        public ApiResult AddDepositLimitInfo(DepositLimitInfo depositLimitInfo)
        {
            var result = depositBLL.AddDepositLimit(depositLimitInfo,User.Identity.Name);
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
            var result = depositBLL.EditDepositLimit(depositLimitInfo,User.Identity.Name);
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
            var result = depositBLL.DeleteDepositLimit(depositLimitInfo,User.Identity.Name);
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
                    string sender = selLangBLL.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
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

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositTimeReport" })]
        public ApiListResult<DepositTimeList> DepositTimeReportGet(DepositTimeReportPara para)
        {
            var apiListResult = new ApiListResult<DepositTimeList>();
            try
            {
                var SearchResult = depositBLL.DepositTimeReportGet(para);
                apiListResult.Data = SearchResult.DepositTimeList;
                apiListResult.PageNumber = SearchResult.PageNumber;
                apiListResult.TotalPages = SearchResult.TotalPages;
                apiListResult.RowCount = SearchResult.RowCount;
                apiListResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiListResult.ErrorCode = 999;
                apiListResult.Message = ex.Message;
            }
            return apiListResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositTimeReport" })]
        public IActionResult ExportDepositTime(string SelectSite, string DepositID, long StartDate, long EndDate, int Status, int CurrencyId)
        {
            var ExportData = depositBLL.DepositTimeExportGet(SelectSite, DepositID, StartDate, EndDate, Status, CurrencyId);
            var stream = excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DepositTime_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
        [HttpPost] 
        public ApiResult SetDepositRequestAmount(DepositRequestAmountPara para)
        {
            var depositRequest = depositBLL.GetDepositRequest(para.TransId);
            if(depositRequest == null)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
            }
            if (depositBLL.SetDepositRequestAmount(para.TransId,para.Amount, User.Identity.Name, depositRequest.CustId) >0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Edit Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
            }
        }
    }
}