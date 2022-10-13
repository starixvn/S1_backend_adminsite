using Amazon.Runtime.Internal.Transform;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa;
using System.Linq;
using BackendSite.Extensions;

namespace BackendSite.Service.BLL
{
    public class MpaWinLossBLL
    {
        private readonly IMpaWinLossService _mpaWinLossService;

        public MpaWinLossBLL(IMpaWinLossService mpaWinLossService)
        {
            this._mpaWinLossService = mpaWinLossService;
        }

        public async Task<Dictionary<string, MpaWinLossByDateResponse>> GetMapWinLossByDateResult(MpaWinLossByDataRequest request)
        {
            var responseResult = new Dictionary<string, MpaWinLossByDateResponse>();

            // 注單資料
            var tpBetRecordResult = await GetTpBetRecordResult(request);

            // 存提款 
            var transactionResult = await GetTransactionResult(request);

            // 注單 + 存提款資料合併
            var combineData = tpBetRecordResult
                .Concat(transactionResult)
                .GroupBy(x => x.Date);

            foreach (var byDay in combineData)
            {
                foreach (var dayItem in byDay)
                {
                    // 組合 (注單 + 存提款)的幣別 Detail
                    var dayCurrencyDetail = ComposeByDate_CurrencyDetail(dayItem);

                    // 組合結果 + day Summary
                    ComposeByDate_ResultItem(responseResult, byDay, dayItem, dayCurrencyDetail);
                }
            }

            return responseResult;
        }

        private static void ComposeByDate_ResultItem(Dictionary<string, MpaWinLossByDateResponse> responseResult,
            IGrouping<string, MpaWinLossByDateDto> byDay,
            MpaWinLossByDateDto dayItem,
            Dictionary<string, MpaWinLossByDayCurrencyDetail> dayCurrencyDetail)
        {
            if (responseResult.ContainsKey(byDay.Key))
            {
                var temp = responseResult[byDay.Key];
                temp.TurnOver += dayItem.TurnOver;
                temp.ValidTurnOver += dayItem.ValidTurnOver;
                temp.GGR += dayItem.GGR;
                temp.PlayerWinLoss += dayItem.PlayerWinLoss;
                temp.BetCount += dayItem.BetCount;
                temp.AvgBetAmount += dayItem.AvgBetAmount;
                temp.UniquePlayers += dayItem.UniquePlayers;
                temp.Margin += dayItem.Margin;
                temp.DepositCount += dayItem.DepositCount;
                temp.TotalDepositAmount += dayItem.TotalDepositAmount;
                temp.WithDrawCount += dayItem.WithDrawCount;
                temp.TotalWithDrawAmount += dayItem.TotalWithDrawAmount;
                temp.CurrencyDetail = dayCurrencyDetail;
            }
            else
            {
                responseResult[byDay.Key] = new MpaWinLossByDateResponse()
                {
                    TurnOver = dayItem.TurnOver,
                    ValidTurnOver = dayItem.ValidTurnOver,
                    GGR = dayItem.GGR,
                    PlayerWinLoss = dayItem.PlayerWinLoss,
                    BetCount = dayItem.BetCount,
                    AvgBetAmount = dayItem.AvgBetAmount,
                    UniquePlayers = dayItem.UniquePlayers,
                    Margin = dayItem.Margin,
                    DepositCount = dayItem.DepositCount,
                    TotalDepositAmount = dayItem.TotalDepositAmount,
                    WithDrawCount = dayItem.WithDrawCount,
                    TotalWithDrawAmount = dayItem.TotalWithDrawAmount,
                    CurrencyDetail = dayCurrencyDetail
                };
            }
        }

