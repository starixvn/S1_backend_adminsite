using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackendSite.Extensions;
using Newtonsoft.Json.Linq;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MessageController : ControllerBase
    {
        #region Fields

        private readonly MessageBLL messageBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly CustInfoBLL custInfoBLL;
        private readonly Common common;
        private readonly ILogger<MessageController> _logger;
        private readonly ExcelBLL _excelBLL;

        #endregion

        #region Constructor

        public MessageController(MessageBLL messageBLL,
            AuthorizeBLL authorizeBLL,
            ServerInfoBLL serverInfoBLL,
            CustInfoBLL custInfoBLL,
            Common common,
            ILogger<MessageController> logger,
            ExcelBLL excelBLL)
        {
            this.messageBLL = messageBLL;
            this.authorizeBLL = authorizeBLL;
            this.serverInfoBLL = serverInfoBLL;
            this.custInfoBLL = custInfoBLL;
            this.common = common;
            _logger = logger;
            _excelBLL = excelBLL;
        }

        #endregion

        #region API Model

        public class GetAnnouncementreq
        {
            public IEnumerable<int> SelectSite { get; set; }
            public int Status { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public int PageNumber { get; set; }
        }
        #endregion

        #region Action

        #region CSD > Announcement

        /// <summary>
        /// Message/GetAnnouncement 取得公告清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Announcement > [Submit]
        /// 
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiListResult<Announcement> GetAnnouncement(GetAnnouncementreq para)
        {
            var PageSize = 10;
            var TotalPages = 0;
            var RowCount = 0;
            var PageNumber = para.PageNumber;
            var apiResult = new ApiListResult<Announcement>();
            try
            {
                var siteList = serverInfoBLL.GetSiteList().ToDictionary(x => x.SiteId, y => y.SiteName);
                if (!siteList.ContainsKey(0))
                {
                    siteList.Add(0, "All");
                }

                if (para.StartDate > 2147483647)
                {
                    para.StartDate = 2147483647;
                }

                if (para.EndDate > 2147483647)
                {
                    para.EndDate = 2147483647;
                }

                var startTime = DateTimeExtensions.UnixToDateTime(para.StartDate);
                var endTime = DateTimeExtensions.UnixToDateTime(para.EndDate);
                var result = messageBLL.GetAnnouncements(para.SelectSite, startTime, endTime, para.Status, PageNumber,
                    PageSize, ref TotalPages, ref RowCount);
                foreach (var r in result)
                {
                    siteList.TryGetValue(r.SiteId, out var siteName);
                    r.SiteName = siteName;
                }

                apiResult.Data = result;
                apiResult.ErrorCode = 0;
                apiResult.PageNumber = PageNumber;
                apiResult.RowCount = RowCount;
                apiResult.TotalPages = TotalPages;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = 999;
            }

            return apiResult;
        }

        /// <summary>
        /// Message/GetAnnouncementById 取得公告
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Announcement > Submit(Query) > [Edit]
        /// 
        /// </remarks>
        /// <param name="Id"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiResult<Announcement> GetAnnouncementById([FromForm] int Id)
        {
            var apiResult = new ApiResult<Announcement>();

            try
            {
                apiResult.Data = messageBLL.GetAnnouncementById(Id);
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
        /// Message/AddAnnouncement 新增公告
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Announcement > [Add Announcement]
        /// 
        /// </remarks>
        /// <param name="Datas"></param>
        /// <param name="_StartTime"></param>
        /// <param name="_EndTime"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiResult AddAnnouncement([FromForm] AddAnnouncementModel Datas, [FromForm] long _StartTime,
            [FromForm] long _EndTime)
        {
            var apiResult = new ApiResult<dynamic>();

            try
            {
                var authData = authorizeBLL.GetAuthData();
                if (_StartTime > 2147483647)
                {
                    _StartTime = 2147483647;
                }

                if (_EndTime > 2147483647)
                {
                    _EndTime = 2147483647;
                }

                var startTime = DateTimeExtensions.UnixToDateTime(_StartTime);
                var endTime = DateTimeExtensions.UnixToDateTime(_EndTime);
                var result = messageBLL.AddAnnouncement(Datas, authData.UserName, authData.UserId, startTime, endTime);
                apiResult.Data = new { result.CreateTime, result.Creator, result.Id };
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
        /// Message/UpdateAnnouncement 更新公告
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Announcement > Submit(Query) > Edit > [Submit]
        /// 
        /// </remarks>
        /// <param name="Datas"></param>
        /// <param name="_StartTime"></param>
        /// <param name="_EndTime"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiResult UpdateAnnouncement([FromForm] UpdateAnnouncementModel Datas, [FromForm] long _StartTime,
            [FromForm] long _EndTime)
        {
            var apiResult = new ApiResult<dynamic>();

            try
            {
                var authData = authorizeBLL.GetAuthData();
                if (_StartTime > 2147483647)
                {
                    _StartTime = 2147483647;
                }

                if (_EndTime > 2147483647)
                {
                    _EndTime = 2147483647;
                }

                var startTime = DateTimeExtensions.UnixToDateTime(_StartTime);
                var endTime = DateTimeExtensions.UnixToDateTime(_EndTime);
                var result =
                    messageBLL.UpdateAnnouncement(Datas, authData.UserName, authData.UserId, startTime, endTime);
                apiResult.Data = new { result.LastUpdater, result.LastUpdateTime };
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
        /// Message/DeleteAnnouncement 刪除公告
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Announcement > Submit(Query) > [Delete]
        /// 
        /// </remarks>
        /// <param name="Id"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiResult DeleteAnnouncement([FromForm] int Id)
        {
            var apiResult = new ApiResult<Announcement>();

            try
            {
                messageBLL.DeleteAnnouncement(Id);
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

        #region CustInfo > Personal Message

        /// <summary>
        /// Message/GetPersonalMsg 取得指定客戶個人訊息資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Personal Message > Get Personal Message
        /// 
        /// </remarks>
        /// <param name="CustId"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute),
            Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
        [HttpPost]
        public ApiListResult<PersonalMsg> GetPersonalMsg([FromForm] int CustId, [FromForm] int PageNumber)
        {
            var PageSize = 10;
            var TotalPages = 0;
            var RowCount = 0;
            var apiResult = new ApiListResult<PersonalMsg>();
            try
            {
                var data = messageBLL.GetPersonalMsg(CustId, PageNumber, PageSize, ref TotalPages, ref RowCount);

                data.ToList().ForEach(s => s.CreateTime = new DateTimeOffset(s.CreateTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));
                apiResult.Data = data;
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
        /// Message/GetCustLang 取得指定客戶語系
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo  > Personal Message > Get Customer Lang
        /// 
        /// </remarks>
        /// <param name="CustId"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute),
            Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
        [HttpPost]
        public ApiResult<CustLang> GetCustLang([FromForm] int CustId)
        {
            var apiResult = new ApiResult<CustLang>();
            try
            {
                apiResult.Data = messageBLL.GetCustLang(CustId);
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
        /// Message/SendPersonalMsg 送出指定客戶個人訊息
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo  > Personal Message >  Click [Add] > Click [Send]
        /// 
        /// </remarks>
        /// <param name="CustId"></param>
        /// <param name="Title"></param>
        /// <param name="Content"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute),
            Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
        [HttpPost]
        public ApiListResult<PersonalMsg> SendPersonalMsg([FromForm] int CustId, [FromForm] string Title,
            [FromForm] string Content, [FromForm] int PageNumber)
        {
            var PageSize = 10;
            var TotalPages = 0;
            var RowCount = 0;
            var apiResult = new ApiListResult<PersonalMsg>();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                messageBLL.SendMessageToPlayer(authData.UserName, Title, Content, CustId, authData.UserId);
                apiResult.Data =
                    messageBLL.GetPersonalMsg(CustId, PageNumber, PageSize, ref TotalPages, ref RowCount);
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
        /// Message/DeletePersonalMsg 刪除指定客戶個人訊息
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo  > Personal Message > Click [Delete]
        /// 
        /// </remarks>
        /// <param name="Id"></param>
        /// <param name="CustId"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute),
            Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
        [HttpPost]
        public ApiListResult<PersonalMsg> DeletePersonalMsg([FromForm] int Id, [FromForm] int CustId,
            [FromForm] int PageNumber)
        {
            var PageSize = 10;
            var TotalPages = 0;
            var RowCount = 0;
            var apiResult = new ApiListResult<PersonalMsg>();
            try
            {
                messageBLL.DeletePersonalMsg(Id);
                apiResult.Data = messageBLL.GetPersonalMsg(CustId, PageNumber, PageSize, ref TotalPages, ref RowCount);
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

        #region CSD > Personal Message Batch

        /// <summary>
        /// Message/SendMessageBatch 發送批次訊息
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > Add New > [Submit]
        /// 
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public async Task<ApiResult<int>> SendMessageBatch([FromForm] SendMessageBatchRequest req)
        {
            var authData = authorizeBLL.GetAuthData();
            var batchId = 0;
            var excelList = new List<ExcelData>();
            messageBLL.NewBatchMessage(req.optSite, authData.UserName, req.IsPopup, req.personalMsg, ref batchId);
            _logger.LogInformation($@"MessageService-Read Excel And Send Start At {DateTime.UtcNow}.");

            if (req.excelUpload != null)
            {
                if (_excelBLL.IsValidFile(req.excelUpload.ContentType))
                {
                    var excelPkg = new ExcelPackage(req.excelUpload.OpenReadStream());
                    var sheet = excelPkg.Workbook.Worksheets.First(); //取得Sheet1
                    var isLastRow = false;
                    var rowId = 2; // 因為有標題列，所以從第2列開始讀起

                    do // 讀取資料，直到讀到空白列為止
                    {
                        var custUsername = sheet.Cells[rowId, 1].Text.Trim();
                        var sabaId = sheet.Cells[rowId, 2].Text.Trim();
                        var custId = common.TpIdToCustId(sabaId); 

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
                        var custInfosDetail = await custInfoBLL.GetCustInfoByCust(req.optSite, data.CustId, data.CustUserName);
                        await messageBLL.PreSendBatchMessage(data, custInfosDetail, req.personalMsg, authData, req.optSite, batchId, req.IsPopup);
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
                        custInfoBLL.SendMessageBatch(batchPersonalMsg);
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
        ///  Message/ExportSendMessageBatchResultReport 匯出指定批次訊息發送結果
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Personal Message Batch > Submit(Query) > [Export Excel]
        /// 
        /// </remarks>
        /// <param name="BatchId"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public IActionResult ExportSendMessageBatchResultReport(int BatchId)
        {
            var data = messageBLL.GetBatchMessageSendLog(BatchId);
            var stream = _excelBLL.Export(data);

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
        public ApiListResult<BatchMsg> DeleteBatchMessage([FromForm] IEnumerable<int> SelectSite,
            [FromForm] long StartDate, [FromForm] long EndDate, [FromForm] int batchId, [FromForm] int PageNumber)
        {
            var PageSize = 10;
            var TotalPages = 0;
            var RowCount = 0;
            var apiResult = new ApiListResult<BatchMsg>();
            try
            {
                messageBLL.DeleteBatchMessage(batchId);
                var startTime = DateTimeExtensions.UnixToDateTime(StartDate);
                var endTime = DateTimeExtensions.UnixToDateTime(EndDate);
                apiResult.Data = messageBLL.GetBatchMessageList(SelectSite, startTime, endTime, PageSize,
                    ref PageNumber, ref TotalPages, ref RowCount);
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
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiResult UpdateBatchMessage([FromForm] UpdateBatchMessageRequest req)
        {
            var result = new ApiResult();
            try
            {
                if (messageBLL.UpdateBatchMessage(req.batchId, req.IsPopup, req.personalMsg))
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
                result.Data = messageBLL.GetBatchMessage(batchId);
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
        public ApiListResult<BatchMsg> GetBatchMessageList([FromForm] IEnumerable<int> SelectSite,
            [FromForm] long StartDate, [FromForm] long EndDate, [FromForm] int PageNumber)
        {
            var PageSize = 10;
            var TotalPages = 0;
            var RowCount = 0;
            var apiResult = new ApiListResult<BatchMsg>();
            try
            {
                var startTime = DateTimeExtensions.UnixToDateTime(StartDate);
                var endTime = DateTimeExtensions.UnixToDateTime(EndDate);
                apiResult.Data = messageBLL.GetBatchMessageList(SelectSite, startTime, endTime, PageSize,
                    ref PageNumber, ref TotalPages, ref RowCount);

                foreach (var data in apiResult.Data)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                }

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

        #region CSD > Change Vip Level Batch

        /// <summary>
        /// Message/ChangeVipLevelBatch 批次更新會員VIP等級
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Change Vip Level Batch > [Submit]
        /// 
        /// </remarks>
        /// <param name="optSite"></param>
        /// <param name="excelUpload"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/ChangeVipLevelBatch" })]
        [HttpPost]
        public async Task<List<ApiResult>> ChangeVipLevelBatch([FromForm] int optSite, [FromForm] IFormFile excelUpload)
        {
            var apiResult = new List<ApiResult>();
            var authData = authorizeBLL.GetAuthData();
            try
            {
                if (excelUpload.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        StringComparison.OrdinalIgnoreCase) ||
                    excelUpload.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) ||
                    excelUpload.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                {
                    var excelPkg = new ExcelPackage(excelUpload.OpenReadStream());
                    var sheet = excelPkg.Workbook.Worksheets.First(); //取得Sheet1
                    var isLastRow = false;
                    var RowId = 2; // 因為有標題列，所以從第2列開始讀起

                    do // 讀取資料，直到讀到空白列為止
                    {
                        var custUsername = string.IsNullOrEmpty(sheet.Cells[RowId, 1].Text)
                            ? null
                            : sheet.Cells[RowId, 1].Text;

                        var sabaId = sheet.Cells[RowId, 2].Text.Trim();
                        var _sabaId = "";
                        if (sabaId.IndexOf("_") > -1)
                        {
                            _sabaId = sabaId.Substring(sabaId.LastIndexOf("_") + 1,
                                sabaId.Length - sabaId.LastIndexOf("_") - 1);
                        }
                        else if (sabaId.IndexOf("x") > -1)
                        {
                            _sabaId = sabaId.Substring(sabaId.LastIndexOf("x") + 1,
                                sabaId.Length - sabaId.LastIndexOf("x") - 1);
                        }

                        var m = Regex.Match(_sabaId, @"\d+");
                        var _custId = int.TryParse(m.Value, out var custId) ? custId : (int?)null;
                        int.TryParse(sheet.Cells[RowId, 3].Text.Trim(), out var vipLevel);
                        var memo = sheet.Cells[RowId, 4].Text.Trim();
                        if (string.IsNullOrEmpty(custUsername) && _custId == null)
                        {
                            isLastRow = true;
                        }
                        else
                        {
                            var custInfoDetail = custInfoBLL.GetCustInfoDetails(_custId, custUsername, optSite);

                            if (custInfoDetail == null)
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = 1002,
                                    Message = $"Not fund. Username: {custUsername}. SabaId: {sabaId}."
                                });
                            }
                            else if (custInfoDetail.Status == 1)
                            {
                                try
                                {
                                    if (messageBLL.ChangeVipLevelBatch(custInfoDetail.SiteId, custInfoDetail.CustId,
                                        custInfoDetail.UserName, vipLevel, memo, authData.UserName) > 0)
                                    {
                                        apiResult.Add(new ApiResult()
                                        {
                                            ErrorCode = 1,
                                            Message = $"Success. Username: {custUsername}. SabaId: {sabaId}. "
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    apiResult.Add(new ApiResult()
                                    {
                                        ErrorCode = 999,
                                        Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {ex}"
                                    });
                                }
                            }
                            else if (custInfoDetail.Status == 2)
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = 1002,
                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Suspend Status."
                                });
                            }
                            else if (custInfoDetail.Status == 4)
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = 1002,
                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Lock Status."
                                });
                            }
                            else
                            {
                                apiResult.Add(new ApiResult()
                                {
                                    ErrorCode = 1002,
                                    Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Closed Status."
                                });
                            }

                            RowId += 1;
                        }
                    } while (!isLastRow);

                    return apiResult;
                }
                else
                {
                    apiResult.Add(new ApiResult() { ErrorCode = -1, Message = "File Type Error" });
                    return apiResult;
                }
            }
            catch (Exception ex)
            {
                apiResult.Add(new ApiResult() { ErrorCode = -1, Message = ex.ToString() });
                return apiResult;
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private async Task PreSendBatchMessage(ExcelData data, string personalMsg, AuthData authData, int optSite, int batchId, bool isPopup)
        {
            string resultMessage;
            var errorCode = 0;
            var dataMessage = $"CustId={data.CustId}. Username={data.CustUserName}. SabaId={data.SabaId}.";
            
            if (data.CustInfoDetail == null)
            {
                resultMessage = $"Not found. {dataMessage}";
            }
            else
            {
                var customerStatus = (CustomerStatusEnum)data.CustInfoDetail.Status;
                switch (customerStatus)
                {
                    case CustomerStatusEnum.Suspend:
                    case CustomerStatusEnum.Closed:
                        resultMessage = $"Fail. {dataMessage} Status={customerStatus.GetDescription()}";
                        break;
                    case CustomerStatusEnum.Running:
                    case CustomerStatusEnum.Lock:
                        try
                        {
                            errorCode = await messageBLL.SendMessageBatch(personalMsg, data.CustInfoDetail.CustId, optSite, authData.UserName, batchId, isPopup);
                            resultMessage = errorCode == 1
                                ? $"Success. {dataMessage} Status: {customerStatus.GetDescription()}."
                                : $"Fail. {dataMessage} Status: {customerStatus.GetDescription()}. Insert done but affect count != 1 ";
                        }
                        catch (Exception ex)
                        {
                            resultMessage = ex.ToString();
                            errorCode = -1;
                        }
                        break;
                    default:
                        resultMessage =
                            $"Fail. {dataMessage} Status=non status";
                        break;
                }
            }

            var status= (errorCode == 1) ? 3 : 4;
            var custId = data.CustInfoDetail?.CustId ?? 0;
            if (!messageBLL.InsertBatchMessageSendLog(batchId, data.RowId, custId, data.CustUserName, data.SabaId, resultMessage, authData.UserName, status))
            {
                _logger.LogError($@"MessageService-InsertBatchMessageSendLog Fail. " +
                                 $@"BatchId: {batchId}. CustId: {custId}. Username: {data.CustUserName}." +
                                 $@" SabaId: {data.SabaId}. Result: {resultMessage}. Status:{status}");
            }
        }

        private void SendBatchMessage(string personalMsg,
            int custId,
            int siteId,
            string updator,
            int batchMessageId,
            string userName,
            string sabaId,
            CustomerStatusEnum custStatus,
            out int errorCode,
            out string resultMessage)
        {

            try
            {
                if (messageBLL.SendMessageBatch(personalMsg, custId, siteId, updator, batchMessageId))
                {
                    resultMessage = $"Success. CustId:{custId}. Username: {userName}. SabaId: {sabaId}. Status: {custStatus.GetDescription()}. ";

                    errorCode = 1;
                }
                else
                {
                    resultMessage = $"Fail. CustId:{custId}. Username: {userName}. SabaId: {sabaId}. Status: {custStatus.GetDescription()}. Insert done but affect count != 1 ";

                    errorCode = 999;
                }
            }
            catch (Exception ex)
            {
                resultMessage = ex.ToString();
                errorCode = -1;
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

            if (!messageBLL.InsertBatchMessageSendLog(batchMessageId,
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
        
        public class MsgContent
        {
            public string Title { get; set; }  
            public string Content { get; set; }
        }
    }
}
