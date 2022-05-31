using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Finance.Deposit
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositController : ControllerBase
    {
        #region Fields

        private readonly DepositBLL _depositBll;
        private readonly SelLangBLL _selLangBll;
        private readonly MessageBLL _messageBll;
        private readonly PendingFundBLL _pendingFundBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ExcelBLL _excelBll;
        private readonly int _mainWalletType;

        #endregion

        #region Constructors

        public DepositController(IServiceProvider provider, IConfiguration configuration)
        {
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _pendingFundBll = provider.GetRequiredService<PendingFundBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _mainWalletType = configuration.GetValue<int>("MainWalletType");
        }

        #endregion

        #region ApiModel

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public string DepositId { get; set; }
            public string UserName { get; set; }
            public IEnumerable<int> DepositTypes { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
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
            public IEnumerable<int> Statuses { get; set; }
            public IEnumerable<int> LevelStatuses { get; set; }
            public IEnumerable<string> SelectBankCode { get; set; }
            public IEnumerable<int> SelectCalCode { get; set; }
            public int AmountType { get; set; }
            public string Amount { get; set; }
        }

        public class GetByTransIdRequest
        {
            public string TransId { get; set; }
        }

        public class SendToNextRequest : GetByTransIdRequest
        {
            public decimal ActualDeposit { set; get; }
            public string Comment { set; get; }
            public int Action { set; get; }
        }

        public class SetAmountByTransIdRequest : GetByTransIdRequest
        {
            public decimal Amount { get; set; }
        }

        public class BatchRejectedDepositList
        {
            public IEnumerable<string> TransIds { get; set; }
        }

        #endregion

        /// <summary>
        /// Deposit/GetList 取得存款清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList" })]
        [HttpPost]
        public ApiListResultCurrencyAmount<DepositRequest> GetList(GetListRequest request)
        {
            var apiListResult = new ApiListResultCurrencyAmount<DepositRequest>();
            var pageNumber = request.PageNumber;
            var currencyAmount = new List<string>();

            if (string.IsNullOrWhiteSpace(request.UserName))
                request.UserName = null;

            var depositList = _depositBll.GetDepositRequestList(request.SiteIds,
                request.DepositTypes,
                request.SelectBankCode,
                request.SelectCalCode,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.DepositId,
                request.UserName,
                request.CurrencyIds,
                request.Statuses,
                request.LevelStatuses,
                request.PageSize,
                request.AmountType,
                request.Amount,
                ref pageNumber, out var totalPages, out var rowCount, ref currencyAmount);

            foreach (var data in depositList)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                if (data.DepositTime != null)
                    data.DepositTime = new DateTimeOffset(data.DepositTime.Value.DateTime, TimeSpan.Zero);
                if (data.UpdateTime != null)
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.Value.DateTime, TimeSpan.Zero);
            }

            apiListResult.Data = depositList;
            apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            apiListResult.CurrencyAmount = currencyAmount;

            return apiListResult;
        }

        /// <summary>
        /// Deposit/Export 匯出存款清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList" })]
        [HttpPost]
        public IActionResult Export(GetListRequest request)
        {
            var exportDeposit = _depositBll.GetDepositExportList(
                JsonConvert.SerializeObject(request.SiteIds),
                JsonConvert.SerializeObject(request.SelectBankCode),
                request.SelectCalCode.Any() ? JsonConvert.SerializeObject(request.SelectCalCode) : null ,
                JsonConvert.SerializeObject(request.DepositTypes),
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.DepositId,
                request.UserName,
                JsonConvert.SerializeObject(request.CurrencyIds),
                JsonConvert.SerializeObject(request.Statuses),
                JsonConvert.SerializeObject(request.LevelStatuses),
                request.AmountType,
                request.Amount);

            foreach (var data in exportDeposit)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                if (data.DepositTime != null)
                    data.DepositTime = new DateTimeOffset(data.DepositTime.Value.DateTime, TimeSpan.Zero);
                if (data.UpdateTime != null)
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.Value.DateTime, TimeSpan.Zero);
            }

            var stream = _excelBll.ExportV2(exportDeposit);
            return _excelBll.ExcelFile(stream, "DepositRequestList");
        }

        /// <summary>
        /// Deposit/GetByTransId 取得指定存款資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit] > Click [Trxn.ID] > Deposit Info
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList" })]
        [HttpPost]
        public ApiResult<DepositRequest> GetByTransId(GetByTransIdRequest request)
        {
            var apiResult = new ApiResult<DepositRequest>();
            try
            {
                var depositRequest = _depositBll.GetDepositRequest(request.TransId);

                depositRequest.CreateTime = new DateTimeOffset(depositRequest.CreateTime.DateTime, TimeSpan.Zero);
                if (depositRequest.DepositTime != null)
                    depositRequest.DepositTime = new DateTimeOffset(depositRequest.DepositTime.Value.DateTime, TimeSpan.Zero);
                if (depositRequest.UpdateTime != null)
                    depositRequest.UpdateTime = new DateTimeOffset(depositRequest.UpdateTime.Value.DateTime, TimeSpan.Zero);

                apiResult.Data = depositRequest;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// Deposit/GetCommentByTransId 取得指定存款註解
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit] > Click [Trxn.ID] > Comment List
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList" })]
        [HttpPost]
        public ApiResult<IEnumerable<Comments>> GetCommentByTransId(GetByTransIdRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<Comments>>();
            var comments = _depositBll.GetDepositRequestComment(request.TransId);

            foreach (var data in comments)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            }

            apiResult.Data = comments;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// Deposit/SendToNext 存款送至下一流程
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Request > [Submit] > Click [Trxn.ID] > [Draft] [OnHold] [Pending] [Approved] [Rejected]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList", "f_dp_execute" })]
        [HttpPost]
        public async Task<ApiResult> SendToNextAsync(SendToNextRequest request)
        {
            var apiResult = new ApiResult(_selLangBll);
            var depositConfirmResult = (int)ErrorCodeEnum.Fail;

            if (request.Action == (int)DepositStatusEnum.Approved)
            {
                if (request.ActualDeposit > 0)
                    depositConfirmResult = _depositBll.DepositApproved(request.TransId, request.ActualDeposit, request.Comment, User.Identity.Name);

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
                        tmp.ErrorCode = (int)ErrorCodeEnum.Success;
                    }

                    if (tmp.ErrorCode != 0)
                    {
                        depositConfirmResult = -4;
                    }
                    else
                    {
                        var depositRequest = _depositBll.GetDepositRequest(request.TransId);
                        var userLang = _custInfoBll.GetCustInfoWithCache(depositRequest.CustId).UserLang;
                        // Send message to player
                        var sender = _selLangBll.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                        var title = _selLangBll.GetResourceValue("lbl_DepositApprovedTitle", userLang);
                        title = string.Format(title, request.TransId);
                        var content = _selLangBll.GetResourceValue("lbl_DepositApprovedContent", userLang);
                        content = string.Format(content, request.TransId);
                        _messageBll.SendMessageToPlayer(sender, title, content, depositRequest.CustId);
                    }
                }
            }
            else if (request.Action == (int)DepositStatusEnum.Rejected)
            {
                depositConfirmResult = _depositBll.DepositRejectd(request.TransId, request.ActualDeposit, request.Comment, User.Identity.Name);

                if (depositConfirmResult > 0)
                {
                    var depositRequest = _depositBll.GetDepositRequest(request.TransId);
                    var custInfo = _custInfoBll.GetCustInfoWithCache(depositRequest.CustId);
                    var userLang = custInfo.UserLang;
                    // Send message to player
                    var sender = _selLangBll.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                    var title = _selLangBll.GetResourceValue("lbl_DepositRejectTitle", userLang);
                    title = string.Format(title, request.TransId);
                    var content = _selLangBll.GetResourceValue("lbl_DepositRejectContent", userLang);
                    content = string.Format(content, request.TransId);
                    _messageBll.SendMessageToPlayer(sender, title, content, depositRequest.CustId);
                    if (custInfo.SiteId == 1)
                    {
                        await _messageBll.SendSMSMessageToPlayer(depositRequest.CustId);
                    }
                }
            }
            else if (request.Action == (int)DepositStatusEnum.Draft)
            {
                depositConfirmResult = _depositBll.DepositDraft(request.TransId, request.ActualDeposit, request.Comment, User.Identity.Name);
            }
            else if (request.Action == (int)DepositStatusEnum.OnHold)
            {
                depositConfirmResult = _depositBll.DepositOnHold(request.TransId, request.ActualDeposit, request.Comment, User.Identity.Name);
            }
            else if (request.Action == (int)DepositStatusEnum.Pending)
            {
                depositConfirmResult = _depositBll.DepositPending(request.TransId, request.ActualDeposit, request.Comment, User.Identity.Name);
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
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }

            return apiResult;
        }

        /// <summary>
        /// Deposit/BatchRejected 回絕多筆存款
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Request > [Submit] > Click CheckBox > [Reject Multiple]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList", "f_dp_execute" })]
        [HttpPost]
        public List<ApiResult> BatchRejected(BatchRejectedDepositList request)
        {
            var apiResult = new List<ApiResult>();
            var comment = $"{ User.Identity.Name} Rejected Deposit Multiple";

            foreach (var transId in request.TransIds)
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
        /// Deposit/SetAmountByTransId 更新指定存款金額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > [Submit] > Click [Trxn.ID] > Deposit Info [Request Amount] > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList" })]
        [HttpPost]
        public ApiResult SetAmountByTransId(SetAmountByTransIdRequest request)
        {
            var depositRequest = _depositBll.GetDepositRequest(request.TransId);
            if (depositRequest == null)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
            }
            if (_depositBll.SetDepositRequestAmount(request.TransId, request.Amount, User.Identity.Name, depositRequest.CustId) > 0)
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
        /// Deposit/GetReceiptByTransId 取得存款收據
        /// </summary>
        /// <remarks>
        /// Http Method：GET
        /// <param name="transId"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositList" })]
        [HttpGet]
        public IActionResult GetReceiptByTransId(string transId)
        {
            var depositReceipt = _depositBll.GetDepositReceipt(transId);
            if (depositReceipt == null)
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            var image = depositReceipt.DepositPic;
            return File(image, "image/jpeg");
        }

        #region Private function

        private async Task<ApiResult<TransferResult>> Process(PendingFund pendingFund)
        {
            return await _pendingFundBll.ProcessAsync(pendingFund);
        }

        #endregion

    }
}