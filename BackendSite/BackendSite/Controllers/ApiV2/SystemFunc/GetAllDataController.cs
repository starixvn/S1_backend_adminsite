using BackendSite.ExceptionHandling;
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
using BackendSite.Service.Model.CustVipLevel;
using BackendSite.Service.Model.TpMaintain;
using BackendSite.Service.Model.BankFinancial;

namespace BackendSite.Controllers.ApiV2.SystemFunc
{
    [Authorize]
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
        private readonly AdminGroupBLL _adminGroupBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly PrizeBLL _prizeBll;
        private readonly PaymentManageBLL _paymentManageBll;
        private readonly ReasonManagementBLL _reasonManagementBll;
        private readonly TpMaintainBLL _tpMaintainBll;
        private readonly BankFinancialBLL _bankFinancialBll;
        private readonly WithdrawalBLL _withdrawalBll;

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
            _adminGroupBll = provider.GetRequiredService<AdminGroupBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _prizeBll = provider.GetRequiredService<PrizeBLL>();
            _paymentManageBll = provider.GetRequiredService<PaymentManageBLL>();
            _reasonManagementBll = provider.GetRequiredService<ReasonManagementBLL>();
            _tpMaintainBll = provider.GetRequiredService<TpMaintainBLL>();
            _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
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
        ///  GetAllData/GetGroupList 取得所有群組權限清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > User Setting > Permission > Group > [Edit]
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<AdminGroupModel>> GetGroupList()
        {
            var result = new List<AdminGroupModel>();
            var data = _adminGroupBll.GetGroupList(1, 1000, out _, out _);

            foreach (var item in data)
            {
                result.Add(new AdminGroupModel()
                {
                    GroupId = (int)item.GroupSEQ,
                    GroupName = item.GroupName,
                    Remark = item.Remark
                });
            }

            var apiResult = new ApiResult<IEnumerable<AdminGroupModel>>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiResult;
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
        /// Func Path：Finance > Adjustment Log Report > DropDownList[Menu]
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<AdjustmentInfoLogMenu>> GetAdjustmentInfoLogMenu()
        {
            var apiResult = new ApiResult<IEnumerable<AdjustmentInfoLogMenu>>
            {
                Data = _adjustmentLogReportBll.GetAdjustmentInfoLogMenu(),
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
        public ApiResult<IEnumerable<PayList>> GetPayList()
        {
            var data = _paymentManageBll.GetPayList();

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<PayList>>
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
        public ApiResult<IEnumerable<PayChannel>> GetPayChannel()
        {
            var data = _paymentManageBll.GetPayChannel();

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<PayChannel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// GetAllData/GetPayCurrency 取得第三方支付廠商幣別資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<PayCurrency>> GetPayCurrency()
        {
            var data = _paymentManageBll.GetPayCurrency();

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<PayCurrency>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// GetAllData/GetTransferAccountList 取得轉帳戶口清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult GetTransferAccountList()
        {
            var apiResult = new ApiResult<IEnumerable<BankFinancialTransferAccountDetailModel>>();

            apiResult.Data = _bankFinancialBll.GetTransferAccountList();
            foreach (var data in apiResult.Data.ToArray())
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }

            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
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
            var data = await _gameCasinoBll.GetTpList();

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<RiskTpListInfo>>
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
            var data = _tpMaintainBll.GetCommonTpList();

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
            var data = _winLostReportBll.GetTpGameType();

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

            var result = _withdrawalBll.GetCryptoInfoData();

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

        #endregion

    }
}
