using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.ApiV2.Finance.WithdrawalSetting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalLimitController : ControllerBase
    {
        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public WithdrawalLimitController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model
        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }

            public IEnumerable<int> CurrencyIds { get; set; }
        }
        
        #endregion

        #region Action

        /// <summary>
        /// WithdrawalLimit/GetList 取得提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Management > Click [SiteName]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        [HttpPost]
        public ApiListResult<WithdrawalLimitInfoManage> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<WithdrawalLimitInfoManage>();

            apiResult.Data = _withdrawalBll.GetWithdrawalLimitInfos(request.SiteIds,
                request.CurrencyIds,
                request.PageNumber,
                request.PageSize,
                out int totalPages,
                out int rowCount);

            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            apiResult.PageNumber = request.PageNumber;
            apiResult.TotalPages = totalPages;
            apiResult.RowCount = rowCount;

            return apiResult;
        }

        /// <summary>
        /// WithdrawalLimit/Create 新增提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Management > Click [SiteName] > [Add] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Create(ApiRequestWithLog<SetWithdrawalLimitInfo> request)
        {
            var result = _withdrawalBll.AddWithdrawalLimit(
                new ParamsWithLog<SetWithdrawalLimitInfo>(request, User.Identity.Name));
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
        /// WithdrawalLimit/Update 更新提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Management > Click [SiteName] > [Edit] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Update(ApiRequestWithLog<SetWithdrawalLimitInfo> request)
        {
            var result = _withdrawalBll.EditWithdrawalLimit(
                new ParamsWithLog<SetWithdrawalLimitInfo>(request, User.Identity.Name));
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
        /// WithdrawalLimit/Delete 刪除提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Limit Management > Click [SiteName] > [Delete]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Delete(ApiRequestWithLog<SetWithdrawalLimitInfo> request)
        {
            var result = _withdrawalBll.DeleteWithdrawalLimit(
                new ParamsWithLog<SetWithdrawalLimitInfo>(request, User.Identity.Name));
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

        #endregion
    }
}
