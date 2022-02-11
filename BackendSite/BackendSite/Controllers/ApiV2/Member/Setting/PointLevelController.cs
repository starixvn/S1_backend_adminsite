using System;
using System.Collections.Generic;
using BackendSite.Attributes;
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
    public class PointLevelController : ControllerBase
    {
        #region Fields

        private readonly SelLangBLL _selLangBll;
        private readonly MarketingBLL _marketingBll;

        #endregion

        #region Constructor

        public PointLevelController(IServiceProvider provider)
        {
            _marketingBll = provider.GetRequiredService<MarketingBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region APIModel

        public class GetRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        public class DeleteRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int PointLevel { get; set; }
        }

        #endregion

        /// <summary>
        /// PointLevel/GetList 取得績分等級資訊清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Point Level Management > Click [Site Name]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        [HttpPost]
        public ApiResult<IEnumerable<PointLevelInfo>> GetList(GetRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<PointLevelInfo>>();
            var pointLevelInfos = _marketingBll.GetPointLevelInfos(request.SiteId, request.CurrencyId);
            apiResult.Data = pointLevelInfos;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;
        }

        /// <summary>
        /// PointLevel/Create 新增積分等級
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Point Level Management > [Add Point Level]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        [HttpPost]
        public ApiResult Create(PointLevelInfo request)
        {
            var result = _marketingBll.AddPointLevel(request);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        /// <summary>
        /// PointLevel/Update 更新積分等級
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Point Level Management > Click [Site Name] > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        [HttpPost]
        public ApiResult Update(PointLevelInfo request)
        {
            var result = _marketingBll.EditPointLevel(request);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        /// <summary>
        /// PointLevel/Delete 刪除積分等級
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Point Level Management  > Click [Site Name] > [Delete]
        /// 
        /// Memo：只能刪除最後一筆績分等級。
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        [HttpPost]
        public ApiResult Delete(DeleteRequest request)
        {
            var data = new PointLevelInfo()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                PointLevel = request.PointLevel
            };

            var result = _marketingBll.DeletePointLevel(data);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

    }
}