using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly MessageBLL messageBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly CustInfoBLL custInfoBLL;
        private readonly Common common;

        public MessageController(MessageBLL messageBLL, AuthorizeBLL authorizeBLL, ServerInfoBLL serverInfoBLL, CustInfoBLL custInfoBLL, Common common)
        {
            this.messageBLL = messageBLL;
            this.authorizeBLL = authorizeBLL;
            this.serverInfoBLL = serverInfoBLL;
            this.custInfoBLL = custInfoBLL;
            this.common = common;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiListResult<Announcement> GetAnnouncement([FromForm] int PageNumber)
        {
            int PageSize = 10;
            int TotalPages = 0;
            int RowCount = 0;
            var apiResult = new ApiListResult<Announcement>();
            try
            {
                Dictionary<int, string> siteList = serverInfoBLL.GetSiteList().ToDictionary(x => x.SiteId, y => y.SiteName);
                if (!siteList.ContainsKey(0))
                {
                    siteList.Add(0, "All");
                }

                var result = messageBLL.GetAnnouncements(PageSize, ref PageNumber, ref TotalPages, ref RowCount);
                foreach (var r in result)
                {
                    siteList.TryGetValue(r.SiteId, out string siteName);
                    r.SiteName = siteName;
                }
                apiResult.Data = result;
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiResult AddAnnouncement([FromForm] Announcement Datas, [FromForm] long _StartTime, [FromForm] long _EndTime)
        {
            var apiResult = new ApiResult<dynamic>();

            try
            {
                var authData = authorizeBLL.GetAuthData();
                var result = messageBLL.AddAnnouncement(Datas, authData.UserName, authData.UserId, _StartTime, _EndTime);
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiResult UpdateAnnouncement([FromForm] Announcement Datas, [FromForm] long _StartTime, [FromForm] long _EndTime)
        {
            var apiResult = new ApiResult<dynamic>();

            try
            {
                var authData = authorizeBLL.GetAuthData();
                var result = messageBLL.UpdateAnnouncement(Datas, authData.UserName, authData.UserId, _StartTime, _EndTime);
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
        [HttpPost]
        public ApiListResult<PersonalMsg> GetPersonalMsg([FromForm] int CustId, [FromForm] int PageNumber)
        {
            int PageSize = 10;
            int TotalPages = 0;
            int RowCount = 0;
            var apiResult = new ApiListResult<PersonalMsg>();
            try
            {
                apiResult.Data = messageBLL.GetPersonalMsg(CustId, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
        [HttpPost]
        public ApiListResult<PersonalMsg> SendPersonalMsg([FromForm] int CustId, [FromForm] string Title, [FromForm] string Content, [FromForm] int PageNumber)
        {
            int PageSize = 10;
            int TotalPages = 0;
            int RowCount = 0;
            var apiResult = new ApiListResult<PersonalMsg>();
            try
            {
                var authData = authorizeBLL.GetAuthData();
                messageBLL.SendMessageToPlayer(authData.UserName, Title, Content, CustId, authData.UserId);
                apiResult.Data = messageBLL.GetPersonalMsg(CustId, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
        [HttpPost]
        public ApiListResult<PersonalMsg> DeletePersonalMsg([FromForm] int Id, [FromForm] int CustId, [FromForm] int PageNumber)
        {
            int PageSize = 10;
            int TotalPages = 0;
            int RowCount = 0;
            var apiResult = new ApiListResult<PersonalMsg>();
            try
            {
                messageBLL.DeletePersonalMsg(Id, CustId);
                apiResult.Data = messageBLL.GetPersonalMsg(CustId, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public List<ApiResult> SendMessageBatch([FromForm] int optSite, [FromForm] IFormFile excelUpload, [FromForm] string personalMsg)
        {
            var apiResult = new List<ApiResult>();
            var authData = authorizeBLL.GetAuthData();
            try
            {
                if (excelUpload.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase) || excelUpload.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) || excelUpload.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                {
                    ExcelPackage excelPkg = new ExcelPackage(excelUpload.OpenReadStream());
                    ExcelWorksheet sheet = excelPkg.Workbook.Worksheets.First();//取得Sheet1
                    bool isLastRow = false;
                    int RowId = 2;   // 因為有標題列，所以從第2列開始讀起
                    int BatchId = 0;
                    messageBLL.NewBatchMessage(optSite, authData.UserName, personalMsg, ref BatchId);
                    do  // 讀取資料，直到讀到空白列為止
                    {
                        string custUsername = string.IsNullOrEmpty(sheet.Cells[RowId, 1].Text) ? null : sheet.Cells[RowId, 1].Text;
                        string sabaId = sheet.Cells[RowId, 2].Text.Trim();
                        var custId = (string.IsNullOrEmpty(sabaId)) ? null : common.TpIdToCustId(sabaId);

                        if (string.IsNullOrEmpty(custUsername) && custId == null)
                        {
                            isLastRow = true;
                        }
                        else
                        {
                            var custInfosDetail = custInfoBLL.GetCustInfoDetails(custId, custUsername, optSite);
                            if (custInfosDetail == null)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Not fund. Username: {custUsername}. SabaId: {sabaId}." });
                            }
                            else if (custInfosDetail.Status == 1)
                            {
                                if (messageBLL.SendMessageBatch(personalMsg, custInfosDetail.CustId, optSite, authData.UserName, BatchId) > 0)
                                {
                                    apiResult.Add(new ApiResult() { ErrorCode = 1, Message = $"Success. Username: {custUsername}. SabaId: {sabaId}. " });
                                }
                                else
                                {
                                    apiResult.Add(new ApiResult() { ErrorCode = 999, Message = $"Success. Username: {custUsername}. SabaId: {sabaId}. " });
                                }
                            }
                            else if (custInfosDetail.Status == 2)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Suspend Status." });
                            }
                            else
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Closed Status." });
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiListResult<BatchMsg> DeleteBatchMessage([FromForm] IEnumerable<int> SelectSite, [FromForm] long? StartDate, [FromForm] long? EndDate, [FromForm] int batchId, [FromForm] int PageNumber)
        {
            int PageSize = 10;
            int TotalPages = 0;
            int RowCount = 0;
            var apiResult = new ApiListResult<BatchMsg>();
            try
            {
                messageBLL.DeleteBatchMessage(batchId);
                apiResult.Data = messageBLL.GetBatchMessageList(SelectSite, StartDate, EndDate, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiResult UpdateBatchMessage([FromForm] int batchId, [FromForm] string personalMsg)
        {
            var result = new ApiResult();
            try
            {
                if (messageBLL.UpdateBatchMessage(batchId, personalMsg))
                    result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.RawMessage = ex.ToString();
                result.ErrorCode = 999;
            }

            return result;
        }
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/PersonalMsgBatch" })]
        [HttpPost]
        public ApiListResult<BatchMsg> GetBatchMessageList([FromForm] IEnumerable<int> SelectSite, [FromForm] long? StartDate, [FromForm] long? EndDate, [FromForm] int PageNumber)
        {
            int PageSize = 10;
            int TotalPages = 0;
            int RowCount = 0;
            var apiResult = new ApiListResult<BatchMsg>();
            try
            {
                apiResult.Data = messageBLL.GetBatchMessageList(SelectSite, StartDate, EndDate, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/ChangeVipLevelBatch" })]
        public async Task<List<ApiResult>> ChangeVipLevelBatch([FromForm] int optSite, [FromForm] IFormFile excelUpload)
        {
            var apiResult = new List<ApiResult>();
            var authData = authorizeBLL.GetAuthData();
            try
            {
                if (excelUpload.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase) || excelUpload.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) || excelUpload.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                {
                    ExcelPackage excelPkg = new ExcelPackage(excelUpload.OpenReadStream());
                    ExcelWorksheet sheet = excelPkg.Workbook.Worksheets.First();//取得Sheet1
                    bool isLastRow = false;
                    int RowId = 2;   // 因為有標題列，所以從第2列開始讀起

                    do  // 讀取資料，直到讀到空白列為止
                    {
                        string custUsername = string.IsNullOrEmpty(sheet.Cells[RowId, 1].Text) ? null : sheet.Cells[RowId, 1].Text;

                        int custId;
                        string sabaId = sheet.Cells[RowId, 2].Text.Trim();
                        string _sabaId = "";
                        if (sabaId.IndexOf("_") > -1)
                        {
                            _sabaId = sabaId.Substring(sabaId.LastIndexOf("_") + 1, sabaId.Length - sabaId.LastIndexOf("_") - 1);
                        }
                        else if (sabaId.IndexOf("x") > -1)
                        {
                            _sabaId = sabaId.Substring(sabaId.LastIndexOf("x") + 1, sabaId.Length - sabaId.LastIndexOf("x") - 1);
                        }

                        Match m = Regex.Match(_sabaId, @"\d+");
                        int? _custId = int.TryParse(m.Value, out custId) ? custId : (int?)null;
                        int.TryParse(sheet.Cells[RowId, 3].Text.Trim(), out int vipLevel);
                        string memo = sheet.Cells[RowId, 4].Text.Trim();
                        if (string.IsNullOrEmpty(custUsername) && _custId == null)
                        {
                            isLastRow = true;
                        }
                        else
                        {
                            var custInfoDetail = custInfoBLL.GetCustInfoDetails(_custId, custUsername, optSite);

                            if (custInfoDetail == null)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Not fund. Username: {custUsername}. SabaId: {sabaId}." });
                            }
                            else if (custInfoDetail.Status == 1)
                            {
                                try
                                {
                                    if (messageBLL.ChangeVipLevelBatch(custInfoDetail.SiteId, custInfoDetail.CustId, custInfoDetail.UserName, vipLevel, memo, authData.UserName) > 0)
                                    {
                                        apiResult.Add(new ApiResult() { ErrorCode = 1, Message = $"Success. Username: {custUsername}. SabaId: {sabaId}. " });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    apiResult.Add(new ApiResult() { ErrorCode = 999, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. {ex}" });
                                }
                            }
                            else if (custInfoDetail.Status == 2)
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Suspend Status." });
                            }
                            else
                            {
                                apiResult.Add(new ApiResult() { ErrorCode = 1002, Message = $"Fail. Username: {custUsername}. SabaId: {sabaId}. Closed Status." });
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
    }
}