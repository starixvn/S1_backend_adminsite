using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class PersonalMessageController : ControllerBase
    {
        #region Fields

        private readonly MessageBLL _messageBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly Common _common;
        private readonly ILogger<PersonalMessageController> _logger;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public PersonalMessageController(IServiceProvider provider)
        {
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _common = provider.GetRequiredService<Common>();
            _logger = provider.GetRequiredService<ILogger<PersonalMessageController>>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion
        
        #region CSD > Personal Message Batch

        /// <summary>
        /// Message/SendMessageBatch 發送批次訊息
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > Add New > [Submit]
        /// </remarks>
        /// <param name="optSite"></param>
        /// <param name="excelUpload"></param>
        /// <param name="personalMsg"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiResult<int> SendMessageBatch([FromForm] int optSite,
            [FromForm] IFormFile excelUpload,
            [FromForm] string personalMsg)
        {
            var batchId = 0;
            try
            {
                var authData = _authorizeBll.GetAuthData();
                if (excelUpload.ContentType.Equals(
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        StringComparison.OrdinalIgnoreCase)
                    || excelUpload.ContentType.Equals("application/vnd.ms-excel",
                        StringComparison.OrdinalIgnoreCase)
                    || excelUpload.ContentType.Equals("application/octet-stream",
                        StringComparison.OrdinalIgnoreCase))
                {
                    var excelPkg = new ExcelPackage(excelUpload.OpenReadStream());
                    var sheet = excelPkg.Workbook.Worksheets.First(); //取得Sheet1
                    var isLastRow = false;
                    var rowId = 2; // 因為有標題列，所以從第2列開始讀起
                    _messageBll.NewBatchMessage(optSite, authData.UserName, personalMsg, ref batchId);

                    do // 讀取資料，直到讀到空白列為止
                    {
                        var custUsername = string.IsNullOrEmpty(sheet.Cells[rowId, 1].Text)
                            ? null
                            : sheet.Cells[rowId, 1].Text.Trim();
                        var sabaId = sheet.Cells[rowId, 2].Text.Trim();
                        var custId = (string.IsNullOrEmpty(sabaId)) ? null : _common.TpIdToCustId(sabaId);
                        var resultMessage = string.Empty;
                        var errorCode = (int) SendMessageStatus.Failed;

                        if (string.IsNullOrEmpty(custUsername) && custId == null)
                        {
                            isLastRow = true;
                        }
                        else
                        {
                            var custInfosDetail = _custInfoBll.GetCustInfoDetails(custId, custUsername, optSite);
                            if (custInfosDetail == null)
                            {
                                resultMessage =
                                    $"Not found. CustId={custId}. Username={custUsername}. SabaId={sabaId}.";
                            }
                            else if (custInfosDetail.Status == 1)
                            {
                                switch (custInfosDetail.Status)
                                {
                                    case (int) CustStatus.Running:
                                    case (int) CustStatus.Lock:
                                        SendBatchMessage(personalMsg,
                                            custInfosDetail.CustId,
                                            optSite,
                                            authData.UserName,
                                            batchId,
                                            custUsername,
                                            sabaId,
                                            (CustStatus) custInfosDetail.Status,
                                            out errorCode,
                                            out resultMessage);
                                        break;

                                    case (int) CustStatus.Suspend:
                                        resultMessage =
                                            $"Fail. CustId={custId}. Username={custUsername}. SabaId={sabaId}. Status={CustStatus.Suspend.GetDescription()}";
                                        break;

                                    case (int) CustStatus.Closed:
                                        resultMessage =
                                            $"Fail. CustId={custId}. Username={custUsername}. SabaId={sabaId}. Status={CustStatus.Closed.GetDescription()}";
                                        break;
                                }
                            }

                            InsertBatchMessageSendLog(
                                batchId,
                                rowId,
                                custInfosDetail?.CustId ?? 0,
                                custUsername,
                                sabaId,
                                resultMessage,
                                authData.UserName,
                                errorCode);

                            rowId += 1;
                        }
                    } while (!isLastRow);
                }
                else
                {
                    return new ApiResult<int>()
                    {
                        ErrorCode = (int)SendMessageStatus.Failed,
                        Message = "File Type Error"
                    };
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                return new ApiResult<int>()
                {
                    ErrorCode = (int) ErrorMessageCodeEnum.SystemException,
                    Message = "Unknown Error"
                };
            }
            
            return new ApiResult<int>()
            {
                Data = batchId,
                ErrorCode = 0,
                Message = "Send Process Finish."
            };
        }

        /// <summary>
        ///  Message/ExportSendMessageBatchResultReport 匯出指定批次訊息發送結果
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > Submit(Query) > [Export Excel]
        /// </remarks>
        /// <param name="BatchId"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public IActionResult ExportSendMessageBatchResultReport(int BatchId)
        {
            var data = _messageBll.GetBatchMessageSendLog(BatchId);
            var stream = _excelBll.Export(data);

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"SendMessageBatchResult_{BatchId}_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        /// <summary>
        /// Message/DeleteBatchMessage 刪除指定批次訊息
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > Submit(Query) > [Delete]
        /// 
        /// </remarks>
        /// <param name="SelectSite"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="batchId"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiListResult<BatchMsg> DeleteBatchMessage([FromForm] IEnumerable<int> SelectSite, [FromForm] long? StartDate, [FromForm] long? EndDate, [FromForm] int batchId, [FromForm] int PageNumber)
        {
            var PageSize = 10;
            var TotalPages = 0;
            var RowCount = 0;
            var apiResult = new ApiListResult<BatchMsg>();
            try
            {
                _messageBll.DeleteBatchMessage(batchId);
                apiResult.Data = _messageBll.GetBatchMessageList(SelectSite, StartDate, EndDate, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
                apiResult.ErrorCode = 0;

                apiResult.PageNumber = PageNumber;
                apiResult.RowCount = RowCount;
                apiResult.TotalPages = TotalPages;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = 999;
            }

            return apiResult;
        }

        /// <summary>
        /// Message/UpdateBatchMessage 更新指定批次訊息
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > Submit(Query) > Edit > [Update]
        /// 
        /// </remarks>
        /// <param name="batchId"></param>
        /// <param name="personalMsg"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiResult UpdateBatchMessage([FromForm] int batchId, [FromForm] string personalMsg)
        {
            var result = new ApiResult();
            try
            {
                if (_messageBll.UpdateBatchMessage(batchId, personalMsg))
                    result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.RawMessage = ex.ToString();
                result.ErrorCode = 999;
            }

            return result;
        }

        /// <summary>
        /// Message/GetBatchMessage 取得指定批次訊息
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > Submit(Query) > [Edit]
        /// 
        /// </remarks>
        /// <param name="batchId"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiResult<BatchMsg> GetBatchMessage([FromForm] int batchId)
        {
            var result = new ApiResult<BatchMsg>();
            try
            {
                result.Data = _messageBll.GetBatchMessage(batchId);
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.RawMessage = ex.ToString();
                result.ErrorCode = 999;
            }
            return result;
        }

        /// <summary>
        /// Message/GetBatchMessageList 取得批次訊息資料清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > [Submit](Query)
        /// 
        /// </remarks>
        /// <param name="SelectSite"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiListResult<BatchMsg> GetBatchMessageList([FromForm] IEnumerable<int> SelectSite, [FromForm] long? StartDate, [FromForm] long? EndDate, [FromForm] int PageNumber)
        {
            var PageSize = 10;
            var TotalPages = 0;
            var RowCount = 0;
            var apiResult = new ApiListResult<BatchMsg>();
            try
            {
                apiResult.Data = _messageBll.GetBatchMessageList(SelectSite, StartDate, EndDate, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
                apiResult.ErrorCode = 0;

                apiResult.PageNumber = PageNumber;
                apiResult.RowCount = RowCount;
                apiResult.TotalPages = TotalPages;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = 999;
            }

            return apiResult;
        }

        #endregion
        
        #region Private Methods

        private void SendBatchMessage(string personalMsg,
            int custId,
            int siteId,
            string updater,
            int batchMessageId,
            string userName,
            string sabaId,
            CustStatus custStatus,
            out int errorCode,
            out string resultMessage)
        {

            try
            {
                if (_messageBll.SendMessageBatch(personalMsg, custId, siteId, updater, batchMessageId))
                {
                    resultMessage = $"Success. CustId:{custId}. Username: {userName}. SabaId: {sabaId}. Status: {custStatus.GetDescription()}. ";
                    errorCode = (int)SendMessageStatus.Success;
                }
                else
                {
                    resultMessage = $"Fail. CustId:{custId}. Username: {userName}. SabaId: {sabaId}. Status: {custStatus.GetDescription()}. Insert done but affect count != 1 ";
                    errorCode = (int)SendMessageStatus.Failed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,MethodExtension.GetCurrentMethod());
                resultMessage = ex.ToString();
                errorCode = (int)SendMessageStatus.Failed;
            }
        }

        private void InsertBatchMessageSendLog(int batchMessageId,
            int rowId,
            int custId,
            string userName,
            string sabaId,
            string resultMessage,
            string updator,
            int status)
        {

            if (!_messageBll.InsertBatchMessageSendLog(batchMessageId,
                rowId,
                custId,
                userName,
                sabaId,
                resultMessage,
                updator,
                status))
            {
                _logger.LogError($@"MessageService-InsertBatchMessageSendLog Fail. " +
                    $@"BatchId: {batchMessageId}. CustId: {custId}. Username: {userName}." +
                    $@" SabaId: {sabaId}. Result: {resultMessage}. Status:{status}");
            }
        }

        #endregion
    }
}