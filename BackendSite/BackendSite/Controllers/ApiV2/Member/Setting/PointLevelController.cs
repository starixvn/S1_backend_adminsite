using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Member.Setting
{
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
        
        public class PointLevelModel
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        #endregion
        
        /// <summary>
        /// PointLevel/Get 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Point Level Management 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        public ApiResult<IEnumerable<PointLevelInfo>> Get(PointLevelModel req)
        {
            var apiResult = new ApiResult<IEnumerable<PointLevelInfo>>();
            var pointLevelInfos = _marketingBll.GetPointLevelInfos(req.SiteId, req.CurrencyId);
            apiResult.Data = pointLevelInfos;
            apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            return apiResult;
        }
        
        /// <summary>
        /// PointLevel/Add 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Point Level Management > [Add]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        public ApiResult Add(PointLevelInfo req)
        {
            var result = _marketingBll.AddPointLevel(req);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
        
        /// <summary>
        /// PointLevel/Edit 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Point Level Management > [Edit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        public ApiResult Edit(PointLevelInfo req)
        {
            var result = _marketingBll.EditPointLevel(req);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        /// <summary>
        /// PointLevel/Delete 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Point Level Management > [Delete]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/PointLevelManagement" })]
        [HttpPost]
        public ApiResult Delete(PointLevelInfo req)
        {
            var result = _marketingBll.DeletePointLevel(req);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
        
    }
}