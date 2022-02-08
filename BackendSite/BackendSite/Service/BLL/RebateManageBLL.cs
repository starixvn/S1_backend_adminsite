using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Service.BLL
{
    public class RebateManageBLL
    {
        private readonly RebateManageService rebateManageService;

        public RebateManageBLL(RebateManageService rebateManageService)
        {
            this.rebateManageService = rebateManageService;
        }
        //public IEnumerable<VipLevel> GetVipLevel(int siteId)
        //{
        //    return rebateManageService.GetVipLevel(siteId);
        //}
        public IEnumerable<TPCategory> GetTPCategoryList()
        {
            return rebateManageService.GetTPCategoryList();
        }
        public IEnumerable<TpGameType> GetTpGameTypeListByCategory(int categoryId)
        {
            return rebateManageService.GetTpGameTypeListByCategory(categoryId);
        }
        public IEnumerable<RebatePayoutLimitData> GetRebatePayoutLimit(int siteId, int currencyId, int? categoryId)
        {
            return rebateManageService.GetRebatePayoutLimit( siteId, currencyId,  categoryId);
        }
        public bool SetRebatePayoutLimit(int siteId, int currencyId, int categoryId, decimal maxPayout, decimal minPayout, string lastUpdateUser)
        {
            return rebateManageService.SetRebatePayoutLimit(siteId, currencyId, categoryId, maxPayout, minPayout, lastUpdateUser);
        }
        public bool SetRebatePayoutLimitBatch(int siteId, int currencyId, string levelPayout, string lastUpdateUser)
        {
            return rebateManageService.SetRebatePayoutLimitBatch(siteId, currencyId, levelPayout, lastUpdateUser);
        }
        public bool DelRebatePayoutLimit(int siteId, int currencyId, int categoryId, string lastUpdateUser)
        {
            return rebateManageService.DelRebatePayoutLimit( siteId, currencyId, categoryId, lastUpdateUser);
        }
        public int GetRebatePeriod(int siteId, out int periodType, out int periodDay, out int redeemHours)
        {
            return rebateManageService.GetRebatePeriod(siteId, out periodType, out periodDay, out redeemHours);
        }
        public bool SetRebatePeriod(int siteId, int periodType, int? periodDay, int redeemHours, string lastUpdateUser)
        {
            return rebateManageService.SetRebatePeriod(siteId, periodType, periodDay, redeemHours, lastUpdateUser);
        }
        public bool DelRebatePeriod(int siteId, string lastUpdateUser)
        {
            return rebateManageService.DelRebatePeriod(siteId, lastUpdateUser);
        }
        public IEnumerable<RebateLevelData> GetRebateDataByCategory(int siteId, int categoryId, int currencyId)
        {
            return rebateManageService.GetRebateDataByCategory(siteId, categoryId, currencyId);
        }
        public RebateCurrencyData GeRebateInfo(int siteId, int currencyId)
        {
            var result = new RebateCurrencyData
            {
                CurrencyId = currencyId,
                PayoutLimitList = GetRebatePayoutLimit(siteId, currencyId, null)
            };
            var rList = GetTPCategoryList().Select(x => new RebateCategoryList() {CategoryId = x.CategoryId, CategoryName = x.CategoryName, CategoryLevelList = GetRebateDataByCategory(siteId, x.CategoryId, currencyId)}).ToList();
            result.CategoryDataList = rList;
            return result;
        }
        public IEnumerable<RebateLevelData> GetRebateDataByGameType(int siteId, int gameType, int currencyId)
        {
            return rebateManageService.GetRebateDataByGameType(siteId, gameType, currencyId);
        }
        public RebateCategoryDetail GeRebateCategoryDetail(int siteId, int currencyId, int categoryId)
        {
            var reault = new RebateCategoryDetail
            {
                CategoryId = categoryId
            };

            var rList = new List<RebateGameTypeList>();
            foreach (var x in GetTpGameTypeListByCategory(categoryId))
            {
                rList.Add(new RebateGameTypeList() { Type = x.Type, TypeName = x.TypeName, GameTypeLevelList = GetRebateDataByGameType(siteId, x.Type, currencyId) });
            }
            reault.GameTypeList = rList;
            return reault;
        }
        public bool SetRebateCategory(int siteId, int currencyId, int categoryId, int level, decimal percentage, string lastUpdateUser)
        {
            return rebateManageService.SetRebateCategory( siteId, currencyId, categoryId, level, percentage, lastUpdateUser);
        }
        public bool SetRebateCategoryBatch(int siteId, int currencyId, int categoryId, string levelPercentage, string lastUpdateUser)
        {
            return rebateManageService.SetRebateCategoryBatch( siteId, currencyId, categoryId, levelPercentage, lastUpdateUser);
        }
        public bool DelRebateCategory(int siteId, int currencyId, int categoryId, int level, string lastUpdateUser)
        {
            return rebateManageService.DelRebateCategory( siteId, currencyId, categoryId, level, lastUpdateUser);
        }
        public bool SetRebateGameType(int siteId, int currencyId, int gameType, int level, decimal percentage, string lastUpdateUser)
        {
            return rebateManageService.SetRebateGameType(siteId, currencyId, gameType, level, percentage, lastUpdateUser);
        }
        public bool SetRebateGameTypeBatch(int siteId, int currencyId, int gameType, string levelPercentage, string lastUpdateUser)
        {
            return rebateManageService.SetRebateGameTypeBatch(siteId, currencyId, gameType, levelPercentage, lastUpdateUser);
        }
        public bool DelRebateGameType(int siteId, int currencyId, int gameType, int level, string lastUpdateUser)
        {
            return rebateManageService.DelRebateGameType(siteId, currencyId, gameType, level, lastUpdateUser);
        }
    }
}
