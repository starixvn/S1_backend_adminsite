using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class PersonalMessageController : ControllerBase
    {
        private readonly MessageBLL _messageBll;
        private readonly ServerInfoService _serverInfoService;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ILogger<PersonalMessageController> _logger;
        private readonly ExcelBLL _excelBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        private readonly NewAuthorizeBLL _newAuthorizeBLL;
        private readonly ICacheHelper _redisHelper;
        
        public PersonalMessageController(IServiceProvider provider)
        {
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _serverInfoService = provider.GetRequiredService<ServerInfoService>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _logger = provider.GetRequiredService<ILogger<PersonalMessageController>>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
            _newAuthorizeBLL = provider.GetRequiredService<NewAuthorizeBLL>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
        }

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
            public int SiteIds { get; set; }
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

        public class GetServerInfoReq
        {
            public int SiteId { get; set; }
        }

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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 25, 3 })]
        [HttpPost]
        public async Task<ApiResult<int>> SendMessageBatch([FromForm] SendMessageBatchRequest req)
        {
            // check wait time key
            var redisKey = "PersonalMessage:SendMessageBatch:WaitTimeKey";
            if (await _redisHelper.KeyExistsAsync(redisKey))
            {
                return new ApiResult<int>()
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message ="Database synchronization time, please wait 3 minutes."
                };
            }
            
            //Check Status
            var totalPages = 0;
            var rowCount = 0;
            var pageNumber = 0;
            var siteIdList = new List<int> { req.optSite };
            var startDate = DateTimeOffset.UtcNow.AddMinutes(-30);
            var batchMessageList = _messageBll.GetBatchMessageList(siteIdList, startDate, DateTimeOffset.UtcNow, 20, ref pageNumber, ref totalPages, ref rowCount);
            if (batchMessageList.Any((i => i.Status == 0)))
            {
                _logger.LogInformation(JsonConvert.SerializeObject(batchMessageList));
                return new ApiResult<int>()
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message ="Message unfinish, please wait finish."
                };
            }
            
            var authData = _newAuthorizeBLL.GetAuthData();
            var batchId = await _messageBll.NewBatchMessage(req.optSite, req.currency, authData.UserName, req.IsPopup, req.personalMsg);
            _logger.LogInformation($@"MessageService-Read Excel And Send Start At {DateTime.UtcNow}.");

            if (req.excelUpload != null)
            {
                if (!_excelBll.IsValidFile(req.excelUpload.ContentType))
                    return new ApiResult<int>() { ErrorCode = (int)ErrorCodeEnum.Fail, Message = "File Type Error" };

                var excelList = _excelBll.Read(req.excelUpload);
                if (!excelList.Any())
                    return new ApiResult<int>() { ErrorCode = (int)ErrorCodeEnum.Fail, Message = "Empty File" };

                _ = Task.Run(async () =>
                {
                    await _messageBll.BatchMessageByFile(req.optSite, batchId, req.IsPopup, authData.UserName, excelList);
                });
                _logger.LogInformation($@"MessageService-Read Excel And Send End At {DateTime.UtcNow}.");
            }
            else
            {
                #region checkMsg

                var (result, errorMsg) = _messageBll.CheckBatchMessage(req.currency, req.personalMsg);
                if (!result)
                {
                    return new ApiResult<int>()
                    {
                        ErrorCode = (int)ErrorCodeEnum.Fail,
                        Message = errorMsg
                    };
                }

                #endregion

                _ = Task.Run(async () =>
                {
                    var userSiteList = _serverInfoService.GetSiteList();
                    var langList = new List<string>();
                    foreach (var b in userSiteList.Select(x => x.LangList).ToList()
                                 .Select(JsonConvert.DeserializeObject<IEnumerable<string>>))
                    {
                        langList.AddRange(b.Except(langList));
                    }

                    foreach (var lang in langList)
                    {
                        var msg = JObject.Parse(req.personalMsg)[lang];
                        if (msg != null)
                        {
                            var batchPersonalMsg = new BatchPersonalMsg()
                            {
                                BatchId = batchId,
                                SiteId = req.optSite,
                                LangId = lang,
                                Operator = authData.UserName,
                                CurrencyId = req.currency,
                                AccountStatus = req.accountStatus,
                                IsPopup = req.IsPopup
                            };
                            _custInfoBll.SendMessageBatch(batchPersonalMsg);
                        }
                    }
                    
                    var waitTime = TimeSpan.FromMinutes(3); 
                    await _redisHelper.StringSetAsync(redisKey, JsonConvert.SerializeObject(req), waitTime);
                    _messageBll.UpdateBatchMessageStatus((int)SendBatchMessageStatus.Finished, batchId);
                    _logger.LogInformation($@"MessageService-Filter Account And Send End At {DateTime.UtcNow}.");
                });
            }
            _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
            {
                Operator = authData.UserName,
                SiteId = req.optSite,
                CurrencyId = req.currency,
                ActionType = ActionType.Create,
                MenuEnum = AdminAdjustmentLogMenuEnum.SMSGenerator,
                BeforeDetailInfo = null,
                AfterDetailInfo = JsonConvert.SerializeObject(req),
                RefTransId = batchId.ToString()
            });
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 25, 2 })]
        [HttpPost]
        public IActionResult ExportSendMessageBatchResultReport(ExportMessageBatchReq req)
        {
            var data = _messageBll.GetBatchMessageSendLog(req.BatchId);
            var stream = _excelBll.ExportV2(data);

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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 25, 4 })]
        [HttpPost]
        public async Task<ApiResult> DeleteBatchMessage(DeleteBatchMessageReq req)
        {
            var authData = _newAuthorizeBLL.GetAuthData();
            var error = await _messageBll.DeleteBatchMessage(authData.SiteId, req.BatchId, authData.UserName);
            if (error == -1)
                return new ApiResult { ErrorCode = (int)ErrorCodeEnum.Fail, Message = "Delete fail." };
            else if (error == -2)
                return new ApiResult { ErrorCode = (int)ErrorCodeEnum.Fail, Message = "batch message is processing, please wait a moment" };
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 25, 3 })]
        [HttpPost]
        public ApiResult UpdateBatchMessage(UpdateBatchMessageReq req)
        {
            var authData = _newAuthorizeBLL.GetAuthData();
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

            if (!_messageBll.UpdateBatchMessage(req.BatchId, req.IsPopup, messageString, authData.UserName))
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Update Fail."
                };
            }

            var siteId = _newAuthorizeBLL.CheckBrand();
            _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
            {
                Operator = authData.UserName,
                SiteId = siteId,
                CurrencyId = 0,
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.SMSGenerator,
                BeforeDetailInfo = null,
                AfterDetailInfo = JsonConvert.SerializeObject(req),
                RefTransId = string.Empty
            });

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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 25, 1 })]
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 25, 1 })]
        [HttpPost]
        public ApiListResult<BatchMsg> GetBatchMessageList(GetBatchMessageListReq req)
        {
            var totalPages = 0;
            var rowCount = 0;
            var apiResult = new ApiListResult<BatchMsg>();
            List<int> SiteIdList = new List<int>();
            SiteIdList.Add(req.SiteIds);
            try
            {
                var pageNumber = req.PageNumber;
                apiResult.Data = _messageBll.GetBatchMessageList(SiteIdList, req.StartDate, req.EndDate, req.PageSize, ref pageNumber, ref totalPages, ref rowCount);
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
    }
}
