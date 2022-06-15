using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class RebateForVipController : ControllerBase
    {
        #region Fields

        private readonly RebateManageBLL _rebateManageBll;
        private readonly AuthorizeBLL _authorizeBll;

        #endregion

        #region Constructors

        public RebateForVipController(IServiceProvider provider)
        {
            _rebateManageBll = provider.GetRequiredService<RebateManageBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
        }

        #endregion

        #region API Model

        public class GetPeriodRequest
        {
            public int SiteId { get; set; }
        }

        public class SetPeriodRequest
        {
            public int SiteId { get; set; }
            public RebatePeriodPeriodTypeEnum PeriodType { get; set; }
            public int PeriodDay { get; set; }
            public int RedeemHours { get; set; }
        }

        public class GetRebateInfoRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        public class GetRebateInfoByGameTypeRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int GameType { get; set; }
        }

        public class SetCategoryRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CategoryId { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
        }

        public class SetGameTypeRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public IEnumerable<RebateGameTypeDetail> Detail { get; set; }
        }

        public class SetPayoutLimitRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CategoryId { get; set; }
            public decimal MaxPayout { get; set; }
            public decimal MinPayout { get; set; }
        }

        #endregion

        /// <summary>
        /// RebateForVip/GetPeriod 取得指定品牌返水週期資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Rebate Mangetment > Select [Available Sites]
        /// </remarks>
        /// <param name="request"></param>
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 49, 1 })]
        [HttpPost]
        public ApiResult<RebatePeriod> GetPeriod(GetPeriodRequest request)
        {
            return new ApiResult<RebatePeriod>
            {
                Data = _rebateManageBll.GetRebatePeriod(request.SiteId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// RebateForVip/GetRebateInfo 取得返水資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Rebate Mangetment > Select [Available Sites] > Select [Currency]
        /// </remarks>
        /// <param name="request"></param>
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 49, 1 })]
        [HttpPost]
        public ApiResult<RebateCurrencyData> GetRebateInfo(GetRebateInfoRequest request)
        {
            var apiResult = new ApiResult<RebateCurrencyData>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = _rebateManageBll.GeRebateInfo(request.SiteId, request.CurrencyId)
            };

            return apiResult;
        }

        /// <summary>
        /// RebateForVip/GetRebateInfoByGameType 取得指定遊戲類型返水資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Rebate Mangetment > Select [Available Sites] > Select [Currency] > Select [Category] > Select [Game Type]
        /// </remarks>
        /// <param name="request"></param>
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 49, 1 })]
        [HttpPost]
        public ApiResult<IEnumerable<RebateLevelData>> GetRebateInfoByGameType(GetRebateInfoByGameTypeRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<RebateLevelData>>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = _rebateManageBll.GetRebateDataByGameType(request.SiteId, request.GameType, request.CurrencyId)
            };

            return apiResult;
        }

        /// <summary>
        /// RebateForVip/SetPeriod 新增/更新指定品牌返水週期資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Rebate Mangetment > Select [Available Sites] > Period Type [Edit] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 49, 3 })]
        [HttpPost]
        public ApiResult SetPeriod(SetPeriodRequest request)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _rebateManageBll.SetRebatePeriod(request.SiteId, (int)request.PeriodType, request.PeriodDay, request.RedeemHours, userName);

            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
            }
            return apiResult;
        }

        /// <summary>
        /// RebateForVip/SetCategory 新增/更新指定遊戲分類返水資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Rebate Mangetment > Select [Available Sites]
        /// </remarks>
        /// <param name="request"></param>
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 49, 3 })]
        [HttpPost]
        public ApiResult SetCategory(SetCategoryRequest request)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _rebateManageBll.SetRebateCategory(request.SiteId, request.CurrencyId, request.CategoryId, request.Level, request.Percentage, userName);

            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
            }

            return apiResult;
        }

        /// <summary>
        /// RebateForVip/BatchSetGameType 批次新增/更新指定遊戲類型返水資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Rebate Mangetment > Select [Available Sites] > Select [Currency] > Select [Category] > Select [Game Type] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 49, 3 })]
        [HttpPost]
        public async Task<ApiResult> BatchSetGameTypeAsync(SetGameTypeRequest request)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = await _rebateManageBll.BatchSetRebateGameTypeAsync(request.SiteId, request.CurrencyId, request.Detail, userName);
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
            }

            return apiResult;
        }

        /// <summary>
        /// RebateForVip/SetPayoutLimit 新增/更新返水支付限額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Vip Rebate Mangetment > Select [Available Sites] > Select [Currency] > Select [Category] > Payout Setting [Edit] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 49, 3 })]
        [HttpPost]
        public ApiResult SetPayoutLimit(SetPayoutLimitRequest request)
        {
            var apiResult = new ApiResult();
            var userName = _authorizeBll.GetAuthData().UserName;
            var result = _rebateManageBll.SetRebatePayoutLimit(request.SiteId, request.CurrencyId, request.CategoryId, request.MaxPayout, request.MinPayout, userName);
            if (result)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
            }

            return apiResult;
        }

    }
}