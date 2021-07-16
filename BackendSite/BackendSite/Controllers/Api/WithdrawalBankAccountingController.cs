using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.WithdrawalBankAccounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WithdrawalBankAccountingController : ControllerBase
    {
        #region Fields

        private readonly ILogger<WithdrawalBankAccountingController> _logger;
        private readonly WithdrawalBLL _withdrawalBLL;
        private readonly AuthorizeBLL _authorizeBLL;

        private readonly string awsKey;
        private readonly string awsSecretKey;
        private readonly string awsWithdrawalAccountingFolder;
        private readonly string awsWithdrawalUrl;
        private readonly string awsWithdrawalBucketName;
        private readonly string ProxyHost;
        private readonly int ProxyPort;

        #endregion

        #region Constructors

        public WithdrawalBankAccountingController(IConfiguration configuration, IServiceProvider provider)
        {
            _withdrawalBLL = provider.GetRequiredService<WithdrawalBLL>();
            _authorizeBLL = provider.GetRequiredService<AuthorizeBLL>();
            _logger = provider.GetRequiredService<ILogger<WithdrawalBankAccountingController>>();

            awsKey = configuration.GetValue<string>("AWSKey");
            awsSecretKey = configuration.GetValue<string>("AwsSecretKey");
            awsWithdrawalAccountingFolder = configuration.GetValue<string>("AwsWithdrawalAccountingFolder");
            awsWithdrawalUrl = configuration.GetValue<string>("AwsWithdrawalUrl");
            awsWithdrawalBucketName = configuration.GetValue<string>("AwsWithdrawalBucketName");
            ProxyHost = configuration.GetValue<string>("ProxyHost");
            ProxyPort = configuration.GetValue<int>("ProxyPort");
        }

        #endregion

        #region API Model

        public class GetWithdrawalInfoAndAccountingListRequest
        {
            public IEnumerable<int> SiteIds { get; set; }

            public IEnumerable<int> CurrencyIds { get; set; }

            public string StartDate { get; set; }

            public string EndDate { get; set; }

            public string TransId { get; set; }

            public string UserName { get; set; }

            public int PageNumber { get; set; }

            public int PageSize { get; set; }
        }

        public class GetWithdrawalInfoAndAccountingByTransIdRequest
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute),
            Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        public ApiResult GetWithdrawalInfoAndAccountingList(GetWithdrawalInfoAndAccountingListRequest request)
        {
            var result = _withdrawalBLL.GetWithdrawalInfoAndAccountingList(request.SiteIds,
                request.CurrencyIds,
                request.StartDate,
                request.EndDate,
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

        public ApiResult GetWithdrawalInfoAndAccountingByTransId(GetWithdrawalInfoAndAccountingByTransIdRequest request)
        {
            var result = _withdrawalBLL.GetAccountingListByTransId(request.TransId,
                out GetWithdrawalInfoAndAccountingModel data);

            if (!result)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "data fail.");
            }
            else
            {
                return new ApiResult<GetWithdrawalInfoAndAccountingModel>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        public ApiResult GetWithdrawalBankAccounting(GetWithdrawalBankAccountingRequest request)
        {
            var result = _withdrawalBLL.GetWithdrawalBankAccountingById(request.Id, out GetWithdrawalBankAccountingModel data);

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

        public ApiResult CreateWithdrawalBankAccounting(CreateWithdrawalAccountingRequest request)
        {
            var authData = _authorizeBLL.GetAuthData();

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
                    Memo = "",
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

        public ApiResult InsertWithdrawalAccounting(SetWithdrawalAccountingRequest request)
        {
            var result = _withdrawalBLL.InsertWithdrawalAccounting(new WithdrawalBankAccountingModel()
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

        public ApiResult UpdateWithdrawalAccounting(SetWithdrawalAccountingRequest request)
        {
            var result = _withdrawalBLL.UpdateWithdrawalAccounting(new WithdrawalBankAccountingModel()
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

        public ApiResult DeleteWithdrawalAccounting(DeleteWithdrawalAccountingRequest request)
        {
            var result = _withdrawalBLL.DeleteWithdrawalAccounting(request.Id, out string transId, out int seq, out int uploadPicTimes);

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

        [HttpPost]
        public async Task<ApiResult> UploadAccountingPicture(UploadAccountingPictureRequest request)
        {
            var result = _withdrawalBLL.UpdateWithdrawalAccountingForUploadPicture(request.Id, out int newUploadPictureTimes);
            if (result)
            {
                string pictureBaseOnSixFour = request.Picture.Replace("data:image/png;base64,", "");
                byte[] bytes = Convert.FromBase64String(pictureBaseOnSixFour);
                MemoryStream stream = new MemoryStream(bytes);

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

        [HttpGet]
        public ActionResult GetAccountingPicture(string transId, int seq, int uploadPicTimes)
        {
            return Redirect($"{awsWithdrawalUrl}{awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png");
        }

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

        [HttpPost]
        public async Task<ApiResult> CheckAccountingPicture(CheckAccountingPictureRequest request)
        {
            return new ApiResult<bool>()
            {
                Data = await CheckAccountingPictureAsync(request.TransId, request.Seq, request.UploadPicTimes),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #region Private Methods

        private async Task<bool> UploadAccountPictureAsync(MemoryStream stream, string transId, int seq, int uploadPicTimes)
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

                if (!string.IsNullOrEmpty(ProxyHost))
                {
                    config.ProxyHost = ProxyHost;
                    config.ProxyPort = ProxyPort;
                }

                IAmazonS3 client = new AmazonS3Client(awsKey, awsSecretKey, config);
                var putRequest = new PutObjectRequest()
                {
                    BucketName = awsWithdrawalBucketName,
                    ContentType = "image/png",
                    InputStream = stream,
                    Key = $"{awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png"
                };

                PutObjectResponse putResponse = await client.PutObjectAsync(putRequest);

                if (putResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    return false;
                }
                return true;
            }
            catch (AmazonS3Exception awsEx)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<bool> DeleteAccountPictureAsync(string transId, int seq, int uploadPicTimes)
        {
            try
            {
                var isExist = CheckAccountingPictureAsync(transId, seq, uploadPicTimes);

                if (await isExist)
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
                    var deleeteRequest = new DeleteObjectRequest()
                    {
                        BucketName = awsWithdrawalBucketName,
                        Key = $"{awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png"
                    };
                    DeleteObjectResponse deleteResponse = await client.DeleteObjectAsync(deleeteRequest);

                    if (await CheckAccountingPictureAsync(transId, seq, uploadPicTimes))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (AmazonS3Exception awsEx)
            {
                return false;
            }
            catch (Exception ex)
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

                if (!string.IsNullOrEmpty(ProxyHost))
                {
                    config.ProxyHost = ProxyHost;
                    config.ProxyPort = ProxyPort;
                }

                IAmazonS3 client = new AmazonS3Client(awsKey, awsSecretKey, config);
                var request = new GetObjectRequest()
                {
                    BucketName = awsWithdrawalBucketName,
                    Key = $"{awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png"
                };
                GetObjectResponse response = await client.GetObjectAsync(request);

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

        #endregion
    }
}