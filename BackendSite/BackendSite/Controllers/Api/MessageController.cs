using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Attributes;

namespace BackendSite.Controllers.Api
{
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
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

        [HttpPost]
        public ApiResult AddAnnouncement(Announcement Datas)
        {
            var apiResult = new ApiResult<dynamic>();

            try
            {
                var authData = authorizeBLL.GetAuthData();
                var result = messageBLL.AddAnnouncement(Datas, authData.UserName, authData.UserId);
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

        [HttpPost]
        public ApiResult UpdateAnnouncement(Announcement Datas)
        {
            var apiResult = new ApiResult<dynamic>();

            try
            {
                var authData = authorizeBLL.GetAuthData();
                var result = messageBLL.UpdateAnnouncement(Datas, authData.UserName, authData.UserId);
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
    }
}