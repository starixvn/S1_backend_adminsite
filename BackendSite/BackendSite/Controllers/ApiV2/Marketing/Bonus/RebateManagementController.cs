using System;
using System.Collections.Generic;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class RebateManagementController : ControllerBase
    {
        #region Fields
        
        private readonly RebateManageBLL _rebateManageBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly VipLevelBLL _vipLevelBll;

        #endregion

        #region Constructors

        public RebateManagementController(IServiceProvider provider)
        {
            _rebateManageBll = provider.GetRequiredService<RebateManageBLL>();
            _vipLevelBll = provider.GetRequiredService<VipLevelBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
        }

        #endregion

        #region API Model

        public class VipLevelReq
        {
            public int SiteId { get; set; }
        }
        
        public class RebatePeriodReq
        {
            public int SiteId { get; set; }
        }

        public class SetRebatePeriodReq
        {
            public int SiteId { get; set; }
            public int PeriodType { get; set; }
            public int PeriodDay { get; set; }
            public int RedeemHours { get; set; }
        }
        
        public class RebateInfoReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }
        
        public class RebateInfoByGameTypeReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int GameType { get; set; }
        }
        
        public class SetRebateCategoryReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CategoryId { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
        }
        
        public class SetRebateGameTypeReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int GameType { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
        }
        
        public class SetRebatePayoutLimitReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CategoryId { get; set; }
            public decimal MaxPayout { get; set; }
            public decimal MinPayout { get; set; }
        }
        
        #endregion
        
        [HttpPost]
        public ApiResult<IEnumerable<VipLevelSetting>> GetVipLevel(VipLevelReq vipLevelReq)
        {
            var apiResult = new ApiResult<IEnumerable<VipLevelSetting>>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = _vipLevelBll.GetVipLevelSetting(vipLevelReq.SiteId)
            };
            return apiResult;
        }
        
        [HttpPost]
        public ApiResult<RebatePeriod> GetRebatePeriod(RebatePeriodReq rebatePeriodReq)
        {
            var apiResult = new ApiResult<RebatePeriod>
            {
                Data = new RebatePeriod()
            };
            
            _rebateManageBll.GetRebatePeriod(rebatePeriodReq.SiteId, out var periodType, out var periodDay, out var redeemHours);
            apiResult.Data.PeriodType = periodType;
            apiResult.Data.PeriodDay = periodDay;
            apiResult.Data.RedeemHours = redeemHours;
            apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            return apiResult;
        }
        
        [HttpPost]
        public ApiResult SetRebatePeriod(SetRebatePeriodReq setRebatePeriodReq)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _rebateManageBll.SetRebatePeriod(setRebatePeriodReq.SiteId, setRebatePeriodReq.PeriodType, setRebatePeriodReq.PeriodDay, setRebatePeriodReq.RedeemHours, userName); ;
            
            if (result)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
            }
            return apiResult;
        }
        
        [HttpPost]
        public ApiResult<RebateCurrencyData> GetRebateInfo(RebateInfoReq rebateInfoReq)
        {
            var apiResult = new ApiResult<RebateCurrencyData>
            {
                ErrorCode = (int) ErrorCodeEnum.Success,
                Data = _rebateManageBll.GeRebateInfo(rebateInfoReq.SiteId, rebateInfoReq.CurrencyId)
            };
            
            return apiResult;
        }
        
        [HttpPost]
        public ApiResult<IEnumerable<RebateLevelData>> GetRebateInfoByGameType(RebateInfoByGameTypeReq rebateInfoByGameTypeReq)
        {
            var apiResult = new ApiResult<IEnumerable<RebateLevelData>>
            {
                ErrorCode = (int) ErrorCodeEnum.Success,
                Data = _rebateManageBll.GetRebateDataByGameType(rebateInfoByGameTypeReq.SiteId, rebateInfoByGameTypeReq.GameType, rebateInfoByGameTypeReq.CurrencyId)
            };
            
            return apiResult;
        }
        
        [HttpPost]
        public ApiResult SetRebateCategory(SetRebateCategoryReq setRebateCategoryReq)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _rebateManageBll.SetRebateCategory(setRebateCategoryReq.SiteId, setRebateCategoryReq.CurrencyId, setRebateCategoryReq.CategoryId, setRebateCategoryReq.Level, setRebateCategoryReq.Percentage, userName);
           
            if (result)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
            }
            
            return apiResult;
        }
        
        [HttpPost]
        public ApiResult SetRebateGameType(SetRebateGameTypeReq setRebateCategoryGameTypeReq)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _rebateManageBll.SetRebateGameType(setRebateCategoryGameTypeReq.SiteId, setRebateCategoryGameTypeReq.CurrencyId, setRebateCategoryGameTypeReq.GameType, setRebateCategoryGameTypeReq.Level, setRebateCategoryGameTypeReq.Percentage, userName);
            if (result)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
            }
            
            return apiResult;
        }
        
        [HttpPost]
        public ApiResult SetRebatePayoutLimit(SetRebatePayoutLimitReq setRebatePayoutLimitReq)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _rebateManageBll.SetRebatePayoutLimit(setRebatePayoutLimitReq.SiteId, setRebatePayoutLimitReq.CurrencyId, setRebatePayoutLimitReq.CategoryId, setRebatePayoutLimitReq.MaxPayout, setRebatePayoutLimitReq.MinPayout, userName);
            if (result)
            {
                apiResult.ErrorCode =(int) ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
            }
            
            return apiResult;
        }
    }
}