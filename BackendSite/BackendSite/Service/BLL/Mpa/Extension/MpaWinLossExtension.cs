using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa;

namespace BackendSite.Service.BLL.Mpa.Extension
{
    public static class MpaWinLossExtension
    {
        public static IEnumerable<MpaTpBetRecordDto> Filter(this IEnumerable<MpaTpBetRecordDto> data, MpaWinLossRequest request)
        {

            // 過濾 GameType、CurrencyId
            data = data.Where(r => request.TpGameTypes.Contains(r.GameType) &&
                                   request.CurrencyIds.Contains(r.CurrencyId));

            // 過濾 UserName
            if (string.IsNullOrEmpty(request.UserName)) return data;
            {
                data = data.Where(r => r.UserName == request.UserName || r.TpUserName == request.UserName );
            }

            return data;
        }

        public static IEnumerable<MpaTpBetRecordDto> SetGgRByOneGamingPoker(this IEnumerable<MpaTpBetRecordDto> data)
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

        public static IEnumerable<MpaTpBetRecordDto> SetGgRByV8Poker(this IEnumerable<MpaTpBetRecordDto> data)
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

        public static IEnumerable<MpaTpBetRecordDto> SetCommissionByNineW(this IEnumerable<MpaTpBetRecordDto> data,
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
