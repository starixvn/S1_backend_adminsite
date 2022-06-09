using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Service.BLL
{
    public class RebateManageBLL
    {
        private readonly RebateManageService _rebateManageService;
        private readonly ServerInfoBLL _serverInfoBll;

        public RebateManageBLL(IServiceProvider provider)
        {
            _rebateManageService = provider.GetRequiredService<RebateManageService>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }
        //public IEnumerable<VipLevel> GetVipLevel(int siteId)
        //{
        //    return rebateManageService.GetVipLevel(siteId);
        //}
        public IEnumerable<TPCategory> GetTPCategoryList()
        {
            return _rebateManageService.GetTPCategoryList();
        }
        public IEnumerable<TpGameType> GetTpGameTypeListByCategory(int categoryId)
        {
            return _rebateManageService.GetTpGameTypeListByCategory(categoryId);
        }
        public IEnumerable<RebatePayoutLimitData> GetRebatePayoutLimit(int siteId, int currencyId, int? categoryId)
        {
            return _rebateManageService.GetRebatePayoutLimit(siteId, currencyId, categoryId);
        }
        public bool SetRebatePayoutLimit(int siteId, int currencyId, int categoryId, decimal maxPayout, decimal minPayout, string lastUpdateUser)
        {
            return _rebateManageService.SetRebatePayoutLimit(siteId, currencyId, categoryId, maxPayout, minPayout, lastUpdateUser);
        }
        public bool SetRebatePayoutLimitBatch(int siteId, int currencyId, string levelPayout, string lastUpdateUser)
        {
            return _rebateManageService.SetRebatePayoutLimitBatch(siteId, currencyId, levelPayout, lastUpdateUser);
        }
        public bool DelRebatePayoutLimit(int siteId, int currencyId, int categoryId, string lastUpdateUser)
        {
            return _rebateManageService.DelRebatePayoutLimit(siteId, currencyId, categoryId, lastUpdateUser);
        }
        public RebatePeriod GetRebatePeriod(int siteId)
        {
            return _rebateManageService.GetRebatePeriod(siteId).FirstOrDefault();
        }
        public bool SetRebatePeriod(int siteId, int periodType, int? periodDay, int redeemHours, string lastUpdateUser)
        {
            var siteData = _serverInfoBll.GetSiteList().Where(x => x.SiteId == siteId).FirstOrDefault();
            return _rebateManageService.SetRebatePeriod(siteId, siteData.ICurrencyList, periodType, periodDay, redeemHours, lastUpdateUser);
        }
        public bool DelRebatePeriod(int siteId, string lastUpdateUser)
        {
            return _rebateManageService.DelRebatePeriod(siteId, lastUpdateUser);
        }
        public IEnumerable<RebateLevelData> GetRebateDataByCategory(int siteId, int categoryId, int currencyId)
        {
            return _rebateManageService.GetRebateDataByCategory(siteId, categoryId, currencyId);
        }
        public RebateCurrencyData GeRebateInfo(int siteId, int currencyId)
        {
            var result = new RebateCurrencyData
            {
                CurrencyId = currencyId,
                PayoutLimitList = GetRebatePayoutLimit(siteId, currencyId, null)
            };
            var rList = GetTPCategoryList().Select(x => new RebateCategoryList() { CategoryId = x.CategoryId, CategoryName = x.CategoryName, CategoryLevelList = GetRebateDataByCategory(siteId, x.CategoryId, currencyId) }).ToList();
            result.CategoryDataList = rList;
            return result;
        }
        public IEnumerable<RebateLevelData> GetRebateDataByGameType(int siteId, int gameType, int currencyId)
        {
            return _rebateManageService.GetRebateDataByGameType(siteId, gameType, currencyId);
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
            return _rebateManageService.SetRebateCategory(siteId, currencyId, categoryId, level, percentage, lastUpdateUser);
        }
        public bool SetRebateCategoryBatch(int siteId, int currencyId, int categoryId, string levelPercentage, string lastUpdateUser)
        {
            return _rebateManageService.SetRebateCategoryBatch(siteId, currencyId, categoryId, levelPercentage, lastUpdateUser);
        }
        public bool DelRebateCategory(int siteId, int currencyId, int categoryId, int level, string lastUpdateUser)
        {
            return _rebateManageService.DelRebateCategory(siteId, currencyId, categoryId, level, lastUpdateUser);
        }
        public bool SetRebateGameType(int siteId, int currencyId, int gameType, int level, decimal percentage, string lastUpdateUser)
        {
            return _rebateManageService.SetRebateGameType(siteId, currencyId, gameType, level, percentage, lastUpdateUser);
        }
        public bool SetRebateGameTypeBatch(int siteId, int currencyId, int gameType, string levelPercentage, string lastUpdateUser)
        {
            return _rebateManageService.SetRebateGameTypeBatch(siteId, currencyId, gameType, levelPercentage, lastUpdateUser);
        }
        public bool DelRebateGameType(int siteId, int currencyId, int gameType, int level, string lastUpdateUser)
        {
            return _rebateManageService.DelRebateGameType(siteId, currencyId, gameType, level, lastUpdateUser);
        }
    }
}
