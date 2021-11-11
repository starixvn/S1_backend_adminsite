using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.SystemFunc
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BaseDataController : ControllerBase
    {

        private readonly BankDetailBLL _bankDetailBll;
        private readonly UserBetLimitBLL _userBetLimitBll;
        private readonly GameCasinoBLL _gameCasinoBll;
        private static readonly BankInfoPayComparer bankInfoPayComparer = new BankInfoPayComparer();

        public BaseDataController(IServiceProvider provider)
        {
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _userBetLimitBll = provider.GetRequiredService<UserBetLimitBLL>();
            _gameCasinoBll = provider.GetRequiredService<GameCasinoBLL>();
        }

        #region Model

        public class GetBankInfoOffListRequest
        {
            /// <summary>
            /// 品牌ID
            /// </summary>
            public int SiteId { get; set; }
            /// <summary>
            /// 幣別
            /// </summary>
            public int CurrencyId { get; set; }
        }

        public class GetPayListRequest
        {
            /// <summary>
            /// 幣別
            /// </summary>
            public int CurrencyId { get; set; }
            /// <summary>
            /// 支付ID
            /// </summary>
            public int PayId { get; set; }
        }

        public class GetCryptoPayListRequest
        {
            /// <summary>
            /// 幣別
            /// </summary>
            public int CurrencyId { get; set; }
            /// <summary>
            /// 渠道名稱
            /// </summary>
            public string ChannelName { get; set; }
        }

        public class Pay
        {
            /// <summary>
            /// 付款渠道ID
            /// </summary>
            public int PayId { get; set; }

            /// <summary>
            /// 付款名稱
            /// </summary>
            public string PayName { get; set; }

            /// <summary>
            /// 渠道ID
            /// </summary>
            public int ChannelId { get; set; }
        }
        
        public class GetTpGameTypeListByCategoryReq
        {
            public int CategoryId { get; set; }
        }
        
        public class GameCasinoByTpIdReq
        {
            public int TpId { get; set; }
        }
        

        #endregion

        /// <summary>
        /// BaseData/GetBankInfoOffList 取得指定銀行戶口資料清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.
        /// 
        /// Use：Finance > Withdrawal Request > [select Pay], When Type = 2.Local Bank.
        /// 
        /// </remarks>
        /// <param name="withdrawalReq"></param>
        /// <returns></returns>
        //有新舊API，這支是新API，另一支新的API=api/Withdrawal/GetBankInfoOffList
        [HttpPost]
        public ApiResult<IEnumerable<BankInfoOff>> GetBankInfoOffList(GetBankInfoOffListRequest withdrawalReq)
        {
            var apiResult = new ApiResult<IEnumerable<BankInfoOff>>();
            var bankInfoOff = _bankDetailBll.GetBankInfoOff(withdrawalReq.SiteId);
            bankInfoOff = bankInfoOff.Where(x => x.CurrencyId == withdrawalReq.CurrencyId && (x.Purpose & 2) > 0)
                .OrderBy(x => x.BSort);
            apiResult.Data = bankInfoOff;
            apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            return apiResult;
        }

        /// <summary>
        /// BaseData/GetPayList 取得指定行動支付清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.
        /// 
        /// Use：Finance > Withdrawal Request > [select Pay], When Type = 1.Online Payment.
        /// </remarks>
        /// <param name="withdrawalReq"></param>
        /// <returns></returns>
        //有新舊API，這支是新API，另一支新的API=api/Withdrawal/GetPayList
        [HttpPost]
        public ApiResult<IEnumerable<Pay>> GetPayList(GetPayListRequest withdrawalReq)
        {
            var apiResult = new ApiResult<IEnumerable<Pay>>();
            var bankInfoPay = _bankDetailBll.GetBankInfoPay();
            var _payInfo = (from x in bankInfoPay
                            where x.CurrencyId == withdrawalReq.CurrencyId && x.SysId == withdrawalReq.PayId
                            select new Pay
                            {
                                PayId = x.PayId,
                                PayName = x.PayName
                            }).Distinct(bankInfoPayComparer);
            apiResult.Data = _payInfo;
            apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            return apiResult;
        }

        /// <summary>
        /// BaseData/GetCryptoPayList 取得指定加密支付清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.
        /// 
        /// Use：Finance > Withdrawal Request > [select Pay], When Type = 4.Crypto Pay.
        /// 
        /// </remarks>
        /// <param name="withdrawalReq"></param>
        /// <returns></returns>
        //有新舊API，這支是新API，另一支新的API=api/Withdrawal/GetCryptoPayList
        [HttpPost]
        public ApiResult<IEnumerable<Pay>> GetCryptoPayList(GetCryptoPayListRequest withdrawalReq)
        {
            var apiResult = new ApiResult<IEnumerable<Pay>>();
            var cryptoPay = _bankDetailBll.GetCryptoPay();
            var _payInfo = (from x in cryptoPay
                            where x.CurrencyId == withdrawalReq.CurrencyId && x.ChannelName.Equals(withdrawalReq.ChannelName, StringComparison.OrdinalIgnoreCase)
                            select new Pay
                            {
                                PayId = x.PayId,
                                PayName = x.PayName,
                                ChannelId = x.ChannelId
                            }).Distinct(bankInfoPayComparer);

            apiResult.Data = _payInfo;
            apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
            return apiResult;
        }

        /// <summary>
        /// BaseData/GetTpGameTypeListbyCategory 取得指定第三方遊戲類型清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Edit] > Check[Available Category] > [Game]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<GameTypeBetLimit>>> GetTpGameTypeListByCategory(GetTpGameTypeListByCategoryReq req)
        {
            var data = await _userBetLimitBll.GetTpGameTypeListbyCategoryAsync(req.CategoryId);
            var apiResult = new ApiResult<IEnumerable<GameTypeBetLimit>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
            return apiResult;
        }

        /// <summary>
        /// BaseData/GetGameCasinoByTpId 取得Slot第三方品牌的遊戲
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Slot Tp List 
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<RiskGameCasinoData>>> GetGameCasinoByTpId(GameCasinoByTpIdReq req)
        {
            var apiResult = new ApiResult<IEnumerable<RiskGameCasinoData>>();
            try
            {
                var gameCasinoData = await _gameCasinoBll.GameCasinoByTPIdV3(req.TpId);
                apiResult.Data = gameCasinoData;
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        #region Private Methods

        private class BankInfoPayComparer : IEqualityComparer<Pay>
        {
            public bool Equals(Pay t1, Pay t2)
            {
                return (t1.PayId == t2.PayId);
            }

            public int GetHashCode(Pay t)
            {
                return t.ToString().GetHashCode();
            }
        }

        #endregion

    }
}
