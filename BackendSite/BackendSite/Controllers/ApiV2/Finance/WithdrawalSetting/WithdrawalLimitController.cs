using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        public class GetListRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        public class InsertOrUpdateRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int VipLevel { get; set; }
            public int WithdrawalMin { get; set; }
            public int WithdrawalMax { get; set; }
        }


        public class DeleteRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int VipLevel { get; set; }
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
        public ApiResult<IEnumerable<WithdrawalLimitInfoManage>> GetList(GetListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<WithdrawalLimitInfoManage>>();
            try
            {
                apiResult.Data = _withdrawalBll.GetWithdrawalLimitInfos(request.SiteId, request.CurrencyId);
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
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Create(InsertOrUpdateRequest request)
        {
            var data = new WithdrawalLimitInfoManage()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                VipLevel = request.VipLevel,
                WithdrawalMax = request.WithdrawalMax,
                WithdrawalMin = request.WithdrawalMin,
            };

            var result = _withdrawalBll.AddWithdrawalLimit(data,
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
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Update(InsertOrUpdateRequest request)
        {
            var data = new WithdrawalLimitInfoManage()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                VipLevel = request.VipLevel,
                WithdrawalMax = request.WithdrawalMax,
                WithdrawalMin = request.WithdrawalMin,
            };

            var result = _withdrawalBll.EditWithdrawalLimit(data,
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
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Delete(DeleteRequest request)
        {

            var data = new WithdrawalLimitInfoManage()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                VipLevel = request.VipLevel
            };

            var result = _withdrawalBll.DeleteWithdrawalLimit(data, User.Identity.Name);
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

        #endregion
    }
}
