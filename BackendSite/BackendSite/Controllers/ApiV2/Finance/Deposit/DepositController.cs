using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Finance.Deposit
{
    //[Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositController : Controller
    {
        #region Fields

        private readonly DepositBLL _depositBll;
        private readonly SelLangBLL _selLangBll;
        private readonly MessageBLL _messageBll;
        private readonly PendingFundBLL _pendingFundBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly BankDetailBLL _bankDetailBll;
        private readonly ExcelBLL _excelBll;
        private readonly int _mainWalletType;

        #endregion

        #region Constructors
        
        public DepositController(IServiceProvider provider,IConfiguration configuration)
        {
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _pendingFundBll = provider.GetRequiredService<PendingFundBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _mainWalletType = configuration.GetValue<int>("MainWalletType");
        }

        #endregion

        #region ApiModel

        public class DepositListReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SelectSite { get; set; }
            public int PageNumber { get; set; }
            public string DepositId { get; set; }
            public string UserName { get; set; }
            public IEnumerable<int> DepositType { get; set; }
            public IEnumerable<int> CurrencyId { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public IEnumerable<int> Status { get; set; }
            public IEnumerable<int> LevelStatus { get; set; }
            public int AmountType { get; set; }
            public string Amount { get; set; }
        }
        
        public class DepositReq
        {
            public string TransId { get; set; }
        }
        
        public class DepositConfirmReq : DepositReq
        {
            public decimal ActualDeposit { set; get; }
            public string Comment { set; get; }
            public int Action { set; get; }
        }
        
        public class BatchRejectedDepositList
        {
            public IEnumerable<string> TransId { get; set; }
        }
        
        public class DepositRequestAmountReq 
        {
            public string TransId { get; set; }
            public decimal Amount { get; set; }
        }
        
        public class ExportDepositListReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public string DepositId { get; set; }
            public string UserName { get; set; }
            public IEnumerable<int> DepositType { get; set; }
            public IEnumerable<int> CurrencyId { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public IEnumerable<int> Status { get; set; }
            public IEnumerable<int> LevelStatus { get; set; }
            public int AmountType { get; set; }
            public string Amount { get; set; }
        }

        #endregion

        /// <summary>
        /// Deposit/GetDepositRequestList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiListResultCurrencyAmount<DepositRequest> GetDepositRequestList(DepositListReq req)
        {
            var apiListResult = new ApiListResultCurrencyAmount<DepositRequest>();
            var pageNumber = req.PageNumber;
            var currencyAmount = new List<string>();
            
            var depositList = _depositBll.GetDepositRequestList(req.SelectSite, req.DepositType, req.StartDate.DateTime, req.EndDate.DateTime, req.DepositId, req.UserName, req.CurrencyId, req.Status, req.LevelStatus, 10, req.AmountType, req.Amount, ref pageNumber, out var totalPages, out var rowCount, ref currencyAmount);
            
            foreach (var data in depositList)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                if (data.DepositTime != null)
                    data.DepositTime = new DateTimeOffset(data.DepositTime.Value.DateTime, TimeSpan.Zero);
                if (data.UpdateTime != null)
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.Value.DateTime, TimeSpan.Zero);
            }
             
            apiListResult.Data = depositList;
            apiListResult.ErrorCode = (int) ErrorCodeEnum.Success;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            apiListResult.CurrencyAmount = currencyAmount;

            return apiListResult;
        }
        
        /// <summary>
        /// Deposit/GetDepositRequest 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit] > ID
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<DepositRequest> GetDepositRequest(DepositReq req)
        {
            var apiResult = new ApiResult<DepositRequest>();
            try
            {
                var depositRequest = _depositBll.GetDepositRequest(req.TransId);

                depositRequest.CreateTime = new DateTimeOffset(depositRequest.CreateTime.DateTime, TimeSpan.Zero);
                if (depositRequest.DepositTime != null)
                    depositRequest.DepositTime = new DateTimeOffset(depositRequest.DepositTime.Value.DateTime, TimeSpan.Zero);
                if (depositRequest.UpdateTime != null)
                    depositRequest.UpdateTime = new DateTimeOffset(depositRequest.UpdateTime.Value.DateTime, TimeSpan.Zero);
                
                apiResult.Data = depositRequest;
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }
        
        /// <summary>
        /// Deposit/GetDepositRequestComment 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit] > ID
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<IEnumerable<Comments>> GetDepositRequestComment(DepositReq req)
        {
            var apiResult = new ApiResult<IEnumerable<Comments>>();
            var comments = _depositBll.GetDepositRequestComment(req.TransId);
            
            foreach (var data in comments)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            }
            
            apiResult.Data = comments;
            apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            
            return apiResult;
        }
        
        /// <summary>
        /// Deposit/DepositConfirm 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit] > ID > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList", "f_dp_execute" })]
        [HttpPost]
        public async Task<ApiResult> DepositConfirmAsync(DepositConfirmReq req)
        {
            var apiResult = new ApiResult(_selLangBll);
            var depositConfirmResult = (int) ErrorCodeEnum.Fail;

            if (req.Action == (int)DepositStatus.Approved)
            {
                if (req.ActualDeposit > 0)
                    depositConfirmResult = _depositBll.DepositApproved(req.TransId, req.ActualDeposit, req.Comment, User.Identity.Name);

                if (depositConfirmResult > 0)
                {
                    var tmp = new ApiResult<TransferResult>();
                    if (_mainWalletType == 2)
                    {
                        var pendingFund = new PendingFund
                        {
                            CustId = depositConfirmResult
                        };
                        tmp = await Process(pendingFund);
                    }
                    else
                    {
                        tmp.ErrorCode = (int) ErrorCodeEnum.Success;
                    }

                    if (tmp.ErrorCode != 0)
                    {
                        depositConfirmResult = -4;
                    }
                    else
                    {
                        var depositRequest = _depositBll.GetDepositRequest(req.TransId);
                        var userLang = _custInfoBll.GetCustInfoWithCache(depositRequest.CustId).UserLang;
                        // Send message to player
                        var sender = _selLangBll.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                        var title = _selLangBll.GetResourceValue("lbl_DepositApprovedTitle", userLang);
                        title = string.Format(title, req.TransId);
                        var content = _selLangBll.GetResourceValue("lbl_DepositApprovedContent", userLang);
                        content = string.Format(content, req.TransId);
                        _messageBll.SendMessageToPlayer(sender, title, content, depositRequest.CustId);
                    }
                }
            }
            else if (req.Action == (int)DepositStatus.Rejected)
            {
                depositConfirmResult = _depositBll.DepositRejectd(req.TransId, req.ActualDeposit, req.Comment, User.Identity.Name);

                if (depositConfirmResult > 0)
                {
                    var depositRequest = _depositBll.GetDepositRequest(req.TransId);
                    var custInfo = _custInfoBll.GetCustInfoWithCache(depositRequest.CustId);
                    var userLang = custInfo.UserLang;
                    // Send message to player
                    var sender = _selLangBll.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                    var title = _selLangBll.GetResourceValue("lbl_DepositRejectTitle", userLang);
                    title = string.Format(title, req.TransId);
                    var content = _selLangBll.GetResourceValue("lbl_DepositRejectContent", userLang);
                    content = string.Format(content, req.TransId);
                    _messageBll.SendMessageToPlayer(sender, title, content, depositRequest.CustId);
                    if (custInfo.SiteId == 1)
                    {
                        await _messageBll.SendSMSMessageToPlayer(depositRequest.CustId);
                    }
                }
            }
            else if (req.Action == (int)DepositStatus.Draft)
            {
                depositConfirmResult = _depositBll.DepositDraft(req.TransId, req.ActualDeposit, req.Comment, User.Identity.Name);
            }
            else if (req.Action == (int)DepositStatus.OnHold)
            {
                depositConfirmResult = _depositBll.DepositOnHold(req.TransId, req.ActualDeposit, req.Comment, User.Identity.Name);
            }
            else if (req.Action == (int)DepositStatus.Pending)
            {
                depositConfirmResult = _depositBll.DepositPending(req.TransId, req.ActualDeposit, req.Comment, User.Identity.Name);
            }

            if (depositConfirmResult == -4)
            {
                apiResult.ErrorCode = 1102;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailedtoSB");
            }
            else if (depositConfirmResult == 0)
            {
                apiResult.ErrorCode = 1101;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }

            return apiResult;
        }
        
        /// <summary>
        /// Deposit/BatchRejectedDeposit 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit] > Reject Multiple
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList", "f_dp_execute" })]
        [HttpPost]
        public List<ApiResult> BatchRejectedDeposit(BatchRejectedDepositList req)
        {
            var apiResult = new List<ApiResult>();
            var comment = $"{ User.Identity.Name} Rejected Deposit Multiple";

            foreach (var transId in req.TransId)
            {
                var depositConfirmResult = _depositBll.DepositRejectd(transId, 0, comment, User.Identity.Name);

                if (depositConfirmResult > 0)
                {
                    var depositRequest = _depositBll.GetDepositRequest(transId);
                    var userLang = _custInfoBll.GetCustInfoWithCache(depositRequest.CustId).UserLang;
                    // Send message to player
                    var sender = _selLangBll.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                    var title = _selLangBll.GetResourceValue("lbl_DepositRejectTitle", userLang);
                    title = string.Format(title, transId);
                    var content = _selLangBll.GetResourceValue("lbl_DepositRejectContent", userLang);
                    content = string.Format(content, transId);
                    _messageBll.SendMessageToPlayer(sender, title, content, depositRequest.CustId);
                }
                if (depositConfirmResult == -4)
                {
                    apiResult.Add(new ApiResult(_selLangBll) { ErrorCode = 1102, Message = $"{transId} {_selLangBll.GetMsg("msg_UpdateFailedtoSB")}" });
                }
                else if (depositConfirmResult == 0)
                {
                    apiResult.Add(new ApiResult(_selLangBll) { ErrorCode = 1101, Message = $"{transId} {_selLangBll.GetMsg("msg_UpdateFailed")}" });
                }
                else
                {
                    apiResult.Add(new ApiResult(_selLangBll) { ErrorCode = 0, Message = $"{transId} {_selLangBll.GetMsg("msg_Updatesuccessful")}" });
                }
            }
            return apiResult;
        }
        
        /// <summary>
        /// Deposit/SetDepositRequestAmount 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit] > ID > Request Amount	[Edit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult SetDepositRequestAmount(DepositRequestAmountReq req)
        {
            var depositRequest = _depositBll.GetDepositRequest(req.TransId);
            if (depositRequest == null)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
            }
            if (_depositBll.SetDepositRequestAmount(req.TransId, req.Amount, User.Identity.Name, depositRequest.CustId) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Edit Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
        }
        
        /// <summary>
        /// Deposit/SetDepositRequestAmount 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit] > Export
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public IActionResult ExportDepositList(ExportDepositListReq req)
        {
            var exportDeposit = _depositBll.GetDepositExportList(
                JsonConvert.SerializeObject(req.SiteIds),
                JsonConvert.SerializeObject(req.DepositType),
                req.StartDate.DateTime, 
                req.EndDate.DateTime, 
                req.DepositId,
                req.UserName, 
                JsonConvert.SerializeObject(req.CurrencyId),
                JsonConvert.SerializeObject(req.Status),
                JsonConvert.SerializeObject(req.LevelStatus),
                req.AmountType, 
                req.Amount);

            foreach (var data in exportDeposit)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime,TimeSpan.Zero);
                if(data.DepositTime !=null)
                    data.DepositTime = new DateTimeOffset(data.DepositTime.Value.DateTime,TimeSpan.Zero);
                if (data.UpdateTime != null) 
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.Value.DateTime,TimeSpan.Zero);
            }

            var stream = _excelBll.Export(exportDeposit);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nameof(exportDeposit)}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
        
        private async Task<ApiResult<TransferResult>> Process(PendingFund pendingFund)
        {
            return await _pendingFundBll.ProcessAsync(pendingFund);
        }
    }
}