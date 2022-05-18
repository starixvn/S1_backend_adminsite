using System;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Member.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
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
        
        public class GetTagDataReq : ApiRequestPageModel
        {
            public int Status { get; set; }
        }
        
        #endregion

        #region CSD > TagMamagement

        /// <summary>
        /// TagManagement/GetTagData 取得標籤資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Tag Management > [Submit](Query)
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TagManagement" })]
        [HttpPost]
        public ApiListResult<TagData> GetTagList(GetTagDataReq req)
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
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// TagManagement/EditTagMamagement 更新標籤資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Tag Management > [Submit](Query) > Click[Edit] > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TagManagement" })]
        [HttpPost]
        public ApiResult EditTagManagement(ApiRequestWithLog<EditTagManagement> req)
        {
            if (_custInfoBll.EditTagManagement(new ParamsWithLog<EditTagManagement>(req, User.Identity.Name)))
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
        /// TagManagement/SetTagStatus 更新標籤狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Tag Management > [Submit](Query) > Change[Status]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TagManagement" })]
        [HttpPost]
        public ApiResult SetTagStatus(ApiRequestWithLog<SetTagStatus> req)
        {
            if (_custInfoBll.SetTagStatus(new ParamsWithLog<SetTagStatus>(req, User.Identity.Name)))
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