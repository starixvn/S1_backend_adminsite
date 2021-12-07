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
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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

        public class GetListRequest : ApiRequestPageModel
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

        /// <summary>
        /// WithdrawalAccounting/GetList 取得提款帳務清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query)
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public ApiResult GetList(GetListRequest request)
        {

            var endDate = new DateTimeOffset(request.EndDate.ToUniversalTime().DateTime, TimeSpan.Zero).AddDays(1).AddMilliseconds(-1);

            var result = _withdrawalBll.GetWithdrawalInfoAndAccountingList(request.SiteIds,
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

        /// <summary>
        /// WithdrawalAccounting/Export 匯出提款帳務清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public IActionResult Export(GetListRequest request)
        {

            var startDate = new DateTimeOffset(request.StartDate.ToUniversalTime().DateTime, TimeSpan.Zero);
            var endDate = new DateTimeOffset(request.EndDate.ToUniversalTime().DateTime, TimeSpan.Zero);

            var data = _withdrawalBll.GetWithdrawalInfoAndAccountingListForExport(JsonConvert.SerializeObject(request.SiteIds),
                JsonConvert.SerializeObject(request.CurrencyIds),
                startDate.DateTime,
                endDate.DateTime,
                request.TransId,
                request.UserName);


            var stream = _excelBll.Export(data);

            return _excelBll.ExcelFile(stream, "WithdrawalInfoAndAccountList");
        }

        /// <summary>
        /// WithdrawalAccounting/GetWithdrawalInfoByTransId 取得指定提款資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Detail] > Withdrawal Info
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public ApiResult GetWithdrawalInfoByTransId(GetByTransIdRequest request)
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

        /// <summary>
        /// WithdrawalAccounting/GetAccountingListByTransId 取得指定提款帳務清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Detail] > Accounting List
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public ApiResult GetAccountingListByTransId(GetByTransIdRequest request)
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

        /// <summary>
        /// WithdrawalAccounting/GetById 取得指定提款帳務
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Detail] > Accounting List > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public ApiResult GetById(GetWithdrawalBankAccountingRequest request)
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

        /// <summary>
        /// WithdrawalAccounting/Create 建立提款帳務(提供Defalt Value)
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Detail] > [Create]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public ApiResult Create(CreateWithdrawalAccountingRequest request)
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

        /// <summary>
        /// WithdrawalAccounting/Insert 新增提款帳務
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Detail] > [Create] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public ApiResult Insert(SetWithdrawalAccountingRequest request)
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

        /// <summary>
        /// WithdrawalAccounting/Update 更新提款帳務
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Detail] > Accounting List > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public ApiResult Update(SetWithdrawalAccountingRequest request)
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

        /// <summary>
        /// WithdrawalAccounting/Delete 刪除提款帳務
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Detail] > Accounting List > [Delete]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpPost]
        public ApiResult Delete(DeleteWithdrawalAccountingRequest request)
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

        /// <summary>
        /// WithdrawalAccounting/UploadAccountingPicture 上傳帳務圖片
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Detail] > Accounting List > [Pic] > [Paste Area]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
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

        /// <summary>
        /// WithdrawalAccounting/GetAccountingPicture 取得帳務圖片
        /// </summary>
        /// <remarks>
        /// Http Method：GET
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Detail] > Accounting List > [Pic]
        /// </remarks>
        /// <param name="transId"></param>
        /// <param name="seq"></param>
        /// <param name="uploadPicTimes"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        [HttpGet]
        public ActionResult GetAccountingPicture(string transId, int seq, int uploadPicTimes)
        {
            return Redirect(
                $"{_awsWithdrawalUrl}{_awsWithdrawalAccountingFolder}/{transId}_{seq}_{uploadPicTimes}.png");
        }

        /// <summary>
        /// WithdrawalAccounting/DeleteAccountingPicture 刪除帳務圖片
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Detail] > Accounting List > [Pic] > [Delete]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
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

        /// <summary>
        /// WithdrawalAccounting/CheckAccountingPicture 檢查帳務圖片
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Accounting > [Submit](Query) > [Detail] > Accounting List > [Pic]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
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
