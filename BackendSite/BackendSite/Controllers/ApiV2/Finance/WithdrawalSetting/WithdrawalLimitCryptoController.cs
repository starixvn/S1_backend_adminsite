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
    [Authorize(AuthenticationSchemes = "backendV2")]
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

        public class CreateRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public double WithdrawalMin { get; set; }
            public double WithdrawalMax { get; set; }
            public int Times { get; set; }
            public NetworkTypeEnum NetworkType { get; set; }
            public CryptoCurrencyEnum CryptoCurrency { get; set; }
        }

        public class UpdateRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int VipLevel { get; set; }
            public int Times { get; set; }
            public double WithdrawalMin { get; set; }
            public double WithdrawalMax { get; set; }
            public NetworkTypeEnum NetworkType { get; set; }
            public CryptoCurrencyEnum CryptoCurrency { get; set; }
        }

        public class DeleteRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int VipLevel { get; set; }
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
        public ApiListResult<CryptoWithdrawalLimitInfoManage> GetList(GetListRequest request)
        {
            var result = _withdrawalBll.GetCryptoWithdrawalLimitInfos(request.SiteIds,
                request.CurrencyIds,
                (int)request.CryptoCurrency,
                Enum.GetName(typeof(NetworkTypeEnum), request.NetworkType),
                request.PageNumber,
                request.PageSize,
                out int totalPages,
                out int rowCount);

            var apiResult = new ApiListResult<CryptoWithdrawalLimitInfoManage>
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
        public ApiResult Create(CreateRequest request)
        {
            var data = new CryptoWithdrawalLimitInfoManage()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                WithdrawalMax = request.WithdrawalMax,
                WithdrawalMin = request.WithdrawalMin,
                NetworkType = (int)request.NetworkType,
                CryptoCurrencyId = (int)request.CryptoCurrency,
                UpdateOperator = User.Identity.Name,
                Times = request.Times
            };

            var success = _withdrawalBll.AddCryptoWithdrawalLimit(data);

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
        public ApiResult Update(UpdateRequest request)
        {
            var data = new CryptoWithdrawalLimitInfoManage()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                VipLevel = request.VipLevel,
                Times = request.Times,
                WithdrawalMax = request.WithdrawalMax,
                WithdrawalMin = request.WithdrawalMin,
                NetworkType = (int)request.NetworkType,
                CryptoCurrencyId = (int)request.CryptoCurrency,
                UpdateOperator = User.Identity.Name
            };

            var success = _withdrawalBll.EditCryptoWithdrawalLimit(data);

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
        public ApiResult Delete(DeleteRequest request)
        {
            var data = new CryptoWithdrawalLimitInfoManage()
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                VipLevel = request.VipLevel,
                NetworkType = (int)request.NetworkType,
                CryptoCurrencyId = (int)request.CryptoCurrency,
                UpdateOperator = User.Identity.Name
            };

            var success = _withdrawalBll.DeleteCryptoWithdrawalLimit(data);

            if (success)
                return new ApiResult((int)ErrorCodeEnum.Success, string.Empty);

            return new ApiResult((int)ErrorCodeEnum.UpdateFailed, _selLangBll.GetMsg("msg_Fail"));
        }

        #endregion
    }
}
