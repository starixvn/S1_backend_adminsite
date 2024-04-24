using System.Net;
using Amazon.S3.Model;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static BackendSite.Controllers.ApiV2.SystemFunc.EnumController;

namespace BackendSite.Controllers.ApiV2.Finance.Deposit
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositController : ControllerBase
    {
        #region Fields

        private readonly DepositBLL _depositBll;
        private readonly SelLangBLL _selLangBll;
        private readonly MessageBLL _messageBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ExcelBLL _excelBll;
        private readonly PermissionBLL _permissionBll;
        private readonly Dictionary<DepositStatusEnum, List<DepositActionEnum>> _usableActionType;
        private readonly NewAuthorizeBLL _newAuthorizeBll;
        private readonly AWSS3Helper _s3Helper;
        private readonly string _awsDepositUrl;
        private readonly string _awsDepositFolder;
        private readonly string _awsDepositBucketName;
        private readonly VerificationRewardBLL _verificationRewardBLL;
        private readonly IGaBLL _gaBll;


        #endregion

        #region Constructors

        public DepositController(IServiceProvider provider)
        {
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _s3Helper = provider.GetRequiredService<AWSS3Helper>();
            _verificationRewardBLL = provider.GetRequiredService<VerificationRewardBLL>();
            _gaBll = provider.GetRequiredService<IGaBLL>();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _awsDepositBucketName = configBll.GetConfigValue("AWSPaymentBucketName");
            _awsDepositUrl = configBll.GetConfigValue("AWSPaymentBucketUrl");
            _awsDepositFolder = configBll.GetConfigValue("AWSDepositFolderName");

            _usableActionType = new Dictionary<DepositStatusEnum, List<DepositActionEnum>>
            {
                { DepositStatusEnum.Pending,
                    new List<DepositActionEnum>
                    {
                        DepositActionEnum.Draft,
                        DepositActionEnum.Approved,
                        DepositActionEnum.ManualApproved,
                        DepositActionEnum.Rejected,
                        DepositActionEnum.OnHold
                    } },
                { DepositStatusEnum.Approved,
                    new List<DepositActionEnum>
                    {
                        DepositActionEnum.Draft
                    } },
                { DepositStatusEnum.Rejected, new List<DepositActionEnum>
                {
                    DepositActionEnum.Draft,
                    DepositActionEnum.ManualApproved
                } },
                { DepositStatusEnum.Canceled, new List<DepositActionEnum>
                {
                    DepositActionEnum.Draft
                } },
                { DepositStatusEnum.OnHold, new List<DepositActionEnum>
                {
                    DepositActionEnum.Draft,
                    DepositActionEnum.Pending,
                    DepositActionEnum.ManualApproved
                } }
            };
        }

        #endregion

        #region ApiModel

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public int SiteId { get; set; }
            public string DepositId { get; set; }
            public string UserName { get; set; }
            public string Agent { get; set; }
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
            public DepositDateTypeEnum DateType { get; set; }
            public IEnumerable<int> Statuses { get; set; }
            public IEnumerable<int> LevelStatuses { get; set; }
            public IEnumerable<string> SelectBankCode { get; set; }
            public IEnumerable<int> SelectCalCode { get; set; }
            public IEnumerable<int> SelectPayId { get; set; }
            public decimal StartAmount { get; set; }
            public decimal EndAmount { get; set; }
        }

        public class GetListExportRequest
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public int SiteId { get; set; }
            public string DepositId { get; set; }
            public string UserName { get; set; }
            public string Agent { get; set; }
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
            public DepositDateTypeEnum DateType { get; set; }
            public IEnumerable<int> Statuses { get; set; }
            public IEnumerable<int> LevelStatuses { get; set; }
            public IEnumerable<string> SelectBankCode { get; set; }
            public IEnumerable<int> SelectCalCode { get; set; }
            public IEnumerable<int> SelectPayId { get; set; }
            public decimal StartAmount { get; set; }
            public decimal EndAmount { get; set; }
        }

        public class GetByTransIdRequest
        {
            public string TransId { get; set; }
        }

        public class SendToNextRequest : GetByTransIdRequest
        {
            public decimal ActualDeposit { set; get; }
            public decimal ActualCryptoAmount { set; get; }
            public string Comment { set; get; }
            public int Action { set; get; }
            private DateTimeOffset? _statementTime { get; set; }
            public DateTimeOffset? StatementTime
            {
                get => _statementTime;
                set
                {
                    if (value != null)
                        _statementTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public int? BankId { set; get; }
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 1 })]
        [HttpPost]
        public ApiListResultCurrencyAmount<DepositRequest> GetList(GetListRequest request)
        {
            var apiListResult = new ApiListResultCurrencyAmount<DepositRequest>();
            var currencyAmountRecord = new List<TotalAmount>();
            var userNames = FormatExtension.SplitRemoveSpace(request.UserName, ',');
            var selectSites = new List<int> { request.SiteId };

            if (request.SiteId == 0)
            {
                selectSites = GetSelectSites();
            }

            var depositList = _depositBll.GetDepositRequestList(
                selectSites,
                request.DepositTypes,
                request.SelectBankCode,
                request.SelectCalCode,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.DateType,
                request.DepositId,
                userNames,
                request.Agent,
                request.CurrencyIds,
                request.Statuses,
                request.LevelStatuses,
                request.PageSize,
                request.StartAmount,
                request.EndAmount,
                request.SelectPayId,
                request.PageNumber, out var totalPages, out var rowCount, ref currencyAmountRecord);

            if (depositList.Any())
            {
                var msgs = _depositBll.GetDepositMessageByDepositIds(depositList.Select(x => x.DepositId));
                var offset = request.StartDate.Offset;
                foreach (var deposit in depositList)
                {
                    deposit.DepositMessageList = msgs
                        .Where(x => x.DepositId == deposit.DepositId)
                        .OrderByDescending(x => x.CreateTime);
                }
            }

            apiListResult.Data = depositList;
            apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
            apiListResult.PageNumber = request.PageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            apiListResult.CurrencyAmountRecord = currencyAmountRecord;

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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 2 })]
        [HttpPost]
        public IActionResult Export(GetListExportRequest request)
        {
            var userNames = FormatExtension.SplitRemoveSpace(request.UserName, ',');
            var selectSites = new List<int> { request.SiteId };

            if (request.SiteId == 0)
            {
                selectSites = GetSelectSites();
            }

            var exportDeposit = _depositBll.GetDepositExportList(
                JsonConvert.SerializeObject(selectSites),
                request.SelectBankCode.Any() ? JsonConvert.SerializeObject(request.SelectBankCode) : null,
                request.SelectCalCode.Any() ? JsonConvert.SerializeObject(request.SelectCalCode) : null,
                JsonConvert.SerializeObject(request.DepositTypes),
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.DateType,
                request.DepositId,
                userNames,
                string.IsNullOrEmpty(request.Agent) ? null : request.Agent,
                request.CurrencyIds.Any() ? JsonConvert.SerializeObject(request.CurrencyIds) : null,
                JsonConvert.SerializeObject(request.Statuses),
                request.LevelStatuses.Any() ? JsonConvert.SerializeObject(request.LevelStatuses) : null,
                request.StartAmount,
                request.EndAmount,
                request.SelectPayId.Any() ? JsonConvert.SerializeObject(request.SelectPayId) : null
                );

            if (exportDeposit.Any())
            {
                var msgs = _depositBll.GetDepositMessageByDepositIds(exportDeposit.Select(x => x.DepositId));

                if (msgs.Any())
                {
                    var offset = request.StartDate.Offset;
                    foreach (var deposit in exportDeposit)
                    {
                        deposit.SendMessages = string.Join(", ", msgs
                                .Where(x => x.DepositId == deposit.DepositId)
                                .OrderBy(x => x.CreateTime)
                                .Select(x => x.Message));
                    }
                }
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 1 })]
        [HttpPost]
        public ApiResult<DepositRequest> GetByTransId(GetByTransIdRequest request)
        {
            var apiResult = new ApiResult<DepositRequest>();
            try
            {
                var depositRequest = _depositBll.GetDepositRequest(request.TransId);

                if (depositRequest != null)
                {
                    var usableActionButton = GetUsableActionButton(depositRequest);
                    depositRequest.UsableActionButton = usableActionButton;
                }

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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 1 })]
        [HttpPost]
        public ApiResult<IEnumerable<DepositComments>> GetCommentByTransId(GetByTransIdRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<DepositComments>>();
            var comments = _depositBll.GetDepositRequestComment(request.TransId);

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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 3 })]
        [HttpPost]
        public async Task<ApiResult> SendToNextAsync(SendToNextRequest request)
        {
            var apiResult = new ApiResult(_selLangBll);
            var depositConfirmResult = (int)ErrorCodeEnum.Fail;
            var depositRequest = _depositBll.GetDepositRequest(request.TransId);

            var calCodeInfo = new DepositCalCodeSetRequest()
            {
                BankId = request.BankId,
                StatementTime = null
            };

            if (request.Action == (int)DepositStatusEnum.Approved)
            {
                if (depositRequest.DepositType is DepositTypeEnum.LocalBanking or DepositTypeEnum.CryptoWallet)
                {
                    string di = null;
                    if (depositRequest.DepositType == DepositTypeEnum.CryptoWallet)
                    {
                        var jObj = JObject.Parse(depositRequest.DepositInfo);
                        jObj.Add("ActualExchangeAmount", request.ActualCryptoAmount);
                        di = jObj.ToString(Formatting.None);
                    }

                    if (request.ActualDeposit > 0)
                        depositConfirmResult = _depositBll.DepositApproved(request.TransId, request.ActualDeposit, request.Comment, User.Identity.Name, calCodeInfo, di);
                }

                if (depositConfirmResult > 0)
                {
                    var userLang = _custInfoBll.GetCustInfoWithCache(depositRequest.CustId).UserLang;
                    // Send message to player
                    var sender = _selLangBll.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                    var title = _selLangBll.GetResourceValue("lbl_DepositApprovedTitle", userLang);
                    title = string.Format(title, request.TransId);
                    var content = _selLangBll.GetResourceValue("lbl_DepositApprovedContent", userLang);
                    content = string.Format(content, request.TransId);
                    await _messageBll.SendMessageAsync(new Message(sender, depositRequest.CustId, userLang, title, content));

                    // verification job queue
                    var custVerificationReward = _verificationRewardBLL.GetCustVerificationRewardByCust(depositRequest.CustId);
                    if (custVerificationReward is null or { EmailVerified: true, EmailReward: 0 } or { PhoneVerified: true, PhoneReward: 0 })
                    {
                        _verificationRewardBLL.InsertQueueJob(depositRequest.CustId, QueueJob.Verification, string.Empty);
                    }

                    // send GA Purchase
                    _  = Task.Run(() =>
                    {
                        _gaBll.PurchaseEventSendAsync(new PurchaseRequest()
                        {
                            SiteId = depositRequest.SiteId,
                            CustId = depositRequest.CustId,
                            TransId = depositRequest.TransId,
                            DepositAmount = depositRequest.ActualDeposit,
                            CurrencyName = depositRequest.IsoCode,
                            ItemId = depositRequest.TransId,
                            ItemName = depositRequest.FirstDeposit == 1
                            ? "firstDeposit Approve"
                            : "reDeposit Approve",
                            EventName = "DepositApprove"
                        });
                    });
                }
            }
            else if (request.Action == (int)DepositStatusEnum.Rejected)
            {
                depositConfirmResult = _depositBll.DepositReject(request.TransId, request.ActualDeposit, request.Comment, User.Identity.Name);

                if (depositConfirmResult > 0)
                {
                    var custInfo = _custInfoBll.GetCustInfoWithCache(depositRequest.CustId);
                    var userLang = custInfo.UserLang;
                    // Send message to player
                    var sender = _selLangBll.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                    var title = _selLangBll.GetResourceValue("lbl_DepositRejectTitle", userLang);
                    title = string.Format(title, request.TransId);
                    var content = _selLangBll.GetResourceValue("lbl_DepositRejectContent", userLang);
                    content = string.Format(content, request.TransId);
                    await _messageBll.SendMessageAsync(new Message(sender, depositRequest.CustId, userLang, title, content));
                    if (custInfo.SiteId == 1)
                    {
                        await _messageBll.SendSMSMessageToPlayer(depositRequest.CustId);
                    }
                }
            }
            else if (request.Action == (int)DepositStatusEnum.Draft)
            {
                calCodeInfo.StatementTime = request.StatementTime;
                depositConfirmResult = _depositBll.DepositDraft(request.TransId, request.ActualDeposit, request.Comment, User.Identity.Name, calCodeInfo);
            }
            else if (request.Action == (int)DepositStatusEnum.OnHold)
            {
                depositConfirmResult = _depositBll.DepositOnHold(request.TransId, request.ActualDeposit, request.Comment, User.Identity.Name, calCodeInfo);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 6 })]
        [HttpPost]
        public List<ApiResult> BatchRejected(BatchRejectedDepositList request)
        {
            var apiResult = new List<ApiResult>();
            var comment = $"{User.Identity.Name} Rejected Deposit Multiple";

            foreach (var transId in request.TransIds)
            {
                var depositConfirmResult = _depositBll.DepositReject(transId, 0, comment, User.Identity.Name);

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
                    _messageBll.SendMessage(new Message(sender, depositRequest.CustId, userLang, title, content));
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 81 })]
        [HttpPost]
        public ApiResult SetAmountByTransId(ApiRequestWithLog<SetAmountRequest> request)
        {
            var depositRequest = _depositBll.GetDepositRequest(request.TransId);
            if (depositRequest == null)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
            }
            if (_depositBll.SetDepositRequestAmount(
                    new ParamsWithLog<SetAmountRequest>(request, User.Identity.Name), depositRequest.CustId) > 0)
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
        /// <remarks/>
        /// Http Method：GET
        /// <param name="transId"></param>
        /// <returns></returns>
        [HttpGet]
        public List<DepositReceiptModel> GetReceiptByTransId(string transId)
        {
            var result = new List<DepositReceiptModel>();
            using var client = new HttpClient();

            var depositReceipt = _depositBll.GetDepositReceipt(transId);
            if (depositReceipt == null)
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            foreach (var receipt in depositReceipt)
            {
                var picUrl = $"{_awsDepositUrl}{_awsDepositFolder}/{transId}_{receipt.ReceiptNo}.png";
                var model = new DepositReceiptModel
                {
                    ReceiptNo = receipt.ReceiptNo,
                    Receipt = picUrl,
                    Creator = receipt.Creator,
                    CreateTime = receipt.CreateTime,
                    CreateFrom = receipt.CreateFrom
                };
                result.Add(model);
            }

            return result;
        }

        /// <summary>
        /// Deposit/UploadReceipt 提交存款收據
        /// </summary>
        /// <remarks/>
        /// Http Method：POST
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 77 })]
        [HttpPost]
        public async Task<ApiResult> UploadReceipt(DepositReceiptFile depositReceipt)
        {
            var apiResult = new ApiResult();
            try
            {
                var depositRequest = _depositBll.GetDepositRequest(depositReceipt.TransId);

                if (depositRequest.Status is not (DepositStatusEnum.Pending or DepositStatusEnum.OnHold or DepositStatusEnum.Approved))
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Wrong status ,Can't upload receipt";
                    return apiResult;
                }

                if (depositReceipt.Receipt != null)
                {
                    var result = await _depositBll.SetDepositReceipt(depositReceipt.SiteId, depositReceipt.CustId, depositReceipt.Receipt, depositReceipt.TransId, User.Identity.Name);

                    if (result > 0)
                    {
                        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    }
                    else
                    {
                        apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                        apiResult.Message = "Upload fail.";
                    }
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "The file is not supported.";
                }
            }
            catch (Exception e)
            {
                throw new AdminException(ErrorCodeEnum.Fail, $"UploadReceipt Fail. {e.Message}");
            }

            return apiResult;
        }

        /// <summary>
        /// Deposit/DeleteReceipt 刪除存款收據
        /// </summary>
        /// <remarks/>
        /// Http Method：POST
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> DeleteReceipt(DeleteReceiptRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var depositReceipt = _depositBll.GetDepositReceiptByNo(request.TransId, request.ReceiptNo);
                if (depositReceipt.CreateFrom == (int)DepositReceiptCreateFromEnum.User)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Can't delete receipt from user";
                    return apiResult;
                }

                var depositRequest = _depositBll.GetDepositRequest(request.TransId);
                var permissionList = _permissionBll.GetUserPermissionByMenuId(27);

                bool permission;
                if (depositRequest.Status is DepositStatusEnum.Approved or DepositStatusEnum.Rejected or DepositStatusEnum.Canceled)
                {
                    permission = permissionList.Contains(78); //刪除權限
                }
                else
                {
                    permission = permissionList.Contains(77); //新增權限
                }

                if (!permission)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.PermissionDenied;
                    apiResult.Message = "Permission Denied";
                    return apiResult;
                }

                var result = await _depositBll.DelDepositReceiptPic(
                    request.SiteId,
                    request.CustId,
                    request.TransId,
                    depositReceipt.PicFileName,
                    request.ReceiptNo,
                    User.Identity.Name);

                if (result > 0)
                {
                    var deleteRequest = new DeleteObjectRequest();
                    var key = $"{_awsDepositFolder}/{request.TransId}_{request.ReceiptNo}.png";
                    deleteRequest.BucketName = _awsDepositBucketName;
                    deleteRequest.Key = key;
                    await _s3Helper.DeleteObjectAsync(deleteRequest);
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Delete Receipt Fail";
                }
            }
            catch (Exception e)
            {
                throw new AdminException(ErrorCodeEnum.Fail, $"Delete Receipt Fail. {e.Message}");
            }

            return apiResult;
        }

        /// <summary>
        /// Deposit/CheckDepositReceipt Check存款單審核
        /// </summary>
        /// <remarks/>
        /// Http Method：POST
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 79 })]
        [HttpPost]
        public ApiResult CheckDepositReceipt(CheckDepositReceiptRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var depositInfo = _depositBll.GetDepositRequest(request.TransId);

                if (depositInfo.CheckReceipt == 1)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "depositId already checked";
                }

                if ((depositInfo.DepositType is DepositTypeEnum.LocalBanking or DepositTypeEnum.CryptoWallet) &&
                    depositInfo.Status == DepositStatusEnum.Pending ||
                    depositInfo.Status == DepositStatusEnum.OnHold)
                {
                    var result = _depositBll.CheckDepositReceipt(depositInfo.SiteId, request.TransId, true, User.Identity.Name);
                    if (result > 0)
                    {
                        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    }
                    else
                    {
                        apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                        apiResult.Message = "CheckDepositReceipt fail.";
                    }
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Only localBanking and pending or onHold can check";
                }

            }
            catch (Exception e)
            {
                throw new AdminException(ErrorCodeEnum.Fail, $"CheckDepositReceipt Fail. {e.Message}");
            }

            return apiResult;
        }

        /// <summary>
        /// Deposit/UnCheckDepositReceipt UnCheck存款單審核
        /// </summary>
        /// <remarks/>
        /// Http Method：POST
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 79 })]
        [HttpPost]
        public ApiResult UnCheckDepositReceipt(CheckDepositReceiptRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var depositInfo = _depositBll.GetDepositRequest(request.TransId);

                if (depositInfo.CheckReceipt == 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "depositId hasn't be checked";
                }

                var result = _depositBll.CheckDepositReceipt(depositInfo.SiteId, request.TransId, false, User.Identity.Name);
                if (result > 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "UnCheckDepositReceipt fail.";
                }

            }
            catch (Exception e)
            {
                throw new AdminException(ErrorCodeEnum.Fail, $"UnCheckDepositReceipt Fail. {e.Message}");
            }

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 3 })]
        [HttpPost]
        public async Task<ApiResult> DepositManualApproval(DepositManualRequest para)
        {
            try
            {
                para.Operator = User.Identity.Name;
                para.ThirdId = string.Empty;
                var response = await _depositBll.DepositManual(para);
                return response;
            }
            catch (Exception ex)
            {
                throw new AdminException(ErrorCodeEnum.Fail, $"ManualApproval Fail. {ex.Message}");
            }
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 73 })]
        public ApiResult DepositMessageStatusList()
        {
            var apiResult = new ApiResult<List<EnumTypeModel>>();
            var result = new List<EnumTypeModel>();

            foreach (DepositMessageStatus status in Enum.GetValues(typeof(DepositMessageStatus)))
            {
                result.Add(new EnumTypeModel
                {
                    Name = status.GetDescription(),
                    Value = (int)status
                });
            }

            apiResult.Data = result;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 27, 73 })]
        public ApiResult SetDepositMessage(ApiRequestWithLog<DepositMessageRequest> request)
        {
            request.After.StatusName = request.After.Status.ToString();
            request.After.ActionName = request.After.Action.ToString();
            if (_depositBll.SetDepositMessage(
                    new ParamsWithLog<DepositMessageRequest>(request, User.Identity.Name)) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Edit Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
        }

        #region Private function

        private List<DepositActionEnum> GetUsableActionButton(DepositRequest depositRequest)
        {
            var permissionList = _permissionBll.GetUserPermissionByMenuId(27);
            var usableList = _usableActionType[depositRequest.Status];
            var usableActionButton = usableList
                .Where(item => permissionList.Contains((int)item)).ToList();

            if (depositRequest.DepositType is DepositTypeEnum.LocalBanking or DepositTypeEnum.CryptoWallet)
            {
                usableActionButton.Remove(DepositActionEnum.ManualApproved);
            }
            else if (depositRequest.DepositType is not (DepositTypeEnum.LocalBanking or DepositTypeEnum.CryptoWallet))
            {
                usableActionButton.Remove(DepositActionEnum.Approved);
            }

            return usableActionButton;
        }

        private List<int> GetSelectSites()
        {
            var authInfo = _newAuthorizeBll.GetAuthData();
            var result = _permissionBll.GetUserBrand(authInfo.UserId).ToList();
            var selectSites = result.Select(x => x.SiteId).ToList();

            return selectSites;
        }

        #endregion

    }
}