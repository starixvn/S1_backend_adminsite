using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Marketing.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        #region Fields

        private readonly MessageBLL messageBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly ServerInfoBLL serverInfoBLL;

        #endregion
        
        #region Constructor

        public AnnouncementController(
            MessageBLL messageBLL,
            AuthorizeBLL authorizeBLL,
            ServerInfoBLL serverInfoBLL
         )
        {
            this.messageBLL = messageBLL;
            this.authorizeBLL = authorizeBLL;
            this.serverInfoBLL = serverInfoBLL;
        }

        #endregion
        
        #region API Model
        public class GetAnnouncementReq : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get;set;}
            private DateTimeOffset _endDate { get;set;}
            public IEnumerable<int> SiteId { get; set; }
            public int Status { get; set; }
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
        public class AnnouncementReq
        {
            public int Id { get; set; }
        }
        public class AddAnnouncementReq
        {
            private DateTimeOffset _startDate { get;set;}
            private DateTimeOffset _endDate { get;set;}
            public Announcement Announcement { get; set; }
            public DateTimeOffset _StartTime
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset _EndTime
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        public class UpdateAnnouncementReq
        {
            private DateTimeOffset _startDate { get;set;}
            private DateTimeOffset _endDate { get;set;}
            public Announcement Announcement { get; set; }
            public DateTimeOffset _StartTime
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset _EndTime
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        #endregion
        
        #region CSD > Announcement
        
        /// <summary>
        /// Message/GetAnnouncement 取得公告清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Announcement > [Submit]
        /// </remarks>
        /// <param name="para"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiListResult<Announcement> GetAnnouncement(GetAnnouncementReq para)
        {
            var totalPages = 0;
            var rowCount = 0;
            var apiResult = new ApiListResult<Announcement>();
            try
            {

                var result = messageBLL.GetAnnouncements(para.SiteId, para.StartDate, para.EndDate, para.Status, para.PageNumber, para.PageSize, ref totalPages, ref rowCount);

                apiResult.Data = result;
                apiResult.PageNumber = para.PageNumber;
                apiResult.RowCount = rowCount;
                apiResult.TotalPages = totalPages;
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.SystemException;
            }

            return apiResult;
        }

        /// <summary>
        /// Message/GetAnnouncementById 取得公告
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Announcement > Submit(Query) > [Edit]
        /// </remarks>
        /// <param name="para"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiResult<Announcement> GetAnnouncementById(AnnouncementReq para)
        {
            var apiResult = new ApiResult<Announcement>();

            try
            {
                apiResult.Data = messageBLL.GetAnnouncementById(para.Id);
                apiResult.ErrorCode = (int) ErrorMessageCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int) ErrorMessageCodeEnum.SystemException;
            }

            return apiResult;
        }

        /// <summary>
        /// Message/AddAnnouncement 新增公告
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Announcement > [Add Announcement]
        /// </remarks>
        /// <param name="para"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiResult AddAnnouncement(AddAnnouncementReq para)
        {
            var apiResult = new ApiResult();

            try
            {
                var authData = authorizeBLL.GetAuthData();
                var result = messageBLL.AddAnnouncement(para.Announcement, authData.UserName, authData.UserId, para._StartTime, para._EndTime);
                apiResult.ErrorCode = (int) ErrorMessageCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode =(int) ErrorMessageCodeEnum.SystemException;
            }

            return apiResult;
        }

        /// <summary>
        /// Message/UpdateAnnouncement 更新公告
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Announcement > Submit(Query) > Edit > [Submit]
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiResult UpdateAnnouncement(UpdateAnnouncementReq para)
        {
            var apiResult = new ApiResult();

            try
            {
                var authData = authorizeBLL.GetAuthData();
                var result = messageBLL.UpdateAnnouncement(para.Announcement, authData.UserName, authData.UserId, para._StartTime, para._EndTime);
                apiResult.ErrorCode = (int) ErrorMessageCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int) ErrorMessageCodeEnum.SystemException;
            }

            return apiResult;
        }

        /// <summary>
        /// Message/DeleteAnnouncement 刪除公告
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Announcement > Submit(Query) > [Delete]
        /// </remarks>
        /// <param name="para"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiResult DeleteAnnouncement(AnnouncementReq para)
        {
            var apiResult = new ApiResult();

            try
            {
                messageBLL.DeleteAnnouncement(para.Id);
                apiResult.ErrorCode = (int) ErrorMessageCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int) ErrorMessageCodeEnum.SystemException;
            }

            return apiResult;
        }
        #endregion
    }
}