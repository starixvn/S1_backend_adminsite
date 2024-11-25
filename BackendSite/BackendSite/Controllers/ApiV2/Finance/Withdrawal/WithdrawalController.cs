using System.Net;

using Amazon.S3;
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
using BackendSite.Service.Model.Option;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Org.BouncyCastle.Ocsp;

using static BackendSite.Controllers.ApiV2.SystemFunc.EnumController;

namespace BackendSite.Controllers.ApiV2.Finance.Withdrawal
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalController : ControllerBase
    {
        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly IWithdrawalStateTransferBLL _withdrawalStateTransferBll;
        private readonly IWithdrawalAccountBLL _withdrawalAccountBll;
        private readonly IWithdrawalReceiveBLL _withdrawalReceiveBll;
        private readonly IAutoWithdrawalPaymentGateWayBLL _autoWithdrawalPaymentGateWayBll;
        private readonly SelLangBLL _selLangBll;
        private readonly ExcelBLL _excelBll;
        private readonly MessageBLL _messageBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly BankDetailBLL _bankDetailBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly CourierAdapter _courierAdapter;
        private readonly AWSS3Helper _s3Helper;
        private readonly PermissionBLL _permissionBll;
        private readonly Dictionary<WithdrawalStatusEnum, List<WithdrawalActionEnum>> _usableActionType;
        private readonly Cryptor _cryptor;
        private readonly NewAuthorizeBLL _newAuthorizeBll;
        private readonly ICacheHelper _redisHelper;

        private readonly string _payApiUrl;
        private readonly string _awsWithdrawalFolder;
        private readonly string _awsWithdrawalAccountingFolder;
        private readonly string _awsWithdrawalUrl;
        private readonly string _awsWithdrawalBucketName;
        private readonly FaceBookPixelBLL _faceBookPixelBll;
        private readonly CRMBLL _cRMBLL;
        private readonly IWebHostEnvironment _environment;

        #endregion

        #region Constructors

        public WithdrawalController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _withdrawalStateTransferBll = provider.GetRequiredService<IWithdrawalStateTransferBLL>();
            _withdrawalAccountBll = provider.GetRequiredService<IWithdrawalAccountBLL>();
            _withdrawalReceiveBll = provider.GetRequiredService<IWithdrawalReceiveBLL>();
            _autoWithdrawalPaymentGateWayBll = provider.GetRequiredService<IAutoWithdrawalPaymentGateWayBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _s3Helper = provider.GetRequiredService<AWSS3Helper>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _cryptor = provider.GetRequiredService<Cryptor>();
            _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();

            var configBll = provider.GetRequiredService<ConfigBLL>();
            _payApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").PayService;
            _awsWithdrawalBucketName = configBll.GetConfigValue("AWSPaymentBucketName");
            _awsWithdrawalUrl = configBll.GetConfigValue("AWSPaymentBucketUrl");
            _awsWithdrawalFolder = configBll.GetConfigValue("AWSWithdrawalFolderName");
            _awsWithdrawalAccountingFolder = configBll.GetConfigValue("AWSWithdrawalAccountingFolder");
            _faceBookPixelBll = provider.GetRequiredService<FaceBookPixelBLL>();
            _cRMBLL = provider.GetRequiredService<CRMBLL>();
            _environment = provider.GetRequiredService<IWebHostEnvironment>();

            _usableActionType = new Dictionary<WithdrawalStatusEnum, List<WithdrawalActionEnum>>
            {
                {WithdrawalStatusEnum.Pending, new List<WithdrawalActionEnum>
                {
                    WithdrawalActionEnum.Draft,
                    WithdrawalActionEnum.Rejected,
                    WithdrawalActionEnum.OnHold,
                    WithdrawalActionEnum.Verified,
                    WithdrawalActionEnum.ToKyc
                }},
                {WithdrawalStatusEnum.OnHold, new List<WithdrawalActionEnum>
                {
                    WithdrawalActionEnum.Draft,
                    WithdrawalActionEnum.Verified,
                    WithdrawalActionEnum.ToKyc,
                    WithdrawalActionEnum.Rejected
                }},
                {WithdrawalStatusEnum.Verify, new List<WithdrawalActionEnum>
                {
                    WithdrawalActionEnum.Draft,
                    WithdrawalActionEnum.Process,
                    WithdrawalActionEnum.Pending,
                    WithdrawalActionEnum.Rejected
                }},
                {WithdrawalStatusEnum.Process, new List<WithdrawalActionEnum>
                {
                    WithdrawalActionEnum.Draft,
                    WithdrawalActionEnum.Verified,
                    WithdrawalActionEnum.Approved,
                    WithdrawalActionEnum.ManualApproved,
                    WithdrawalActionEnum.ManualRejected
                }},
                {WithdrawalStatusEnum.Cancelled, new List<WithdrawalActionEnum>
                {
                    WithdrawalActionEnum.Draft
                }},
                {WithdrawalStatusEnum.Approved, new List<WithdrawalActionEnum>
                {
                    WithdrawalActionEnum.Draft,
                    WithdrawalActionEnum.ManualRejected
                }},
                {WithdrawalStatusEnum.Rejected, new List<WithdrawalActionEnum>
                {
                    WithdrawalActionEnum.Draft,
                    WithdrawalActionEnum.ManualApproved
                }},
                {WithdrawalStatusEnum.ToKyc, new List<WithdrawalActionEnum>
                {
                    WithdrawalActionEnum.Draft,
                    WithdrawalActionEnum.Pending,
                    WithdrawalActionEnum.Rejected
                }}
            };
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;
            public int SiteId { get; set; }
            public string WithdrawalId { get; set; }
            public string UserName { get; set; }
            public IEnumerable<int> WithdrawalTypes { get; set; }
            public WithdrawalDateTypeEnum DateType { get; set; }
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
            public WithdrawalMethodEnum WithdrawalMethod { get; set; }
            public IEnumerable<string> SelectBankCode { get; set; }
            public IEnumerable<int> SelectCalCode { get; set; }
            public string Agent { get; set; }
            public decimal StartAmount { get; set; }
            public decimal EndAmount { get; set; }

        }

        public class GetExportRequest
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;
            public int SiteId { get; set; }
            public WithdrawalDateTypeEnum DateType { get; set; }
            public string WithdrawalId { get; set; }
            public string UserName { get; set; }
            public IEnumerable<int> WithdrawalTypes { get; set; }
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
            public WithdrawalMethodEnum WithdrawalMethod { get; set; }
            public IEnumerable<string> SelectBankCode { get; set; }
            public IEnumerable<int> SelectCalCode { get; set; }
            public string Agent { get; set; }
            public decimal StartAmount { get; set; }
            public decimal EndAmount { get; set; }

        }

        public class UploadPictureRequest : GetByTransIdRequest
        {
            public IEnumerable<string> Pic { get; set; }
        }

        public class DeletePictureRequest : GetByTransIdRequest
        {
            public int Number { get; set; }
        }

        public class GetExchangeInfoRequest
        {
            public string CryptoCurrencyName { get; set; }
            public string Network { get; set; }
            public int CurrencyId { get; set; }
            public decimal Amount { get; set; }
        }

        #endregion

        #region Actions

        /// <summary>
        /// Withdrawal/GetList 取得提款清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Request > [Search]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResultCurrencyAmount<WithdrawalRequest> GetList(GetListRequest request)
        {
            var apiListResult = new ApiListResultCurrencyAmount<WithdrawalRequest>();
            var currencyAmount = new List<TotalAmount>();
            var userNames = FormatExtension.SplitRemoveSpace(request.UserName, ',');
            var selectSites = new List<int> { request.SiteId };

            if (request.SiteId == 0)
            {
                selectSites = GetSelectSites();
            }

            var withdrawalRequestList = _withdrawalBll.GetWithdrawalRequestList(
                selectSites,
                request.WithdrawalTypes,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.DateType,
                request.WithdrawalId,
                userNames,
                request.CurrencyIds,
                request.Statuses,
                request.LevelStatuses,
                request.PageSize,
                request.StartAmount,
                request.EndAmount,
                request.PageNumber,
                request.WithdrawalMethod,
                request.SelectBankCode,
                request.SelectCalCode,
                request.Agent,
                out var totalPages, out var rowCount, ref currencyAmount);

            var msgs = _withdrawalBll.GetWithdrawalMessageByWithdrawalIds(
                withdrawalRequestList.Select(x => x.WithdrawalId));
            foreach (var data in withdrawalRequestList)
            {
                data.TransferPin = _cryptor.Decrypt(data.TransferPin);
                data.LoginPin = _cryptor.Decrypt(data.LoginPin);
                data.MobilePin = _cryptor.Decrypt(data.MobilePin);
                data.CorporateID = _cryptor.Decrypt(data.CorporateID);
                data.WithdrawalMessageList = msgs
                    .Where(x => x.WithdrawalId == data.WithdrawalId)
                    .OrderByDescending(x => x.CreateTime);
            }

            apiListResult.Data = withdrawalRequestList;
            apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
            apiListResult.PageNumber = request.PageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            apiListResult.CurrencyAmountRecord = currencyAmount;

            return apiListResult;
        }

        /// <summary>
        /// Withdrawal/Export 匯出提款清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Request > [Search] > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Export(GetExportRequest request)
        {
            var exportData = new List<WithdrawalInfoForExportData>();
            var userNames = FormatExtension.SplitRemoveSpace(request.UserName, ',');
            var selectSites = new List<int> { request.SiteId };

            if (request.SiteId == 0)
            {
                selectSites = GetSelectSites();
            }

            var exportWithdrawal = _withdrawalBll.GetWithdrawalExportList(
                JsonConvert.SerializeObject(selectSites),
                JsonConvert.SerializeObject(request.WithdrawalTypes),
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.DateType,
                request.WithdrawalId,
                userNames.Any() ? JsonConvert.SerializeObject(userNames) : null,
                request.CurrencyIds.Any() ? JsonConvert.SerializeObject(request.CurrencyIds) : null,
                JsonConvert.SerializeObject(request.Statuses),
                request.LevelStatuses.Any() ? JsonConvert.SerializeObject(request.LevelStatuses) : null,
                request.StartAmount,
                request.EndAmount,
                request.SelectBankCode.Any() ? JsonConvert.SerializeObject(request.SelectBankCode) : null,
                request.SelectCalCode.Any() ? JsonConvert.SerializeObject(request.SelectCalCode) : null,
                string.IsNullOrEmpty(request.Agent) ? null : request.Agent,
                request.WithdrawalMethod);

            var msgs = _withdrawalBll.GetWithdrawalMessageByWithdrawalIds(
                exportWithdrawal.Select(x => x.WithdrawalId));

            foreach (var data in exportWithdrawal)
            {
                if (!string.IsNullOrWhiteSpace(data.EWalletName))
                {
                    var length = data.AccountNo.Length;
                    var rightLength = length - 4;
                    if (rightLength > 0)
                    {
                        data.AccountNo = $"****{data.AccountNo.Substring(4, rightLength)}";
                    }
                }

                exportData.Add(new WithdrawalInfoForExportData
                {
                    WithdrawalId = data.WithdrawalId,
                    TransId = data.TransId,
                    WithdrawalType = Enum.GetName(typeof(WithdrawalTypeEnum), data.WithdrawalType),
                    CustId = data.CustId,
                    UserName = data.UserName,
                    NetWork = data.NetWork,
                    CryptoAmount = data.CryptoAmount,
                    CurrencyId = data.CurrencyName,
                    IsTest = data.IsTest,
                    Amount = data.Amount,
                    BankName = !string.IsNullOrWhiteSpace(data.EWalletName) ? data.EWalletName : data.BankName,
                    BankCode = data.BankCode,
                    AccountNo = data.AccountNo,
                    AccountName = data.AccountName,
                    CreateTime = data.CreateTime,
                    Status = data.Status,
                    SiteName = data.SiteName,
                    FromBankName = data.FromBankName,
                    FromBankCode = data.FromBankCode,
                    FromAccountNo = data.FromAccountNo,
                    FromAccountName = data.FromAccountName,
                    PayName = data.PayName,
                    RefId = data.RefId,
                    CalCode = data.CalCode,
                    IsKyc = data.IsKyc,
                    IsRisk = data.IsRisk,
                    WithdrawlCnt = data.WithdrawlCnt,
                    Operator = data.Operator,
                    UpdateTime = data.UpdateTime,
                    FlowEndTime = data.FlowEndTime,
                    WithdrawalMessage = string.Join(", ", msgs
                        .Where(x => x.WithdrawalId == data.WithdrawalId)
                        .OrderBy(x => x.CreateTime)
                        .Select(x => x.Message))
                });
            }

            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WithdrawalRequestList");
        }

        /// <summary>
        /// Withdrawal/GetByTransId 取得指定提款資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Request > [Search] > Click [Trxn.ID] > Withdrawal Info
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<WithdrawalRequest>> GetByTransId(GetByTransIdRequest request)
        {
            var apiResult = new ApiResult<WithdrawalRequest>();
            var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);

            if (withdrawalRequest != null)
            {
                withdrawalRequest.WithdrawalAccountingModels = _withdrawalBll.GetWithdrawalAccountingByTransId(request.TransId);
                withdrawalRequest.UsableActionButton = await GetUsableActionButton(withdrawalRequest);
            }

            if (withdrawalRequest.Operator == User.Identity.Name)
            {
                withdrawalRequest.SameOperator = true;
            }

            try
            {
                if (withdrawalRequest.Status is WithdrawalStatusEnum.Process or WithdrawalStatusEnum.Approved)
                {
                    var getRequest = new GetObjectRequest();
                    var key = $"{_awsWithdrawalFolder}/{request.TransId}_{withdrawalRequest.BankSlipPicCount}.png";
                    getRequest.BucketName = _awsWithdrawalBucketName;
                    getRequest.Key = key;
                    await _s3Helper.GetObjectAsync(getRequest);
                    withdrawalRequest.HasPic = true;
                }
                else
                {
                    withdrawalRequest.HasPic = false;
                }
            }
            catch (Exception ex)
            {
                withdrawalRequest.HasPic = false;
            }

            apiResult.Data = withdrawalRequest;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;
        }

        /// <summary>
        /// Withdrawal/GetCommentByTransId 取得指定提款註解
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Request > [Search] > Click [Trxn.ID] > Withdrawal Comment
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<WithdrawalComments>> GetCommentByTransId(GetByTransIdRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<WithdrawalComments>>();
            var comments = _withdrawalBll.GetWithdrawalRequestComment(request.TransId);
            apiResult.Data = comments;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        /// <summary>
        /// Withdrawal/SendToNext 提款送至下一流程
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Request > [Search] > Click [Trxn.ID] > [Draft] [Pending] [OnHold] [Verify] [Process] [Approved] [Rejected] [ToKyc]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 3 })]
        [HttpPost]
        public async Task<ApiResult> SendToNextAsync(SendToNextRequest request)
        {
            var apiResult = new ApiResult(_selLangBll);
            var authData = _authorizeBll.GetAuthData();
            var withdrawConfirmResult = -1;

            //第一把鎖，鎖定後臺同一操作者短時間操作相同單號單
            var lockKey = $"Withdrawal:SendToNext:Lock:{authData.UserName}:{request.TransId}";
            var lockValue = Guid.NewGuid().ToString(); // 每次取鎖使用唯一值，確保為同一個執行序所執行
            var lockExpiry = TimeSpan.FromSeconds(15); //lock 15 sec        
            if (await _redisHelper.TryCreateLockAsync(lockKey, lockValue, lockExpiry))
            {
                //第二把鎖，鎖定後臺不同操作者操作相同單號
                var key = $"lock:withdrawalTransId:{request.TransId}";
                var lockUser = await _redisHelper.StringGetAsync(key);

                //非 Draft 才判斷自己是否還是 OnUser
                if (request.Action != WithdrawalStatusEnum.Draft &&
                    lockUser != authData.UserName)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                    apiResult.Message = $"{request.TransId} Now OnUser is {lockUser}";
                    return apiResult;
                }

                var withdrawalData = _withdrawalBll.GetWithdrawalRequest(request.TransId);
                switch (request.Action)
                {
                    case WithdrawalStatusEnum.Draft:

                        var checkWithdrawalDraftFailResult = CheckWithdrawalDraft(request, withdrawalData, apiResult);
                        if (checkWithdrawalDraftFailResult != null) return checkWithdrawalDraftFailResult;

                        withdrawConfirmResult = _withdrawalStateTransferBll.WithdrawalDraft(request.TransId,
                            request.Comment,
                            request.WithdrawalAccountingModels,
                            authData.UserName);
                        break;
                    case WithdrawalStatusEnum.Pending:
                        withdrawConfirmResult = _withdrawalStateTransferBll.WithdrawalPending(request.TransId,
                            request.Comment,
                            authData.UserName);
                        break;
                    case WithdrawalStatusEnum.OnHold:
                        withdrawConfirmResult = _withdrawalStateTransferBll.WithdrawalOnHold(request.TransId,
                            request.Comment,
                            authData.UserName);
                        break;
                    case WithdrawalStatusEnum.Verify:

                        if (withdrawalData.ProcessWay == WithdrawalInfoProcessWayEnum.Auto) // 自動出款要走 Verify + Process 流程
                        {
                            apiResult = _autoWithdrawalPaymentGateWayBll.AutoWithdrawalProcessWay(request, withdrawalData, authData, apiResult, out withdrawConfirmResult);
                        }
                        else
                        {
                            withdrawConfirmResult = _withdrawalStateTransferBll.WithdrawalVerify(request.TransId,
                                request.Comment,
                                authData.UserName);
                        }
                        break;
                    case WithdrawalStatusEnum.Process:
                        {
                            var withdrawalInfo = JsonConvert.DeserializeObject<WithdrawalInfo>(request.WithdrawalInfo);
                            var checkWithdrawalTypeFailResult = CheckWithdrawalTypeAndSetObject(request, apiResult, withdrawalInfo);
                            if (checkWithdrawalTypeFailResult != null) return checkWithdrawalTypeFailResult;

                            withdrawConfirmResult = _withdrawalStateTransferBll.WithdrawalProcess(
                                request.TransId,
                                request.Comment,
                                authData.UserName,
                                authData.UserId,
                                JsonConvert.SerializeObject(withdrawalInfo),
                                (int)request.WithdrawalType,
                                request.WithdrawalAccountingModels,
                                out var custId, out var amount);

                            if (withdrawConfirmResult > 0 && IsOnlinePayment(request.WithdrawalType.Value))
                            {
                                var createFailResult = await _autoWithdrawalPaymentGateWayBll.PaymentOnlineCreate(request.TransId, apiResult, authData.UserName);
                                if (createFailResult != null) return createFailResult;
                            }
                            break;
                        }
                    case WithdrawalStatusEnum.Approved:
                        {
                            //Check has pic or not
                            var hasPic = await CheckWithdrawalPictureAsync(request.TransId);

                            withdrawConfirmResult = _withdrawalStateTransferBll.WithdrawalApproved(request.TransId, request.Comment,
                                authData.UserName, authData.UserId, hasPic);

                            if (withdrawConfirmResult > 0)
                            {
                                var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);
                                var userLang = _custInfoBll.GetCustInfoWithCache(withdrawalRequest.CustId).UserLang;
                                // Send message to player
                                var sender = _selLangBll.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                                var title = _selLangBll.GetResourceValue("lbl_WithdrawalApprovedTitle", userLang);
                                title = string.Format(title, request.TransId);
                                var content = _selLangBll.GetResourceValue("lbl_WithdrawalApprovedContent", userLang);
                                content = string.Format(content, request.TransId);
                                await _messageBll.SendMessageAsync(new Message(sender, withdrawConfirmResult, userLang, title, content));

                                #region Copy Picture

                                if ((withdrawalRequest.WithdrawalType is WithdrawalTypeEnum.LocalBank or WithdrawalTypeEnum.CryptoWallet)
                                    && hasPic)
                                {
                                    await CopyWithdrawalPictureAsync(withdrawalRequest.TransId);
                                }
                                #endregion
                                #region 埋點
                                if (withdrawalData.SiteId is 1 or 2 or 3)
                                {
                                    _ = Task.Run(() =>
                                    {
                                        var pixelEvent = new List<FaceBookPixel> { FaceBookPixel.withdraw };
                                        var pixelRequest = _faceBookPixelBll.CreatePixelRequest(request.TransId ,withdrawalData.SiteId, withdrawalData.CustId, withdrawalData.Amount, pixelEvent);
                                        _ = _faceBookPixelBll.Send(pixelRequest);
                                    });
                                    
                                    _ = Task.Run(() =>
                                    {
                                        var para = new CRMRequest()
                                        {
                                            Event = (int)CRMEvent.withdrawal_sucessful,
                                            CustId = withdrawalData.CustId,
                                            SiteId = withdrawalData.SiteId,
                                            OrderAmount = withdrawalData.Amount,
                                            CurrencyId = withdrawalData.CurrencyId,
                                            Environment = _environment.IsProduction() ? "P" : "D"
                                        };
                                        _ = _cRMBLL.Send(para);
                                    });                                  
                                }
                                #endregion
                            }
                            break;
                        }
                    case WithdrawalStatusEnum.Rejected:
                        {
                            withdrawConfirmResult =
                                _withdrawalStateTransferBll.WithdrawalRejected(request.TransId, request.Comment, authData.UserName);

                            if (withdrawConfirmResult > 0)
                            {
                                var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);
                                var userLang = _custInfoBll.GetCustInfoWithCache(withdrawalRequest.CustId).UserLang;
                                // Send message to player
                                var sender = _selLangBll.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                                var title = _selLangBll.GetResourceValue("lbl_WithdrawalRejectTitle", userLang);
                                title = string.Format(title, request.TransId);
                                var content = _selLangBll.GetResourceValue("lbl_WithdrawalRejectContent", userLang);
                                content = string.Format(content, request.TransId);
                                await _messageBll.SendMessageAsync(new Message(sender, withdrawConfirmResult, userLang, title, content));
                            }
                            break;
                        }
                    case WithdrawalStatusEnum.ToKyc:
                        withdrawConfirmResult = _withdrawalStateTransferBll.WithdrawalToKyc(request.TransId,
                            request.Comment,
                            authData.UserName);
                        break;

                }

                if (withdrawConfirmResult <= 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                    apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
                }
                else
                {
                    if (request.Action != WithdrawalStatusEnum.Draft)
                    {
                        var _ = await _withdrawalReceiveBll.UnLockWithdrawalTicketAsync(request.TransId, authData.SiteId);
                    }

                    var changeStatus = request.Action;
                    await _withdrawalAccountBll.WithdrawalAccountCounter(withdrawalData, changeStatus);

                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
                }

                // 解鎖除第一把
                await _redisHelper.UnLockAsync(lockKey, lockValue);
            }
            return apiResult;
        }

        /// <summary>
        /// Withdrawal/ManualApproval
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Request > [Search] > Click [Trxn.ID] > [ManualApproval]
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 3 })]
        [HttpPost]
        public async Task<ApiResult> ManualApproval(WithdrawalManualRequest para)
        {
            try
            {
                var withdrawalData = _withdrawalBll.GetWithdrawalRequest(para.TransId);
                para.Operator = User.Identity.Name;
                para.ThirdId = string.Empty;
                var response = await _withdrawalBll.WithdrawalManualApproval(para);

                if (response.ErrorCode == 0)
                {
                    var authData = _authorizeBll.GetAuthData();

                    //解除鎖定
                    var _ = await _withdrawalReceiveBll.UnLockWithdrawalTicketAsync(para.TransId, authData.SiteId);

                    // Api 成功更新提款成功次數
                    await _withdrawalAccountBll.WithdrawalAccountCounter(withdrawalData, WithdrawalStatusEnum.Approved);
                }
                return response;
            }
            catch (Exception ex)
            {
                return new ApiResult()
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = $"ManualApproval Fail. {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Withdrawal/ManualReject
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Request > [Search] > Click [Trxn.ID] > [ManualReject]
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 3 })]
        [HttpPost]
        public async Task<ApiResult> ManualReject(WithdrawalManualRequest para)
        {
            try
            {
                var withdrawalData = _withdrawalBll.GetWithdrawalRequest(para.TransId);
                para.Operator = User.Identity.Name;
                para.ThirdId = string.Empty;
                var response = await _withdrawalBll.WithdrawalManualReject(para);

                if (response.ErrorCode == 0)
                {
                    var authData = _authorizeBll.GetAuthData();

                    //解除鎖定
                    var _ = await _withdrawalReceiveBll.UnLockWithdrawalTicketAsync(para.TransId, authData.SiteId);

                    // Api 成功更新提款成功次數
                    await _withdrawalAccountBll.WithdrawalAccountCounter(withdrawalData, WithdrawalStatusEnum.Rejected);
                }

                return response;
            }
            catch (Exception ex)
            {
                return new ApiResult()
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = $"ManualReject Fail. {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Withdrawal/GetPicture 取得提款圖擋
        /// </summary>
        /// <remarks>
        /// Http Method：GET
        /// 
        /// **Memo：只有在Status=Process Or Approved 才會有此功能
        /// 
        /// Func Path：Finance > Withdrawal Request > [Search] > Click [Trxn.ID] > Withdrawal Pic
        /// </remarks>
        /// <param name="transId"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        [HttpGet]
        public List<string> GetPicture(string transId, int number)
        {
            using var client = new HttpClient();

            var result = new List<string>();
            if (number == -1)
            {
                var data = _withdrawalBll.GetWithdrawalPicCount(transId);
                if (data != null)
                {
                    var picCount = _withdrawalBll.GetWithdrawalPicCount(transId).BankSlipPicCount;
                    for (var i = 1; i <= picCount; i++)
                    {
                        var picUrl = Redirect($"{_awsWithdrawalUrl}{_awsWithdrawalFolder}/{transId}_{i}.png").Url;
                        var picRsp = client.GetAsync(picUrl).Result;
                        if (picRsp.StatusCode == HttpStatusCode.OK)
                        {
                            result.Add(picUrl);
                        }
                    }
                }
            }
            else
            {
                var pic = Redirect($"{_awsWithdrawalUrl}{_awsWithdrawalFolder}/{transId}_{number}.png").Url;
                result.Add(pic);
            }
            return result;
        }

        /// <summary>
        /// Withdrawal/UploadPicture 上傳提款圖擋
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// **Memo：只有在Status=Process Or Approved 才會有此功能
        /// 
        /// Func Path：Finance > Withdrawal Request > [Search] > Click [Trxn.ID] > Withdrawal Pic > [Paste Area]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 77 })]
        [HttpPost]
        public async Task<ApiResult> UploadPicture(UploadPictureRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);

                if (withdrawalRequest.Status is not (WithdrawalStatusEnum.Process or WithdrawalStatusEnum.Approved))
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Wrong status ,Can't upload receipt";
                    return apiResult;
                }

                var requestCount = request.Pic.Count();
                var result = _withdrawalBll.UpdateWithdrawalPicCount(request.TransId, requestCount);
                if (result)
                {
                    var withdrawalPic = _withdrawalBll.GetWithdrawalPicCount(request.TransId);

                    var count = 1;
                    foreach (var pic in request.Pic)
                    {
                        var countName = withdrawalPic.BankSlipPicCount - requestCount + count;
                        var x = pic.Replace("data:image/png;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");
                        var bytes = Convert.FromBase64String(x);
                        var stream = new MemoryStream(bytes);

                        var putRequest = new PutObjectRequest();
                        var key = $"{_awsWithdrawalFolder}/{request.TransId}_{countName}.png";
                        putRequest.BucketName = _awsWithdrawalBucketName; // Your bucket name
                        putRequest.ContentType = "image/png";
                        putRequest.InputStream = stream;
                        putRequest.Key = key;
                        await _s3Helper.PutObjectAsync(putRequest);

                        count += 1;
                    }

                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// Withdrawal/DeletePicture  刪除提款圖擋
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// **Memo：只有在Status=Process Or Approved 才會有此功能
        /// 
        /// Func Path：Finance > Withdrawal Request > [Search] > Click [Trxn.ID] > Withdrawal Pic > [Delete]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> DeletePicture(DeletePictureRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);
                var permissionList = _permissionBll.GetUserPermissionByMenuId(28);

                bool permission;
                if (withdrawalRequest.Status is WithdrawalStatusEnum.Approved or WithdrawalStatusEnum.Rejected or WithdrawalStatusEnum.Cancelled)
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

                var deleteRequest = new DeleteObjectRequest();
                var key = $"{_awsWithdrawalFolder}/{request.TransId}_{request.Number}.png";
                deleteRequest.BucketName = _awsWithdrawalBucketName;
                deleteRequest.Key = key;
                var a = await _s3Helper.DeleteObjectAsync(deleteRequest);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// Withdrawal/WithdrawalMessageStatusList  get withdrawal cannot be successful	status list
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 73 })]
        [HttpPost]
        public ApiResult WithdrawalMessageStatusList()
        {
            var apiResult = new ApiResult<List<EnumTypeModel>>();
            var result = new List<EnumTypeModel>();

            foreach (WithdrawalMessageStatus status in Enum.GetValues(typeof(WithdrawalMessageStatus)))
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

        public class WithdrawalMessageRequest
        {
            public WithdrawalMessageStatus Status { get; set; }
            public string StatusName { get; set; }
            public int WithdrawalId { get; set; }
            public WithdrawalStatusEnum Action { get; set; }
            public string ActionName { get; set; }
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 73 })]
        [HttpPost]
        public ApiResult SetWithdrawalMessage(ApiRequestWithLog<WithdrawalMessageRequest> request)
        {
            request.After.StatusName = request.After.Status.ToString();
            request.After.ActionName = request.After.Action.ToString();
            if (_withdrawalBll.SetWithdrawalMessage(
                    new ParamsWithLog<WithdrawalMessageRequest>(request, User.Identity.Name)) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Edit Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
        }

        [HttpPost]
        public async Task<ApiResult<CryptoExchangeInfo>> GetExchangeInfoAsync(GetExchangeInfoRequest req)
        {
            var authData = _newAuthorizeBll.GetAuthData();
            var exchangeReq = new CryptoExchangeReq
            {
                SiteId = authData.SiteId,
                CurrencyId = req.CurrencyId,
                Amount = req.Amount,
                ChannelName = req.CryptoCurrencyName.GetChannelName(req.Network),
                Type = ExchangeTypeEnum.Sell
            };
            var myUrl = $"{_payApiUrl}CryptoExchange";
            return await _courierAdapter.MyPostAsync<CryptoExchangeInfo, CryptoExchangeReq>(myUrl, exchangeReq);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 確認是否存在提款單圖檔
        /// </summary>
        /// <param name="transId">提款單單號</param>
        /// <returns>是否存在</returns>
        private async Task<bool> CheckWithdrawalPictureAsync(string transId)
        {
            try
            {
                var withdrawalPicCount = _withdrawalBll.GetWithdrawalPicCount(transId);

                var request = new GetObjectRequest()
                {
                    BucketName = _awsWithdrawalBucketName,
                    Key = $"{_awsWithdrawalFolder}/{transId}_{withdrawalPicCount.BankSlipPicCount}.png"
                };
                var response = await _s3Helper.GetObjectAsync(request);

                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 複製提款單圖片檔
        /// </summary>
        /// <param name="transId">提款單單號</param>
        /// <returns>是否成功</returns>
        private async Task<bool> CopyWithdrawalPictureAsync(string transId)
        {
            try
            {
                var withdrawalPicCount = _withdrawalBll.GetWithdrawalPicCount(transId);

                for (var seq = 1; seq <= withdrawalPicCount.BankSlipPicCount; seq++)
                {
                    var request = new CopyObjectRequest();
                    var withdrawalKey = $"{_awsWithdrawalFolder}/{transId}_{seq}.png";
                    var accountingKey = $"{_awsWithdrawalAccountingFolder}/{transId}_{seq}_1.png";
                    request.SourceBucket = _awsWithdrawalBucketName;
                    request.SourceKey = withdrawalKey;
                    request.DestinationBucket = _awsWithdrawalBucketName;
                    request.DestinationKey = accountingKey;
                    var response = await _s3Helper.CopyObjectAsync(request);
                    if (response.HttpStatusCode != HttpStatusCode.OK)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<List<WithdrawalActionEnum>> GetUsableActionButton(WithdrawalRequest withdrawalRequest)
        {
            var key = $"lock:withdrawalTransId:{withdrawalRequest.TransId}";
            var lockExists = await _redisHelper.KeyExistsAsync(key);

            if (!lockExists) //沒有人Lok
                return new List<WithdrawalActionEnum>() { WithdrawalActionEnum.Draft, WithdrawalActionEnum.Receive };

            var authData = _authorizeBll.GetAuthData();
            var lockUser = await _redisHelper.StringGetAsync(key);
            var permissionList = _permissionBll.GetUserPermissionByMenuId(28);

            if (lockUser == authData.UserName) // 是不自己 Lock
            {
                var usableList = _usableActionType[withdrawalRequest.Status];
                var usableActionButton = usableList.Where(item => permissionList.Contains((int)item)).ToList();

                if (IsLocalPayment(withdrawalRequest.WithdrawalType))
                {
                    usableActionButton.Remove(WithdrawalActionEnum.ManualApproved);
                    usableActionButton.Remove(WithdrawalActionEnum.ManualRejected);
                }

                if (withdrawalRequest.Status is WithdrawalStatusEnum.Approved &&
                    IsLocalPayment(withdrawalRequest.WithdrawalType))
                {
                    usableActionButton.Add(WithdrawalActionEnum.Rejected);
                    usableActionButton.Remove(WithdrawalActionEnum.Receive);
                }

                if (withdrawalRequest.Status == WithdrawalStatusEnum.Process &&
                    IsOnlinePayment(withdrawalRequest.WithdrawalType))
                {
                    usableActionButton.Remove(WithdrawalActionEnum.Verified);
                    usableActionButton.Remove(WithdrawalActionEnum.Approved);
                }

                if (withdrawalRequest.Status == WithdrawalStatusEnum.Rejected &&
                    IsLocalPayment(withdrawalRequest.WithdrawalType))
                {
                    usableActionButton.Remove(WithdrawalActionEnum.ManualApproved);
                }

                usableActionButton.Remove(WithdrawalActionEnum.Receive);
                return usableActionButton;
            }

            // 如果有人Lock 但不是自己，就判斷權限是否是主管，主管可以有 Receive 按鈕
            var forceProceedPermission = permissionList.Any(x => x == 83);
            return forceProceedPermission
                ? new List<WithdrawalActionEnum>() { WithdrawalActionEnum.Draft, WithdrawalActionEnum.Receive }
                : new List<WithdrawalActionEnum>() { WithdrawalActionEnum.Draft };

        }

        private List<int> GetSelectSites()
        {
            var authInfo = _newAuthorizeBll.GetAuthData();
            var result = _permissionBll.GetUserBrand(authInfo.UserId).ToList();
            var selectSites = result.Select(x => x.SiteId).ToList();

            return selectSites;
        }

        #region Withdrawal to Draft

        private ApiResult CheckWithdrawalDraft(SendToNextRequest request, WithdrawalRequest withdrawalData,
            ApiResult apiResult)
        {
            if (request.WithdrawalAccountingModels != null &&
                withdrawalData.WithdrawalType is WithdrawalTypeEnum.LocalBank or WithdrawalTypeEnum.CryptoWallet)
            {
                var sumAmountData = request.WithdrawalAccountingModels.Sum(model => model.Amount);
                if (withdrawalData.Amount != sumAmountData)
                {
                    apiResult.ErrorCode = 1101;
                    apiResult.Message = "Accounting sum is Not EQUAL withdrawal num.";
                    return apiResult;
                }
            }

            return null;
        }

        #endregion

        #region Withdrawal to Process

        private ApiResult CheckWithdrawalTypeAndSetObject(SendToNextRequest request, ApiResult apiResult, WithdrawalInfo withdrawalInfo)
        {
            switch (request.WithdrawalType)
            {
                case null:
                case WithdrawalTypeEnum.Undefined:
                    apiResult.ErrorCode = 1101;
                    apiResult.Message = "Select 1 Type of Withdrawal Please.";
                    return apiResult;

                case WithdrawalTypeEnum.OnlineBanking:
                case WithdrawalTypeEnum.EWalletPayment:
                case WithdrawalTypeEnum.CryptoPay:
                case WithdrawalTypeEnum.Settlement:
                    withdrawalInfo.PayId = request.SelId;
                    withdrawalInfo.ChannelId = request.ChannelId;
                    break;
                
                case WithdrawalTypeEnum.LocalBank:
                case WithdrawalTypeEnum.CryptoWallet:
                    {
                        var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);
                        if (request.WithdrawalAccountingModels == null)
                        {
                            apiResult.ErrorCode = 1101;
                            apiResult.Message = "Required Withdrawal Accounting data";
                            return apiResult;
                        }

                        var sumAmount = request.WithdrawalAccountingModels.Sum(model => model.Amount);
                        if (withdrawalRequest.Amount != sumAmount)
                        {
                            apiResult.ErrorCode = 1101;
                            apiResult.Message = "Accounting sum is Not EQUAL withdrawal num.";
                            return apiResult;
                        }

                        var bankInfoOff = _bankDetailBll
                            .GetOfflineBankAccount(withdrawalRequest.SiteId, withdrawalRequest.CurrencyId)
                            .FirstOrDefault(x => x.BankId == request.SelId);

                        if (bankInfoOff != null)
                        {
                            withdrawalInfo.PayId = null;
                            withdrawalInfo.FromAccountNo = bankInfoOff.AccountNo;
                            withdrawalInfo.FromAccountName = bankInfoOff.AccountName;
                            withdrawalInfo.FromSysId = bankInfoOff.SysId;
                            withdrawalInfo.BankId = bankInfoOff.BankId;
                        }

                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private static bool IsOnlinePayment(WithdrawalTypeEnum type)
            => type is (WithdrawalTypeEnum.CryptoPay
                or WithdrawalTypeEnum.OnlineBanking
                or WithdrawalTypeEnum.EWalletPayment);

        private static bool IsLocalPayment(WithdrawalTypeEnum type)
            => type is (WithdrawalTypeEnum.LocalBank
                or WithdrawalTypeEnum.CryptoWallet
                or WithdrawalTypeEnum.Settlement);

        #endregion

        #endregion

    }
}
