using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackendSite.Controllers.ApiV2.Marketing.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        #region Fields

        private readonly MessageBLL _messageBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly ILogger<AnnouncementController> _logger;

        #endregion

        #region Constructor

        public AnnouncementController(IServiceProvider provider)
        {
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _logger = provider.GetRequiredService<ILogger<AnnouncementController>>();
        }

        #endregion

        #region API Model
        public class GetAnnouncementReq : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
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
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public AddAnnouncementModel Announcement { get; set; }
            public DateTimeOffset StartTime
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndTime
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        
        public class UpdateAnnouncementReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public UpdateAnnouncementModel Announcement { get; set; }
            public DateTimeOffset StartTime
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndTime
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        #endregion

        #region CSD > Announcement

        /// <summary>
        /// Announcement/GetAnnouncement 取得公告清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Announcement > [Submit]
        /// </remarks>
        /// <param name="para"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        [HttpPost]
        public ApiListResult<Announcement> GetAnnouncementList(GetAnnouncementReq para)
        {
            var totalPages = 0;
            var rowCount = 0;
            var apiResult = new ApiListResult<Announcement>();
            try
            {
                var result = _messageBll.GetAnnouncements(para.SiteIds, para.StartDate, para.EndDate, para.Status, para.PageNumber, para.PageSize, ref totalPages, ref rowCount);
                apiResult.Data = result;
                apiResult.PageNumber = para.PageNumber;
                apiResult.RowCount = rowCount;
                apiResult.TotalPages = totalPages;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }

        /// <summary>
        /// Announcement/GetAnnouncementById 取得公告
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
                apiResult.Data = _messageBll.GetAnnouncementById(para.Id);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }

        /// <summary>
        /// Announcement/AddAnnouncement 新增公告
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
                var authData = _authorizeBll.GetAuthData();
                var result = _messageBll.AddAnnouncement(para.Announcement, authData.UserName, authData.UserId, para.StartTime, para.EndTime);
                if (result != null)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Create Fail.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }

        /// <summary>
        /// Announcement/UpdateAnnouncement 更新公告
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
                var authData = _authorizeBll.GetAuthData();
                var result = _messageBll.UpdateAnnouncement(para.Announcement, authData.UserName, authData.UserId, para.StartTime, para.EndTime);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }

        /// <summary>
        /// Announcement/DeleteAnnouncement 刪除公告
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
                var result = _messageBll.DeleteAnnouncement(para.Id, User.Identity.Name);
                if (result > 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Delete Fail";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }
        #endregion
    }
}