        /// <summary>
        /// 組合 CurrencyDetail
        /// </summary>
        /// <param name="dayItem"></param>
        /// <returns></returns>
        private static Dictionary<string, MpaWinLossByDayCurrencyDetail> ComposeByDate_CurrencyDetail(MpaWinLossByDateDto dayItem)
        {
            var dayCurrencyDetail = new Dictionary<string, MpaWinLossByDayCurrencyDetail>();
            foreach (var currencyDetailKeyPair in dayItem.CurrencyDetail)
            {
                if (dayCurrencyDetail.ContainsKey(currencyDetailKeyPair.Currency))
                {
                    var temp = dayCurrencyDetail[currencyDetailKeyPair.Currency];
                    temp.TurnOver += currencyDetailKeyPair.TurnOver;
                    temp.ValidTurnOver += currencyDetailKeyPair.ValidTurnOver;
                    temp.GGR += currencyDetailKeyPair.GGR;
                    temp.PlayerWinLoss += currencyDetailKeyPair.PlayerWinLoss;
                    temp.BetCount += currencyDetailKeyPair.BetCount;
                    temp.AvgBetAmount += currencyDetailKeyPair.AvgBetAmount;
                    temp.UniquePlayers += currencyDetailKeyPair.UniquePlayers;
                    temp.Margin += currencyDetailKeyPair.Margin;
                    temp.DepositCount += currencyDetailKeyPair.DepositCount;
                    temp.TotalDepositAmount += currencyDetailKeyPair.TotalDepositAmount;
                    temp.WithDrawCount += currencyDetailKeyPair.WithDrawCount;
                    temp.TotalWithDrawAmount += currencyDetailKeyPair.TotalWithDrawAmount;
                }
                else
                {
                    dayCurrencyDetail[currencyDetailKeyPair.Currency] = new MpaWinLossByDayCurrencyDetail()
                    {
                        TurnOver = currencyDetailKeyPair.TurnOver,
                        ValidTurnOver = currencyDetailKeyPair.ValidTurnOver,
                        GGR = currencyDetailKeyPair.GGR,
                        PlayerWinLoss = currencyDetailKeyPair.PlayerWinLoss,
                        BetCount = currencyDetailKeyPair.BetCount,
                        AvgBetAmount = currencyDetailKeyPair.AvgBetAmount,
                        UniquePlayers = currencyDetailKeyPair.UniquePlayers,
                        Margin = currencyDetailKeyPair.Margin,
                        DepositCount = currencyDetailKeyPair.DepositCount,
                        TotalDepositAmount = currencyDetailKeyPair.TotalDepositAmount,
                        WithDrawCount = currencyDetailKeyPair.WithDrawCount,
                        TotalWithDrawAmount = currencyDetailKeyPair.TotalWithDrawAmount
                    };
                }
            }

            return dayCurrencyDetail;
        }

        #region TpBetRecodr

        /// <summary>
        /// 取得注單結果
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<IEnumerable<MpaWinLossByDateDto>> GetTpBetRecordResult(MpaWinLossByDataRequest request)
        {
            var tpBetRecordResult = new List<MpaWinLossByDateDto>();

            // 注單 Todo Redis + 過濾查詢條件
            var tpBetRecordRelayResult = await _mpaWinLossService.GetMapTpBetRecordByDate<MpaTpBetRecordByDateDto>(
                request.SiteId,
                request.SettledStarTime, request.SettledEndTime, "%Y-%m-%d");

            var currencyMap = await GetCurrencyIsoCodeMapper();
            var rateMap = await GetRateMapper();

            foreach (var byDateResult in tpBetRecordRelayResult.GroupBy(r => r.Date)) // by日期
            {
                var currencyDetail = GetTpBetRecordCurrencyDetail(byDateResult, currencyMap, rateMap);
                var daySummary = GetTpBetRecordCurrencySummary(currencyDetail);

                tpBetRecordResult.Add(new MpaWinLossByDateDto()
                {
                    Date = byDateResult.Key.ToString("yyyy-MM-dd"),
                    TurnOver = daySummary.TurnOver,
                    ValidTurnOver = daySummary.ValidTurnOver,
                    GGR = daySummary.GGR,
                    PlayerWinLoss = daySummary.PlayerWinLoss,
                    BetCount = daySummary.BetCount,
                    AvgBetAmount = daySummary.AvgBetAmount,
                    Margin = daySummary.Margin,
                    UniquePlayers = daySummary.UniquePlayers,
                    CurrencyDetail = currencyDetail
                });
            }

            return tpBetRecordResult;
        }

