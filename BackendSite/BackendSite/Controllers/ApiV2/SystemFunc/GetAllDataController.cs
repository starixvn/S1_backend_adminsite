using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.CustVipLevel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.TpMaintain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace BackendSite.Controllers.ApiV2.SystemFunc
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class GetAllDataController : ControllerBase
    {
        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly GameCasinoBLL _gameCasinoBll;
        private readonly WinLostReportBLL _winLostReportBll;
        private readonly PaymentBLL _paymentBll;
        private readonly BankDetailBLL _bankDetailBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly PrizeBLL _prizeBll;
        private readonly PaymentManageBLL _paymentManageBll;
        private readonly ReasonManagementBLL _reasonManagementBll;
        private readonly TpMaintainBLL _tpMaintainBll;
        private readonly WithdrawalBLL _withdrawalBll;
        private readonly BonusManageBLL _bonusManageBll;
        private readonly DeepLinkBLL _deepLinkBll;
        private readonly NewAuthorizeBLL _authorizeBll;
        private readonly ServerInfoBLL _serverInfoBll;

        #endregion

        #region Constructors

        public GetAllDataController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _gameCasinoBll = provider.GetRequiredService<GameCasinoBLL>();
            _winLostReportBll = provider.GetRequiredService<WinLostReportBLL>();
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _prizeBll = provider.GetRequiredService<PrizeBLL>();
            _paymentManageBll = provider.GetRequiredService<PaymentManageBLL>();
            _reasonManagementBll = provider.GetRequiredService<ReasonManagementBLL>();
            _tpMaintainBll = provider.GetRequiredService<TpMaintainBLL>();
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _bonusManageBll = provider.GetRequiredService<BonusManageBLL>();
            _deepLinkBll = provider.GetRequiredService<DeepLinkBLL>();
            _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }

        #endregion


        #region Api Model

        public class GetBySiteAndCurrencyRequest
        {
            /// <summary>
            /// 品牌
            /// </summary>
            public IEnumerable<int>? SiteIds { get; set; } = null;
            /// <summary>
            /// 幣別
            /// </summary>
            public int CurrencyId { get; set; } = 0;
        }

        #endregion

        #region Actions

        //==============================================================================================================================================
        // System 相關：Permission, Currency. Module(Menu)
        //==============================================================================================================================================

        /// <summary>
        /// GetAllData/GetCurrency 取得幣別資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.
        /// </remarks>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<IEnumerable<CurrencyModel>> GetCurrency()
        {
            var data = _reportBll.GetCurrency();

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<CurrencyModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// GetAllData/GetFuncKeyAboutReason 取得原因碼的FuncKey
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<ReasonManagementFuncKey>> GetFuncKeyAboutReason()
        {
            var apiResult = new ApiResult<IEnumerable<ReasonManagementFuncKey>>
            {
                Data = _reasonManagementBll.GetFuncKey(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;

        }

        //==============================================================================================================================================
        // Finance 相關：Payment, Bank
        //==============================================================================================================================================

        /// <summary>
        /// GetAllData/BankInfoSys 取得銀行系統資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Bank Card Management.
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<BankInfoSys>> GetBankInfoSys()
        {
            var apiResult = new ApiResult<IEnumerable<BankInfoSys>>();
            var sysBankList = _bankDetailBll.GetSysBankList();
            apiResult.Data = sysBankList;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;
        }

        /// <summary>
        /// GetAllData/GetAdjustmentInfoLogMenu 取得調整資料記錄程式清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > Activity Log Report > DropDownList[Function]
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<SelectListItem>> GetAdjustmentInfoLogMenu()
        {
            var apiResult = new ApiResult<IEnumerable<SelectListItem>>
            {
                Data = _adjustmentLogReportBll.GetAdminAdjustmentInfoLogMenu(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiResult;
        }

        /// <summary>
        /// GetAllData/GetActionTypeList 取得調整資料記錄類型清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > Activity Log Report > DropDownList[Type]
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<SelectListItem>> GetActionTypeList()
        {
            var apiResult = new ApiResult<IEnumerable<SelectListItem>>
            {
                Data = _adjustmentLogReportBll.GetActionTypeList(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiResult;
        }

        /// <summary>
        /// GetAllData/GetPayList 取得第三方支付廠商清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<PayListInfo>> GetPayList()
        {
            var authData = _authorizeBll.GetAuthData();
            var data = _paymentManageBll.GetPayList(authData.SiteId);

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<PayListInfo>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }
        /// <summary>
        /// GetAllData/GetUserAllPayList 驗證登入者權限可見第三方支付廠商清單
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [HttpPost]
        public ApiResult<IEnumerable<PayListInfo>> GetUserAllPayList()
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            var data = _paymentManageBll.GetPayListByMultiSite(userServerInfo.Select(x => x.SiteId));

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<PayListInfo>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// GetAllData/GetPayChannel 取得第三方支付廠商通道清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<PayChannelInfo>> GetPayChannel()
        {

            var authData = _authorizeBll.GetAuthData();
            var data = _paymentManageBll.GetPayChannel(authData.SiteId);

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<PayChannelInfo>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        [HttpPost]
        public ApiResult<IEnumerable<PaySetting>> GetPaySetting()
        {
            var authData = _authorizeBll.GetAuthData();
            var data = _paymentManageBll.GetPaySetting(authData.SiteId);

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<PaySetting>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        //==============================================================================================================================================
        // TP 相關
        //==============================================================================================================================================

        /// <summary>
        /// GetAllData/GetTpList 取得Slot第三方品牌
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Show Slot Tp List
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<RiskTpListInfo>>> GetSlotTpList()
        {
            string requestBody = null;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                requestBody = reader.ReadToEnd();
            }

            var request = string.IsNullOrWhiteSpace(requestBody) ? new GetBySiteAndCurrencyRequest()
                : JsonSerializer.Deserialize<GetBySiteAndCurrencyRequest>(requestBody);
            if (request.SiteIds is null)
            {
                request.SiteIds = _serverInfoBll.GetSiteListByLoginUserV2().Select(x => x.SiteId);
            }
            var data = await _gameCasinoBll.GetTpList(JsonSerializer.Serialize(request.SiteIds), request.CurrencyId);

            return new ApiResult<IEnumerable<RiskTpListInfo>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// GetAllData/GetTpList 取得Slot第三方品牌
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Show Slot Tp List
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<SlotGameDetail>>> GetSlotGame()
        {
            var data = await _gameCasinoBll.GetSlotGame();

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<SlotGameDetail>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// GetAllData/GetTpList 取得第三方品牌
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<CommonTpInfo>> GetTpList()
        {
            string requestBody = null;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                requestBody = reader.ReadToEnd();
            }

            var request = string.IsNullOrWhiteSpace(requestBody) ? new GetBySiteAndCurrencyRequest()
                : JsonSerializer.Deserialize<GetBySiteAndCurrencyRequest>(requestBody);

            if (request.SiteIds is null)
            {
                request.SiteIds = _serverInfoBll.GetSiteListByLoginUserV2().Select(x => x.SiteId);
            }

            var data = _tpMaintainBll.GetCommonTpList(JsonSerializer.Serialize(request.SiteIds), request.CurrencyId);

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<CommonTpInfo>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// GetAllData/GetTpCategory 取得第三方遊戲分類
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： [Available Category]
        /// </remarks>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<TpCategoryBetLimit>>> GetTpCategory()
        {
            var data = await _reportBll.GetTpCategoryList();

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<TpCategoryBetLimit>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// GetAllData/GetTpGameType 取得第三方遊戲類型
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Show tp  GameType List
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<GameType>> GetTpGameType()
        {
            string requestBody = null;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                requestBody = reader.ReadToEnd();
            }

            var request = string.IsNullOrWhiteSpace(requestBody) ? new GetBySiteAndCurrencyRequest()
                : JsonSerializer.Deserialize<GetBySiteAndCurrencyRequest>(requestBody);

            if (request.SiteIds is null)
            {
                request.SiteIds = _serverInfoBll.GetSiteListByLoginUserV2().Select(x => x.SiteId);
            }

            var data = _winLostReportBll.GetTpGameType(JsonSerializer.Serialize(request.SiteIds), request.CurrencyId);

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<GameType>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        //==============================================================================================================================================
        // Marketing 相關：Promotion
        //==============================================================================================================================================

        /// <summary>
        /// GetAllData/GetPromotionCategory 取得促銷分類
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// CSD > Customer Administration > CustInfo > Bonus Log [Promotion Category]
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<PromotionCategory>> GetPromotionCategory()
        {
            var data = _paymentBll.GetPromotionCategory();

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            return new ApiResult<IEnumerable<PromotionCategory>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// AllData/GetEventNameList 取得活動名稱清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Draw Prize Report > EventName
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<EventDraw>> GetEventName()
        {
            var apiResult = new ApiResult<IEnumerable<EventDraw>>
            {
                Data = _prizeBll.GetEventName(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// GetAllData/GetCryptoInfoData 取得加密貨幣資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Crypto Withdrawal Limit Management > Crypto Filter
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<CryptoInfoData> GetCryptoInfoData()
        {
            var authData = _authorizeBll.GetAuthData();
            var result = _withdrawalBll.GetCryptoInfoData(authData.SiteId);

            return new ApiResult<CryptoInfoData>((int)ErrorCodeEnum.Success, string.Empty, result);
        }

        /// <summary>
        /// BaseData/GetAllSiteVipLevelList  取得所有品牌VIP等級清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<VipLevelModel>> GetAllSiteVipLevelList()
        {
            return new ApiResult<IEnumerable<VipLevelModel>>
            {
                Data = _custInfoBll.GetAllSiteVipLevelList(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// GetAllData/GetPromotionType  取得PromotionType List
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<PromotionType>> GetPromotionType(GetPromotionTypeRequest request)
        {
            return new ApiResult<IEnumerable<PromotionType>>
            {
                Data = _bonusManageBll.GetPromotionType(request.isPayout, request.isNew),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        //==============================================================================================================================================
        // CMS 相關
        //==============================================================================================================================================

        /// <summary>
        /// GetAllData/GetDeepLinkList  取得DeepLink List
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<DeepLink>> GetDeepLinkList()
        {
            var apiResult = new ApiResult<IEnumerable<DeepLink>>
            {
                Data = _deepLinkBll.GetDeepLinkList(),
                ErrorCode = 0
            };

            return apiResult;
        }

        public class GetPromotionTypeRequest
        {
            /// <summary>
            /// Bonus Payout使用
            /// </summary>
            public bool isPayout { get; set; } = false;

            /// <summary>
            /// 新後台Bonus Manage使用，排除Rebate
            /// </summary>
            public bool isNew { get; set; } = true;
        }

        public class GetServerInfoReq
        {
            public int SiteId { get; set; }
        }
        /// <summary>
        /// GetAllData/GetServerInfo  取得ServerInfo
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<ServerInfo> GetServerInfo(GetServerInfoReq req)
        {
            var apiResult = new ApiResult<ServerInfo>();
            try
            {
                apiResult.Data = _serverInfoBll.GetServerInfo(req.SiteId);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public async Task<ApiResult<IEnumerable<LocalPaySetting>>> GetLocalPaySettingAsync()
        {
            var result = await _bankDetailBll.GetLocalPaySettingAsync();

            return new ApiResult<IEnumerable<LocalPaySetting>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion

    }
}
