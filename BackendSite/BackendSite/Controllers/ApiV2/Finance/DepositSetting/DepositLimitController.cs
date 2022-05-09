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
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Finance.DepositSetting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositLimitController : ControllerBase
    {
        #region Fields

        private readonly DepositBLL _depositBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public DepositLimitController(IServiceProvider provider)
        {
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<DepositTypeEnum> DepositTypes { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
        }

        #endregion

        #region Finance > Deposit Limit Management 

        // <summary>
        // DepositLimit/GetDepositLimitCurrencyList
        // </summary>
        // <remarks>
        // Http Method：POST
        // 
        // Func Path：Finance > Deposit Limit Management > [Submit]
        // </remarks>
        // <param name="req"></param>
        // 判斷流程，應可以被Permission/GetUserSiteList取代，Currency可以透過GetAllData/GetCurrency取得，先暫時mark
        //[HttpPost]
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        //public ApiResult<IEnumerable<DepositLimitCurrencyInfo>> GetDepositLimitCurrencyList(DepositLimit req)
        //{
        //    var apiResult = new ApiResult<IEnumerable<DepositLimitCurrencyInfo>>();
        //    var depositLimitCurrencyList = _depositBll.GetDepositLimitCurrencyList(req.SiteId, req.DepositType);
        //    apiResult.Data = depositLimitCurrencyList;
        //    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
        //    return apiResult;
        //}

        /// <summary>
        /// DepositLimit/GetList 取得存款限額資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > Clikc[Site Name] [Deposit Type]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        [HttpPost]
        public ApiListResult<DepositLimitInfo> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<DepositLimitInfo>();
            var depositTypeList = request.DepositTypes.Select(x => (int)x);

            var depositLimitInfos = _depositBll.GetDepositLimitInfos(request.SiteIds,
                depositTypeList,
                request.CurrencyIds,
                request.PageNumber,
                request.PageSize,
                out int totalPages,
                out int rowCount);
            apiResult.Data = depositLimitInfos;
            apiResult.PageNumber = request.PageNumber;
            apiResult.TotalPages = totalPages;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;
        }

        /// <summary>
        /// DepositLimit/Create 新增存款限額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > [Submit] > Add
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        [HttpPost]
        public ApiResult Create(ApiRequestWithLog<DepositLimitModel> req)
        {
            var result =
                _depositBll.AddDepositLimit(new ParamsWithLog<DepositLimitModel>(req, User.Identity.Name));
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        /// <summary>
        /// DepositLimit/Update 更新存款限額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > Clikc[Site Name] [Deposit Type] > [Edit] > [Save]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        [HttpPost]
        public ApiResult Update(ApiRequestWithLog<DepositLimitModel> req)
        {
            var result =
                _depositBll.EditDepositLimit(new ParamsWithLog<DepositLimitModel>(req, User.Identity.Name));
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        /// <summary>
        /// DepositLimit/Delete 刪除存款限額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > Clikc[Site Name] [Deposit Type] > [Delete]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        [HttpPost]
        public ApiResult Delete(ApiRequestWithLog<DepositLimitModel> req)
        {
            var result =
                _depositBll.DeleteDepositLimit(new ParamsWithLog<DepositLimitModel>(req, User.Identity.Name));
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        #endregion
    }
}