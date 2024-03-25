using BackendSite.Service.Model.enums;

namespace BackendSite.Extensions
{
    public static class CommonExtension
    {
        private static IEnumerable<int> _canRewardPointSiteIds = new[]
        {
            1
        };
        private static IEnumerable<int> _canRewardPointCurrencyIds = new[]
        {
            (int) CurrencyEnum.MYR,
            (int) CurrencyEnum.THB,
            (int) CurrencyEnum.IDR
        };

        public static bool CanRewardPoint(int siteId, int currencyId)
            => _canRewardPointSiteIds.Contains(siteId)
               && _canRewardPointCurrencyIds.Contains(currencyId);
    }
}