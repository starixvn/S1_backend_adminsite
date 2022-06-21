using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Service.Model.TpMaintain;
using Microsoft.Extensions.DependencyInjection;
using TpGameType = BackendSite.Service.Model.TpGameType;

namespace BackendSite.Controllers.ApiV2.SystemFunc
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BaseDataController : ControllerBase
    {
        #region Fields

        private readonly BankDetailBLL _bankDetailBll;
        private readonly UserBetLimitBLL _userBetLimitBll;
        private readonly GameCasinoBLL _gameCasinoBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly VipLevelBLL _vipLevelBll;
        private readonly CustVerifyBLL _custVerifyBll;
        private readonly BonusManageBLL _bonusManageBll;
        private readonly SportsBLL _sportsBll;
        private readonly TpBLL _tpBll;
        private readonly BankFinancialBLL _bankFinancialBll;
        private static readonly BankInfoPayComparer bankInfoPayComparer = new BankInfoPayComparer();

        #endregion

        #region Constructors

        public BaseDataController(IServiceProvider provider)
        {
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _userBetLimitBll = provider.GetRequiredService<UserBetLimitBLL>();
            _gameCasinoBll = provider.GetRequiredService<GameCasinoBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _vipLevelBll = provider.GetRequiredService<VipLevelBLL>();
            _custVerifyBll = provider.GetRequiredService<CustVerifyBLL>();
            _bonusManageBll = provider.GetRequiredService<BonusManageBLL>();
            _sportsBll = provider.GetRequiredService<SportsBLL>();
            _tpBll = provider.GetRequiredService<TpBLL>();
            _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
        }

        #endregion

        #region API Model

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
            /// <summary>
            /// 1. Deposit, 2. Withdrawal
            /// </summary>
            public int Purpose { get; set; }
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

        public class GetTagRequest
        {
            /// <summary>
            /// 資料狀態(0 = All, 1 = Enable, 2 = Disable)
            /// </summary>
            public int Status { get; set; }
        }

        public class SiteIdRequest
        {
            public int SiteId { get; set; }
        }

        public class GetTpGameListReq
        {
            public int TpId { get; set; }
            public int CategoryId { get; set; }
        }

        public class GetBankInfoOffListByCustVerifyRequest
        {
            public string SabaId { get; set; }
            public string UserName { get; set; }
            public int SiteId { get; set; }
        }

        public class GetPromoCategoryRequest
        {
            /// <summary>
            /// 0=全部, 1=主錢包, 2=體育錢包
            /// </summary>
            public int WalletType { get; set; }

        }

        public class GameTypeListReq
        {
            public int CategoryId { get; set; }
        }

        public class GetSportLeaguesRequest
        {
            public int CategoryId { get; set; }
        }

        public class GetSportTeamsRequest
        {
            public int LeaguesId { get; set; }
        }

        public class GetBySiteIdAndUserNameRequest
        {
            /// <summary>
            /// 品牌ID
            /// </summary>
            public int SiteId { get; set; }
            /// <summary>
            /// 幣別
            /// </summary>
            public string UserName { get; set; }
        }

        public class GetReasonByFuncKeyRequest
        {
            public string FuncKey { get; set; }
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
        /// <param name="request"></param>
        /// <returns></returns>
        //有新舊API，這支是新API，另一支新的API=api/Withdrawal/GetBankInfoOffList
        [HttpPost]
        public ApiResult<IEnumerable<BankInfoOff>> GetBankInfoOffList(GetBankInfoOffListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<BankInfoOff>>();
            var bankInfoOff = _bankDetailBll.GetBankInfoOff(request.SiteId);
            // request.Purpose = 1 => x.purpose = 1,3
            // request.Purpose = 2 => x.purpose = 2,3
            bankInfoOff = bankInfoOff.Where(x => x.CurrencyId == request.CurrencyId && (x.Purpose & request.Purpose) > 0)
                .OrderBy(x => x.BSort);
            apiResult.Data = bankInfoOff;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
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
        /// <param name="request"></param>
        /// <returns></returns>
        //有新舊API，這支是新API，另一支新的API=api/Withdrawal/GetPayList
        [HttpPost]
        public ApiResult<IEnumerable<Pay>> GetPayList(GetPayListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<Pay>>();
            var bankInfoPay = _bankDetailBll.GetBankInfoPay();
            var _payInfo = (from x in bankInfoPay
                            where x.CurrencyId == request.CurrencyId && x.SysId == request.PayId
                            select new Pay
                            {
                                PayId = x.PayId,
                                PayName = x.PayName
                            }).Distinct(bankInfoPayComparer);
            apiResult.Data = _payInfo;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
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
        /// <param name="request"></param>
        /// <returns></returns>
        //有新舊API，這支是新API，另一支新的API=api/Withdrawal/GetCryptoPayList
        [HttpPost]
        public ApiResult<IEnumerable<Pay>> GetCryptoPayList(GetCryptoPayListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<Pay>>();
            var cryptoPay = _bankDetailBll.GetCryptoPay();
            var _payInfo = (from x in cryptoPay
                            where x.CurrencyId == request.CurrencyId && x.ChannelName.Equals(request.ChannelName, StringComparison.OrdinalIgnoreCase)
                            select new Pay
                            {
                                PayId = x.PayId,
                                PayName = x.PayName,
                                ChannelId = x.ChannelId
                            }).Distinct(bankInfoPayComparer);

            apiResult.Data = _payInfo;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;
        }

        /// <summary>
        /// BaseData/GetBankInfoOffListByCustVerify 根據客戶驗證取得銀行戶口資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Submission > Click[Submit] > Get [Account No] List
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [HttpPost]
        public ApiResult<IEnumerable<BankInfoOff>> GetBankInfoOffListByCustVerify(GetBankInfoOffListByCustVerifyRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<BankInfoOff>>();
            var result = _custVerifyBll.CustVerify(request.SiteId, request.UserName, request.SabaId, out CustInfoDetail custInfo);

            if (result != ErrorCodeEnum.Success)
            {
                throw new AdminException(result);
            }

            if (custInfo != null)
            {
                var bankInfoOff = _bankDetailBll.GetBankInfoOff(request.SiteId).Where(x => x.CurrencyId == custInfo.CurrencyId);

                if (bankInfoOff != null)
                {
                    apiResult.Data = bankInfoOff;
                    apiResult.ErrorCode = 0;
                }
                else
                {
                    apiResult.ErrorCode = 21;
                    apiResult.Message = "BankAccount not found.";
                }
            }
            else
            {
                apiResult.ErrorCode = 21;
                apiResult.Message = "Player not found.";
            }

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
        /// <param name="request"></param>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<GameTypeBetLimit>>> GetTpGameTypeListByCategory(GetTpGameTypeListByCategoryReq request)
        {
            var data = await _userBetLimitBll.GetTpGameTypeListbyCategoryAsync(request.CategoryId);
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
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<RiskGameCasinoData>>> GetGameCasinoByTpId(GameCasinoByTpIdReq request)
        {
            var apiResult = new ApiResult<IEnumerable<RiskGameCasinoData>>();
            try
            {
                var gameCasinoData = await _gameCasinoBll.GameCasinoByTPIdV3(request.TpId);
                apiResult.Data = gameCasinoData;
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
        /// BaseData/GetTpCustInfoList 取得指定客戶第三方資訊清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<TpCustInfo>>> GetTpCustInfoList(GetBySiteIdAndUserNameRequest request)
        {
            var custInfo = _custInfoBll.GetCustInfoDetails(null, request.UserName, request.SiteId);

            if (custInfo == null)
                throw new AdminException(ErrorCodeEnum.CustNotFound);

            var datas = await _tpBll.GetTpCustInfoList(custInfo.CustId);

            if (!datas.Any())
                throw new AdminException(ErrorCodeEnum.NoData);

            return new ApiResult<IEnumerable<TpCustInfo>>()
            {
                Data = datas,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// BaseData/GetTagByStatus 取得指定狀態標籤資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Show Tag List in Status
        /// 
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<TagData>> GetTagByStatus(GetTagRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<TagData>>();
            try
            {
                var data = _custInfoBll.GetAllTag();

                switch (request.Status)
                {

                    case 1: //1 = Enable
                        data = data.Where(x => x.Status == 1);
                        break;
                    case 2:  //2 = Disable
                        data = data.Where(x => x.Status == 0);
                        break;
                    case 0: //0 = All
                    default:
                        break;
                }

                apiResult.Data = data;

                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }

        /// <summary>
        /// BaseData/GetVipLevelSettingBySiteId 取得指定品牌的VIP等級設定資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Get Vip Level Setting
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<VipLevelSetting> GetVipLevelSettingBySiteId(SiteIdRequest request)
        {
            var apiListResult = new ApiListResult<VipLevelSetting>
            {
                Data = _vipLevelBll.GetVipLevelSetting(request.SiteId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiListResult;
        }

        /// <summary>
        /// BaseData/GetCashBackGameList 取得指定現金回饋遊戲清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<GetCashBackGamerep>> GetCashBackGameList(GetTpGameListReq para)
        {
            var apiResult = new ApiResult<IEnumerable<GetCashBackGamerep>>();

            apiResult.Data = _bonusManageBll.GetCashBackGameList(para.TpId, para.CategoryId);
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// BaseData/GetPromotionCategoryList 取得促銷分類清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<PromoCategory>> GetPromotionCategoryList(GetPromoCategoryRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<PromoCategory>>();

            try
            {
                apiResult.Data = _bonusManageBll.GetPromoCategories(request.WalletType);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        /// <summary>
        /// BaseData/GetSportLeaguesList 取得體育聯賽清單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<SportLeagues>> GetSportLeaguesList(GetSportLeaguesRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<SportLeagues>>
            {
                Data = _sportsBll.GetSportLeaguesList(request.CategoryId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// BaseData/GetSportTeamsList 取得體育賽隊清單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<SportTeams>> GetSportTeamsList(GetSportTeamsRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<SportTeams>>
            {
                Data = _sportsBll.GetSportTeamsList(request.LeaguesId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// BaseData/GetReasonList 取得原因碼清單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<BankFinancialReasonModel>> GetReasonList(GetReasonByFuncKeyRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<BankFinancialReasonModel>>();

            apiResult.Data = _bankFinancialBll.GetReason(request.FuncKey);
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }
        
        /// <summary>
        /// 取得第三方遊戲分類清單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<TpCategory>>> GetTpCategoryListAsync(SiteIdRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<TpCategory>>
            {
                Data = await _tpBll.GetTpCategoryListAsync(request.SiteId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }
        
        /// <summary>
        /// 取得第三方遊戲類型清單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<TpGameType>>> GetTpGameTypeListAsync(GetTpGameTypeListByCategoryReq request)
        {
            var apiResult = new ApiResult<IEnumerable<TpGameType>>
            {
                Data = await _tpBll.GetTpGameTypeListByCategoryAsync(request.CategoryId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

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
