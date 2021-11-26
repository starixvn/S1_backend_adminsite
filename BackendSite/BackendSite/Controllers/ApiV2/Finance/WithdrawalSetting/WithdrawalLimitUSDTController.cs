using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.ApiV2.Finance.WithdrawalSetting
{
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalLimitUSDTController : ControllerBase
    {
        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public WithdrawalLimitUSDTController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region Actions

        /// <summary>
        /// WithdrawalLimitUSDT/GetList 取得USDT提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > USDT Withdrawal Limit Management > Click [SiteName] & [NetWork]
        /// </remarks>
        /// <param name="USDTwithdrawalLimit"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult<IEnumerable<USDTWithdrawalLimitInfoManage>> GetList(USDTWithdrawalLimit USDTwithdrawalLimit)
        {
            var apiResult = new ApiResult<IEnumerable<USDTWithdrawalLimitInfoManage>>();
            var USDTwithdrawalLimitInfoManages =
                _withdrawalBll.GetUSDTWithdrawalLimitInfos(USDTwithdrawalLimit.SiteId, USDTwithdrawalLimit.CurrencyId, Enum.GetName(typeof(NetworkType), USDTwithdrawalLimit.NetworkType));
            apiResult.Data = USDTwithdrawalLimitInfoManages;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        /// <summary>
        /// WithdrawalLimitUSDT/Create 新增USDT提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > USDT Withdrawal Limit Management > Click [SiteName] & [NetWork]  > [Add] > [Save]
        /// </remarks>
        /// <param name="USDTwithdrawalLimitInfoManage"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Create(USDTWithdrawalLimitInfoManage USDTwithdrawalLimitInfoManage)
        {
            var result = _withdrawalBll.AddUSDTWithdrawalLimit(USDTwithdrawalLimitInfoManage, User.Identity.Name);
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
        /// WithdrawalLimitUSDT/Update 更新USDT提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > USDT Withdrawal Limit Management > Click [SiteName] & [NetWork] > [Edit] > [Save]
        /// </remarks>
        /// <param name="USDTwithdrawalLimitInfoManage"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Update(USDTWithdrawalLimitInfoManage USDTwithdrawalLimitInfoManage)
        {
            var result = _withdrawalBll.EditUSDTWithdrawalLimit(USDTwithdrawalLimitInfoManage, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }

            return apiResult;
        }

        /// <summary>
        /// WithdrawalLimitUSDT/Delete 刪除USDT提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > USDT Withdrawal Limit Management > Click [SiteName] & [NetWork] > [Delete]
        /// </remarks>
        /// <param name="USDTwithdrawalLimitInfoManage"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Delete(USDTWithdrawalLimitInfoManage USDTwithdrawalLimitInfoManage)
        {
            var result = _withdrawalBll.DeleteUSDTWithdrawalLimit(USDTwithdrawalLimitInfoManage, User.Identity.Name);
            var apiResult = new ApiResult();
            if (result > 0)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }

            return apiResult;
        }

        #endregion

    }
}
