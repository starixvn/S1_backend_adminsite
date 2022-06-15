using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendbackendV2")]
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

        #region Model

        public class ExportMessageBatchReq
        {
            public int BatchId { get; set; }
        }
        public class DeleteBatchMessageReq
        {
            public int BatchId { get; set; }
        }
        public class UpdateBatchMessageReq
        {
            public int BatchId { get; set; }
            public bool IsPopup { get; set; }
            public List<LanguageDataModel> LanguageDatas { get; set; }
        }

        public class LanguageDataModel
        {
            public string Lang { get; set; }
            public MessageModel Message { get; set; }
        }

        public class MessageModel
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public string PopupMessage { get; set; }
        }

        public class GetBatchMessageReq
        {
            public int BatchId { get; set; }
        }

        public class GetBatchMessageListReq : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
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
        }
        #endregion

        #region Action

        /// <summary>
        /// PersonalMessage/SendMessageBatch 發送批次訊息
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Content-Type：Form-Data
        /// 
        /// Func Path：CSD > Personal Message Batch > Add New > [Submit]
        /// </remarks>
        /// <param name="siteId"></param>
        /// <param name="currency"></param>
        /// <param name="excelUpload"></param>
        /// <param name="personalMsg"></param>
        /// <param name="accountStatus"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public async Task<ApiResult<int>> SendMessageBatch([FromForm] SendMessageBatchRequest req)
        {
            var authData = _authorizeBll.GetAuthData();
            var batchId = 0;
            var excelList = new List<ExcelData>();
            _messageBll.NewBatchMessage(req.optSite, authData.UserName, req.IsPopup, req.personalMsg, ref batchId);
            _logger.LogInformation($@"MessageService-Read Excel And Send Start At {DateTime.UtcNow}.");

            if (req.excelUpload != null)
            {
                if (_excelBll.IsValidFile(req.excelUpload.ContentType))
                {
                    var excelPkg = new ExcelPackage(req.excelUpload.OpenReadStream());
                    var sheet = excelPkg.Workbook.Worksheets.First(); //取得Sheet1
                    var isLastRow = false;
                    var rowId = 2; // 因為有標題列，所以從第2列開始讀起

                    do // 讀取資料，直到讀到空白列為止
                    {
                        var custUsername = sheet.Cells[rowId, 1].Text.Trim();
                        var sabaId = sheet.Cells[rowId, 2].Text.Trim();
                        var custId = _common.TpIdToCustId(sabaId);

                        if (string.IsNullOrEmpty(custUsername) && custId == null)
                        {
                            isLastRow = true;
                        }
                        else
                        {
                            excelList.Add(new ExcelData
                            {
                                CustId = custId,
                                CustUserName = string.IsNullOrEmpty(custUsername) ? null : custUsername,
                                SabaId = sabaId,
                                RowId = rowId,
                            });
                            rowId += 1;
                        }
                    } while (!isLastRow);

                    foreach (var data in excelList)
                    {
                        var custInfosDetail = await _custInfoBll.GetCustInfoByCust(req.optSite, data.CustId, data.CustUserName);
                        await _messageBll.PreSendBatchMessage(data, custInfosDetail, req.personalMsg, authData, req.optSite, batchId, req.IsPopup);
                    }
                    _logger.LogInformation($@"MessageService-Read Excel And Send End At {DateTime.UtcNow}.");

                }
                else
                {
                    return new ApiResult<int>()
                    {
                        ErrorCode = -1,
                        Message = "File Type Error"
                    };
                }
            }
            else
            {
                var langList = new List<string>
                {
                    "en","th","cs","id","vn","hi","hi_en","id_en","th_en"
                };

                foreach (var lang in langList)
                {
                    var msg = JObject.Parse(req.personalMsg)[lang];
                    if (msg != null)
                    {
                        var popupMessage = msg.Value<string>("PopupMessage");
                        var title = msg.Value<string>("Title");
                        var content = msg.Value<string>("Content");
                        var batchPersonalMsg = new BatchPersonalMsg()
                        {
                            BatchId = batchId,
                            SiteId = req.optSite,
                            LangId = lang,
                            Operator = authData.UserName,
                            CurrencyId = req.currency,
                            AccountStatus = req.accountStatus,
                            IsPopup = req.IsPopup,
                            PopupMessage = popupMessage,
                            Content = content,
                            Title = title
                        };
                        _custInfoBll.SendMessageBatch(batchPersonalMsg);
                    }
                }
                _logger.LogInformation($@"MessageService-Filter Account And Send End At {DateTime.UtcNow}.");
            }

            return new ApiResult<int>()
            {
                Data = batchId,
                ErrorCode = 0,
                Message = "Send Process Finish."
            };
        }

        /// <summary>
        ///  PersonalMessage/ExportSendMessageBatchResultReport 匯出指定批次訊息發送結果
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > Submit(Query) > [Export Excel]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public IActionResult ExportSendMessageBatchResultReport(ExportMessageBatchReq req)
        {
            var data = _messageBll.GetBatchMessageSendLog(req.BatchId);
            var stream = _excelBll.Export(data);

            return _excelBll.ExcelFile(stream, "SendMessageBatchResult");
        }

        /// <summary>
        /// PersonalMessage/DeleteBatchMessage 刪除指定批次訊息
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > Submit(Query) > [Delete]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiResult DeleteBatchMessage(DeleteBatchMessageReq req)
        {
            if (!_messageBll.DeleteBatchMessage(req.BatchId))
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Delete fail."
                };
            }

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// PersonalMessage/UpdateBatchMessage 更新指定批次訊息
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > Submit(Query) > Edit > [Update]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiResult UpdateBatchMessage(UpdateBatchMessageReq req)
        {
            var message = new Dictionary<string, MessageModel>();

            foreach (var item in req.LanguageDatas)
            {
                if (!message.ContainsKey(item.Lang))
                {
                    message.Add(item.Lang, item.Message);
                }
                else
                {
                    throw new AdminException(ErrorCodeEnum.DuplicateData);
                }
            }

            var messageString = JsonConvert.SerializeObject(message);

            if (!_messageBll.UpdateBatchMessage(req.BatchId, req.IsPopup, messageString))
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Update Fail."
                };
            }

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// PersonalMessage/GetBatchMessage 取得指定批次訊息
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > Submit(Query) > [Edit]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiResult<BatchMsg> GetBatchMessage(GetBatchMessageReq req)
        {
            var data = _messageBll.GetBatchMessage(req.BatchId);

            if (data == null)
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            return new ApiResult<BatchMsg>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// PersonalMessage/GetBatchMessageList 取得批次訊息資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > [Submit](Query)
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiListResult<BatchMsg> GetBatchMessageList(GetBatchMessageListReq req)
        {
            var totalPages = 0;
            var rowCount = 0;
            var apiResult = new ApiListResult<BatchMsg>();
            try
            {
                var pageNumber = req.PageNumber;
                apiResult.Data = _messageBll.GetBatchMessageList(req.SiteIds, req.StartDate, req.EndDate, req.PageSize, ref pageNumber, ref totalPages, ref rowCount);
                apiResult.PageNumber = pageNumber;
                apiResult.RowCount = rowCount;
                apiResult.TotalPages = totalPages;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }

        #endregion

        #region Private Methods

        private void PreSendBatchMessage(ExcelData data, string personalMsg, AuthData authData, int optSite, int batchId)
        {
            var resultMessage = string.Empty;
            var errorCode = 0;
            if (data.CustInfoDetail == null)
            {
                resultMessage =
                    $"Not found. CustId={data.CustId}. Username={data.CustUserName}. SabaId={data.SabaId}.";
            }
            else
            {
                switch (data.CustInfoDetail.Status)
                {
                    case (int)CustomerStatusEnum.Running:
                        SendBatchMessage(personalMsg,
                            data.CustInfoDetail.CustId,
                            optSite,
                            authData.UserName,
                            batchId,
                            data.CustUserName,
                            data.SabaId,
                            (CustomerStatusEnum)data.CustInfoDetail.Status,
                            out errorCode,
                            out resultMessage);
                        break;

                    case (int)CustomerStatusEnum.Suspend:
                        resultMessage =
                            $"Fail. CustId={data.CustId}. Username={data.CustUserName}. SabaId={data.SabaId}. Status={CustomerStatusEnum.Suspend.GetDescription()}";
                        break;

                    case (int)CustomerStatusEnum.Closed:
                        resultMessage =
                            $"Fail. CustId={data.CustId}. Username={data.CustUserName}. SabaId={data.SabaId}. Status={CustomerStatusEnum.Closed.GetDescription()}";
                        break;

                    case (int)CustomerStatusEnum.Lock:
                        SendBatchMessage(personalMsg,
                            data.CustInfoDetail.CustId,
                            optSite,
                            authData.UserName,
                            batchId,
                            data.CustUserName,
                            data.SabaId,
                            (CustomerStatusEnum)data.CustInfoDetail.Status,
                            out errorCode,
                            out resultMessage);
                        break;
                }
            }

            int status = (errorCode == 1) ? 3 : 4;

            InsertBatchMessageSendLog(batchId,
                data.RowId,
                data.CustInfoDetail?.CustId ?? 0,
                data.CustUserName,
                data.SabaId,
                resultMessage,
                authData.UserName,
                status);
        }


        private void SendBatchMessage(string personalMsg,
            int custId,
            int siteId,
            string updater,
            int batchMessageId,
            string userName,
            string sabaId,
            CustomerStatusEnum custStatus,
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
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
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
