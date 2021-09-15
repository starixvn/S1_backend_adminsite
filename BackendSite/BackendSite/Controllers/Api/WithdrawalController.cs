using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Net;
using Amazon;
using BackendSite.ExceptionHandling;

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
        private readonly CustInfoBLL custInfoBLL;
        private readonly BankDetailBLL bankDetailBLL;
        private readonly ExcelBLL excelBLL;
        private readonly static BankInfoPayComparer bankInfoPayComparer = new BankInfoPayComparer();
        private readonly string payApiUrl;
        private readonly CourierAdapter courierAdapter;
        private readonly string awsKey;
        private readonly string awsSecretKey;
        private readonly string awsWithdrawalFolder;
        private readonly string awsWithdrawalAccountingFolder;
        private readonly string awsWithdrawalUrl;
        private readonly string awsWithdrawalBucketName;
        private readonly string ProxyHost;
        private readonly int ProxyPort;
        private readonly AuthorizeBLL _authorizeBLL;

        public WithdrawalController(IConfiguration configuration,
            CourierAdapter courierAdapter,
            WithdrawalBLL withdrawalBLL,
            SelLangBLL selLangBLL,
            MessageBLL messageBLL,
            CustInfoBLL custInfoBLL,
            BankDetailBLL bankDetailBLL,
            ExcelBLL excelBLL,
            AuthorizeBLL authorizeBLL)
        {
            this.withdrawalBLL = withdrawalBLL;
            this.selLangBLL = selLangBLL;
            this.messageBLL = messageBLL;
            this.custInfoBLL = custInfoBLL;
            this.bankDetailBLL = bankDetailBLL;
            this.excelBLL = excelBLL;
            _authorizeBLL = authorizeBLL;

            payApiUrl = configuration.GetValue<string>("PayService");
            this.courierAdapter = courierAdapter;
            awsKey = configuration.GetValue<string>("AWSKey");
            awsSecretKey = configuration.GetValue<string>("AwsSecretKey");
            awsWithdrawalFolder = configuration.GetValue<string>("AwsWithdrawalFolder");
            awsWithdrawalAccountingFolder = configuration.GetValue<string>("AwsWithdrawalAccountingFolder");
            awsWithdrawalUrl = configuration.GetValue<string>("AwsWithdrawalUrl");
            awsWithdrawalBucketName = configuration.GetValue<string>("AwsWithdrawalBucketName");
            ProxyHost = configuration.GetValue<string>("ProxyHost");
            ProxyPort = configuration.GetValue<int>("ProxyPort");
        }

        public class WithdrawalListReq
        {
            public IEnumerable<int> SelectSite { get; set; }
            public int PageNumber { get; set; }
            public string WithdrawalId { get; set; }
            public string UserName { get; set; }
            public IEnumerable<int> withdrawalType { get; set; }
            public IEnumerable<int> CurrencyId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
            public IEnumerable<int> status { get; set; }
            public IEnumerable<int> LevelStatus { get; set; }
        }

        [HttpPost]
        public ApiListResultCurrencyAmount<WithdrawalRequest> GetWithdrawalRequestList(
            WithdrawalListReq withdrawalListReq)
        {
            var apiListResult = new ApiListResultCurrencyAmount<WithdrawalRequest>();
            var pageNumber = withdrawalListReq.PageNumber;
            var totalPages = 0;
            var rowCount = 0;
            var CurrencyAmount = new List<string>();
            var withdrawalRequestList = withdrawalBLL.GetWithdrawalRequestList(withdrawalListReq.SelectSite,
                withdrawalListReq.withdrawalType, withdrawalListReq.startDate, withdrawalListReq.endDate,
                withdrawalListReq.WithdrawalId, withdrawalListReq.UserName, withdrawalListReq.CurrencyId,
                withdrawalListReq.status, withdrawalListReq.LevelStatus, 10, ref pageNumber, out totalPages,
                out rowCount, ref CurrencyAmount);
            apiListResult.Data = withdrawalRequestList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            apiListResult.CurrencyAmount = CurrencyAmount;

            return apiListResult;
        }

        public class WithdrawalReq
        {
            public string TransId { get; set; }
        }

        [HttpPost]
        public async Task<ApiResult<WithdrawalRequest>> GetWithdrawalRequest(WithdrawalReq withdrawalReq)
        {
            var apiResult = new ApiResult<WithdrawalRequest>();
            var withdrawalRequest = withdrawalBLL.GetWithdrawalRequest(withdrawalReq.TransId);

            if (withdrawalRequest.BankId > 0)
            {
            }

            if (withdrawalRequest.PayId > 0)
            {
            }

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

                    if (!string.IsNullOrEmpty(ProxyHost))
                    {
                        config.ProxyHost = ProxyHost;
                        config.ProxyPort = ProxyPort;
                    }

                    IAmazonS3 client = new AmazonS3Client(awsKey, awsSecretKey, config);

                    var getRequest = new GetObjectRequest();
                    var key = string.Format("{0}/{1}_{2}.png", awsWithdrawalFolder, withdrawalReq.TransId,
                        withdrawalRequest.BankSlipPicCount);
                    getRequest.BucketName = awsWithdrawalBucketName;
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

        [HttpPost]
        public ApiResult<IEnumerable<Comments>> GetWithdrawalRequestComment(WithdrawalReq withdrawalReq)
        {
            var apiResult = new ApiResult<IEnumerable<Comments>>();
            var comments = withdrawalBLL.GetWithdrawalRequestComment(withdrawalReq.TransId);
            apiResult.Data = comments;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        public class WithdrawalReq2 : WithdrawalReq
        {
            public string Comment { set; get; }
            public int Action { set; get; }
            public string WithdrawalInfo { set; get; }
            public int? WithdrawalType { set; get; }
            public int? SelId { set; get; }
            public int? ChannelId { set; get; }
        }

        [HttpPost]
        public async Task<ApiResult> WithdrawalConfirmAsync(WithdrawalReq2 withdrawalReq)
        {
            var apiResult = new ApiResult(selLangBLL);
            //var apiResult = new ApiResult();
            var withdrawConfirmReuslt = -1;
            var authData = _authorizeBLL.GetAuthData();
            if (withdrawalReq.Action == (int) WithdrawalStatus.Approved)
            {
                //Check has pic or not
                var hasPic = await CheckWithdrawalPictureAsync(withdrawalReq.TransId);

                withdrawConfirmReuslt = withdrawalBLL.WithdrawalApproved(withdrawalReq.TransId, withdrawalReq.Comment,
                    User.Identity.Name, authData.UserId,hasPic);

                if (withdrawConfirmReuslt > 0)
                {
                    var withdrawalRequest = withdrawalBLL.GetWithdrawalRequest(withdrawalReq.TransId);
                    var userLang = custInfoBLL.GetCustInfoWithCache(withdrawalRequest.CustId).UserLang;
                    // Send message to player
                    var sender = selLangBLL.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                    var title = selLangBLL.GetResourceValue("lbl_WithdrawalApprovedTitle", userLang);
                    title = string.Format(title, withdrawalReq.TransId);
                    var content = selLangBLL.GetResourceValue("lbl_WithdrawalApprovedContent", userLang);
                    content = string.Format(content, withdrawalReq.TransId);
                    messageBLL.SendMessageToPlayer(sender, title, content, withdrawConfirmReuslt);

                    #region Copy Picture

                    if (withdrawalRequest.WithdrawalType == (int) WithdrawalType.LocalBank
                        && hasPic)
                    {
                        await CopyWithdrawalPictureAsync(withdrawalRequest.TransId);
                    }

                    #endregion
                }
            }
            else if (withdrawalReq.Action == (int) WithdrawalStatus.Rejected)
            {
                withdrawConfirmReuslt =
                    withdrawalBLL.WithdrawalRejected(withdrawalReq.TransId, withdrawalReq.Comment, User.Identity.Name);

                if (withdrawConfirmReuslt > 0)
                {
                    var withdrawalRequest = withdrawalBLL.GetWithdrawalRequest(withdrawalReq.TransId);
                    var userLang = custInfoBLL.GetCustInfoWithCache(withdrawalRequest.CustId).UserLang;
                    // Send message to player
                    var sender = selLangBLL.GetResourceValue("lbl_MessageDepositWithdrawalSender", userLang);
                    var title = selLangBLL.GetResourceValue("lbl_WithdrawalRejectTitle", userLang);
                    title = string.Format(title, withdrawalReq.TransId);
                    var content = selLangBLL.GetResourceValue("lbl_WithdrawalRejectContent", userLang);
                    content = string.Format(content, withdrawalReq.TransId);
                    messageBLL.SendMessageToPlayer(sender, title, content, withdrawConfirmReuslt);
                }
            }
            else if (withdrawalReq.Action == (int) WithdrawalStatus.Verify)
            {
                withdrawConfirmReuslt =
                    withdrawalBLL.WithdrawalVerify(withdrawalReq.TransId, withdrawalReq.Comment, User.Identity.Name);
            }
            else if (withdrawalReq.Action == (int) WithdrawalStatus.Process)
            {
                var withdrawalInfo =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<WithdrawalInfo>(withdrawalReq.WithdrawalInfo);
                if (withdrawalReq.WithdrawalType == (int) WithdrawalType.OnlinePayment)
                {
                    withdrawalInfo.PayId = withdrawalReq.SelId;
                }
                else if (withdrawalReq.WithdrawalType == (int) WithdrawalType.LocalBank)
                {
                    var withdrawalRequest = withdrawalBLL.GetWithdrawalRequest(withdrawalReq.TransId);
                    var bankInfoOff = bankDetailBLL.GetBankInfoOff(withdrawalRequest.SiteId)
                        .FirstOrDefault(x => x.BankId == withdrawalReq.SelId);

                    if (bankInfoOff != null)
                    {
                        withdrawalInfo.PayId = null;
                        withdrawalInfo.FromAccountNo = bankInfoOff.AccountNo;
                        withdrawalInfo.FromAccountName = bankInfoOff.AccountName;
                        withdrawalInfo.FromSysId = bankInfoOff.SysId;
                        withdrawalInfo.BankId = bankInfoOff.BankId;
                    }
                }
                else if (withdrawalReq.WithdrawalType == (int) WithdrawalType.CryptoPay)
                {
                    withdrawalInfo.PayId = withdrawalReq.SelId;
                    withdrawalInfo.ChannelId = withdrawalReq.ChannelId;
                }

                if (withdrawalReq.WithdrawalType == null ||
                    withdrawalReq.WithdrawalType == (int) WithdrawalType.Undefine)
                {
                    apiResult.ErrorCode = 1101;
                    apiResult.Message = "Select 1 Type of Withdrawal Please.";
                    return apiResult;
                }
                else
                {
                    var custId = 0;
                    var amount = 0m;
                    withdrawConfirmReuslt = withdrawalBLL.WithdrawalProcess(withdrawalReq.TransId,
                        withdrawalReq.Comment, User.Identity.Name,
                        Newtonsoft.Json.JsonConvert.SerializeObject(withdrawalInfo), withdrawalReq.WithdrawalType,
                        out custId, out amount);

                    if (withdrawConfirmReuslt > 0 &&
                        ((withdrawalReq.WithdrawalType == (int) WithdrawalType.OnlinePayment) ||
                         (withdrawalReq.WithdrawalType == (int) WithdrawalType.CryptoPay)))
                    {
                        var reqWithdrawalOnline = new ReqWithdrawalOnline
                        {
                            SysId = withdrawalInfo.SysId,
                            TransId = withdrawalReq.TransId,
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
                        if (withdrawalReq.WithdrawalType == (int) WithdrawalType.CryptoPay)
                            myUrl = $"{payApiUrl}CryptoWithdrawal";
                        else
                            myUrl = $"{payApiUrl}Withdrawal";

                        var apiOnlineResult =
                            await courierAdapter.MyPostAsync<bool, ReqWithdrawalOnline>(myUrl, reqWithdrawalOnline);
                        if (apiOnlineResult == null)
                        {
                            withdrawConfirmReuslt = withdrawalBLL.WithdrawalVerify(withdrawalReq.TransId,
                                apiResult.Message, User.Identity.Name);
                            apiResult.ErrorCode = 1101;
                            apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
                            return apiResult;
                        }
                        else if (apiOnlineResult.ErrorCode != 0)
                        {
                            withdrawConfirmReuslt = withdrawalBLL.WithdrawalVerify(withdrawalReq.TransId,
                                apiOnlineResult.Message, User.Identity.Name);
                            return apiOnlineResult;
                        }
                    }
                }
            }
            else if (withdrawalReq.Action == (int) WithdrawalStatus.OnHold)
            {
                withdrawConfirmReuslt =
                    withdrawalBLL.WithdrawalOnHold(withdrawalReq.TransId, withdrawalReq.Comment, User.Identity.Name);
            }
            else if (withdrawalReq.Action == (int) WithdrawalStatus.Draft)
            {
                withdrawConfirmReuslt =
                    withdrawalBLL.WithdrawalDraft(withdrawalReq.TransId, withdrawalReq.Comment, User.Identity.Name);
            }
            else if (withdrawalReq.Action == (int) WithdrawalStatus.Pending)
            {
                withdrawConfirmReuslt =
                    withdrawalBLL.WithdrawalPending(withdrawalReq.TransId, withdrawalReq.Comment, User.Identity.Name);
            }
            else if (withdrawalReq.Action == (int) WithdrawalStatus.ToKyc)
            {
                withdrawConfirmReuslt =
                    withdrawalBLL.WithdrawalToKyc(withdrawalReq.TransId, withdrawalReq.Comment, User.Identity.Name);
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

        public class ReqWithdrawalOnline
        {
            public int SysId { get; set; }
            public string TransId { get; set; }
            public int PayId { get; set; }
            public int ChannelId { get; set; }
            public int CustId { get; set; }
            public decimal Amount { get; set; }

            //public string ClientIP { set; get; }
            public string AccountName { set; get; }
            public string AccountNo { set; get; }
            public string IFSCCode { set; get; }
            //public string ReturnUrl { set; get; }
        }

        public class WithdrawalReq3
        {
            public int CurrencyId { get; set; }
            public int SysId { get; set; }
            public int SiteId { get; set; }
            public string ChannelName { get; set; }
        }

        [HttpPost]
        public ApiResult<IEnumerable<BankInfoOff>> GetBankInfoOffList(WithdrawalReq3 withdrawalReq)
        {
            var apiResult = new ApiResult<IEnumerable<BankInfoOff>>();
            var bankInfoOff = bankDetailBLL.GetBankInfoOff(withdrawalReq.SiteId);
            bankInfoOff = bankInfoOff.Where(x => x.CurrencyId == withdrawalReq.CurrencyId && (x.Purpose & 2) > 0)
                .OrderBy(x => x.BSort);
            apiResult.Data = bankInfoOff;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        public class Pay
        {
            public int PayId { get; set; }
            public string PayName { get; set; }
            public int ChannelId { get; set; }
        }

        [HttpPost]
        public ApiResult<IEnumerable<Pay>> GetPayList(WithdrawalReq3 withdrawalReq)
        {
            var apiResult = new ApiResult<IEnumerable<Pay>>();
            var bankInfoPay = bankDetailBLL.GetBankInfoPay();
            var _payInfo = (from x in bankInfoPay
                where x.CurrencyId == withdrawalReq.CurrencyId && x.SysId == withdrawalReq.SysId
                select new Pay
                {
                    PayId = x.PayId,
                    PayName = x.PayName
                }).Distinct(bankInfoPayComparer);
            apiResult.Data = _payInfo;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<Pay>> GetCryptoPayList(WithdrawalReq3 withdrawalReq)
        {
            var apiResult = new ApiResult<IEnumerable<Pay>>();
            var cryptoPay = bankDetailBLL.GetCryptoPay();
            var _payInfo = (from x in cryptoPay
                            where x.CurrencyId == withdrawalReq.CurrencyId && x.ChannelName == withdrawalReq.ChannelName
                             select new Pay
                            {
                                PayId = x.PayId,
                                PayName = x.PayName,
                                ChannelId = x.ChannelId
                            }).Distinct(bankInfoPayComparer);
            
            apiResult.Data = _payInfo;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        private class BankInfoPayComparer : IEqualityComparer<Pay>
        {
            public bool Equals(Pay t1, Pay t2)
            {
                return (t1.PayId == t2.PayId);
            }

            public int GetHashCode(Pay t)
            {
                return t.ToString().GetHashCode();
            }
        }

        public class WithdrawalReq4
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
        /// CSD > Customer Administration > CustInfo > Withdrawal Info
        /// </remarks>
        /// <param name="withdrawalReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResultCurrencyAmount<WithdrawalRequest> GetWithdrawalListByCust(WithdrawalReq4 withdrawalReq)
        {
            var pageNumber = withdrawalReq.PageNumber;
            var totalPages = 0;
            var rowCount = 0;
            var CurrencyAmount = new List<string>();
            var data = withdrawalBLL.GetWithdrawalListByCust(withdrawalReq.CustId,
                withdrawalReq.TransId,
                withdrawalReq.Status,
                withdrawalReq.startDate,
                withdrawalReq.endDate,
                10,
                ref pageNumber,
                out totalPages,
                out rowCount,
                ref CurrencyAmount);

            return new ApiListResultCurrencyAmount<WithdrawalRequest>()
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                RowCount = rowCount,
                CurrencyAmount = CurrencyAmount,
                ErrorCode = (int) ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] {"Finance/WithdrawalLimitManagement"})]
        public ApiResult<IEnumerable<WithdrawalLimitInfoManage>> GetWithdrawalLimitInfos(
            WithdrawalLimit withdrawalLimit)
        {
            var apiResult = new ApiResult<IEnumerable<WithdrawalLimitInfoManage>>();
            var withdrawalLimitInfoManages =
                withdrawalBLL.GetWithdrawalLimitInfos(withdrawalLimit.SiteId, withdrawalLimit.CurrencyId);
            apiResult.Data = withdrawalLimitInfoManages;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] {"Finance/WithdrawalLimitManagement"})]
        public ApiResult AddWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage)
        {
            var result = withdrawalBLL.AddWithdrawalLimit(withdrawalLimitInfoManage, User.Identity.Name);
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] {"Finance/WithdrawalLimitManagement"})]
        public ApiResult EditWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage)
        {
            var result = withdrawalBLL.EditWithdrawalLimit(withdrawalLimitInfoManage, User.Identity.Name);
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] {"Finance/WithdrawalLimitManagement"})]
        [HttpPost]
        public ApiResult DeleteWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage)
        {
            var result = withdrawalBLL.DeleteWithdrawalLimit(withdrawalLimitInfoManage,User.Identity.Name);
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] {"Message/WithdrawalKYCControl"})]
        [HttpPost]
        public ApiResult<IEnumerable<WithdrawalKYC>> WithdrawalKYCList(WithdrawalKYC withdrawalKYC)
        {
            var result = withdrawalBLL.GetWithdrawalKYCList(withdrawalKYC.SiteId);
            var apiResult = new ApiResult<IEnumerable<WithdrawalKYC>>
            {
                Data = result,
                ErrorCode = 0
            };

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] {"Message/WithdrawalKYCControl"})]
        [HttpPost]
        public ApiResult EditWithdrawalKYCList(WithdrawalKYC withdrawalKYC)
        {
            var result = withdrawalBLL.EditWithdrawalKYCList(withdrawalKYC);
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

        public class WithdrawalLimitPara
        {
            public int SiteId { set; get; }
        }

        [HttpPost]
        public ApiResult<IEnumerable<WithdrawalData>> GetWithdrawalLimitData(WithdrawalLimitPara para)
        {
            var apiResult = new ApiResult<IEnumerable<WithdrawalData>>();
            try
            {
                apiResult.Data = withdrawalBLL.GetWithdrawalLimitData(para.SiteId);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        public class InsertWithdrawalLimitPara
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int WithdrawalMin { get; set; }
        }

        [HttpPost]
        public ApiResult InsertWithdrawalLimit(InsertWithdrawalLimitPara para)
        {
            var result = withdrawalBLL.InsertWithdrawalLimit(para.SiteId, para.CurrencyId, para.WithdrawalMin,
                User.Identity.Name);
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

        public class UpdateWithdrawalLimitPara
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int WithdrawalMin { get; set; }
        }

        [HttpPost]
        public ApiResult UpdateWithdrawalLimit(UpdateWithdrawalLimitPara para)
        {
            var result = withdrawalBLL.UpdateWithdrawalLimit(para.SiteId, para.CurrencyId, para.WithdrawalMin,
                User.Identity.Name);
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

        public class DeleteWithdrawalLimitPara
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        [HttpPost]
        public ApiResult DeleteWithdrawalLimit(DeleteWithdrawalLimitPara para)
        {
            var result = withdrawalBLL.DeleteWithdrawalLimit(para.SiteId, para.CurrencyId);
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

        public class UploadWithdrawalPicData
        {
            public string Pic { get; set; }
            public string TransId { get; set; }
        }

        [HttpPost]
        public async Task<ApiResult> UploadWithdrawalPic(UploadWithdrawalPicData para)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = withdrawalBLL.UpdateWithdrawalPicCount(para.TransId);
                if (result)
                {
                    var WithdrawalPic = withdrawalBLL.GetWithdrawalPicCount(para.TransId);
                    var x = para.Pic.Replace("data:image/png;base64,", "");
                    var bytes = Convert.FromBase64String(x);
                    var stream = new MemoryStream(bytes);
                    var config = new AmazonS3Config
                    {
                        RegionEndpoint = RegionEndpoint.APSoutheast1
                    };

                    if (!string.IsNullOrEmpty(ProxyHost))
                    {
                        config.ProxyHost = ProxyHost;
                        config.ProxyPort = ProxyPort;
                    }

                    IAmazonS3 client = new AmazonS3Client(awsKey, awsSecretKey, config);
                    var putRequest = new PutObjectRequest();
                    var key = string.Format("{0}/{1}_{2}.png", awsWithdrawalFolder, para.TransId,
                        WithdrawalPic.BankSlipPicCount);
                    putRequest.BucketName = awsWithdrawalBucketName; // Your bucket name
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

        public class WithdrawalPicUrl
        {
            public string Url { get; set; }
        }

        [HttpGet]
        public ActionResult GetWithdrawalPic(string transId, int BankSlipPicCount)
        {
            var WithdrawalPic = withdrawalBLL.GetWithdrawalPicCount(transId);
            return Redirect(string.Format("{0}{1}/{2}_{3}.png", awsWithdrawalUrl, awsWithdrawalFolder, transId,
                WithdrawalPic.BankSlipPicCount));
        }

        public class DeleteWithdrawalPicPara
        {
            public string TransId { get; set; }
        }

        [HttpPost]
        public async Task<ApiResult> DeleteWithdrawalPic(DeleteWithdrawalPicPara para)
        {
            var apiResult = new ApiResult();
            try
            {
                var WithdrawalPic = withdrawalBLL.GetWithdrawalPicCount(para.TransId);
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.APSoutheast1
                };

                if (!string.IsNullOrEmpty(ProxyHost))
                {
                    config.ProxyHost = ProxyHost;
                    config.ProxyPort = ProxyPort;
                }

                IAmazonS3 client = new AmazonS3Client(awsKey, awsSecretKey, config);
                var request = new DeleteObjectRequest();
                var key = string.Format("{0}/{1}_{2}.png", awsWithdrawalFolder, para.TransId,
                    WithdrawalPic.BankSlipPicCount);
                request.BucketName = awsWithdrawalBucketName;
                request.Key = key;
                var Response = await client.DeleteObjectAsync(request);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] {"Finance/WithdrawalTimeReport"})]
        public ApiListResult<WithdrawalTimeList> WithdrawalTimeReportGet(WithdrawalTimeReportPara para)
        {
            var apiListResult = new ApiListResult<WithdrawalTimeList>();
            try
            {
                var SearchResult = withdrawalBLL.WithdrawalTimeReportGet(para);
                apiListResult.Data = SearchResult.WithdrawalTimeList;
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] {"Finance/WithdrawalTimeReport"})]
        public IActionResult ExportWithdrawalTime(string SelectSite, string WithdrawalID, long StartDate, long EndDate,
            int Status, int CurrencyId)
        {
            var ExportData =
                withdrawalBLL.WithdrawalTimeExportGet(SelectSite, WithdrawalID, StartDate, EndDate, Status, CurrencyId);
            var stream = excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"WithdrawalTime_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

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
                var withdrawalPicCount = withdrawalBLL.GetWithdrawalPicCount(transId);
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.APSoutheast1
                };

                if (!string.IsNullOrEmpty(ProxyHost))
                {
                    config.ProxyHost = ProxyHost;
                    config.ProxyPort = ProxyPort;
                }

                IAmazonS3 client = new AmazonS3Client(awsKey, awsSecretKey, config);
                var request = new GetObjectRequest()
                {
                    BucketName = awsWithdrawalBucketName,
                    Key = $"{awsWithdrawalFolder}/{transId}_{withdrawalPicCount.BankSlipPicCount}.png"
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
                var withdrawalPicCount = withdrawalBLL.GetWithdrawalPicCount(transId);
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.APSoutheast1
                };

                if (!string.IsNullOrEmpty(ProxyHost))
                {
                    config.ProxyHost = ProxyHost;
                    config.ProxyPort = ProxyPort;
                }

                IAmazonS3 client = new AmazonS3Client(awsKey, awsSecretKey, config);
                var request = new CopyObjectRequest();
                var withdrawalKey = $"{awsWithdrawalFolder}/{transId}_{withdrawalPicCount.BankSlipPicCount}.png";
                //NameingRule:{TransId}_{Seq}_{UploadTimes}
                var accountingKey = $"{awsWithdrawalAccountingFolder}/{transId}_1_1.png";
                request.SourceBucket = awsWithdrawalBucketName;
                request.SourceKey = withdrawalKey;
                request.DestinationBucket = awsWithdrawalBucketName;
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