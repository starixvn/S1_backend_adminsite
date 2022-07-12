using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.ApiV2.Finance.WithdrawalSetting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalLimitEWalletController : ControllerBase
    {
        #region Fields
        private readonly WithdrawalBLL _withdrawalBll;
        private readonly SelLangBLL _selLangBll;
        #endregion

        #region Constructors

        public WithdrawalLimitEWalletController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model
        public class GetListRequest
        {
            public int SiteId { get; set; }
        }
        public class UpdateRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int WithdrawalMin { get; set; }
            public int WithdrawalMax { get; set; }
            public int Status { get; set; }
        }
        #endregion

        #region Action
        /// <summary>
        /// WithdrawalLimitEWallet/GetList 取得E-Wallet提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<EWalletWithdrawalLimitInfoManage>> GetList(GetListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<EWalletWithdrawalLimitInfoManage>>();

            apiResult.Data = _withdrawalBll.GetEWalletWithdrawalLimitInfo(request.SiteId);

            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// WithdrawalLimitEWallet/Update 更新E-Wallet提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult Update(UpdateRequest request)
        {
            var data = new EWalletWithdrawalLimitInfoManage()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                WithdrawalMax = request.WithdrawalMax,
                WithdrawalMin = request.WithdrawalMin,
            };

            var result = _withdrawalBll.EditEWalletWithdrawalLimit(data,
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
        /// WithdrawalLimitEWallet/SwitchStatus 更新E-Wallet提款限額狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult SwitchStatus(UpdateRequest request)
        {
            var data = new EWalletWithdrawalLimitInfoManage()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                Status = request.Status
            };

            var result = _withdrawalBll.UpdateEWalletWithdrawalLimitInfoStatus(data,
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
        #endregion
    }
}
