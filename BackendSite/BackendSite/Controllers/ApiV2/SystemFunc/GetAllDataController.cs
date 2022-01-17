using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BackendSite.Controllers.Api.AdminUserController;
using BackendSite.Extensions;
using BackendSite.Service.Model.CustVipLevel;

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
        private readonly BonusManageBLL _bonusManageBll;
        private readonly RebateManageBLL _rebateManageBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly PrizeBLL _prizeBll;

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
            _bonusManageBll = provider.GetRequiredService<BonusManageBLL>();
            _rebateManageBll = provider.GetRequiredService<RebateManageBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _prizeBll = provider.GetRequiredService<PrizeBLL>();
        }

        #endregion

        #region Actions

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
        /// GetAllData/GetTpList 取得Slot第三方品牌
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Show Slot Tp List
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<RiskTpListInfo>>> GetTpList()
        {
            var apiResult = new ApiResult<IEnumerable<RiskTpListInfo>>();
            try
            {
                {
                    apiResult.Data = await _gameCasinoBll.GetTpList();
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                };
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
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
            var apiResult = new ApiResult<IEnumerable<TpCategoryBetLimit>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
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
        public ApiResult GetTpGameType()
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
                    ErrorCode = 0
                };
            }
        }

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
        ///  GetAllData/GetGroupList 取得所有群組權限清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > User Setting > Permission > Group > [Edit]
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<AdminGroup>> GetGroupList()
        {
            var apiResult = new ApiResult<IEnumerable<AdminGroup>>
            {
                Data = _adminGroupBll.GetGroupList(1, 1000, out _, out _),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiResult;
        }

        //[HttpPost]
        //public ApiResult<IEnumerable<TpGameType>> GetTpGameList()
        //{
        //    var apiResult = new ApiResult<IEnumerable<TpGameType>>();

        //    try
        //    {
        //        apiResult.Data = _bonusManageBll.GetTpGameList();
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
        //        apiResult.RawMessage = ex.ToString();
        //    }

        //    return apiResult;
        //}

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

        #endregion

    }
}
