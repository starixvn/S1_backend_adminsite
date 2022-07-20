using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Store.PointSetting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Store.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class PointSettingController : ControllerBase
    {
        private readonly PointSettingBLL _pointSettingBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        private readonly UserBetLimitBLL _userBetLimitBll;
        private readonly VipLevelBLL _vipLevelBll;

        public PointSettingController(IServiceProvider provider)
        {
            _pointSettingBll = provider.GetRequiredService<PointSettingBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
            _userBetLimitBll = provider.GetRequiredService<UserBetLimitBLL>();
            _vipLevelBll = provider.GetRequiredService<VipLevelBLL>();
        }

        [HttpPost]
        public Task<ApiResult<InactivePointExpirySetting>> GetInactivePointExpirySettingAsync(GetInactivePointExpirySettingRequest request)
        {
            var result = _pointSettingBll.GetInactivePointExpirySettingAsync(request);

            return result;
        }

        [HttpPost]
        public async Task<ApiResult> SetInactivePointExpirySettingAsync(SetInactivePointExpirySettingRequest request)
        {
            var user = User.Identity.Name;
            var before = await _pointSettingBll.GetInactivePointExpirySettingAsync(
                new GetInactivePointExpirySettingRequest
                {
                    SiteId = request.SiteId
                });
            var result = await _pointSettingBll.SetInactivePointExpirySettingAsync(request, user);

            if (result.ErrorCode == (int) ErrorCodeEnum.Success)
            {
                var vip = _vipLevelBll.GetVipLevelSetting(request.SiteId);
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = user,
                    SiteId = request.SiteId,
                    CurrencyId = 0,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.InactivePointExpirySetting,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        before.Data.Days,
                        Status = before.Data.Status.ToString()
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        request.Days,
                        Status = request.Status.ToString()
                    })
                });
            }
            return result;
        }

        [HttpPost]
        public Task<ApiResult<object>> GetTurnoverPointSettingListAsync(GetTurnoverPointSettingListRequest request)
        {
            var result = _pointSettingBll.GetTurnoverPointSettingListAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult<IEnumerable<TurnoverPointSetting>>> GetTurnoverPointSettingAsync(GetTurnoverPointSettingRequest request)
        {
            var result = _pointSettingBll.GetTurnoverPointSettingAsync(request);

            return result;
        }

        [HttpPost]
        public async Task<ApiResult> SetTurnoverPointSettingAsync(SetTurnoverPointSettingRequest request)
        {
            var user = User.Identity.Name;
            var before = await _pointSettingBll.GetTurnoverPointSettingAsync(
                new GetTurnoverPointSettingRequest
                {
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    VipLevel = request.Infos.First().VipLevel
                });
            var result = await _pointSettingBll.SetTurnoverPointSettingAsync(request, user);
            
            if (result.ErrorCode == (int) ErrorCodeEnum.Success)
            {
                var vip = _vipLevelBll.GetVipLevelSetting(request.SiteId);
                var vipLevel = vip.First(x => x.Level == request.Infos.First().VipLevel).LevelName;
                var beforeData = before.Data.Select(x => new TurnoverPointSettingForLog(x, vipLevel));
                var afterData = request.Infos
                    .Select(x => new TurnoverPointSettingForLog
                    {
                        TpCategory = before.Data.First(y => y.TpCategoryId == x.TpCategoryId).TpCategoryName,
                        Turnover = x.Turnover,
                        PromotionStatus = x.PromotionStatus.ToString(),
                        PromotionPointRate = x.PromotionPointRate,
                        PromotionStartTime = x.PromotionStartTime,
                        PromotionEndTime = x.PromotionEndTime
                    });
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = user,
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.TurnoverRewardPointSetting,
                    BeforeDetailInfo = JsonConvert.SerializeObject(beforeData),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        VipLevel = vip.First(x => x.Level == request.Infos.First().VipLevel).LevelName,
                        Infos = afterData
                    })
                });
            }
            
            return result;
        }

        [HttpPost]
        public Task<ApiResult<IEnumerable<DepositPointSetting>>> GetDepositPointSettingAsync(GetDepositPointSettingRequest request)
        {
            var result = _pointSettingBll.GetDepositPointSettingAsync(request);

            return result;
        }

        [HttpPost]
        public async Task<ApiResult> SetDepositPointSettingAsync(SetDepositPointSettingRequest request)
        {
            var user = User.Identity.Name;
            var before = await _pointSettingBll.GetDepositPointSettingAsync(
                new GetDepositPointSettingRequest
                {
                    SiteId = request.SiteId
                });
            var result = await _pointSettingBll.SetDepositPointSettingAsync(request, user);

            if (result.ErrorCode == (int) ErrorCodeEnum.Success)
            {
                var b = before.Data.FirstOrDefault(x => x.CurrencyId == request.CurrencyId);
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = user,
                    SiteId = b.SiteId,
                    CurrencyId = b.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.DepositRewardPointSetting,
                    BeforeDetailInfo = b == null ? string.Empty: JsonConvert.SerializeObject(new
                    {
                        b.DepositAmount,
                        PromotionStatus = b.PromotionStatus.ToString(),
                        b.PromotionPointRate,
                        b.PromotionStartTime,
                        b.PromotionEndTime
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        request.DepositAmount,
                        PromotionStatus = request.PromotionStatus.ToString(),
                        request.PromotionPointRate,
                        request.PromotionStartTime,
                        request.PromotionEndTime
                    })
                });
            }
            
            return result;
        }

        [HttpPost]
        public Task<ApiResult<IEnumerable<TurnoverPointStatusSetting>>> GetTurnoverPointStatusSettingAsync(GetTurnoverPointStatusSettingRequest request)
        {
            var result = _pointSettingBll.GetTurnoverPointStatusSettingAsync(request);

            return result;
        }

        [HttpPost]
        public async Task<ApiResult> SetTurnoverPointStatusSettingAsync(SetTurnoverPointStatusSettingRequest request)
        {
            var user = User.Identity.Name;
            var before = await _pointSettingBll.GetTurnoverPointStatusSettingAsync(
                new GetTurnoverPointStatusSettingRequest
                {
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    TpCategoryId = request.TpCategoryId
                });
            var result = await _pointSettingBll.SetTurnoverPointStatusSettingAsync(request, user);

            if (result.ErrorCode == (int) ErrorCodeEnum.Success)
            {
                var typeList = await _userBetLimitBll.GetTpGameTypeListbyCategoryAsync(request.TpCategoryId);
                var beforeData = before.Data.Any() 
                    ? JsonConvert.SerializeObject(new
                    {
                        Enabled = string.Join(", ", before.Data
                            .Where(x => x.Status == GeneralStatusEnum.Enable)
                            .Select(x => x.TpGameTypeName))
                    })
                    : null;
                
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = user,
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.TurnoverRewardPointGameTypeSetting,
                    BeforeDetailInfo = beforeData,
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        Enabled = string.Join(", ", request.Infos
                            .Where(x => x.Status == GeneralStatusEnum.Enable) 
                            .Select(x => typeList.FirstOrDefault(y => y.TypeId == x.TpGameTypeId)?.TypeName))
                    })
                });
            }
            
            return result;
        }
    }
}