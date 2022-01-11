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
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RebateForVipController : ControllerBase
    {
        #region Fields
        
        private readonly RebateManageBLL _rebateManageBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly VipLevelBLL _vipLevelBll;

        #endregion

        #region Constructors

        public RebateForVipController(IServiceProvider provider)
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
        
        /// <summary>
        /// RebateManagement/GetVipLevel 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<IEnumerable<VipLevelSetting>> GetVipLevel(VipLevelReq req)
        {
            var apiResult = new ApiResult<IEnumerable<VipLevelSetting>>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = _vipLevelBll.GetVipLevelSetting(req.SiteId)
            };
            return apiResult;
        }
        
        /// <summary>
        /// RebateManagement/GetRebatePeriod 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<RebatePeriod> GetRebatePeriod(RebatePeriodReq req)
        {
            var apiResult = new ApiResult<RebatePeriod>
            {
                Data = new RebatePeriod()
            };
            
            _rebateManageBll.GetRebatePeriod(req.SiteId, out var periodType, out var periodDay, out var redeemHours);
            apiResult.Data.PeriodType = periodType;
            apiResult.Data.PeriodDay = periodDay;
            apiResult.Data.RedeemHours = redeemHours;
            apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            return apiResult;
        }
        
        /// <summary>
        /// RebateManagement/SetRebatePeriod 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult SetRebatePeriod(SetRebatePeriodReq req)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _rebateManageBll.SetRebatePeriod(req.SiteId, req.PeriodType, req.PeriodDay, req.RedeemHours, userName); ;
            
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
        
        /// <summary>
        /// RebateManagement/GetRebateInfo 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<RebateCurrencyData> GetRebateInfo(RebateInfoReq req)
        {
            var apiResult = new ApiResult<RebateCurrencyData>
            {
                ErrorCode = (int) ErrorCodeEnum.Success,
                Data = _rebateManageBll.GeRebateInfo(req.SiteId, req.CurrencyId)
            };
            
            return apiResult;
        }
        
        /// <summary>
        /// RebateManagement/GetRebateInfoByGameType 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult<IEnumerable<RebateLevelData>> GetRebateInfoByGameType(RebateInfoByGameTypeReq req)
        {
            var apiResult = new ApiResult<IEnumerable<RebateLevelData>>
            {
                ErrorCode = (int) ErrorCodeEnum.Success,
                Data = _rebateManageBll.GetRebateDataByGameType(req.SiteId, req.GameType, req.CurrencyId)
            };
            
            return apiResult;
        }
        
        /// <summary>
        /// RebateManagement/SetRebateCategory 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult SetRebateCategory(SetRebateCategoryReq req)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _rebateManageBll.SetRebateCategory(req.SiteId, req.CurrencyId, req.CategoryId, req.Level, req.Percentage, userName);
           
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
        
        /// <summary>
        /// RebateManagement/SetRebateGameType 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult SetRebateGameType(SetRebateGameTypeReq req)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _rebateManageBll.SetRebateGameType(req.SiteId, req.CurrencyId, req.GameType, req.Level, req.Percentage, userName);
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
        
        /// <summary>
        /// RebateManagement/SetRebatePayoutLimit 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult SetRebatePayoutLimit(SetRebatePayoutLimitReq req)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _rebateManageBll.SetRebatePayoutLimit(req.SiteId, req.CurrencyId, req.CategoryId, req.MaxPayout, req.MinPayout, userName);
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