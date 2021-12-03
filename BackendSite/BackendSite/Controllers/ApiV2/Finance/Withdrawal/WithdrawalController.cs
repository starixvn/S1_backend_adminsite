using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Finance.Withdrawal
{
    [Authorize]
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

        private readonly string _payApiUrl;

        private readonly string _awsKey;
        private readonly string _awsSecretKey;
        private readonly string _awsWithdrawalFolder;
        private readonly string _awsWithdrawalAccountingFolder;
        private readonly string _awsWithdrawalUrl;
        private readonly string _awsWithdrawalBucketName;
        private readonly string _proxyHost;
        private readonly int _proxyPort;

        #endregion

        #region Constructors

        public WithdrawalController(IServiceProvider provider,
            IConfiguration configuration)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();

            _courierAdapter = provider.GetRequiredService<CourierAdapter>();

            _payApiUrl = configuration.GetValue<string>("PayService");

            _awsKey = configuration.GetValue<string>("AWSKey");
            _awsSecretKey = configuration.GetValue<string>("AwsSecretKey");
            _awsWithdrawalFolder = configuration.GetValue<string>("AwsWithdrawalFolder");
            _awsWithdrawalAccountingFolder = configuration.GetValue<string>("AwsWithdrawalAccountingFolder");
            _awsWithdrawalUrl = configuration.GetValue<string>("AwsWithdrawalUrl");
            _awsWithdrawalBucketName = configuration.GetValue<string>("AwsWithdrawalBucketName");
            _proxyHost = configuration.GetValue<string>("ProxyHost");
            _proxyPort = configuration.GetValue<int>("ProxyPort");
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;
            public IEnumerable<int> SiteIds { get; set; }
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
            public int AmountType { get; set; }
            public string Amount { get; set; }
        }

        public class GetByTransIdRequest
        {
            public string TransId { get; set; }
        }

        public class ConfirmRequest : GetByTransIdRequest
        {
            public string Comment { set; get; }
            public int Action { set; get; }
            public string WithdrawalInfo { set; get; }
            public int? WithdrawalType { set; get; }
            public int? SelId { set; get; }
            public int? ChannelId { set; get; }
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

        public class WithdrawalPicUrl
        {
            public string Url { get; set; }
        }

        public class UploadPictureRequest
        {
            public string Pic { get; set; }
            public string TransId { get; set; }
        }

        public class DeletePictureRequest
        {
            public string TransId { get; set; }
        }


        #endregion

        #region Actions

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalList" })]
        [HttpPost]
        public ApiListResultCurrencyAmount<WithdrawalRequest> GetList(GetListRequest request)
        {
            var apiListResult = new ApiListResultCurrencyAmount<WithdrawalRequest>();
            var totalPages = 0;
            var rowCount = 0;
            var CurrencyAmount = new List<string>();
            var withdrawalRequestList = _withdrawalBll.GetWithdrawalRequestList(request.SiteIds,
                request.WithdrawalTypes, request.StartDate.DateTime, request.EndDate.DateTime,
                request.WithdrawalId, request.UserName, request.CurrencyIds,
                request.Statuses, request.LevelStatuses, request.PageSize, request.AmountType, request.Amount, request.PageNumber, out totalPages,
                out rowCount, ref CurrencyAmount);
            apiListResult.Data = withdrawalRequestList;
            apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
            apiListResult.PageNumber = request.PageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            apiListResult.CurrencyAmount = CurrencyAmount;

            return apiListResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalList" })]
        [HttpPost]
        public IActionResult Export(GetListRequest request)
        {

            var data = _withdrawalBll.GetWithdrawalExportList(JsonConvert.SerializeObject(request.SiteIds),
                JsonConvert.SerializeObject(request.WithdrawalTypes),
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.WithdrawalId,
                request.UserName,
                JsonConvert.SerializeObject(request.CurrencyIds),
                JsonConvert.SerializeObject(request.Statuses),
                JsonConvert.SerializeObject(request.LevelStatuses),
                request.AmountType,
                request.Amount);

            //foreach (var item in data)
            //{
            //    item.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            //    if (item.DepositTime != null)
            //        item.DepositTime = new DateTimeOffset(data.DepositTime.Value.DateTime, TimeSpan.Zero);
            //    if (item.UpdateTime != null)
            //        item.UpdateTime = new DateTimeOffset(data.UpdateTime.Value.DateTime, TimeSpan.Zero);
            //}

            var stream = _excelBll.Export(data);
            return _excelBll.ExcelFile(stream, "WithdrawalRequestList");
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalList" })]
        [HttpPost]
        public async Task<ApiResult<WithdrawalRequest>> GetByTransId(GetByTransIdRequest request)
        {
            var apiResult = new ApiResult<WithdrawalRequest>();
            var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);

            if (withdrawalRequest.Operator == User.Identity.Name)
            {
                withdrawalRequest.SameOperator = true;
            }

            try
            {
                if (withdrawalRequest.Status == 4 || withdrawalRequest.Status == 6)
                {
                    var config = new AmazonS3Config
                    {
                        RegionEndpoint = RegionEndpoint.APSoutheast1
                    };

                    if (!string.IsNullOrEmpty(_proxyHost))
                    {
                        config.ProxyHost = _proxyHost;
                        config.ProxyPort = _proxyPort;
                    }

                    IAmazonS3 client = new AmazonS3Client(_awsKey, _awsSecretKey, config);

                    var getRequest = new GetObjectRequest();
                    var key = string.Format("{0}/{1}_{2}.png", _awsWithdrawalFolder, request.TransId,
                        withdrawalRequest.BankSlipPicCount);
                    getRequest.BucketName = _awsWithdrawalBucketName;
                    getRequest.Key = key;
                    var getResponse = await client.GetObjectAsync(getRequest);
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
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalList" })]
        [HttpPost]
        public ApiResult<IEnumerable<Comments>> GetCommentByTransId(GetByTransIdRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<Comments>>();
            var comments = _withdrawalBll.GetWithdrawalRequestComment(request.TransId);
            apiResult.Data = comments;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalList" })]
        [HttpPost]
        public async Task<ApiResult> ConfirmAsync(ConfirmRequest request)
        {
            var apiResult = new ApiResult(_selLangBll);
            var withdrawConfirmReuslt = -1;
            var authData = _authorizeBll.GetAuthData();

            switch (request.Action)
            {
                case (int)WithdrawalStatus.Draft:
                    withdrawConfirmReuslt = _withdrawalBll.WithdrawalDraft(request.TransId,
                        request.Comment,
                        User.Identity.Name);
                    break;
                case (int)WithdrawalStatus.Pending:
                    withdrawConfirmReuslt = _withdrawalBll.WithdrawalPending(request.TransId,
                        request.Comment,
                        User.Identity.Name);
                    break;
                case (int)WithdrawalStatus.OnHold:
                    withdrawConfirmReuslt = _withdrawalBll.WithdrawalOnHold(request.TransId,
                        request.Comment,
                        User.Identity.Name);
                    break;
                case (int)WithdrawalStatus.Verify:
                    withdrawConfirmReuslt = _withdrawalBll.WithdrawalVerify(request.TransId,
                        request.Comment,
                        User.Identity.Name);
                    break;
                case (int)WithdrawalStatus.Process:
                    {
                        if (request.WithdrawalType == null || request.WithdrawalType == (int)WithdrawalType.Undefine)
                        {
                            apiResult.ErrorCode = 1101;
                            apiResult.Message = "Select 1 Type of Withdrawal Please.";
                            return apiResult;
                        }

                        var withdrawalInfo = JsonConvert.DeserializeObject<WithdrawalInfo>(request.WithdrawalInfo);

                        switch (request.WithdrawalType)
                        {
                            case (int)WithdrawalType.OnlinePayment:
                                withdrawalInfo.PayId = request.SelId;
                                break;
                            case (int)WithdrawalType.LocalBank:
                                {
                                    var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);
                                    var bankInfoOff = _bankDetailBll.GetBankInfoOff(withdrawalRequest.SiteId)
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
                            case (int)WithdrawalType.CryptoPay:
                                withdrawalInfo.PayId = request.SelId;
                                withdrawalInfo.ChannelId = request.ChannelId;
                                break;
                        }

                        var custId = 0;
                        var amount = 0m;
                        withdrawConfirmReuslt = _withdrawalBll.WithdrawalProcess(request.TransId,
                            request.Comment,
                            User.Identity.Name,
                            JsonConvert.SerializeObject(withdrawalInfo),
                            request.WithdrawalType,
                            out custId, out amount);

                        if (withdrawConfirmReuslt > 0 &&
                            ((request.WithdrawalType == (int)WithdrawalType.OnlinePayment) ||
                             (request.WithdrawalType == (int)WithdrawalType.CryptoPay)))
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

                            var myUrl = "";
                            if (request.WithdrawalType == (int)WithdrawalType.CryptoPay)
                                myUrl = $"{_payApiUrl}CryptoWithdrawal";
                            else
                                myUrl = $"{_payApiUrl}Withdrawal";

                            var apiOnlineResult =
                                await _courierAdapter.MyPostAsync<bool, ReqWithdrawalOnline>(myUrl, reqWithdrawalOnline);
                            if (apiOnlineResult == null)
                            {
                                withdrawConfirmReuslt = _withdrawalBll.WithdrawalVerify(request.TransId,
                                    apiResult.Message, User.Identity.Name);
                                apiResult.ErrorCode = 1101;
                                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
                                return apiResult;
                            }
                            else if (apiOnlineResult.ErrorCode != 0)
                            {
                                withdrawConfirmReuslt = _withdrawalBll.WithdrawalVerify(request.TransId,
                                    apiOnlineResult.Message, User.Identity.Name);
                                return apiOnlineResult;
                            }
                        }
                        break;
                    }
                case (int)WithdrawalStatus.Approved:
                    {
                        //Check has pic or not
                        var hasPic = await CheckWithdrawalPictureAsync(request.TransId);

                        withdrawConfirmReuslt = _withdrawalBll.WithdrawalApproved(request.TransId, request.Comment,
                            User.Identity.Name, authData.UserId, hasPic);

                        if (withdrawConfirmReuslt > 0)
                        {
                            var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);
                            var userLang = _custInfoBll.GetCustInfoWithCache(withdrawalRequest.CustId).UserLang;
                            // Send message to player
                            var sender = _selLangBll.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                            var title = _selLangBll.GetResourceValue("lbl_WithdrawalApprovedTitle", userLang);
                            title = string.Format(title, request.TransId);
                            var content = _selLangBll.GetResourceValue("lbl_WithdrawalApprovedContent", userLang);
                            content = string.Format(content, request.TransId);
                            _messageBll.SendMessageToPlayer(sender, title, content, withdrawConfirmReuslt);

                            #region Copy Picture

                            if (withdrawalRequest.WithdrawalType == (int)WithdrawalType.LocalBank
                                && hasPic)
                            {
                                await CopyWithdrawalPictureAsync(withdrawalRequest.TransId);
                            }

                            #endregion
                        }
                        break;
                    }
                case (int)WithdrawalStatus.Rejected:
                    {
                        withdrawConfirmReuslt =
                            _withdrawalBll.WithdrawalRejected(request.TransId, request.Comment, User.Identity.Name);

                        if (withdrawConfirmReuslt > 0)
                        {
                            var withdrawalRequest = _withdrawalBll.GetWithdrawalRequest(request.TransId);
                            var userLang = _custInfoBll.GetCustInfoWithCache(withdrawalRequest.CustId).UserLang;
                            // Send message to player
                            var sender = _selLangBll.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                            var title = _selLangBll.GetResourceValue("lbl_WithdrawalRejectTitle", userLang);
                            title = string.Format(title, request.TransId);
                            var content = _selLangBll.GetResourceValue("lbl_WithdrawalRejectContent", userLang);
                            content = string.Format(content, request.TransId);
                            _messageBll.SendMessageToPlayer(sender, title, content, withdrawConfirmReuslt);
                        }
                        break;
                    }
                case (int)WithdrawalStatus.ToKyc:
                    withdrawConfirmReuslt = _withdrawalBll.WithdrawalToKyc(request.TransId,
                        request.Comment,
                        User.Identity.Name);
                    break;

            }

            if (withdrawConfirmReuslt <= 0)
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalList" })]
        [HttpGet]
        public ActionResult GetPicture(string transId, int BankSlipPicCount)
        {
            var WithdrawalPic = _withdrawalBll.GetWithdrawalPicCount(transId);
            return Redirect(string.Format("{0}{1}/{2}_{3}.png", _awsWithdrawalUrl, _awsWithdrawalFolder, transId,
                WithdrawalPic.BankSlipPicCount));
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalList" })]
        [HttpPost]
        public async Task<ApiResult> UploadPicture(UploadPictureRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = _withdrawalBll.UpdateWithdrawalPicCount(request.TransId);
                if (result)
                {
                    var WithdrawalPic = _withdrawalBll.GetWithdrawalPicCount(request.TransId);
                    var x = request.Pic.Replace("data:image/png;base64,", "");
                    var bytes = Convert.FromBase64String(x);
                    var stream = new MemoryStream(bytes);
                    var config = new AmazonS3Config
                    {
                        RegionEndpoint = RegionEndpoint.APSoutheast1
                    };

                    if (!string.IsNullOrEmpty(_proxyHost))
                    {
                        config.ProxyHost = _proxyHost;
                        config.ProxyPort = _proxyPort;
                    }

                    IAmazonS3 client = new AmazonS3Client(_awsKey, _awsSecretKey, config);
                    var putRequest = new PutObjectRequest();
                    var key = string.Format("{0}/{1}_{2}.png", _awsWithdrawalFolder, request.TransId,
                        WithdrawalPic.BankSlipPicCount);
                    putRequest.BucketName = _awsWithdrawalBucketName; // Your bucket name
                    putRequest.ContentType = "image/png";
                    putRequest.InputStream = stream;
                    putRequest.Key = key;


                    var putResponse = await client.PutObjectAsync(putRequest);
                    apiResult.ErrorCode = 0;
                }
                else
                {
                    apiResult.ErrorCode = 999;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalList" })]
        [HttpPost]
        public async Task<ApiResult> DeletePicture(DeletePictureRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var WithdrawalPic = _withdrawalBll.GetWithdrawalPicCount(request.TransId);
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.APSoutheast1
                };

                if (!string.IsNullOrEmpty(_proxyHost))
                {
                    config.ProxyHost = _proxyHost;
                    config.ProxyPort = _proxyPort;
                }

                IAmazonS3 client = new AmazonS3Client(_awsKey, _awsSecretKey, config);
                var deleteRequest = new DeleteObjectRequest();
                var key = string.Format("{0}/{1}_{2}.png", _awsWithdrawalFolder, request.TransId,
                    WithdrawalPic.BankSlipPicCount);
                deleteRequest.BucketName = _awsWithdrawalBucketName;
                deleteRequest.Key = key;
                var Response = await client.DeleteObjectAsync(deleteRequest);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
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
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.APSoutheast1
                };

                if (!string.IsNullOrEmpty(_proxyHost))
                {
                    config.ProxyHost = _proxyHost;
                    config.ProxyPort = _proxyPort;
                }

                IAmazonS3 client = new AmazonS3Client(_awsKey, _awsSecretKey, config);
                var request = new GetObjectRequest()
                {
                    BucketName = _awsWithdrawalBucketName,
                    Key = $"{_awsWithdrawalFolder}/{transId}_{withdrawalPicCount.BankSlipPicCount}.png"
                };
                var response = await client.GetObjectAsync(request);

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
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.APSoutheast1
                };

                if (!string.IsNullOrEmpty(_proxyHost))
                {
                    config.ProxyHost = _proxyHost;
                    config.ProxyPort = _proxyPort;
                }

                IAmazonS3 client = new AmazonS3Client(_awsKey, _awsSecretKey, config);
                var request = new CopyObjectRequest();
                var withdrawalKey = $"{_awsWithdrawalFolder}/{transId}_{withdrawalPicCount.BankSlipPicCount}.png";
                //NameingRule:{TransId}_{Seq}_{UploadTimes}
                var accountingKey = $"{_awsWithdrawalAccountingFolder}/{transId}_1_1.png";
                request.SourceBucket = _awsWithdrawalBucketName;
                request.SourceKey = withdrawalKey;
                request.DestinationBucket = _awsWithdrawalBucketName;
                request.DestinationKey = accountingKey;
                var response = await client.CopyObjectAsync(request);

                return response.HttpStatusCode == HttpStatusCode.OK;
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

        #endregion

    }
}