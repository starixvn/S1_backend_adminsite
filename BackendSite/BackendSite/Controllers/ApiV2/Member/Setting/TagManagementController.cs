using System;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Member.Setting
{ 
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TagManagementController : ControllerBase
    {
        #region Fields
        
        private readonly CustInfoBLL _custInfoBll;

        #endregion

        #region Constructor

        public TagManagementController(IServiceProvider provider)
        {
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
        }

        #endregion

        #region Model
        
        public class GetTagDataReq
        {
            public int Status { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }
        public class EditTagManagementReq
        {
            public int Id { get; set; }
            public string TagName { get; set; }
            public string TagColour { get; set; }
            public string Description { get; set; }
        }
        public class SetTagStatusReq
        {
            public int Id { get; set; }
        }
        
        #endregion

        #region CSD > TagMamagement

        /// <summary>
        /// CustManage/GetTagData 取得標籤資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Tag Management > [Submit](Query)
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TagManagement" })]
        [HttpPost]
        public ApiListResult<TagData> GetTagData(GetTagDataReq req)
        {
            var apiResult = new ApiListResult<TagData>();
            try
            {
                var searchResult = _custInfoBll.GetTagData(req.Status, req.PageNumber, req.PageSize);
                foreach (var tagData in searchResult.TagDatas)
                {
                    tagData.UpdateTime = new DateTimeOffset(tagData.UpdateTime.DateTime, TimeSpan.Zero);
                }
                apiResult.Data = searchResult.TagDatas;
                apiResult.PageNumber = searchResult.PageNumber;
                apiResult.TotalPages = searchResult.TotalPages;
                apiResult.RowCount = searchResult.RowCount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int) ErrorMessageCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// CustManage/EditTagMamagement 更新標籤資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Tag Management > [Submit](Query) > Click[Edit] > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TagManagement" })]
        [HttpPost]
        public ApiResult EditTagManagement(EditTagManagementReq req)
        {
            if (_custInfoBll.EditTagManagement(req.Id, req.TagName, req.TagColour, req.Description, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Edit Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
        }
        
        /// <summary>
        /// CustManage/SetTagStatus 更新標籤狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Tag Management > [Submit](Query) > Change[Status]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TagManagement" })]
        [HttpPost]
        public ApiResult SetTagStatus(SetTagStatusReq req)
        {
            if (_custInfoBll.SetTagStatus(req.Id, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Edit Success."
                };
            }
            throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
        }

        #endregion
    }
}