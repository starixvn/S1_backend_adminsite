using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Finance.WithdrawalSetting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalKYCController : ControllerBase
    {
        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public WithdrawalKYCController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region Model

        public class WithdrawalKYCListReq
        {
            public int SiteId { get; set; }
        }

        #endregion
        
        #region CSD > Withdrawal KYC Control

        /// <summary>
        /// Withdrawal/WithdrawalKYCList 取得提款KYC控制資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Withdrawal KYC Control > Change [Site]
        /// </remarks>
        /// <param name="req">參數</param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] {"Message/WithdrawalKYCControl"})]
        [HttpPost]
        public ApiResult<IEnumerable<WithdrawalKYC>> WithdrawalKYCList(WithdrawalKYCListReq req)
        {
            var result = _withdrawalBll.GetWithdrawalKYCList(req.SiteId);
            var apiResult = new ApiResult<IEnumerable<WithdrawalKYC>>
            {
                Data = result,
                ErrorCode = (int)ErrorMessageCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// Withdrawal/EditWithdrawalKYCList 更新提款KYC資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Withdrawal KYC Control > Edit > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] {"Message/WithdrawalKYCControl"})]
        [HttpPost]
        public ApiResult EditWithdrawalKYCList(WithdrawalKYC req)
        {
            var result = _withdrawalBll.EditWithdrawalKYCList(req);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Fail;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }

            return apiResult;
        }

        #endregion
    }
}