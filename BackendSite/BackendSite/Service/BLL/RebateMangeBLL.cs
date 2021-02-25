using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class RebateMangeBLL
    {
        private readonly RebateMangeService rebateMangeService;

        public RebateMangeBLL(RebateMangeService rebateMangeService)
        {
            this.rebateMangeService = rebateMangeService;
        }
        public IEnumerable<VipLevel> GetVipLevel(int siteId)
        {
            return rebateMangeService.GetVipLevel(siteId);
        }
        public IEnumerable<TPCategory> GetTPCategoryList()
        {
            return rebateMangeService.GetTPCategoryList();
        }
        public IEnumerable<TpGameType> GetTpGameTypeListbyCategory(int categoryId)
        {
            return rebateMangeService.GetTpGameTypeListbyCategory(categoryId);
        }
        public IEnumerable<RebatePayoutLimitData> GetRebatePayoutLimit(int siteId, int currencyId, int? categoryId)
        {
            return rebateMangeService.GetRebatePayoutLimit( siteId, currencyId,  categoryId);
        }
        public int GetRebatePeriod(int siteId, out int periodType, out int periodDay, out int redeemHours)
        {
            return rebateMangeService.GetRebatePeriod(siteId, out periodType, out periodDay, out redeemHours);
        }
        public IEnumerable<RebateLevelData> GetRebateDataByCategory(int siteId, int categoryId, int currencyId)
        {
            return rebateMangeService.GetRebateDataByCategory(siteId, categoryId, currencyId);
        }
        public RebateCurrencyData GeRebateInfo(int siteId, int currencyId)
        {
            var reault = new RebateCurrencyData();
            reault.CurrencyId = currencyId;
            reault.PayoutLimitList = GetRebatePayoutLimit(siteId, currencyId, null);
            var rList= new List<RebateCategoryList>();
            foreach (var x in GetTPCategoryList())
            {
                rList.Add( new RebateCategoryList() { CategoryId = x.CategoryId, CategoryName = x.CategoryName, CategoryLevelList = GetRebateDataByCategory(siteId, x.CategoryId, currencyId)});
            }
            reault.CategoryDataList = rList;
            return reault;
        }
        public IEnumerable<RebateLevelData> GetRebateDataByGameType(int siteId, int gameType, int currencyId)
        {
            return rebateMangeService.GetRebateDataByGameType(siteId, gameType, currencyId);
        }
        public RebateCategoryDetail GeRebateCategoryDetail(int siteId, int currencyId, int categoryId)
        {
            var reault = new RebateCategoryDetail();
            reault.CategoryId = categoryId;

            var rList = new List<RebateGameTypeList>();
            foreach (var x in GetTpGameTypeListbyCategory(categoryId))
            {
                rList.Add(new RebateGameTypeList() { Type = x.Type, TypeName = x.TypeName, GameTypeLevelList = GetRebateDataByGameType(siteId, x.Type, currencyId) });
            }
            reault.GameTypeList = rList;
            return reault;
        }
    }
}
