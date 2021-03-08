using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RebateMangeController : ControllerBase
    {
        private RebateMangeBLL rebateMangeBLL;
        public RebateMangeController(RebateMangeBLL rebateMangeBLL)
        {
            this.rebateMangeBLL = rebateMangeBLL;

        }
        public class VipLevelReq
        {
            public int SiteId { get; set; }
        }
        public ApiResult<IEnumerable<VipLevel>> GetVipLevel(VipLevelReq vipLevelReq)
        {
            var apiResult = new ApiResult<IEnumerable<VipLevel>>();
            apiResult.ErrorCode = 0;
            apiResult.Data = rebateMangeBLL.GetVipLevel(vipLevelReq.SiteId);
            return apiResult;
        }
        public ApiResult<IEnumerable<TPCategory>> GetTPCategoryList()
        {
            var apiResult = new ApiResult<IEnumerable<TPCategory>>();
            apiResult.ErrorCode = 0;
            apiResult.Data = rebateMangeBLL.GetTPCategoryList();
            return apiResult;
        }
        public class GameTypeListReq
        {
            public int CategoryId { get; set; }
        }
        public ApiResult<IEnumerable<TpGameType>> GetTpGameTypeListbyCategory(GameTypeListReq gameTypeListReq)
        {
            var apiResult = new ApiResult<IEnumerable<TpGameType>>();
            apiResult.ErrorCode = 0;
            apiResult.Data = rebateMangeBLL.GetTpGameTypeListbyCategory(gameTypeListReq.CategoryId);
            return apiResult;
        }

        public class RebatePeriodReq
        {
            public int SiteId { get; set; }
        }
        public ApiResult<RebatePeriod> GetRebatePeriod(RebatePeriodReq rebatePeriodReq)
        {
            var apiResult = new ApiResult<RebatePeriod>();
            apiResult.Data = new RebatePeriod();
            int periodType = 0;
            int periodDay = 0;
            int redeemHours = 0;
            apiResult.ErrorCode = 0;
            rebateMangeBLL.GetRebatePeriod(rebatePeriodReq.SiteId, out periodType, out periodDay, out redeemHours);
            apiResult.Data.PeriodType = periodType;
            apiResult.Data.PeriodDay = periodDay;
            apiResult.Data.RedeemHours = redeemHours;
            return apiResult;
        }

        public class RebateInfoReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }
        public ApiResult<RebateCurrencyData> GetRebateInfo(RebateInfoReq rebateInfoReq)
        {
            var apiResult = new ApiResult<RebateCurrencyData>();
            apiResult.ErrorCode = 0;
            apiResult.Data = rebateMangeBLL.GeRebateInfo(rebateInfoReq.SiteId, rebateInfoReq.CurrencyId);
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
            var apiResult = new ApiResult<IEnumerable<RebateLevelData>>();
            apiResult.ErrorCode = 0;
            apiResult.Data = rebateMangeBLL.GetRebateDataByGameType(rebateInfoByGameTypeReq.SiteId, rebateInfoByGameTypeReq.GameType, rebateInfoByGameTypeReq.CurrencyId);
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
            var apiResult = new ApiResult<RebateCategoryDetail>();
            apiResult.ErrorCode = 0;
            apiResult.Data = rebateMangeBLL.GeRebateCategoryDetail(rebateCategoryDetailReq.SiteId, rebateCategoryDetailReq.CurrencyId, rebateCategoryDetailReq.CategoryId);
            return apiResult;
        }

        public class SetRebateCategoryReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CategoryId { get; set; }
            public int level { get; set; }
            public decimal percentage { get; set; }
            public string lastUpdateUser { get; set; }

        }
        public ApiResult  SetRebateCategory(SetRebateCategoryReq setRebateCategoryReq)
        {
            var apiResult = new ApiResult ();
            apiResult.ErrorCode = 0;
            rebateMangeBLL.SetRebateCategory(setRebateCategoryReq.SiteId, setRebateCategoryReq.CurrencyId, setRebateCategoryReq.CategoryId, setRebateCategoryReq.level, setRebateCategoryReq.percentage, setRebateCategoryReq.lastUpdateUser);

            return apiResult;
        }

    }
}
