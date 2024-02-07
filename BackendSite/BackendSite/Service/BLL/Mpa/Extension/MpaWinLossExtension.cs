using BackendSite.Extensions;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.Mpa.WinLoss;

namespace BackendSite.Service.BLL.Mpa.Extension
{
    public static class MpaWinLossExtension
    {
        #region ToExcelFormat

        public static IEnumerable<MpaWinLossGroupByModel> SortThen2ExcelData(this MpaWinLossReportResponse data, IOrderBy orderByOption)
        {
            OrderByMpaWinLossReportResponse(data, orderByOption);
        
            var exportResult = new List<MpaWinLossGroupByModel>();

            foreach (var item in data.Result)
            {
                var child = data.ChildResult.Where(x => x.Key == item.Key);

                exportResult.Add(item);
                exportResult.AddRange(child);
            }
            
            exportResult.Add(data.Total);
            
            return exportResult;
        }
        
        public static T ToExcelFormat<T>(this MpaWinLossGroupByModel data) where T : IMpaWinLossDisplay, new()
        {
            var result = ToExcelFormatSimple<T>(data);

            result.DepositCount = data.DepositCount;
            result.TotalDepositAmount = data.TotalDepositAmount.ToString("N2");
            result.WithDrawCount = data.WithdrawCount;
            result.TotalWithDrawAmount = data.TotalWithdrawAmount.ToString("N2");
            result.TotalBonus = data.TotalBonus.ToString("N2");

            return result;
        }

        public static T ToExcelFormatSimple<T>(this MpaWinLossGroupByModel data) where T : IMpaWinLossDisplaySimple, new()
            => new T
            {
                TurnOver = data.TurnOver.ToString("N2"),
                ValidTurnOver = data.ValidTurnOver.ToString("N2"),
                GGR = data.Ggr.ToString("N2"),
                PlayerWinLoss = data.PlayerWinLoss.ToString("N2"),
                BAPlayerWinLoss = data.BaPlayerWinLoss.ToString("N2"),
                ProgressiveWin = data.ProgressiveWin.ToString("N2"),
                ProgressiveBet = data.ProgressiveBet.ToString("N2"),
                Commission = data.Commission.ToString("N2"),
                BACommission = data.BaCommission.ToString("N2"),
                BetCount = data.BetCount,
                AvgBetAmount = data.AvgBetAmount.ToString("N2"),
                UniquePlayers = data.UniquePlayers,
                Margin = (data.Margin * 100).ToString("N2") + "%"
            };

        #endregion

        #region ToApiDataFormat

        public static IEnumerable<MpaWinLossResponseForFrontEnd> SortThen2ApiFormat(this MpaWinLossReportResponse source, IOrderBy orderByOption)
        {
            OrderByMpaWinLossReportResponse(source, orderByOption);
            
            var result = new List<MpaWinLossResponseForFrontEnd>();

            foreach (var item in source.Result)
            {
                var children = source.ChildResult.Where(x => x.Key == item.Key)
                    .Select(x => new MpaWinLossDetailForFrontEnd<MpaWinLoss>(new MpaWinLoss(x), x.Key));

                var data = new MpaWinLossResponseForFrontEnd(item, children);

                result.Add(data);
            }

            return result;
        }
        
        public static IEnumerable<MpaWinLossResponseForFrontEndSimple> SortThen2ApiFormatSimple(this MpaWinLossReportResponse source, IOrderBy orderByOption)
        {
            OrderByMpaWinLossReportResponse(source, orderByOption);

            var result = new List<MpaWinLossResponseForFrontEndSimple>();

            foreach (var item in source.Result)
            {
                var children = source.ChildResult.Where(x => x.Key == item.Key)
                    .Select(x => new MpaWinLossDetailForFrontEnd<MpaWinLossSimple>(new MpaWinLossSimple(x), x.Key));

                var data = new MpaWinLossResponseForFrontEndSimple(item, children);

                result.Add(data);
            }

            return result;
        }
        
        public static IEnumerable<MpaWinLossResponseForFrontEndWithPlayer> SortThen2ApiFormatWithPlayer(this MpaWinLossReportResponse source, IOrderBy orderByOption)
        {
            OrderByMpaWinLossReportResponse(source, orderByOption);
            
            var result = new List<MpaWinLossResponseForFrontEndWithPlayer>();

            foreach (var item in source.Result)
            {
                var children = source.ChildResult.Where(x => x.Key == item.Key)
                    .Select(x => new MpaWinLossDetailForFrontEnd<MpaWinLossWithPlayerInfo>(new MpaWinLossWithPlayerInfo(x), x.Key));

                var data = new MpaWinLossResponseForFrontEndWithPlayer(item, children);

                result.Add(data);
            }

            return result;
        }

        private static void OrderByMpaWinLossReportResponse(this MpaWinLossReportResponse source, IOrderBy orderByOption)
        {
            source.Result = source.Result.OrderBy(orderByOption);

            if (orderByOption.OrderByProperty.Equals("key"))
                orderByOption.OrderByProperty = "childKey";
            
            source.ChildResult = source.Result.OrderBy(orderByOption);
        }

        #endregion
    }
}