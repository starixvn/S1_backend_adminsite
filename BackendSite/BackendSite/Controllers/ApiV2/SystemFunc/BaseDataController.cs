using System.ComponentModel;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendSite.ExceptionHandling;
using BackendSite.Service.Library;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.TpMaintain;
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
        private readonly MarketingBLL _marketingBll;
		private readonly WinLostReportBLL _winLostReportBll;
        private readonly ReportBLL _reportBll;
        private readonly NewAuthorizeBLL _authorizeBll;
        private readonly PermissionBLL _permissionBll;
        private readonly Cryptor _cryptor;
        private readonly PaymentManageBLL _paymentManageBll;

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
            _marketingBll = provider.GetRequiredService<MarketingBLL>();
			_winLostReportBll = provider.GetRequiredService<WinLostReportBLL>();
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
            _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _cryptor = provider.GetRequiredService<Cryptor>();
            _paymentManageBll = provider.GetRequiredService<PaymentManageBLL>();
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
            public TransferAccountPurposeEnum Purpose { get; set; }
        }

        public class GetPayListRequest
        {
            /// <summary>
            /// 幣別
            /// </summary>
            public int CurrencyId { get; set; }
            /// <summary>
            /// 銀行ID
            /// </summary>
            public int SysId { get; set; }
            /// <summary>
            /// 金額，取提款金流商列表以外非必填
            /// </summary>
            public decimal? Amount { get; set; }
            /// <summary>
            /// 取提款金流商列表以外非必填
            /// </summary>
            public int? CustId { get; set; }
        }
        
        public class GetPaymentListRequest
        {
            /// <summary>
            /// 支付Id，帶null可取全部
            /// </summary>
            public int? PayId { get; set; }
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
            /// <summary>
            /// 金額，取提款金流商列表以外非必填
            /// </summary>
            public decimal? Amount { get; set; }
        }

        public class GetTpGameTypeListByCategoryReq
        {
            public int CategoryId { get; set; }
        }
        
        public class GetTpListByCategoryReq
        {
            public int CategoryId { get; set; }
        }
		
		public class GetTpGameTypeByCategoryAndTpIdReq
        {
            public int TpId { get; set; }
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

        public class TpCategoryByTpIdRequest
        {
            public int TpId { get; set; }
        }

        public class GetTpGameListReq
        {
            [Description("tpId = 0 WalletType is Seamless, tpId > 0 WalletType is Transfer")]
            public int TpId { get; set; }
            
            [Description("Promotion CategoryId: 1=Slots Game, 2=Live Casino, 3=Fish, 4=Saba Sports, 5=Lottery, 6=Sports, 7=PT Slots, 8=PT Live Casino, 9=SBSW, 10=Mega888 Slot RNG & Fishing")]
            public int CategoryId { get; set; }
        }

        public class GetBankInfoOffListByCustVerifyRequest
        {
            public string SabaId { get; set; }
            public string UserName { get; set; }
            public int SiteId { get; set; }
            /// <summary>
            /// 1. Deposit, 2. Withdrawal
            /// </summary>
            public TransferAccountPurposeEnum Purpose { get; set; }
        }

        public class GetPromoCategoryRequest
        {
            /// <summary>
            /// 0=All, 1=Seamless Wallet, 2=Transfer Wallet
            /// </summary>
            public int? WalletType { get; set; }
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
        
        public class WithdrawalEWalletReq
        {
            public int CurrencyId { get; set; }
        }
        
        public class GetPointLevelListRequest
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
        
        public class GetBankInfoSysRequest
        {
            /// <summary>
            /// 幣別
            /// </summary>
            public int CurrencyId { get; set; }
        }
        
        public class GetBankCodeListRequest
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
        
        public class GetCalCodeListRequest
        {
            /// <summary>
            /// 品牌ID
            /// </summary>
            public int SiteId { get; set; }
        }

        public class GetTpBySiteAndCurrencyReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
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
        public ApiResult<IEnumerable<OfflineBankAccount>> GetBankInfoOffList(GetBankInfoOffListRequest request)
        {
            var data = _bankDetailBll.GetOfflineBankAccount(request.SiteId, request.CurrencyId)
                .Where(x => x.Purposes.Contains(request.Purpose)
                            && x.AccountStatus == TransferAccountStatusEnum.Open
                            && x.Channel != TransferChannelEnum.PSP)
                .OrderBy(x => x.SettingStatus);

            // 有 withdrawal / deposit approved 權限才顯示密碼
            if ((request.Purpose == TransferAccountPurposeEnum.Withdrawal && _permissionBll.CheckPermission(28, new []{65}))
                || (request.Purpose == TransferAccountPurposeEnum.Deposit && _permissionBll.CheckPermission(27, new []{65})))
            {
                foreach (var d in data)
                {
                    d.BankLoginAccount = _cryptor.Decrypt(d.BankLoginAccount);
                    d.BankLoginPassword = _cryptor.Decrypt(d.BankLoginPassword);
                    d.TransferPin = _cryptor.Decrypt(d.TransferPin);
                    d.LoginPin = _cryptor.Decrypt(d.LoginPin);
                    d.MobilePin = _cryptor.Decrypt(d.MobilePin);
                    d.CorporateID = _cryptor.Decrypt(d.CorporateID);
                }
            }
            
            return new ApiResult<IEnumerable<OfflineBankAccount>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
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
        [HttpPost]
        public ApiResult GetPayList(GetPayListRequest request)
        {
            var authData = _authorizeBll.GetAuthData();
            var result = _bankDetailBll
                .GetBankInfoPayList(authData.SiteId, request.CurrencyId, request.SysId, request.Amount, request.CustId)
                .ToList();
            
            if (_permissionBll.CheckPermission(28, new []{65}))
            {
                result.ForEach(d =>
                {
                    d.LoginAccount = _cryptor.Decrypt(d.LoginAccount);
                    d.LoginPassword = _cryptor.Decrypt(d.LoginPassword);
                    d.TransferPin = _cryptor.Decrypt(d.TransferPin);
                    d.LoginPin = _cryptor.Decrypt(d.LoginPin);
                });
            }
            
            return new ApiResult<IEnumerable<PayInfo>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        /// <summary>
        /// 取得金流列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult GetPaymentList(GetPaymentListRequest request)
        {
            var authData = _authorizeBll.GetAuthData();
            var data = _paymentManageBll.GetPayList(authData.SiteId)
                .Where(x => x.PayStatus == PayListStatusEnum.Enable
                    && (request.PayId == null || x.PayId == request.PayId));

            if (_permissionBll.CheckPermission(27, new []{65}))
            {
                foreach (var d in data)
                {
                    d.LoginAccount = _cryptor.Decrypt(d.LoginAccount);
                    d.LoginPassword = _cryptor.Decrypt(d.LoginPassword);
                    d.TransferPin = _cryptor.Decrypt(d.TransferPin);
                    d.LoginPin = _cryptor.Decrypt(d.LoginPin);
                }
            }

            var result = data.Select(x => new PayInfo
            {
                PayId = x.PayId,
                PayName = x.PayName,
                LoginUrl = x.LoginUrl,
                LoginAccount = x.LoginAccount,
                LoginPassword = x.LoginPassword,
                LoginPin = x.TransferPin,
                TransferPin = x.TransferPin
            });

            return new ApiResult<IEnumerable<PayInfo>>
            {
                Data = result,
                ErrorCode = (int) ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// BaseData/GetPayList 取得指定EWallet清單
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
        //有新舊API，這支是新API，另一支新的API=api/Withdrawal/GetEWalletPayList
        [HttpPost]
        public ApiResult<IEnumerable<EWalletPay>> GetEWalletPayList(WithdrawalEWalletReq request)
        {
            var apiResult = new ApiResult<IEnumerable<EWalletPay>>();
            var eWalletPay = _bankDetailBll.GetEWalletPayList();
            var _eWalletPayInfo = (from x in eWalletPay
                where x.CurrencyId == request.CurrencyId
                select new EWalletPay
                {
                    PayId = x.PayId,
                    PayName = x.PayName,
                    CurrencyId = x.CurrencyId
                });
            apiResult.Data = _eWalletPayInfo;
            apiResult.ErrorCode = 0;
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
            var authData = _authorizeBll.GetAuthData();
            var result =
                _bankDetailBll.GetCryptoPayList(authData.SiteId, request.CurrencyId, request.ChannelName, request.Amount);

            return new ApiResult<IEnumerable<Pay>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
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
        public ApiResult<IEnumerable<OfflineBankAccount>> GetBankInfoOffListByCustVerify(GetBankInfoOffListByCustVerifyRequest request)
        {
            var custVerify =
                _custVerifyBll.CustVerify(request.SiteId, request.UserName, request.SabaId, out var custInfo);

            if (custVerify != ErrorCodeEnum.Success)
            {
                throw new AdminException(custVerify);
            }

            if (custInfo == null)
            {
                return new ApiResult<IEnumerable<OfflineBankAccount>>
                {
                    ErrorCode = 21,
                    Message = "Player not found."
                };
            }
                
            var bankInfoOff = _bankDetailBll
                .GetOfflineBankAccount(request.SiteId, custInfo.CurrencyId)
                .Where(x => x.Purposes.Contains(request.Purpose) 
                            && x.AccountStatus == TransferAccountStatusEnum.Open
                            && x.Channel == TransferChannelEnum.LocalBank);

            return new ApiResult<IEnumerable<OfflineBankAccount>>
            {
                Data = bankInfoOff,
                ErrorCode = 0
            };

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
        /// BaseData/GetTpListbyCategory 取得指定第三方遊戲類型清單
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<SiteTpListIfo>>> GetTpListByCategory(GetTpListByCategoryReq request)
        {
            var data = await _userBetLimitBll.GetTpListByCategoryAsync(request.CategoryId);
            var apiResult = new ApiResult<IEnumerable<SiteTpListIfo>>
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
                var gameCasinoData = await _gameCasinoBll.GetGameCasinoByTpId(request.TpId);
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
                apiResult.Data = _bonusManageBll.GetPromoCategoriesV2(Convert.ToInt32(request.WalletType));
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
        /// BaseData/GetPointLevelList 取得績分等級資訊清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult<IEnumerable<int>> GetPointLevelList(GetPointLevelListRequest request)
        {
            var pointLevelInfos = _marketingBll
                .GetPointLevelInfos(request.SiteId, request.CurrencyId)
                .Select(x => x.PointLevel)
                .Union(new []{ 0, 1 })
                .Distinct()
                .OrderBy(x => x);
            
            return new ApiResult<IEnumerable<int>>
            {
                Data = pointLevelInfos,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        /// <summary>
        /// BaseData/BankInfoSys 取得銀行系統資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Bank Card Management.
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<BankInfoSys>> GetBankInfoSys(GetBankInfoSysRequest request)
        {
            var sysBankList = _bankDetailBll.GetSysBankList(request.CurrencyId);
            
            return new ApiResult<IEnumerable<BankInfoSys>>
            {
                Data = sysBankList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        /// <summary>
        /// BaseData/GetBankCodeList
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<BankCodeModel>> GetBankCodeList(GetBankCodeListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<BankCodeModel>>();
            var bankInfoOff =
                _bankDetailBll.GetOfflineBankAccount(request.SiteId, request.CurrencyId)
                    .Select(x => new BankCodeModel
                    {
                        SysId = x.SysId,
                        BankCode = x.BankCode ,
                        BankName = x.BankName ,
                        CurrencyId = x.CurrencyId
                    }).Distinct(new BankCodeModelComparer());
            apiResult.Data = bankInfoOff;
            apiResult.ErrorCode = 0;
            return apiResult;
        }
        
        /// <summary>
        /// BaseData/GetCalCodeList
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// Func Path：Deposit Request > CalCode DropDownList
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<CalCodeModel>> GetCalCodeList(GetCalCodeListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<CalCodeModel>>();
            var bankInfoOff = 
                _bankDetailBll.GetOfflineBankAccount(request.SiteId);
            bankInfoOff = bankInfoOff
                .Where(x => x.Purposes.Contains(TransferAccountPurposeEnum.Deposit))
                .OrderBy(x => x.BankId);
            var data = bankInfoOff
                .Select(x => new CalCodeModel
                {
                    BankId = x.BankId,
                    CalCodeName = x.DisplayName
                });
            apiResult.Data = data;
            apiResult.ErrorCode = 0;
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
        /// 取得第三方遊戲分類清單ByTpId
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<TpCategory>>> GetTpCategoryByTpId(TpCategoryByTpIdRequest request)
        {
            var gameTypeList = _winLostReportBll.GetTpGameType().Where(x=>x.TpId == request.TpId);
            var categories = gameTypeList.DistinctBy(x => x.CategoryId).Select(x => x.CategoryId);
            
            var categoryList = await _reportBll.GetTpCategoryList();
            
            var query = categoryList.Join(categories, a => a.CategoryId, b => b,
                (a, b) => new TpCategory {CategoryId = a.CategoryId, CategoryName = a.CategoryName});
            
            var apiResult = new ApiResult<IEnumerable<TpCategory>>
            {
                Data = query,
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
        
        /// <summary>
        /// 取得第三方遊戲類型清單ByCategoryAndTpId
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<TpGameType>>> GetTpGameTypeByCategoryAndTpId(GetTpGameTypeByCategoryAndTpIdReq request)
        {
            var data = await _tpBll.GetTpGameTypeListByCategoryAsync(request.CategoryId);
            var result = data.Where(x => x.TpId == request.TpId);
            
            var apiResult = new ApiResult<IEnumerable<TpGameType>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }
        
        [HttpPost]
        public ApiResult GetTransferAccountList(GetAccountListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<BankFinancialTransferAccount>>
            {
                Data = _bankFinancialBll.GetTransferAccountList(request.SiteId),
                
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }
        
        [HttpPost]
        public ApiResult GetFinancialAccountList(GetFinancialAccountListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<BankFinancialTransferAccount>>
            {
                Data = _bankFinancialBll.GetFinancialAccountList(request.SiteId, request.Purposes),
                
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }
        
        /// <summary>
        /// 取得該幣別支援的tpList
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<SiteTpListIfo>> GetTpBySiteAndCurrency(GetTpBySiteAndCurrencyReq request)
        {
            var apiListResult = new ApiResult<IEnumerable<SiteTpListIfo>>();

            var tpList = _tpBll.GetTpBySiteAndCurrency(request.SiteId,request.CurrencyId);
            apiListResult.Data = tpList;
            apiListResult.ErrorCode = 0;
            return apiListResult;
        }
    }
}
