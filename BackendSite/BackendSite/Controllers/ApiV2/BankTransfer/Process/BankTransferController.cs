using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.BankTransfer.Process
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferManage" })]
    public class BankTransferController : ControllerBase
    {
        #region Fields
        
        private readonly AuthorizeBLL _authorizeBll;
        private readonly BankFinancialBLL _bankFinancialBll;
        private readonly BankDetailBLL _bankDetailBll;
        private readonly ExcelBLL _excelBll;
        
        #endregion

        #region Constructors

        public BankTransferController(IServiceProvider provider)
        {
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion
        
        #region API Model

        public class GetTransferInfoReq
        {
            public TransferTypeEnum? TransferType { get; set; }
            public string TransFromDate { get; set; }
            public string TransToDate { get; set; }
            public string FromAccountNo { get; set; }
            public string FromAccountDisplayName { get; set; }
            public string ToAccountNo { get; set; }
            public string ToAccountDisplayName { get; set; }
            public int? Reason { get; set; }
            public string TransferId { get; set; }
        }
        
        public class DelTransferInfoReq
        {
            public string TransferId { get; set; }
        }
        
        public class ProcessedImage
        {
            public string FileName { get; set; }
            public byte[] FileBytes { get; set; }
        }
        
        public class BankFinancialTransferInfoTransDetailModelForSet
        {
            public BankFinancialTransferInfoTransDetailModel TransferInfoDetails { get; set; }
            public IFormFile UploadReceipt { get; set; }
        }

        #endregion

        #region Action

        [HttpPost]
        public ApiResult GetTransferInfoList(GetTransferInfoReq req)
        {
            var apiResult = new ApiResult<IEnumerable<BankFinancialTransferInfoTransDetailModel>>();

            try
            {
                apiResult.Data = _bankFinancialBll.GetTransferInfoList(false,
                                                                       req.TransferType,
                                                                       req.TransFromDate,
                                                                       req.TransToDate,
                                                                       req.FromAccountNo,
                                                                       req.FromAccountDisplayName,
                                                                       req.ToAccountNo,
                                                                       req.ToAccountDisplayName,
                                                                       req.Reason,
                                                                       req.TransferId);
                foreach (var data in apiResult.Data.ToArray())
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
                }
                
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult CreateTransferInfo()
        {
            var apiResult = new ApiResult<BankFinancialTransferInfoTransDetailModel>();
            try
            {
                var authData = _authorizeBll.GetAuthData();
                var dateTime = new DateTimeOffset(DateTimeOffset.Now.ToUniversalTime().DateTime, TimeSpan.Zero);
                apiResult.Data = new BankFinancialTransferInfoTransDetailModel()
                {
                    TransDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    FromAmount = 0,
                    FromFee = 0,
                    ToAmount = 0,
                    ToFee = 0,
                    CreateTime = dateTime,
                    Creator = authData.UserId,
                    CreatorName = authData.UserName,
                    UpdateTime = dateTime,
                    Updator = authData.UserId,
                    UpdatorName = authData.UserName,
                };

                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<BankFinancialTransferInfoTransDetailModel> GetTransferInfoData(GetTransferInfoReq req)
        {
            var apiResult = new ApiResult<BankFinancialTransferInfoTransDetailModel>();
            
            try
            {
                var result = _bankFinancialBll.GetTransferInfoList(true,
                                                                   req.TransferType,
                                                                   req.TransFromDate,
                                                                   req.TransToDate,
                                                                   req.FromAccountNo,
                                                                   req.FromAccountDisplayName,
                                                                   req.ToAccountNo,
                                                                   req.ToAccountDisplayName,
                                                                   req.Reason,
                                                                   req.TransferId);

                if (result.Count() == 1)
                {
                    var data = result.FirstOrDefault();

                    var receipt = _bankFinancialBll.GetTransferIngoReceipt(data.TransferId);

                    var authData = _authorizeBll.GetAuthData();
                    var dateTime = new DateTimeOffset(DateTimeOffset.Now.ToUniversalTime().DateTime, TimeSpan.Zero);

                    if(receipt!=null)
                    {
                        data.UploadReceiptName = receipt.PicFileName;
                        data.TransferPic = receipt.TransferPic;
                    }

                    data.Updator = authData.UserId;
                    data.UpdatorName = authData.UserName;
                    data.UpdateTime = dateTime;

                    apiResult.Data = data;
                    apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = -1;
                    apiResult.Message = "The Transfer Account Detail Is Not Exist.";
                }
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult SetTransferInfo(BankFinancialTransferInfoTransDetailModelForSet request)
        {
            var apiResult = new ApiResult<int>();

            try
            {
                request.TransferInfoDetails.CreateTime = new DateTimeOffset(request.TransferInfoDetails.CreateTime.DateTime , TimeSpan.Zero);
                request.TransferInfoDetails.UpdateTime = new DateTimeOffset(request.TransferInfoDetails.UpdateTime.DateTime , TimeSpan.Zero);

                var (status, count, message) = _bankFinancialBll.SetTransferInfo(request.TransferInfoDetails, request.TransferInfoDetails.UploadReceipt);

                if (status)
                {
                    apiResult.Data = count;
                    apiResult.Message = message;
                    apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.Data = count;
                    apiResult.Message = message;
                    apiResult.ErrorCode = 500;
                }
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult DeleteTransferInfoData(DelTransferInfoReq req)
        {
            var apiResult = new ApiResult<int>();

            try
            {
                var (status, count, message) = _bankFinancialBll.DeleteTransferInfo(req.TransferId);

                if (status)
                {
                    apiResult.Data = count;
                    apiResult.RawMessage = message;
                    apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.Data = count;
                    apiResult.RawMessage = message;
                    apiResult.ErrorCode = 500;
                }
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public IActionResult ExportTransferInfoList(GetTransferInfoReq request)
        {
            var exportData = _bankFinancialBll.GetTransferInfoList(false,
                request.TransferType,
                request.TransFromDate,
                request.TransToDate,
                request.FromAccountNo,
                request.FromAccountDisplayName,
                request.ToAccountNo,
                request.ToAccountDisplayName,
                request.Reason,
                null);
            
            var stream = _excelBll.Export(exportData);
            return File(stream,
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       $"TransferInfoList_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        [HttpPost]
        public ApiResult<ProcessedImage> ReviewTransferReceiptPic([FromForm] IFormFile uploadReceipt)
        {
            var processedImage = new ProcessedImage();
            var apiResult = new ApiResult<ProcessedImage>();
            var transferReceipt = _bankFinancialBll.ProcessImage(uploadReceipt);

            processedImage.FileName = transferReceipt.FileName;
            processedImage.FileBytes = transferReceipt.FileBytes;
            apiResult.Data = processedImage;

            return apiResult;
        }

        #region Data For Set

        [HttpPost]
        public ApiResult GetTransferAccountReason()
        {
            var apiResult = new ApiResult<IEnumerable<BankFinancialReasonModel>>();

            try
            {
                apiResult.Data = _bankFinancialBll.GetReason("BankFinancial/TransferManage");
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult GetSysBankList()
        {
            var apiResult = new ApiResult<IEnumerable<BankInfoSys>>();

            try
            {
                apiResult.Data = _bankDetailBll.GetSysBankList();
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult GetPayList()
        {
            var apiResult = new ApiResult<IEnumerable<PayList>>();

            try
            {
                apiResult.Data = _bankFinancialBll.GetPayList();
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult GetTransferAccountList()
        {
            var apiResult = new ApiResult<IEnumerable<BankFinancialTransferAccountDetailModel>>();

            try
            {
                apiResult.Data = _bankFinancialBll.GetTransferAccountList();
                foreach (var data in apiResult.Data.ToArray())
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
                }
                
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        #endregion

        #endregion
    }
}