        /// <summary>
        /// 取得注單每日每個幣別的Detail
        /// </summary>
        /// <param name="byDateResult"></param>
        /// <param name="currencyMap"></param>
        /// <param name="rateMap"></param>
        /// <returns></returns>
        private static List<MpaWinLossByDayCurrencyDetail> GetTpBetRecordCurrencyDetail(IGrouping<DateTime, MpaTpBetRecordByDateDto> byDateResult,
        Dictionary<int, string> currencyMap,
        Dictionary<int, decimal> rateMap)
        {
            // todo 確認計算小數點公式
            var currencyDetail = new List<MpaWinLossByDayCurrencyDetail>();

            foreach (var byDateByCurrencyIds in byDateResult.GroupBy(x => x.CurrencyId)) // by 幣別，同個幣別一天會有多筆
            {
                var currencyKey = currencyMap[byDateByCurrencyIds.Key];
                var rate = rateMap[byDateByCurrencyIds.Key];
                var detail = new MpaWinLossByDayCurrencyDetail();
                var uniquePlayerCount = byDateByCurrencyIds.DistinctBy(x => x.CustId).Count();

                foreach (var byDateByCurrencyId in byDateByCurrencyIds)
                {
                    detail.TurnOver += (byDateByCurrencyId.StakeSummary / rate).ToDecimal2();
                    detail.ValidTurnOver += (byDateByCurrencyId.ValidBetSummary / rate).ToDecimal2();
                    detail.PlayerWinLoss += (byDateByCurrencyId.WinLossSummary / rate).ToDecimal2();
                    detail.BetCount += byDateByCurrencyId.RowCount;
                }

                detail.GGR = (detail.PlayerWinLoss * -1).ToDecimal2();
                detail.AvgBetAmount = detail.BetCount != 0 ?(detail.ValidTurnOver / detail.BetCount).ToDecimal2() : 0.00m;
                detail.Margin = detail.GGR != 0 ? (detail.GGR / detail.ValidTurnOver).ToDecimal2() : 0.00m;
                detail.UniquePlayers = uniquePlayerCount;
                detail.Currency = currencyKey;
                detail.Date = byDateResult.Key.ToString("yyyy-MM-dd");
                currencyDetail.Add(detail);
            }

            return currencyDetail;
        }

        /// <summary>
        /// 取得注單每日不分幣別總和
        /// </summary>
        /// <param name="currencyDetail"></param>
        /// <returns></returns>
        private static MpaWinLossByDayCurrencySummary GetTpBetRecordCurrencySummary(
            IEnumerable<MpaWinLossByDayCurrencyDetail> currencyDetail)
        {
            // by day Summary 當日加總
            var daySummary = new MpaWinLossByDayCurrencySummary();
            foreach (var detail in currencyDetail)
            {
                daySummary.TurnOver += detail.TurnOver;
                daySummary.ValidTurnOver += detail.ValidTurnOver;
                daySummary.GGR += detail.GGR;
                daySummary.PlayerWinLoss += detail.PlayerWinLoss;
                daySummary.BetCount += detail.BetCount;
                daySummary.AvgBetAmount += detail.AvgBetAmount;
                daySummary.Margin += detail.Margin;
                daySummary.UniquePlayers += detail.UniquePlayers;
            }

            return daySummary;
        }

        #endregion

        #region Transaction

