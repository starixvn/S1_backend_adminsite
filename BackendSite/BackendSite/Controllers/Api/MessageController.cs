using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Attributes;
using Ubiety.Dns.Core.Records.NotUsed;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly MessageBLL messageBLL;
        private readonly AuthorizeBLL authorizeBLL;

        public MessageController(MessageBLL messageBLL, AuthorizeBLL authorizeBLL)
        {
            this.messageBLL = messageBLL;
            this.authorizeBLL = authorizeBLL;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiListResult<Announcement> GetAnnouncement([FromForm]int PageNumber)
        {
            int PageSize = 10;
            int TotalPages = 0;
            int RowCount = 0;
            var apiResult = new ApiListResult<Announcement>();
            try
            {
                apiResult.Data = messageBLL.GetAnnouncements(PageSize, ref PageNumber, ref TotalPages, ref RowCount);
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
        public ApiResult<Announcement> GetAnnouncementById([FromForm]int Id)
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
        public ApiResult AddAnnouncement([FromForm]Announcement Datas, [FromForm]long _StartTime, [FromForm]long _EndTime)
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
        public ApiResult UpdateAnnouncement([FromForm]Announcement Datas, [FromForm]long _StartTime, [FromForm]long _EndTime)
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
        public ApiResult DeleteAnnouncement([FromForm]int Id)
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
        public ApiListResult<PersonalMsg> GetPersonalMsg([FromForm]int CustId, [FromForm]int PageNumber)
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
        public ApiResult<CustLang> GetCustLang([FromForm]int CustId)
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
        public ApiListResult<PersonalMsg> SendPersonalMsg([FromForm]int CustId, [FromForm]string Title, [FromForm]string Content, [FromForm]int PageNumber)
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
        public ApiListResult<PersonalMsg> DeletePersonalMsg([FromForm]int Id, [FromForm]int CustId, [FromForm]int PageNumber)
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
        public ApiResult SendMessageBatch([FromForm]IDictionary<string, PersonalMsgBase> personalMsg, [FromForm]IEnumerable<string> custList)
        {
            var result = new ApiResult<int>();

            try
            {
                var authData = authorizeBLL.GetAuthData();
                result.Data = messageBLL.SendMessageBatch(personalMsg, custList, authData.UserName);
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }
    }
}