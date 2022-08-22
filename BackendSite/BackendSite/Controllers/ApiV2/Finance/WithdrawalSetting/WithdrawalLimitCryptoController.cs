using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.WithdrawalSetting
{
    [Authorize(AuthenticationSchemes = "backend")]
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

        public class GetListRequest 
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
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
        public ApiResult GetList(GetListRequest request)
        {
            var result = _withdrawalBll.GetCryptoWithdrawalLimitInfos(request.SiteId, request.CurrencyId);

            var apiResult = new ApiResult<IEnumerable<CryptoWithdrawalLimitInfo>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
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
        public ApiResult Update(ApiRequestWithLog<IEnumerable<UpdateCryptoWithdrawalLimitInfo>> request)
        {
            var success = _withdrawalBll.EditCryptoWithdrawalLimit(
                new ParamsWithLog<IEnumerable<UpdateCryptoWithdrawalLimitInfo>>(request, User.Identity.Name));

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
