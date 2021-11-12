using System.Collections.Generic;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RebateManageController : ControllerBase
    {
        private RebateManageBLL rebateManageBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private VipLevelBLL vipLevelBLL;

        public RebateManageController(RebateManageBLL rebateManageBLL, AuthorizeBLL authorizeBLL, VipLevelBLL vipLevelBLL)
        {
            this.rebateManageBLL = rebateManageBLL;
            this.authorizeBLL = authorizeBLL;
            this.vipLevelBLL = vipLevelBLL;
        }
        public class VipLevelReq
        {
            public int SiteId { get; set; }
        }
        public ApiResult<IEnumerable<VipLevelSetting>> GetVipLevel(VipLevelReq vipLevelReq)
        {
            var apiResult = new ApiResult<IEnumerable<VipLevelSetting>>
            {
                ErrorCode = 0,
                //apiResult.Data = rebateManageBLL.GetVipLevel(vipLevelReq.SiteId);
                Data = vipLevelBLL.GetVipLevelSetting(vipLevelReq.SiteId)
            };
            return apiResult;
        }
        public ApiResult<IEnumerable<TPCategory>> GetTPCategoryList()
        {
            var apiResult = new ApiResult<IEnumerable<TPCategory>>
            {
                ErrorCode = 0,
                Data = rebateManageBLL.GetTPCategoryList()
            };
            return apiResult;
        }
        public class GameTypeListReq
        {
            public int CategoryId { get; set; }
        }
        public ApiResult<IEnumerable<TpGameType>> GetTpGameTypeListbyCategory(GameTypeListReq gameTypeListReq)
        {
            var apiResult = new ApiResult<IEnumerable<TpGameType>>
            {
                ErrorCode = 0,
                Data = rebateManageBLL.GetTpGameTypeListbyCategory(gameTypeListReq.CategoryId)
            };
            return apiResult;
        }

        public class RebatePeriodReq
        {
            public int SiteId { get; set; }
        }
        public ApiResult<RebatePeriod> GetRebatePeriod(RebatePeriodReq rebatePeriodReq)
        {
            var apiResult = new ApiResult<RebatePeriod>
            {
                Data = new RebatePeriod()
            };
            var periodType = 0;
            var periodDay = 0;
            var redeemHours = 0;
            apiResult.ErrorCode = 0;
            rebateManageBLL.GetRebatePeriod(rebatePeriodReq.SiteId, out periodType, out periodDay, out redeemHours);
            apiResult.Data.PeriodType = periodType;
            apiResult.Data.PeriodDay = periodDay;
            apiResult.Data.RedeemHours = redeemHours;
            return apiResult;
        }

        public class SetRebatePeriodReq
        {
            public int SiteId { get; set; }
            public int PeriodType { get; set; }
            public int PeriodDay { get; set; }
            public int RedeemHours { get; set; }


        }
        public ApiResult SetRebatePeriod(SetRebatePeriodReq setRebatePeriodReq)
        {
            var apiResult = new ApiResult();
            var UserName = authorizeBLL.GetAuthData().UserName;
            apiResult.ErrorCode = 99;
            var result = rebateManageBLL.SetRebatePeriod(setRebatePeriodReq.SiteId, setRebatePeriodReq.PeriodType, setRebatePeriodReq.PeriodDay, setRebatePeriodReq.RedeemHours, UserName); ;
            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = -1;
            }
            return apiResult;
        }

        public class DelRebatePeriodReq
        {
            public int SiteId { get; set; }
        }
        public ApiResult DelRebatePeriod(DelRebatePeriodReq delRebatePeriodReq)
        {
            var apiResult = new ApiResult
            {
                ErrorCode = 99
            };
            var UserName = authorizeBLL.GetAuthData().UserName;
            var result = rebateManageBLL.DelRebatePeriod(delRebatePeriodReq.SiteId, UserName);
            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = -1;
            }
            return apiResult;
        }

        public class RebateInfoReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }
        public ApiResult<RebateCurrencyData> GetRebateInfo(RebateInfoReq rebateInfoReq)
        {
            var apiResult = new ApiResult<RebateCurrencyData>
            {
                ErrorCode = 0,
                Data = rebateManageBLL.GeRebateInfo(rebateInfoReq.SiteId, rebateInfoReq.CurrencyId)
            };
            return apiResult;
        }
        public class RebateInfoByGameTypeReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int GameType { get; set; }
        }
        public ApiResult<IEnumerable<RebateLevelData>> GetRebateInfoByGameType(RebateInfoByGameTypeReq rebateInfoByGameTypeReq)
        {
            var apiResult = new ApiResult<IEnumerable<RebateLevelData>>
            {
                ErrorCode = 0,
                Data = rebateManageBLL.GetRebateDataByGameType(rebateInfoByGameTypeReq.SiteId, rebateInfoByGameTypeReq.GameType, rebateInfoByGameTypeReq.CurrencyId)
            };
            return apiResult;
        }
        public class RebateCategoryDetailReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CategoryId { get; set; }
        }
        public ApiResult<RebateCategoryDetail> GeRebateCategoryDetail(RebateCategoryDetailReq rebateCategoryDetailReq)
        {
            var apiResult = new ApiResult<RebateCategoryDetail>
            {
                ErrorCode = 0,
                Data = rebateManageBLL.GeRebateCategoryDetail(rebateCategoryDetailReq.SiteId, rebateCategoryDetailReq.CurrencyId, rebateCategoryDetailReq.CategoryId)
            };
            return apiResult;
        }

        public class SetRebateCategoryReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CategoryId { get; set; }
            public int level { get; set; }
            public decimal percentage { get; set; }

        }
        public ApiResult SetRebateCategory(SetRebateCategoryReq setRebateCategoryReq)
        {
            var apiResult = new ApiResult
            {
                ErrorCode = 0
            };
            var UserName = authorizeBLL.GetAuthData().UserName;
            apiResult.ErrorCode = 99;
            var result = rebateManageBLL.SetRebateCategory(setRebateCategoryReq.SiteId, setRebateCategoryReq.CurrencyId, setRebateCategoryReq.CategoryId, setRebateCategoryReq.level, setRebateCategoryReq.percentage, UserName);
            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = -1;
            }
            return apiResult;
        }

        public class SetRebateCategoryBatchReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CategoryId { get; set; }
            public string levelPercentage { get; set; }

        }
        public ApiResult SetRebateCategoryBatch(SetRebateCategoryBatchReq setRebateCategoryBatchReq)
        {
            var apiResult = new ApiResult
            {
                ErrorCode = 0
            };
            var UserName = authorizeBLL.GetAuthData().UserName;
            apiResult.ErrorCode = 99;
            var result = rebateManageBLL.SetRebateCategoryBatch(setRebateCategoryBatchReq.SiteId, setRebateCategoryBatchReq.CurrencyId, setRebateCategoryBatchReq.CategoryId, setRebateCategoryBatchReq.levelPercentage, UserName);

            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = -1;
            }

            return apiResult;
        }
        public class DelRebateCategoryReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CategoryId { get; set; }
            public int level { get; set; }

        }
        public ApiResult DelRebateCategory(DelRebateCategoryReq delRebateCategoryReq)
        {
            var apiResult = new ApiResult
            {
                ErrorCode = 99
            };
            var UserName = authorizeBLL.GetAuthData().UserName;
            var result = rebateManageBLL.DelRebateCategory(delRebateCategoryReq.SiteId, delRebateCategoryReq.CurrencyId, delRebateCategoryReq.CategoryId, delRebateCategoryReq.level, UserName);
            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = -1;
            }
            return apiResult;
        }
        public class SetRebateGameTypeReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int GameType { get; set; }
            public int level { get; set; }
            public decimal percentage { get; set; }

        }
        public ApiResult SetRebateGameType(SetRebateGameTypeReq setRebateCategoryGameTypeReq)
        {
            var apiResult = new ApiResult();
            var UserName = authorizeBLL.GetAuthData().UserName;
            apiResult.ErrorCode = 99;
            var result = rebateManageBLL.SetRebateGameType(setRebateCategoryGameTypeReq.SiteId, setRebateCategoryGameTypeReq.CurrencyId, setRebateCategoryGameTypeReq.GameType, setRebateCategoryGameTypeReq.level, setRebateCategoryGameTypeReq.percentage, UserName);
            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = -1;
            }
            return apiResult;
        }

        public class SetRebateGameTypeBatchReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int GameType { get; set; }
            public string levelPercentage { get; set; }

        }
        public ApiResult SetRebateGameTypeBatch(SetRebateGameTypeBatchReq setRebateGameTypeBatchReq)
        {
            var apiResult = new ApiResult();
            var UserName = authorizeBLL.GetAuthData().UserName;
            apiResult.ErrorCode = 99;
            var result = rebateManageBLL.SetRebateGameTypeBatch(setRebateGameTypeBatchReq.SiteId, setRebateGameTypeBatchReq.CurrencyId, setRebateGameTypeBatchReq.GameType, setRebateGameTypeBatchReq.levelPercentage, UserName);
            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = -1;
            }
            return apiResult;
        }
        public class DelRebateGameTypeReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int GameType { get; set; }
            public int level { get; set; }

        }
        public ApiResult DelRebateGameType(DelRebateGameTypeReq delRebateGameTypeReq)
        {
            var apiResult = new ApiResult
            {
                ErrorCode = 99
            };
            var UserName = authorizeBLL.GetAuthData().UserName;
            var result = rebateManageBLL.DelRebateGameType(delRebateGameTypeReq.SiteId, delRebateGameTypeReq.CurrencyId, delRebateGameTypeReq.GameType, delRebateGameTypeReq.level, UserName);
            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = -1;
            }
            return apiResult;
        }
        public class SetRebatePayoutLimitReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CategoryId { get; set; }
            public decimal MaxPayout { get; set; }
            public decimal MinPayout { get; set; }

        }
        public ApiResult SetRebatePayoutLimit(SetRebatePayoutLimitReq setRebatePayoutLimitReq)
        {
            var apiResult = new ApiResult();
            var UserName = authorizeBLL.GetAuthData().UserName;
            apiResult.ErrorCode = 99;
            var result = rebateManageBLL.SetRebatePayoutLimit(setRebatePayoutLimitReq.SiteId, setRebatePayoutLimitReq.CurrencyId, setRebatePayoutLimitReq.CategoryId, setRebatePayoutLimitReq.MaxPayout, setRebatePayoutLimitReq.MinPayout, UserName);
            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = -1;
            }
            return apiResult;
        }
        public class SetRebatePayoutLimitBatchReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public string LevelPayout { get; set; }

        }
        public ApiResult SetRebatePayoutLimitBatch(SetRebatePayoutLimitBatchReq setRebatePayoutLimitBatchReq)
        {
            var apiResult = new ApiResult();
            var UserName = authorizeBLL.GetAuthData().UserName;
            apiResult.ErrorCode = 99;
            var result = rebateManageBLL.SetRebatePayoutLimitBatch(setRebatePayoutLimitBatchReq.SiteId, setRebatePayoutLimitBatchReq.CurrencyId, setRebatePayoutLimitBatchReq.LevelPayout, UserName);
            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = -1;
            }
            return apiResult;
        }
        public class DelRebatePayoutLimitReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CategoryId { get; set; }

        }
        public ApiResult DelRebatePayoutLimit(DelRebatePayoutLimitReq delRebatePayoutLimitReq)
        {
            var apiResult = new ApiResult
            {
                ErrorCode = 99
            };
            var UserName = authorizeBLL.GetAuthData().UserName;
            var result = rebateManageBLL.DelRebatePayoutLimit(delRebatePayoutLimitReq.SiteId, delRebatePayoutLimitReq.CurrencyId, delRebatePayoutLimitReq.CategoryId, UserName);
            if (result)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = -1;
            }
            return apiResult;
        }
    }
}
