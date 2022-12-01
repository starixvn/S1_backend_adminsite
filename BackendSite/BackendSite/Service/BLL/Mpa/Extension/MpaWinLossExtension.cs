using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;

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
                data = data.Where(r => r.UserName == request.UserName || r.TpUserName == request.UserName);
            }

            if (!request.NoDraw)
            {
                //Win = 1,Lost = 2,Refund = 3,/*Reject, Refund, Void*/,Draw = 4,Related = 5,FreeBetWin = 6
                data = data.Where(x => x.TicketStatus != 4);
            }

            if (!request.IncludeFreeSpins)
            {
                //Win = 1,Lost = 2,Refund = 3,/*Reject, Refund, Void*/,Draw = 4,Related = 5,FreeBetWin = 6
                data = data.Where(x => x.TicketStatus != 6);
            }


            //For product has jackpot: GGR = - (Player WL – Progressive Win) – Progressive Bet
            foreach (var item in data)
            {
                item.WinLossSummary -= item.ProgressiveWinSummary;
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
            Func<DateTimeOffset, DateTimeOffset, string, MpaDateType, IEnumerable<Commission9WDto>> getCommission)
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

        public static IEnumerable<MpaWinLossByDateDto> ToExcelFormat(this List<MpaWinLossByDateDto> data)
        {
            foreach (var item in data)
            {
                item.TurnOver = Math.Round(item.TurnOver, 2);
                item.ValidTurnOver = Math.Round(item.ValidTurnOver, 2);
                item.GGR = Math.Round(item.GGR, 2);
                item.PlayerWinLoss = Math.Round(item.PlayerWinLoss, 2);
                item.ProgressiveWin = Math.Round(item.ProgressiveWin, 2);
                item.ProgressiveBet = Math.Round(item.ProgressiveBet, 2);
                item.Commission = Math.Round(item.Commission, 2);
                item.AvgBetAmount = Math.Round(item.AvgBetAmount, 2);
                item.Margin = Math.Round(item.Margin, 2);
                item.TotalDepositAmount = Math.Round(item.TotalDepositAmount, 2);
                item.TotalWithDrawAmount = Math.Round(item.TotalWithDrawAmount, 2);
                item.TotalBonus = Math.Round(item.TotalBonus, 2);
            }

            return data;
        }

        public static IEnumerable<MpaWinLossByTpGameTypeDto> ToExcelFormat(this List<MpaWinLossByTpGameTypeDto> data)
        {
            foreach (var item in data)
            {
                item.TurnOver = Math.Round(item.TurnOver, 2);
                item.ValidTurnOver = Math.Round(item.ValidTurnOver, 2);
                item.GGR = Math.Round(item.GGR, 2);
                item.PlayerWinLoss = Math.Round(item.PlayerWinLoss, 2);
                item.ProgressiveWin = Math.Round(item.ProgressiveWin, 2);
                item.ProgressiveBet = Math.Round(item.ProgressiveBet, 2);
                item.Commission = Math.Round(item.Commission, 2);
                item.AvgBetAmount = Math.Round(item.AvgBetAmount, 2);
                item.Margin = Math.Round(item.Margin, 2);
            }

            return data;
        }

        public static IEnumerable<MpaWinLossByTpCategoryDto> ToExcelFormat(this List<MpaWinLossByTpCategoryDto> data)
        {
            foreach (var item in data)
            {
                item.TurnOver = Math.Round(item.TurnOver, 2);
                item.ValidTurnOver = Math.Round(item.ValidTurnOver, 2);
                item.GGR = Math.Round(item.GGR, 2);
                item.PlayerWinLoss = Math.Round(item.PlayerWinLoss, 2);
                item.ProgressiveWin = Math.Round(item.ProgressiveWin, 2);
                item.ProgressiveBet = Math.Round(item.ProgressiveBet, 2);
                item.Commission = Math.Round(item.Commission, 2);
                item.AvgBetAmount = Math.Round(item.AvgBetAmount, 2);
                item.Margin = Math.Round(item.Margin, 2);
            }

            return data;
        }

        public static IEnumerable<MpaWinLossByPlayerDto> ToExcelFormat(this List<MpaWinLossByPlayerDto> data)
        {
            foreach (var item in data)
            {
                item.TurnOver = Math.Round(item.TurnOver, 2);
                item.ValidTurnOver = Math.Round(item.ValidTurnOver, 2);
                item.GGR = Math.Round(item.GGR, 2);
                item.PlayerWinLoss = Math.Round(item.PlayerWinLoss, 2);
                item.ProgressiveWin = Math.Round(item.ProgressiveWin, 2);
                item.ProgressiveBet = Math.Round(item.ProgressiveBet, 2);
                item.Commission = Math.Round(item.Commission, 2);
                item.AvgBetAmount = Math.Round(item.AvgBetAmount, 2);
                item.Margin = Math.Round(item.Margin, 2);
                item.ClosingBalance = Math.Round(item.ClosingBalance, 2);
                item.TotalDepositAmount = Math.Round(item.TotalDepositAmount, 2);
                item.TotalWithDrawAmount = Math.Round(item.TotalWithDrawAmount, 2);
                item.TotalBonus = Math.Round(item.TotalBonus, 2);
            }

            return data;
        }

        public static IEnumerable<MpaWinLossByCurrencyDto> ToExcelFormat(this List<MpaWinLossByCurrencyDto> data)
        {
            foreach (var item in data)
            {
                item.TurnOver = Math.Round(item.TurnOver, 2);
                item.ValidTurnOver = Math.Round(item.ValidTurnOver, 2);
                item.GGR = Math.Round(item.GGR, 2);
                item.PlayerWinLoss = Math.Round(item.PlayerWinLoss, 2);
                item.ProgressiveWin = Math.Round(item.ProgressiveWin, 2);
                item.ProgressiveBet = Math.Round(item.ProgressiveBet, 2);
                item.Commission = Math.Round(item.Commission, 2);
                item.AvgBetAmount = Math.Round(item.AvgBetAmount, 2);
                item.Margin = Math.Round(item.Margin, 2);
                item.TotalDepositAmount = Math.Round(item.TotalDepositAmount, 2);
                item.TotalWithDrawAmount = Math.Round(item.TotalWithDrawAmount, 2);
                item.TotalBonus = Math.Round(item.TotalBonus, 2);
            }

            return data;
        }
    }
}
