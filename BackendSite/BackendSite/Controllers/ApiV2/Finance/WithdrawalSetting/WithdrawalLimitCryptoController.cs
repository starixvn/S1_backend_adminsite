using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Finance.WithdrawalSetting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalLimitCryptoController : ControllerBase
    {
        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public WithdrawalLimitCryptoController(IServiceProvider provider)
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
            public NetworkTypeEnum NetworkType { get; set; }
            public CryptoCurrencyEnum CryptoCurrency { get; set; }
        }

        #endregion

        #region Actions

        /// <summary>
        /// WithdrawalLimitCrypto/GetList 取得加密貨幣提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Crypto Withdrawal Limit Management > Select [SiteName] & [CryptoCurrency] & [NetWork]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiListResult<CryptoWithdrawalLimitInfo> GetList(GetListRequest request)
        {
            var result = _withdrawalBll.GetCryptoWithdrawalLimitInfos(request.SiteIds,
                request.CurrencyIds,
                (int)request.CryptoCurrency,
                Enum.GetName(typeof(NetworkTypeEnum), request.NetworkType),
                request.PageNumber,
                request.PageSize,
                out int totalPages,
                out int rowCount);

            var apiResult = new ApiListResult<CryptoWithdrawalLimitInfo>
            {
                Data = result,
                PageNumber = request.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount,
                ErrorCode = 0
            };

            return apiResult;
        }

        /// <summary>
        /// WithdrawalLimitCrypto/Create 新增加密貨幣提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Crypto Withdrawal Limit Management > Select [SiteName] & [CryptoCurrency] & [NetWork]  > [Add] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Create(ApiRequestWithLog<CreateCryptoWithdrawalLimitInfo> request)
        {
            var success = _withdrawalBll.AddCryptoWithdrawalLimit(
                new ParamsWithLog<CreateCryptoWithdrawalLimitInfo>(request, User.Identity.Name));

            if (success)
                return new ApiResult((int)ErrorCodeEnum.Success, string.Empty);

            return new ApiResult((int)ErrorCodeEnum.UpdateFailed, _selLangBll.GetMsg("msg_Fail"));
        }

        /// <summary>
        /// WithdrawalLimitCrypto/Update 更新加密貨幣提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Crypto Withdrawal Limit Management > Select [SiteName] & [CryptoCurrency] & [NetWork] > [Edit] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Update(ApiRequestWithLog<UpdateCryptoWithdrawalLimitInfo> request)
        {
            var success = _withdrawalBll.EditCryptoWithdrawalLimit(
                new ParamsWithLog<UpdateCryptoWithdrawalLimitInfo>(request, User.Identity.Name));

            if (success)
                return new ApiResult((int)ErrorCodeEnum.Success, string.Empty);

            return new ApiResult((int)ErrorCodeEnum.UpdateFailed, _selLangBll.GetMsg("msg_Fail"));
        }

        /// <summary>
        /// WithdrawalLimitCrypto/Delete 刪除加密貨幣提款限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Crypto Withdrawal Limit Management > Select [SiteName] & [CryptoCurrency] & [NetWork] > [Delete]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        [HttpPost]
        public ApiResult Delete(ApiRequestWithLog<DeleteCryptoWithdrawalLimitInfo> request)
        {
            var success = _withdrawalBll.DeleteCryptoWithdrawalLimit(
                new ParamsWithLog<DeleteCryptoWithdrawalLimitInfo>(request, User.Identity.Name));

            if (success)
                return new ApiResult((int)ErrorCodeEnum.Success, string.Empty);

            return new ApiResult((int)ErrorCodeEnum.UpdateFailed, _selLangBll.GetMsg("msg_Fail"));
        }

        #endregion
    }
}
