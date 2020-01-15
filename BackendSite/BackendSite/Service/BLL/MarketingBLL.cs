using System.Collections.Generic;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class MarketingBLL
    {
        private readonly MarketingService marketingService;

        public MarketingBLL(MarketingService marketingService)
        {
            this.marketingService = marketingService;
        }

        public IEnumerable<PointLevelInfo> GetPointLevelInfos(int siteId, int currencyId) => marketingService.GetPointLevelInfos(siteId, currencyId);

        public bool AddPointLevel(PointLevelInfo pointLevelInfo)
        {
            if (pointLevelInfo.PointLevel > 1)
            {
                var currentMaxPoint = marketingService.GetPointLevelInfo(pointLevelInfo.SiteId, pointLevelInfo.CurrencyId, pointLevelInfo.PointLevel - 1);

                if (pointLevelInfo.BetLeast <= currentMaxPoint.BetLeast || pointLevelInfo.DepositLeast <= currentMaxPoint.DepositLeast)
                    return false;
            }
            else
            {
                if (pointLevelInfo.BetLeast <= 0 || pointLevelInfo.DepositLeast <= 0)
                    return false;
            }

            return marketingService.AddPointLevelInfo(pointLevelInfo);
        }

        public bool EditPointLevel(PointLevelInfo pointLevelInfo)
        {
            var currentBelowPoint = marketingService.GetPointLevelInfo(pointLevelInfo.SiteId, pointLevelInfo.CurrencyId, pointLevelInfo.PointLevel - 1);
            var currentUpperPoint = marketingService.GetPointLevelInfo(pointLevelInfo.SiteId, pointLevelInfo.CurrencyId, pointLevelInfo.PointLevel + 1);

            if (currentBelowPoint != null)
            {
                if (pointLevelInfo.BetLeast <= currentBelowPoint.BetLeast || pointLevelInfo.DepositLeast <= currentBelowPoint.DepositLeast)
                    return false;
            }
            else
            {
                if (pointLevelInfo.BetLeast <= 0 || pointLevelInfo.DepositLeast <= 0)
                    return false;
            }

            if (currentUpperPoint != null)
            {
                if (pointLevelInfo.BetLeast >= currentUpperPoint.BetLeast || pointLevelInfo.DepositLeast >= currentUpperPoint.DepositLeast)
                    return false;
            }

            return marketingService.EditPointLevelInfo(pointLevelInfo);
        }

        public int DeletePointLevel(PointLevelInfo pointLevelInfo)
        {
            var currentUpperPoint = marketingService.GetPointLevelInfo(pointLevelInfo.SiteId, pointLevelInfo.CurrencyId, pointLevelInfo.PointLevel + 1);

            if (currentUpperPoint != null)
                return 0;

            return marketingService.DeletePointLevelInfo(pointLevelInfo);
        }
    }
}