        /// <summary>
        /// 取得 (充值單/提款單) 結果
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<IEnumerable<MpaWinLossByDateDto>> GetTransactionResult(MpaWinLossByDataRequest request)
        {
            // Todo Redis
            var transactionRelayResult = (await _mpaWinLossService.GetMapTransactionByDate<TransactionRelayDto>(request.SiteId,
                request.SettledStarTime, request.SettledEndTime, "%Y-%m-%d")).ToList();

            var currencyMap = await GetCurrencyIsoCodeMapper();
            var rateMap = await GetRateMapper();

            var transactionResult = new List<MpaWinLossByDateDto>();
            foreach (var transactionByDate in transactionRelayResult.GroupBy(r => r.Date))
            {
                var currencyDetail = GetTransactionCurrencyDetail(transactionByDate, currencyMap, rateMap);
                var daySummary = GetTransactionCurrencySummary(currencyDetail);

                transactionResult.Add(new MpaWinLossByDateDto()
                {
                    Date = transactionByDate.Key.ToString("yyyy-MM-dd"),
                    DepositCount = daySummary.DepositCount,
                    TotalDepositAmount = daySummary.TotalDepositAmount,
                    WithDrawCount = daySummary.WithDrawCount,
                    TotalWithDrawAmount = daySummary.TotalWithDrawAmount,
                    CurrencyDetail = currencyDetail
                });
            }

            return transactionResult;
        }

        /// <summary>
        /// 取得 (充值單/提款單) 每日每個幣別的Detail
        /// </summary>
        /// <param name="transactionByDate"></param>
        /// <param name="currencyMap"></param>
        /// <param name="rateMap"></param>
        /// <returns></returns>
        private static List<MpaWinLossByDayCurrencyDetail> GetTransactionCurrencyDetail(IGrouping<DateTime, TransactionRelayDto> transactionByDate,
            Dictionary<int, string> currencyMap,
            Dictionary<int, decimal> rateMap)
        {
            var currencyDetail = new List<MpaWinLossByDayCurrencyDetail>();
            foreach (var byDateByCurrencyIds in transactionByDate.GroupBy(x => x.CurrencyId))
            {
                var currencyKey = currencyMap[byDateByCurrencyIds.Key];
                var rate = rateMap[byDateByCurrencyIds.Key];
                var detail = new MpaWinLossByDayCurrencyDetail()
                {
                    Date = transactionByDate.Key.ToString("yyyy-MM-dd"),
                    Currency = currencyKey,
                };

                foreach (var byDateByCurrencyId in byDateByCurrencyIds)
                {
                    if (byDateByCurrencyId.SummaryType == TransactionRelaySummaryTypeEnum.Deposit)
                    {
                        detail.DepositCount += byDateByCurrencyId.RowCount;
                        detail.TotalDepositAmount += (byDateByCurrencyId.Amount / rate).ToDecimal2();
                    }
                    else
                    {
                        detail.WithDrawCount += byDateByCurrencyId.RowCount;
                        detail.TotalWithDrawAmount += (byDateByCurrencyId.Amount / rate).ToDecimal2();
                    }
                }
                currencyDetail.Add(detail);
            }

            return currencyDetail;
        }

        /// <summary>
        /// 取得 (充值單/提款單) 每日不分幣別總和
        /// </summary>
        /// <param name="currencyDetail"></param>
        /// <returns></returns>
        private static MpaWinLossByDayCurrencySummary GetTransactionCurrencySummary(List<MpaWinLossByDayCurrencyDetail> currencyDetail)
        {
            // by day Summary 當日加總
            var daySummary = new MpaWinLossByDayCurrencySummary();
            foreach (var detail in currencyDetail)
            {
                daySummary.DepositCount += detail.DepositCount;
                daySummary.TotalDepositAmount += detail.TotalDepositAmount;
                daySummary.WithDrawCount += detail.WithDrawCount;
                daySummary.TotalWithDrawAmount += detail.TotalWithDrawAmount;
            }

            return daySummary;
        }

        #endregion

        #region Common
        /// <summary>
        /// 取得幣別匯率
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<int, decimal>> GetRateMapper()
        {
            // todo redis
            var usdRateResult = await _mpaWinLossService.GetMpaUsdRate();
            return usdRateResult
                .ToDictionary(k => k.CurrencyId, v => v.USDRate);
        }

        /// <summary>
        /// 取得 Currency 的 IsoCode
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<int, string>> GetCurrencyIsoCodeMapper()
        {
            // todo redis
            var usdRateResult = await _mpaWinLossService.GetMpaUsdRate();
            return usdRateResult
                .ToDictionary(k => k.CurrencyId, v => v.IsoCode);
        }

        #endregion
    }
}
