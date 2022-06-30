using Amazon.S3;
using Amazon.S3.Model;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.WithdrawalBankAccounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WithdrawalBankAccountingController : ControllerBase
    {
        #region Fields


        private readonly WithdrawalBLL _withdrawalBLL;
        private readonly AuthorizeBLL _authorizeBLL;
        private readonly ExcelBLL _excelBLL;
        private readonly AWSS3Helper _s3Helper;

        private readonly string _awsWithdrawalAccountingFolder;
        private readonly string _awsWithdrawalUrl;
        private readonly string _awsWithdrawalBucketName;

        #endregion

        #region Constructors

        public WithdrawalBankAccountingController(IServiceProvider provider)
        {
            _withdrawalBLL = provider.GetRequiredService<WithdrawalBLL>();
            _authorizeBLL = provider.GetRequiredService<AuthorizeBLL>();
            _excelBLL = provider.GetRequiredService<ExcelBLL>();
            _s3Helper = provider.GetRequiredService<AWSS3Helper>();

            var configuration = provider.GetRequiredService<IConfiguration>();

            _awsWithdrawalAccountingFolder = configuration.GetValue<string>("AwsWithdrawalAccountingFolder");
            _awsWithdrawalUrl = configuration.GetValue<string>("AwsWithdrawalUrl");
            _awsWithdrawalBucketName = configuration.GetValue<string>("AwsWithdrawalBucketName");
        }

        #endregion

        #region API Model

        public class GetWithdrawalInfoAndAccountingListRequest
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }

            public IEnumerable<int> SiteIds { get; set; }

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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public ApiResult GetWithdrawalInfoAndAccountingList(GetWithdrawalInfoAndAccountingListRequest request)
        {
            var endDate = new DateTimeOffset(request.EndDate.ToUniversalTime().DateTime, TimeSpan.Zero).AddDays(1).AddMilliseconds(-1);

            var result = _withdrawalBLL.GetWithdrawalInfoAndAccountingList(request.SiteIds,
                request.CurrencyIds,
                request.StartDate.DateTime,
                endDate.DateTime,
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
            endDate = new DateTimeOffset(endDate.ToUniversalTime().DateTime, TimeSpan.Zero).AddDays(1).AddMilliseconds(-1);

            var data = _withdrawalBLL.GetWithdrawalInfoAndAccountingListForExport(siteIds,
                currencyIds,
                startDate.DateTime,
                endDate.DateTime,
                transId,
                userName);


            var stream = _excelBLL.Export(data);

            return _excelBLL.ExcelFile(stream, "WithdrawalInfoAndAccountList");
        }

        [HttpPost]
        public ApiResult GetWithdrawalInfoAndAccountingByTransId(GetWithdrawalInfoAndAccountingByTransIdRequest request)
        {
            var result = _withdrawalBLL.GetAccountingListByTransId(request.TransId,
                out var data);

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

        [HttpPost]
        public ApiResult GetWithdrawalBankAccounting(GetWithdrawalBankAccountingRequest request)
        {
            var result =
                _withdrawalBLL.GetWithdrawalBankAccountingById(request.Id, out var data);

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

        [HttpPost]
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

        [HttpPost]
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
            }, User.Identity.Name);

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

        [HttpPost]
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
            }, User.Identity.Name);

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

        [HttpPost]
        public ApiResult DeleteWithdrawalAccounting(DeleteWithdrawalAccountingRequest request)
        {
            var result = _withdrawalBLL.DeleteWithdrawalAccounting(request.Id, User.Identity.Name,
                out var transId, out var seq,
                out var uploadPicTimes, out var detailInfo);

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
            var result =
                _withdrawalBLL.UpdateWithdrawalAccountingForUploadPicture(request.Id, User.Identity.Name,
                    out var newUploadPictureTimes);
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

        [HttpGet]
        public ActionResult GetAccountingPicture(string transId, int seq, int uploadPicTimes)
        {
            return Redirect(
                $"{_awsWithdrawalUrl}{_awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png");
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

        private async Task<bool> UploadAccountPictureAsync(MemoryStream stream, string transId, int seq,
            int uploadPicTimes)
        {
            try
            {
                if (await CheckAccountingPictureAsync(transId, seq, uploadPicTimes))
                {
                    return false;
                }

                var putRequest = new PutObjectRequest
                {
                    BucketName = _awsWithdrawalBucketName,
                    ContentType = "image/png",
                    InputStream = stream,
                    Key = $"{_awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png"
                };

                var putResponse = await _s3Helper.PutObjectAsync(putRequest);

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

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _awsWithdrawalBucketName,
                    Key = $"{_awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png"
                };

                await _s3Helper.DeleteObjectAsync(deleteRequest);

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
                var request = new GetObjectRequest()
                {
                    BucketName = _awsWithdrawalBucketName,
                    Key = $"{_awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png"
                };
                var response = await _s3Helper.GetObjectAsync(request);

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

        #endregion
    }
}