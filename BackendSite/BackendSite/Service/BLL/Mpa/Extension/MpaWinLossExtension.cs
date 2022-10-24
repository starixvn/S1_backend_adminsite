using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa;

namespace BackendSite.Service.BLL.Mpa.Extension
{
    public static class MpaWinLossExtension
    {
        public static IEnumerable<MpaTpBetRecordByDateDto> Filter(this IEnumerable<MpaTpBetRecordByDateDto> data, MpaWinLossRequest request, Func<int, string, int> getCustIdMethod)
        {

            // 過濾 GameType、CurrencyId
            data = data.Where(r => request.TpGameTypes.Contains(r.GameType) &&
                                   request.CurrencyIds.Contains(r.CurrencyId));

            // 過濾 UserName
            if (string.IsNullOrEmpty(request.UserName)) return data;
            {
                var custId = getCustIdMethod(request.SiteId, request.UserName);
                data = data.Where(r => r.CustId == custId);
            }

            return data;
        }

        public static IEnumerable<MpaTpBetRecordByDateDto> SetGgRByOneGamingPoker(this IEnumerable<MpaTpBetRecordByDateDto> data)
        {
            foreach (var item in data)
            {
                if (item.GameType == (int)MpaGGRCustomizationGameTypeEnum.OneGamingPoker)
                {
                    // For p2p products (OneGaming Poker), show commission: GGR = sum(revenue) 
                    item.WinLossSummary = item.RevenueSummary;
                }

            }

            return data;
        }

        public static IEnumerable<MpaTpBetRecordByDateDto> SetGgRByV8Poker(this IEnumerable<MpaTpBetRecordByDateDto> data)
        {
            foreach (var item in data)
            {
                if (item.GameType == (int)MpaGGRCustomizationGameTypeEnum.V8Poker)
                {
                    /// For p2p products (V8 Poker), show commission: GGR = sum(revenue) 
                    item.WinLossSummary = item.RevenueSummary;
                }

            }

            return data;
        }

        public static IEnumerable<MpaTpBetRecordByDateDto> SetCommissionByNineW(this IEnumerable<MpaTpBetRecordByDateDto> data,
            MpaWinLossRequest request,
            string format,
            Func<DateTime, DateTime, string, MpaDateType, IEnumerable<Commission9WDto>> getCommission)
        {
            var commission = getCommission(request.StarTime, request.EndTime, format, request.DateType);

            foreach (var item in data)
            {
                if (item.TpId == (int)MpaCommissionCustomizationTpIdEnum.NineW)
                {
                    item.RevenueSummary = commission.Where(r => r.Date == item.Date && r.CustId == item.CustId)
                        .Select(x => x.Commission).FirstOrDefault();
                }
            }

            return data;
        }
    }
}
