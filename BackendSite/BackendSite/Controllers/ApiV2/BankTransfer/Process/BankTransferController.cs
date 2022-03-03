using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
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

        public class GetListRequest
        {
            private DateTimeOffset _transFromDate;
            private DateTimeOffset _transToDate;
            public TransferTypeEnum? TransferType { get; set; }
            public DateTimeOffset? TransFromDate
            {
                get => _transFromDate;
                set
                {
                    if (value != null)
                        _transFromDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset? TransToDate
            {
                get => _transToDate;
                set
                {
                    if (value != null)
                        _transToDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public string FromAccountNo { get; set; }
            public string FromAccountDisplayName { get; set; }
            public string ToAccountNo { get; set; }
            public string ToAccountDisplayName { get; set; }
            public int? Reason { get; set; }
            public string TransferId { get; set; }
        }

        public class TransferIdRequest
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
            public BankFinancialTransferInfoTransDetailModel TransferInfoDetail { get; set; }
            public IFormFile UploadReceipt { get; set; }
        }

        #endregion

        #region Action

        /// <summary>
        /// BankTransfer/GetList 取得銀行轉帳資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Bank Transfer > Transfer Info Management > [Submit](Query)
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferManage" })]
        [HttpPost]
        public ApiResult<IEnumerable<BankFinancialTransferInfoTransDetailModel>> GetList(GetListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<BankFinancialTransferInfoTransDetailModel>>();

            if (string.IsNullOrWhiteSpace(request.FromAccountNo))
            {
                request.FromAccountNo = null;
            }

            if (string.IsNullOrWhiteSpace(request.FromAccountDisplayName))
            {
                request.FromAccountDisplayName = null;
            }

            if (string.IsNullOrWhiteSpace(request.ToAccountNo))
            {
                request.ToAccountNo = null;
            }

            if (string.IsNullOrWhiteSpace(request.ToAccountDisplayName))
            {
                request.ToAccountDisplayName = null;
            }

            var transFromDate = request.TransFromDate != new DateTimeOffset() ? request.TransFromDate?.DateTime : null;
            var transToDate = request.TransToDate != new DateTimeOffset() ? request.TransToDate?.DateTime : null;


            apiResult.Data = _bankFinancialBll.GetTransferInfoList(false,
                                                                   request.TransferType,
                                                                   transFromDate,
                                                                   transToDate,
                                                                   request.FromAccountNo,
                                                                   request.FromAccountDisplayName,
                                                                   request.ToAccountNo,
                                                                   request.ToAccountDisplayName,
                                                                   request.Reason,
                                                                   request.TransferId);
            foreach (var data in apiResult.Data.ToArray())
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }

            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// BankTransfer/Export 匯出銀行轉帳資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Bank Transfer > Transfer Info Management > [Submit](Query) > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferManage" })]
        [HttpPost]
        public IActionResult Export(GetListRequest request)
        {

            if (string.IsNullOrWhiteSpace(request.FromAccountNo))
            {
                request.FromAccountNo = null;
            }

            if (string.IsNullOrWhiteSpace(request.FromAccountDisplayName))
            {
                request.FromAccountDisplayName = null;
            }

            if (string.IsNullOrWhiteSpace(request.ToAccountNo))
            {
                request.ToAccountNo = null;
            }

            if (string.IsNullOrWhiteSpace(request.ToAccountDisplayName))
            {
                request.ToAccountDisplayName = null;
            }

            var transFromDate = request.TransFromDate != new DateTimeOffset() ? request.TransFromDate?.DateTime : null;
            var transToDate = request.TransToDate != new DateTimeOffset() ? request.TransToDate?.DateTime : null;

            var exportData = _bankFinancialBll.GetTransferInfoList(false,
                request.TransferType,
                transFromDate,
                transToDate,
                request.FromAccountNo,
                request.FromAccountDisplayName,
                request.ToAccountNo,
                request.ToAccountDisplayName,
                request.Reason,
                null);

            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "TransferInfoList");
        }

        /// <summary>
        /// BankTransfer/GetByTransferId 取得指定銀行轉帳資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Bank Transfer > Transfer Info Management > [Submit](Query) > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferManage" })]
        [HttpPost]
        public ApiResult<BankFinancialTransferInfoTransDetailModel> GetByTransferId(TransferIdRequest request)
        {
            var apiResult = new ApiResult<BankFinancialTransferInfoTransDetailModel>();

            var result = _bankFinancialBll.GetTransferInfoList(true,
                                                               null,
                                                               null,
                                                               null,
                                                               null,
                                                               null,
                                                               null,
                                                               null,
                                                               null,
                                                               request.TransferId);

            if (result.Count() == 1)
            {
                var data = result.FirstOrDefault();

                var receipt = _bankFinancialBll.GetTransferIngoReceipt(data.TransferId);

                var authData = _authorizeBll.GetAuthData();
                //var dateTime = new DateTimeOffset(DateTimeOffset.Now.ToUniversalTime().DateTime, TimeSpan.Zero);

                if (receipt != null)
                {
                    data.UploadReceiptName = receipt.PicFileName;
                    data.TransferPic = receipt.TransferPic;
                }

                data.Updator = authData.UserId;
                data.UpdatorName = authData.UserName;
                //data.UpdateTime = dateTime;

                apiResult.Data = data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "The Transfer Account Detail Is Not Exist.");
            }

            return apiResult;
        }

        /// <summary>
        /// BankTransfer/Set 新增/更新指定銀行轉帳資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Bank Transfer > Transfer Info Management > [Submit](Query) > [Edit] > [Submit]
        /// 
        /// Bank Transfer > Transfer Info Management > [Create] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferManage" })]
        [HttpPost]
        public ApiResult Set([FromForm] BankFinancialTransferInfoTransDetailModelForSet request)
        {
            var apiResult = new ApiResult<int>();

            request.TransferInfoDetail.CreateTime = new DateTimeOffset(request.TransferInfoDetail.CreateTime.DateTime, TimeSpan.Zero);
            request.TransferInfoDetail.UpdateTime = new DateTimeOffset(request.TransferInfoDetail.UpdateTime.DateTime, TimeSpan.Zero);

            var (status, count, message) = _bankFinancialBll.SetTransferInfo(request.TransferInfoDetail, request.UploadReceipt);

            if (status)
            {
                apiResult.Data = count;
                apiResult.Message = message;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.Data = count;
                apiResult.Message = message;
                apiResult.ErrorCode = 500;
            }

            return apiResult;
        }

        /// <summary>
        /// BankTransfer/Delete 刪除指定銀行轉帳資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Bank Transfer > Transfer Info Management > [Submit](Query) > [Delete]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferManage" })]
        [HttpPost]
        public ApiResult Delete(TransferIdRequest request)
        {
            var apiResult = new ApiResult();

            var (status, count, message) = _bankFinancialBll.DeleteTransferInfo(request.TransferId);

            if (status)
            {
                apiResult.RawMessage = message;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, message);
            }

            return apiResult;
        }

        /// <summary>
        /// 預覽上傳圖片
        /// </summary>
        /// <param name="uploadReceipt"></param>
        /// <returns></returns>
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferManage" })]
        //[HttpPost]
        //public ApiResult<ProcessedImage> ReviewTransferReceiptPic([FromForm] IFormFile uploadReceipt)
        //{
        //    var processedImage = new ProcessedImage();
        //    var apiResult = new ApiResult<ProcessedImage>();
        //    var transferReceipt = _bankFinancialBll.ProcessImage(uploadReceipt);

        //    processedImage.FileName = transferReceipt.FileName;
        //    processedImage.FileBytes = transferReceipt.FileBytes;
        //    apiResult.Data = processedImage;

        //    return apiResult;
        //}

        #endregion
    }
}