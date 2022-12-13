using System.Data;
using System.Net;
using System.Xml.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Finance.Withdrawal
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalController : ControllerBase
    {
        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly SelLangBLL _selLangBll;
        private readonly ExcelBLL _excelBll;
        private readonly MessageBLL _messageBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly BankDetailBLL _bankDetailBll;
        private readonly AuthorizeBLL _authorizeBll;   
        private readonly CourierAdapter _courierAdapter;
        private readonly AWSS3Helper _s3Helper;
        private readonly PermissionBLL _permissionBll;
        private readonly Dictionary<int, List<int>> _usableActionType;
        private readonly Cryptor _cryptor;
        
        private readonly string _payApiUrl;
        private readonly string _awsWithdrawalFolder;
        private readonly string _awsWithdrawalAccountingFolder;
        private readonly string _awsWithdrawalUrl;
        private readonly string _awsWithdrawalBucketName;

        #endregion

        #region Constructors

        public WithdrawalController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
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
            
            var configuration = provider.GetRequiredService<IConfiguration>();
            _payApiUrl = configuration.GetValue<string>("PayService");
            _awsWithdrawalFolder = configuration.GetValue<string>("AwsWithdrawalFolder");
            _awsWithdrawalAccountingFolder = configuration.GetValue<string>("AwsWithdrawalAccountingFolder");
            _awsWithdrawalUrl = configuration.GetValue<string>("AwsWithdrawalUrl");
            _awsWithdrawalBucketName = configuration.GetValue<string>("AwsWithdrawalBucketName");
            _usableActionType = new Dictionary<int, List<int>>
            {
                { 1, new List<int> {66,6,63,69,71} }, //Pending -> Draft,Reject,OnHold,Verified,ToKyc
                { 2, new List<int> {66,69,71,6} },//On-Hold -> Draft,Verified,ToKyc,Reject
                { 3, new List<int> {66,70,64,6} },//Verified -> Draft,Process,Pending,Reject
                { 4, new List<int> {66,69,65,67,68} },//Process -> Draft ,Verified ,Approved,ManualApproved,ManualRejected
                { 6, new List<int> {66,68} },//Approved -> Draft,Manual Rejected
                { 7, new List<int> {66,67} },//Reject -> Draft,Manual Approved
                { 8, new List<int> {66,64,6} }//ToKyc -> Draft,Pending,Reject
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
        
        public class GetByTransIdRequest
        {
            public string TransId { get; set; }
        }

        public class SendToNextRequest : GetByTransIdRequest
        {
            public string Comment { set; get; }
            public int Action { set; get; }
            public string WithdrawalInfo { set; get; }
            public int? WithdrawalType { set; get; }
            public int? SelId { set; get; }
            public int? ChannelId { set; get; }
            public IEnumerable<WithdrawalAccountingModel> WithdrawalAccountingModels { set; get; }
        }

        public class ReqWithdrawalOnline
        {
            public int SysId { get; set; }
            public string TransId { get; set; }
            public int PayId { get; set; }
            public int ChannelId { get; set; }
            public int CustId { get; set; }
            public decimal Amount { get; set; }
            public string AccountName { set; get; }
            public string AccountNo { set; get; }
            public string IFSCCode { set; get; }
        }
        
        public class UploadPictureRequest : GetByTransIdRequest
        {
            public IEnumerable<string> Pic { get; set; }
        }

        public class DeletePictureRequest : GetByTransIdRequest
        {
            public int Number { get; set; }
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 1 })]
        [HttpPost]
        public ApiListResultCurrencyAmount<WithdrawalRequest> GetList(GetListRequest request)
        {
            var apiListResult = new ApiListResultCurrencyAmount<WithdrawalRequest>();
            var currencyAmount = new List<TotalAmount>();
            var userNames = FormatExtension.SplitRemoveSpace(request.UserName, ',');
            
            var withdrawalRequestList = _withdrawalBll.GetWithdrawalRequestList(
                new List<int> {request.SiteId},
                request.WithdrawalTypes, 
                request.StartDate.DateTime,
                request.EndDate.DateTime,
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
                out var totalPages,out var rowCount, ref currencyAmount);

            foreach (var data in withdrawalRequestList)
            {
                data.TransferPin = _cryptor.Decrypt(data.TransferPin);
                data.LoginPin = _cryptor.Decrypt(data.LoginPin);
                data.MobilePin = _cryptor.Decrypt(data.MobilePin);
                data.CorporateID = _cryptor.Decrypt(data.CorporateID);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 2 })]
        [HttpPost]
        public IActionResult Export(GetExportRequest request)
        {
            var exportData = new List<WithdrawalInfoForExportData>();
            var userNames = FormatExtension.SplitRemoveSpace(request.UserName, ',');
            
            var exportWithdrawal = _withdrawalBll.GetWithdrawalExportList(
                JsonConvert.SerializeObject(new List<int>{request.SiteId}),
                JsonConvert.SerializeObject(request.WithdrawalTypes),
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.WithdrawalId,
                userNames.Any() ? JsonConvert.SerializeObject(userNames): null,
                JsonConvert.SerializeObject(request.CurrencyIds),
                JsonConvert.SerializeObject(request.Statuses),
                JsonConvert.SerializeObject(request.LevelStatuses),
                request.StartAmount, 
                request.EndAmount,
                request.SelectBankCode.Any() ? JsonConvert.SerializeObject(request.SelectBankCode) : null,
                request.SelectCalCode.Any() ? JsonConvert.SerializeObject(request.SelectCalCode) : null,
                string.IsNullOrEmpty(request.Agent) ? null : request.Agent,
                request.WithdrawalMethod);
            
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

                exportData.Add(new WithdrawalInfoForExportData()
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
                    FlowEndTime = data.FlowEndTime
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 1 })]
        [HttpPost]
        public async Task<ApiResult<WithdrawalRequest>> GetByTransId(GetByTransIdRequest request)
        {
            var apiResult = new ApiResult<WithdrawalRequest>();
            var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);
            
            if (withdrawalRequest != null)
            {
                withdrawalRequest.WithdrawalAccountingModels = _withdrawalBll.GetWithdrawalAccountingByTransId(request.TransId);
                withdrawalRequest.UsableActionButton = GetUsableActionButton(withdrawalRequest);
            }
            
            if (withdrawalRequest.Operator == User.Identity.Name)
            {
                withdrawalRequest.SameOperator = true;
            }

            try
            {
                if (withdrawalRequest.Status is 4 or 6)
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 1 })]
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
            var withdrawConfirmResult = -1;
            var authData = _authorizeBll.GetAuthData();

            switch (request.Action)
            {
                case (int)WithdrawalStatusEnum.Draft:
                    withdrawConfirmResult = _withdrawalBll.WithdrawalDraft(request.TransId,
                        request.Comment,
                        User.Identity.Name);
                    break;
                case (int)WithdrawalStatusEnum.Pending:
                    withdrawConfirmResult = _withdrawalBll.WithdrawalPending(request.TransId,
                        request.Comment,
                        User.Identity.Name);
                    break;
                case (int)WithdrawalStatusEnum.OnHold:
                    withdrawConfirmResult = _withdrawalBll.WithdrawalOnHold(request.TransId,
                        request.Comment,
                        User.Identity.Name);
                    break;
                case (int)WithdrawalStatusEnum.Verify:
                    withdrawConfirmResult = _withdrawalBll.WithdrawalVerify(request.TransId,
                        request.Comment,
                        User.Identity.Name);
                    break;
                case (int)WithdrawalStatusEnum.Process:
                    {
                        if (request.WithdrawalType is null or (int)WithdrawalTypeEnum.Undefined)
                        {
                            apiResult.ErrorCode = 1101;
                            apiResult.Message = "Select 1 Type of Withdrawal Please.";
                            return apiResult;
                        }

                        var withdrawalInfo = JsonConvert.DeserializeObject<WithdrawalInfo>(request.WithdrawalInfo);

                        switch (request.WithdrawalType)
                        {
                            case (int)WithdrawalTypeEnum.OnlinePayment:
                                withdrawalInfo.PayId = request.SelId;
                                break;
                            
                            case (int)WithdrawalTypeEnum.LocalBank:
                                {
                                    var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);
                                    var bankInfoOff = _bankDetailBll
                                        .GetOfflineBankAccount(withdrawalRequest.SiteId, withdrawalRequest.CurrencyId)
                                        .FirstOrDefault(x => x.BankId == request.SelId);

                                    if (request.WithdrawalAccountingModels == null)
                                    {
                                        apiResult.ErrorCode = 1101;
                                        apiResult.Message = "Required Withdrawal Accounting data";
                                        return apiResult;
                                    }

                                    var sumAmount = request.WithdrawalAccountingModels.Sum(model => model.Amount);
                                    if ( withdrawalRequest.Amount != sumAmount )
                                    {
                                        apiResult.ErrorCode = 1101;
                                        apiResult.Message = "Accounting sum is Not EQUAL withdrawal num.";
                                        return apiResult;
                                    }

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
                            
                            case (int)WithdrawalTypeEnum.CryptoPay:
                                withdrawalInfo.PayId = request.SelId;
                                withdrawalInfo.ChannelId = request.ChannelId;
                                break;
                        }

                        withdrawConfirmResult = _withdrawalBll.WithdrawalProcess(
                            request.TransId,
                            request.Comment,
                            User.Identity.Name,
                            // authData.UserId,
                            1,
                            JsonConvert.SerializeObject(withdrawalInfo),
                            request.WithdrawalType,
                            request.WithdrawalAccountingModels,
                            out var custId, out var amount );

                        if (withdrawConfirmResult > 0 &&
                            request.WithdrawalType is (int)WithdrawalTypeEnum.OnlinePayment or (int)WithdrawalTypeEnum.CryptoPay)
                        {
                            var reqWithdrawalOnline = new ReqWithdrawalOnline
                            {
                                SysId = withdrawalInfo.SysId,
                                TransId = request.TransId,
                                CustId = custId,
                                Amount = amount,
                                PayId = withdrawalInfo.PayId ?? 0,
                                ChannelId = withdrawalInfo.ChannelId ?? 0,
                                AccountName = withdrawalInfo.AccountName,
                                AccountNo = withdrawalInfo.AccountNo,
                                IFSCCode = withdrawalInfo.IFSCCode
                                //ReturnUrl = Url.RouteUrl("desktop", new { action = "Deposit", controller = "Cashier", authData.LangId }, protocol: Request.Scheme, host: Request.Host.Port == 80 ? Request.Host.Host : $"{Request.Host.Host}:{Request.Host.Port}")
                            };

                            var myUrl = request.WithdrawalType == (int)WithdrawalTypeEnum.CryptoPay ? 
                                $"{_payApiUrl}CryptoWithdrawal" : $"{_payApiUrl}Withdrawal";

                            var apiOnlineResult =
                                await _courierAdapter.MyPostAsync<bool, ReqWithdrawalOnline>(myUrl, reqWithdrawalOnline);
                            if (apiOnlineResult == null)
                            {
                                _withdrawalBll.WithdrawalVerify(request.TransId,
                                    apiResult.Message, User.Identity.Name);
                                apiResult.ErrorCode = 1101;
                                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
                                return apiResult;
                            }
                            else if (apiOnlineResult.ErrorCode != 0)
                            {
                                _withdrawalBll.WithdrawalVerify(request.TransId,
                                    apiOnlineResult.Message, User.Identity.Name);
                                return apiOnlineResult;
                            }
                        }
                        break;
                    }
                case (int)WithdrawalStatusEnum.Approved:
                    {
                        //Check has pic or not
                        var hasPic = await CheckWithdrawalPictureAsync(request.TransId);

                        withdrawConfirmResult = _withdrawalBll.WithdrawalApproved(request.TransId, request.Comment,
                            User.Identity.Name, authData.UserId , hasPic);

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
                            _messageBll.SendMessageToPlayer(sender, title, content, withdrawConfirmResult);

                            #region Copy Picture

                            if (withdrawalRequest.WithdrawalType == (int)WithdrawalTypeEnum.LocalBank
                                && hasPic)
                            {
                                await CopyWithdrawalPictureAsync(withdrawalRequest.TransId);
                            }

                            #endregion
                        }
                        break;
                    }
                case (int)WithdrawalStatusEnum.Rejected:
                    {
                        withdrawConfirmResult =
                            _withdrawalBll.WithdrawalRejected(request.TransId, request.Comment, User.Identity.Name);

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
                            _messageBll.SendMessageToPlayer(sender, title, content, withdrawConfirmResult);
                        }
                        break;
                    }
                case (int)WithdrawalStatusEnum.ToKyc:
                    withdrawConfirmResult = _withdrawalBll.WithdrawalToKyc(request.TransId,
                        request.Comment,
                        User.Identity.Name);
                    break;

            }

            if (withdrawConfirmResult <= 0)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
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
                para.Operator = User.Identity.Name;
                para.ThirdId = string.Empty;
                var response = await _withdrawalBll.WithdrawalManualApproval(para);
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
                para.Operator = User.Identity.Name;
                para.ThirdId = string.Empty;
                var response = await _withdrawalBll.WithdrawalManualReject(para);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 3 })]
        [HttpPost]
        public async Task<ApiResult> UploadPicture(UploadPictureRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var requestCount = request.Pic.Count();
                var result = _withdrawalBll.UpdateWithdrawalPicCount(request.TransId , requestCount);
                if (result)
                {
                    var withdrawalPic = _withdrawalBll.GetWithdrawalPicCount(request.TransId);
                    
                    var count = 1;
                    foreach (var pic in request.Pic)
                    {
                        var countName = withdrawalPic.BankSlipPicCount - requestCount + count;
                        var x = pic.Replace("data:image/png;base64,", "");
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 28, 3 })]
        [HttpPost]
        public async Task<ApiResult> DeletePicture(DeletePictureRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
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

        
        private List<int> GetUsableActionButton(WithdrawalRequest withdrawalRequest)
        {
            var permissionList = _permissionBll.GetUserPermissionByMenuId(28);
            var usableList = _usableActionType[withdrawalRequest.Status];
            var usableActionButton = usableList.Where(item => permissionList.Contains(item)).ToList();
                    
            if (withdrawalRequest.WithdrawalType == (int) WithdrawalTypeEnum.LocalBank)
            {
                usableActionButton.Remove((int) WithdrawalActionEnum.ManualApproved);
                usableActionButton.Remove((int) WithdrawalActionEnum.ManualRejected);
            }
            else
            {
                usableActionButton.Remove((int) WithdrawalActionEnum.Draft);
                usableActionButton.Remove((int) WithdrawalActionEnum.Verified);
                usableActionButton.Remove((int) WithdrawalActionEnum.Approved);
            }

            return usableActionButton;
        }
        #endregion

    }
}
