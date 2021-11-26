using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
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

        public class WithdrawalLimitPara
        {
            public int SiteId { set; get; }
        }

        public class InsertWithdrawalLimitPara
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int WithdrawalMin { get; set; }
        }

        public class UpdateWithdrawalLimitPara
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int WithdrawalMin { get; set; }
        }

        public class DeleteWithdrawalLimitPara
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
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
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult<IEnumerable<WithdrawalData>> GetList(WithdrawalLimitPara para)
        {
            var apiResult = new ApiResult<IEnumerable<WithdrawalData>>();
            try
            {
                apiResult.Data = _withdrawalBll.GetWithdrawalLimitData(para.SiteId);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

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
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Create(InsertWithdrawalLimitPara para)
        {
            var result = _withdrawalBll.InsertWithdrawalLimit(para.SiteId, para.CurrencyId, para.WithdrawalMin,
                User.Identity.Name);
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
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Update(UpdateWithdrawalLimitPara para)
        {
            var result = _withdrawalBll.UpdateWithdrawalLimit(para.SiteId, para.CurrencyId, para.WithdrawalMin,
                User.Identity.Name);
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
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Delete(DeleteWithdrawalLimitPara para)
        {
            var result = _withdrawalBll.DeleteWithdrawalLimit(para.SiteId, para.CurrencyId);
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
