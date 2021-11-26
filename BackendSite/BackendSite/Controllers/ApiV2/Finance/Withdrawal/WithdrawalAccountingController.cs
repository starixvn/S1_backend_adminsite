using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BackendSite.Attributes;
using BackendSite.Controllers.Api;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.WithdrawalBankAccounting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BackendSite.Controllers.ApiV2.Finance.Withdrawal
{
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalAccountingController : ControllerBase
    {

        #region Fields

        private readonly ILogger<WithdrawalBankAccountingController> _logger;
        private readonly WithdrawalBLL _withdrawalBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly ExcelBLL _excelBll;

        private readonly string _awsKey;
        private readonly string _awsSecretKey;
        private readonly string _awsWithdrawalAccountingFolder;
        private readonly string _awsWithdrawalUrl;
        private readonly string _awsWithdrawalBucketName;
        private readonly string _proxyHost;
        private readonly int _proxyPort;

        #endregion

        #region Contructors

        public WithdrawalAccountingController(IConfiguration configuration, IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _logger = provider.GetRequiredService<ILogger<WithdrawalBankAccountingController>>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();

            _awsKey = configuration.GetValue<string>("AWSKey");
            _awsSecretKey = configuration.GetValue<string>("AwsSecretKey");
            _awsWithdrawalAccountingFolder = configuration.GetValue<string>("AwsWithdrawalAccountingFolder");
            _awsWithdrawalUrl = configuration.GetValue<string>("AwsWithdrawalUrl");
            _awsWithdrawalBucketName = configuration.GetValue<string>("AwsWithdrawalBucketName");
            _proxyHost = configuration.GetValue<string>("ProxyHost");
            _proxyPort = configuration.GetValue<int>("ProxyPort");
        }

        #endregion

        #region API Model

        public class GetWithdrawalInfoAndAccountingListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }

            public IEnumerable<int> CurrencyIds { get; set; }

            public DateTimeOffset StartDate { get; set; }

            public DateTimeOffset EndDate { get; set; }

            public string TransId { get; set; }

            public string UserName { get; set; }
        }

        public class GetByTransIdRequest
        {
            public string TransId { get; set; }
        }

        public class GetWithdrawalBankAccountingRequest
        {
            public int Id { get; set; }
        }

        public class CreateWithdrawalAccountingRequest
        {
            public string TransId { get; set; }

            public int MaxSeq { get; set; }
        }

        public class SetWithdrawalAccountingRequest
        {
            public int Id { get; set; }

            public string TransId { get; set; }

            public int Seq { get; set; }

            public string BankAccountNo { get; set; }

            public decimal Amount { get; set; }

            public decimal ChargeFee { get; set; }

            public string Memo { get; set; }


            public int UploadPicTimes { get; set; }

            public int Creator { get; set; }
        }

        public class DeleteWithdrawalAccountingRequest
        {
            public int Id { get; set; }
        }

        public class UploadAccountingPictureRequest
        {
            public string Picture { get; set; }
            public int Id { get; set; }
            public string TransId { get; set; }
            public int Seq { get; set; }
        }

        public class CheckAccountingPictureRequest
        {
            public string TransId { get; set; }
            public int Seq { get; set; }
            public int UploadPicTimes { get; set; }
        }

        public class DeleteAccountingPictureRequest
        {
            public string TransId { get; set; }
            public int Seq { get; set; }
            public int UploadPicTimes { get; set; }
        }

        #endregion

        #region Action

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public ApiResult GetWithdrawalInfoAndAccountingList(GetWithdrawalInfoAndAccountingListRequest request)
        {

            var result = _withdrawalBll.GetWithdrawalInfoAndAccountingList(request.SiteIds,
                request.CurrencyIds,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.TransId,
                request.UserName,
                request.PageNumber,
                request.PageSize);

            return new ApiListResult<WithdrawalInfoAndAccountingListModel>
            {
                Data = result.Datas,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = result.PageNumber,
                TotalPages = result.TotalPages,
                RowCount = result.RowCount
            };
        }

        [HttpPost]
        public IActionResult ExportWithdrawalAccountingList(string siteIds,
            string currencyIds,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            string transId,
            string userName)
        {

            startDate = new DateTimeOffset(startDate.ToUniversalTime().DateTime, TimeSpan.Zero);
            endDate = new DateTimeOffset(endDate.ToUniversalTime().DateTime, TimeSpan.Zero);

            var data = _withdrawalBll.GetWithdrawalInfoAndAccountingListForExport(siteIds,
                currencyIds,
                startDate.DateTime,
                endDate.DateTime,
                transId,
                userName);


            var stream = _excelBll.Export(data);

            return _excelBll.ExcelFile(stream, "WithdrawalInfoAndAccountList");
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        public ApiResult GetWithdrawalByTransId(GetByTransIdRequest request)
        {
            var result = _withdrawalBll.GetWithdrawalInfoByTransId(request.TransId,
                out var data);

            if (!result)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "data fail.");
            }
            else
            {
                return new ApiResult<WithdrawalRequest>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        public ApiResult GetWithdrawalAccountingByTransId(GetByTransIdRequest request)
        {
            var result = _withdrawalBll.GetWithdrawalAccountingByTransId(request.TransId,
                out var data);

            if (!result)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "data fail.");
            }
            else
            {
                return new ApiResult<IEnumerable<GetWithdrawalBankAccountingModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        public ApiResult GetWithdrawalBankAccounting(GetWithdrawalBankAccountingRequest request)
        {
            var result = _withdrawalBll.GetWithdrawalBankAccountingById(request.Id, out var data);

            if (!result)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail
                };
            }
            else
                return new ApiResult<GetWithdrawalBankAccountingModel>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        public ApiResult CreateWithdrawalBankAccounting(CreateWithdrawalAccountingRequest request)
        {
            var authData = _authorizeBll.GetAuthData();

            return new ApiResult<GetWithdrawalBankAccountingModel>
            {
                Data = new GetWithdrawalBankAccountingModel()
                {
                    Id = 0,
                    TransId = request.TransId,
                    Seq = request.MaxSeq + 1,
                    BankAccountNo = null,
                    Amount = 0,
                    ChargeFee = 0,
                    UploadPicTimes = 0,
                    Memo = string.Empty,
                    Creator = authData.UserId,
                    CreatorName = authData.UserName,
                    CreateTime = DateTime.Now.AddHours(8),
                    Operator = authData.UserId,
                    OperatorName = authData.UserName,
                    OperateTime = DateTime.Now.AddHours(8)
                },
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        public ApiResult InsertWithdrawalAccounting(SetWithdrawalAccountingRequest request)
        {
            var result = _withdrawalBll.InsertWithdrawalAccounting(new WithdrawalBankAccountingModel()
            {
                Id = request.Id,
                TransId = request.TransId,
                Seq = request.Seq,
                BankAccountNo = request.BankAccountNo,
                Amount = request.Amount,
                ChargeFee = request.ChargeFee,
                Memo = request.Memo,
                Creator = request.Creator
            });

            if (!result)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Insert fail.");
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Insert Successful."
                };
            }
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        public ApiResult UpdateWithdrawalAccounting(SetWithdrawalAccountingRequest request)
        {
            var result = _withdrawalBll.UpdateWithdrawalAccounting(new WithdrawalBankAccountingModel()
            {
                Id = request.Id,
                TransId = request.TransId,
                Seq = request.Seq,
                BankAccountNo = request.BankAccountNo,
                Amount = request.Amount,
                ChargeFee = request.ChargeFee,
                Memo = request.Memo,
                Creator = request.Creator
            });

            if (!result)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "update fail.");
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Successful."
                };
            }
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        public ApiResult DeleteWithdrawalAccounting(DeleteWithdrawalAccountingRequest request)
        {
            var result = _withdrawalBll.DeleteWithdrawalAccounting(request.Id, out var transId, out var seq,
                out var uploadPicTimes);

            if (!result)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "delete fail.");
            }
            else
            {
                _ = DeleteAccountPictureAsync(transId, seq, uploadPicTimes);

                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Delete Successful."
                };
            }
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public async Task<ApiResult> UploadAccountingPicture(UploadAccountingPictureRequest request)
        {
            var result = _withdrawalBll.UpdateWithdrawalAccountingForUploadPicture(request.Id, out var newUploadPictureTimes);
            if (result)
            {
                var pictureBaseOnSixFour = request.Picture.Replace("data:image/png;base64,", "");
                var bytes = Convert.FromBase64String(pictureBaseOnSixFour);
                var stream = new MemoryStream(bytes);

                if (await UploadAccountPictureAsync(stream, request.TransId, request.Seq, newUploadPictureTimes))
                {
                    return new ApiResult()
                    {
                        ErrorCode = (int)ErrorCodeEnum.Success
                    };
                }
            }

            return new ApiResult()
            {
                ErrorCode = (int)ErrorCodeEnum.Fail,
                Message = "Upload Fail."
            };
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpGet]
        public ActionResult GetAccountingPicture(string transId, int seq, int uploadPicTimes)
        {
            return Redirect(
                $"{_awsWithdrawalUrl}{_awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png");
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public async Task<ApiResult> DeleteAccountingPicture(DeleteAccountingPictureRequest request)
        {
            var deletePicResult = DeleteAccountPictureAsync(request.TransId, request.Seq, request.UploadPicTimes);

            if (!await deletePicResult)
            {
                return new ApiResult()
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Delete Fail."
                };
            }

            return new ApiResult()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = "Delete Successful."
            };
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public async Task<ApiResult> CheckAccountingPicture(CheckAccountingPictureRequest request)
        {
            return new ApiResult<bool>()
            {
                Data = await CheckAccountingPictureAsync(request.TransId, request.Seq, request.UploadPicTimes),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion

        #region Private Methods

        private async Task<bool> UploadAccountPictureAsync(MemoryStream stream, string transId, int seq,
            int uploadPicTimes)
        {
            try
            {
                if (await CheckAccountingPictureAsync(transId, seq, uploadPicTimes))
                {
                    return false;
                }

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
                var putRequest = new PutObjectRequest
                {
                    BucketName = _awsWithdrawalBucketName,
                    ContentType = "image/png",
                    InputStream = stream,
                    Key = $"{_awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png"
                };

                var putResponse = await client.PutObjectAsync(putRequest);

                return putResponse.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (AmazonS3Exception)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> DeleteAccountPictureAsync(string transId, int seq, int uploadPicTimes)
        {
            try
            {
                if (!await CheckAccountingPictureAsync(transId, seq, uploadPicTimes))
                    return false; //Picture not exist before delete.

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
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _awsWithdrawalBucketName,
                    Key = $"{_awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png"
                };

                await client.DeleteObjectAsync(deleteRequest);

                return !await CheckAccountingPictureAsync(transId, seq,
                    uploadPicTimes); //Picture not exist after delete.
            }
            catch (AmazonS3Exception)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> CheckAccountingPictureAsync(string transId, int seq, int uploadPicTimes)
        {
            try
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
                var request = new GetObjectRequest()
                {
                    BucketName = _awsWithdrawalBucketName,
                    Key = $"{_awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png"
                };
                var response = await client.GetObjectAsync(request);

                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (AmazonS3Exception)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

    }
}
