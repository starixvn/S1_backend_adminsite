using Amazon.Runtime.Internal.Transform;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa;
using System.Linq;
using BackendSite.Extensions;
using BackendSite.Service.Model;
using BackendSite.Service.Library;
using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.BLL
{
    public class MpaWinLossBLL
    {
        private readonly IMpaWinLossService _mpaWinLossService;
        private readonly ICacheHelper _redisHelper;

        public MpaWinLossBLL(IMpaWinLossService mpaWinLossService, ICacheHelper redisHelper)
        {
            this._mpaWinLossService = mpaWinLossService;
            this._redisHelper = redisHelper;
        }

        #region Public BLL Method

        public async Task<Dictionary<string, MpaWinLossByDateResponse>> GetMapWinLossByDateResult(MpaWinLossByDataRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var responseResult = new Dictionary<string, MpaWinLossByDateResponse>();

            // 注單資料
            var tpBetRecordResult = await GetTpBetRecordByDateResult(request, groupByType);

            // 存提款 
            var transactionResult = await GetTransactionResult(request, groupByType);

            // 注單 + 存提款資料合併
            var combineData = request.OrderBy
                ? tpBetRecordResult
                    .Concat(transactionResult)
                    .OrderBy(request)
                    .GroupBy(x => x.Date)
                : tpBetRecordResult
                    .Concat(transactionResult)
                    .GroupBy(x => x.Date);

            foreach (var byDay in combineData)
            {
                foreach (var dayItem in byDay)
                {
                    // 組合 (注單 + 存提款)的幣別 Detail
                    var dayCurrencyDetail = Compose_CurrencyDetail_ByDate(dayItem);

                    // 組合結果 + day Summary
                    ComposeByDate_ResultItem(responseResult, byDay, dayItem, dayCurrencyDetail);
                }
            }

            // total
            responseResult.Add("total", new MpaWinLossByDateResponse()
            {
                TurnOver = responseResult.Sum(x => x.Value.TurnOver),
                ValidTurnOver = responseResult.Sum(x => x.Value.ValidTurnOver),
                GGR = responseResult.Sum(x => x.Value.GGR),
                PlayerWinLoss = responseResult.Sum(x => x.Value.PlayerWinLoss),
                BetCount = responseResult.Sum(x => x.Value.BetCount),
                AvgBetAmount = responseResult.Sum(x => x.Value.AvgBetAmount),
                UniquePlayers = responseResult.Sum(x => x.Value.UniquePlayers),
                Margin = responseResult.Sum(x => x.Value.Margin),
                DepositCount = responseResult.Sum(x => x.Value.DepositCount),
                TotalDepositAmount = responseResult.Sum(x => x.Value.TotalDepositAmount),
                WithDrawCount = responseResult.Sum(x => x.Value.WithDrawCount),
                TotalWithDrawAmount = responseResult.Sum(x => x.Value.TotalWithDrawAmount),
                TotalBonus = responseResult.Sum(x => x.Value.TotalBonus),
            });

            return responseResult;
        }

        public async Task<Dictionary<string, MpaWinLossByTpGameTypeResponse>> GetMapWinLossByTpGameTypeResult(MpaWinLossByDataRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var responseResult = new Dictionary<string, MpaWinLossByTpGameTypeResponse>();

            // 注單資料
            var tpBetRecordResult = await GetTpBetRecordByTpGameTypeResult(request, groupByType);

            var groupByResult = request.OrderBy
                ? tpBetRecordResult
                    .OrderBy(request)
                    .GroupBy(x => x.TpGameTypeName)
                : tpBetRecordResult
                    .GroupBy(x => x.TpGameTypeName);

            foreach (var byGameType in groupByResult)
            {
                foreach (var gameTypeItem in byGameType)
                {
                    //(注單) 的幣別 Detail
                    var dayCurrencyDetail = Compose_CurrencyDetail_ByTpGameType(gameTypeItem);

                    // 組合結果 + day Summary
                    ComposeByTpGameType_ResultItem(responseResult, byGameType, gameTypeItem, dayCurrencyDetail);
                }
            }

            // total
            responseResult.Add("total", new MpaWinLossByTpGameTypeResponse()
            {
                TurnOver = responseResult.Sum(x => x.Value.TurnOver),
                ValidTurnOver = responseResult.Sum(x => x.Value.ValidTurnOver),
                GGR = responseResult.Sum(x => x.Value.GGR),
                PlayerWinLoss = responseResult.Sum(x => x.Value.PlayerWinLoss),
                BetCount = responseResult.Sum(x => x.Value.BetCount),
                AvgBetAmount = responseResult.Sum(x => x.Value.AvgBetAmount),
                UniquePlayers = responseResult.Sum(x => x.Value.UniquePlayers),
                Margin = responseResult.Sum(x => x.Value.Margin)
            });

            return responseResult;
        }

        #endregion

        #region TpBetRecodr

        /// <summary>
        /// 取得 GroupBy 日期、月份注單結果
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<IEnumerable<MpaWinLossByDateDto>> GetTpBetRecordByDateResult(MpaWinLossByDataRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var tpBetRecordResult = new List<MpaWinLossByDateDto>();

            // 注單過濾
            var tpBetRecordRelayResult = (await GetMapTpBetRecordByDate(request, groupByType))
                .Where(r => request.TpGameTypes.Contains(r.GameType) &&
                            request.CurrencyIds.Contains(r.CurrencyId));

            // 過濾過濾
            if (!string.IsNullOrEmpty(request.UserName))
            {
                var custId = await GetCustIdByUserName(request.SiteId, request.UserName);
                tpBetRecordRelayResult = tpBetRecordRelayResult.Where(r => r.CustId == custId);
            }

            var currencyMap = await GetCurrencyIsoCodeMapper();
            var rateMap = await GetRateMapper();

            foreach (var byDateResult in tpBetRecordRelayResult.GroupBy(r => r.Date)) // by日期
            {
                var currencyDetail = GetTpBetRecordCurrencyDetail(byDateResult, currencyMap, rateMap, groupByType);
                var daySummary = GetTpBetRecordCurrencySummary(currencyDetail);

                tpBetRecordResult.Add(new MpaWinLossByDateDto()
                {
                    Date = groupByType == MpaWinLossGroupByTypeEnum.Date
                        ? byDateResult.Key.ToString("yyyy-MM-dd")
                        : byDateResult.Key.ToString("yyyy-MM"),
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
        /// 取得 GroupBy TpGameType 注單結果
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<IEnumerable<MpaWinLossByTpGameTypeDto>> GetTpBetRecordByTpGameTypeResult(MpaWinLossByDataRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {

            var tpBetRecordResult = new List<MpaWinLossByTpGameTypeDto>();

            // 注單過濾
            var tpBetRecordRelayResult = (await GetMapTpBetRecordByDate(request, groupByType))
                .Where(r => request.TpGameTypes.Contains(r.GameType) &&
                            request.CurrencyIds.Contains(r.CurrencyId));

            // 過濾過濾
            if (!string.IsNullOrEmpty(request.UserName))
            {
                var custId = await GetCustIdByUserName(request.SiteId, request.UserName);
                tpBetRecordRelayResult = tpBetRecordRelayResult.Where(r => r.CustId == custId);
            }

            var currencyMap = await GetCurrencyIsoCodeMapper();
            var rateMap = await GetRateMapper();
            var tpGameTypeMap = await GetTpGameTypeMapper();

            var gtype = tpBetRecordRelayResult.GroupBy(r => r.GameType);

            foreach (var byDateResult in tpBetRecordRelayResult.GroupBy(r => r.GameType)) // by GameType
            {
                var currencyDetail = GetTpBetRecordCurrencyDetail(byDateResult, currencyMap, rateMap);
                var daySummary = GetTpBetRecordCurrencySummary(currencyDetail);

                tpBetRecordResult.Add(new MpaWinLossByTpGameTypeDto()
                {
                    TpGameTypeName = tpGameTypeMap[byDateResult.Key],
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
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private static List<MpaWinLossByDayCurrencyDetailDto> GetTpBetRecordCurrencyDetail(IGrouping<DateTime, MpaTpBetRecordByDateDto> byDateResult,
        Dictionary<int, string> currencyMap,
        Dictionary<int, decimal> rateMap,
        MpaWinLossGroupByTypeEnum groupByType)
        {
            // todo 確認計算小數點公式
            var currencyDetail = new List<MpaWinLossByDayCurrencyDetailDto>();

            foreach (var byDateByCurrencyIds in byDateResult.GroupBy(x => x.CurrencyId)) // by 幣別，同個幣別一天會有多筆
            {
                var currencyKey = currencyMap[byDateByCurrencyIds.Key];
                var rate = rateMap[byDateByCurrencyIds.Key];
                var detail = new MpaWinLossByDayCurrencyDetailDto();
                var uniquePlayerCount = byDateByCurrencyIds.DistinctBy(x => x.CustId).Count();

                foreach (var byDateByCurrencyId in byDateByCurrencyIds)
                {
                    detail.TurnOver += (byDateByCurrencyId.StakeSummary / rate).ToDecimal2();
                    detail.ValidTurnOver += (byDateByCurrencyId.ValidBetSummary / rate).ToDecimal2();
                    detail.PlayerWinLoss += (byDateByCurrencyId.WinLossSummary / rate).ToDecimal2();
                    detail.BetCount += byDateByCurrencyId.RowCount;
                }

                detail.GGR = (detail.PlayerWinLoss * -1).ToDecimal2();
                detail.AvgBetAmount = detail.BetCount != 0 ? (detail.TurnOver / detail.BetCount).ToDecimal2() : 0.00m;
                detail.Margin = detail.GGR != 0 ? (detail.GGR / detail.TurnOver).ToDecimal2() : 0.00m;
                detail.UniquePlayers = uniquePlayerCount;
                detail.Currency = currencyKey;
                detail.Date = groupByType == MpaWinLossGroupByTypeEnum.Date
                    ? byDateResult.Key.ToString("yyyy-MM-dd")
                    : byDateResult.Key.ToString("yyyy-MM");
                currencyDetail.Add(detail);
            }

            return currencyDetail;
        }

        private static List<MpaWinLossByDayCurrencyDetailDto> GetTpBetRecordCurrencyDetail(IGrouping<int, MpaTpBetRecordByDateDto> byDateResult,
        Dictionary<int, string> currencyMap,
        Dictionary<int, decimal> rateMap)
        {
            // todo 確認計算小數點公式
            var currencyDetail = new List<MpaWinLossByDayCurrencyDetailDto>();

            foreach (var byDateByCurrencyIds in byDateResult.GroupBy(x => x.CurrencyId)) // by 幣別，同個幣別一天會有多筆
            {
                var currencyKey = currencyMap[byDateByCurrencyIds.Key];
                var rate = rateMap[byDateByCurrencyIds.Key];
                var detail = new MpaWinLossByDayCurrencyDetailDto();
                var uniquePlayerCount = byDateByCurrencyIds.DistinctBy(x => x.CustId).Count();

                foreach (var byDateByCurrencyId in byDateByCurrencyIds)
                {
                    detail.TurnOver += (byDateByCurrencyId.StakeSummary / rate).ToDecimal2();
                    detail.ValidTurnOver += (byDateByCurrencyId.ValidBetSummary / rate).ToDecimal2();
                    detail.PlayerWinLoss += (byDateByCurrencyId.WinLossSummary / rate).ToDecimal2();
                    detail.BetCount += byDateByCurrencyId.RowCount;
                }

                detail.GGR = (detail.PlayerWinLoss * -1).ToDecimal2();
                detail.AvgBetAmount = detail.BetCount != 0 ? (detail.TurnOver / detail.BetCount).ToDecimal2() : 0.00m;
                detail.Margin = detail.GGR != 0 ? (detail.GGR / detail.TurnOver).ToDecimal2() : 0.00m;
                detail.UniquePlayers = uniquePlayerCount;
                detail.Currency = currencyKey;
                detail.GameType = byDateResult.Key;
                currencyDetail.Add(detail);
            }

            return currencyDetail;
        }

        /// <summary>
        /// 取得注單每日不分幣別總和
        /// </summary>
        /// <param name="currencyDetail"></param>
        /// <returns></returns>
        private static MpaWinLossByDayCurrencySummaryDto GetTpBetRecordCurrencySummary(
            IEnumerable<MpaWinLossByDayCurrencyDetailDto> currencyDetail)
        {
            // by day Summary 當日加總
            var daySummary = new MpaWinLossByDayCurrencySummaryDto();
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
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<IEnumerable<MpaWinLossByDateDto>> GetTransactionResult(MpaWinLossByDataRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var transactionRelayResult = (await GetMapTransactionByDate(request, groupByType))
                .Where(r => request.CurrencyIds.Contains(r.CurrencyId));

            if (!string.IsNullOrEmpty(request.UserName))
            {
                var custId = await GetCustIdByUserName(request.SiteId, request.UserName);
                transactionRelayResult = transactionRelayResult.Where(r => r.CustId == custId);
            }

            var currencyMap = await GetCurrencyIsoCodeMapper();
            var rateMap = await GetRateMapper();

            var transactionResult = new List<MpaWinLossByDateDto>();
            foreach (var transactionByDate in transactionRelayResult.GroupBy(r => r.Date))
            {
                var currencyDetail = GetTransactionCurrencyDetail(transactionByDate, currencyMap, rateMap, groupByType);
                var daySummary = GetTransactionCurrencySummary(currencyDetail);

                transactionResult.Add(new MpaWinLossByDateDto()
                {
                    Date = groupByType == MpaWinLossGroupByTypeEnum.Date
                        ? transactionByDate.Key.ToString("yyyy-MM-dd")
                        : transactionByDate.Key.ToString("yyyy-MM"),
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
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private static List<MpaWinLossByDayCurrencyDetailDto> GetTransactionCurrencyDetail(IGrouping<DateTime, TransactionRelayDto> transactionByDate,
            Dictionary<int, string> currencyMap,
            Dictionary<int, decimal> rateMap,
            MpaWinLossGroupByTypeEnum groupByType)
        {
            var currencyDetail = new List<MpaWinLossByDayCurrencyDetailDto>();
            foreach (var byDateByCurrencyIds in transactionByDate.GroupBy(x => x.CurrencyId))
            {
                var currencyKey = currencyMap[byDateByCurrencyIds.Key];
                var rate = rateMap[byDateByCurrencyIds.Key];
                var detail = new MpaWinLossByDayCurrencyDetailDto()
                {
                    Date = groupByType == MpaWinLossGroupByTypeEnum.Date
                    ? transactionByDate.Key.ToString("yyyy-MM-dd")
                    : transactionByDate.Key.ToString("yyyy-MM"),
                    Currency = currencyKey,
                };

                foreach (var byDateByCurrencyId in byDateByCurrencyIds)
                {
                    switch (byDateByCurrencyId.SummaryType)
                    {
                        case MpaTransactionRelaySummaryTypeEnum.Deposit:
                            detail.DepositCount += byDateByCurrencyId.RowCount;
                            detail.TotalDepositAmount += (byDateByCurrencyId.Amount / rate).ToDecimal2();
                            break;
                        case MpaTransactionRelaySummaryTypeEnum.Withdrawal:
                            detail.WithDrawCount += byDateByCurrencyId.RowCount;
                            detail.TotalWithDrawAmount += (byDateByCurrencyId.Amount / rate).ToDecimal2();
                            break;
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
        private static MpaWinLossByDayCurrencySummaryDto GetTransactionCurrencySummary(List<MpaWinLossByDayCurrencyDetailDto> currencyDetail)
        {
            // by day Summary 當日加總
            var daySummary = new MpaWinLossByDayCurrencySummaryDto();
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

        #region Compose 組合資料

        private static void ComposeByDate_ResultItem(Dictionary<string, MpaWinLossByDateResponse> responseResult,
           IGrouping<string, MpaWinLossByDateDto> byDay,
           MpaWinLossByDateDto dayItem,
           Dictionary<string, MpaWinLossByDayCurrencyDetailDto> dayCurrencyDetail)
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

                foreach (var currencyDetail in dayCurrencyDetail)
                {
                    if (temp.CurrencyDetail.ContainsKey(currencyDetail.Key))
                    {
                        var currencyItem = temp.CurrencyDetail[currencyDetail.Key];
                        currencyItem.TurnOver += currencyDetail.Value.TurnOver;
                        currencyItem.ValidTurnOver += currencyDetail.Value.ValidTurnOver;
                        currencyItem.GGR += currencyDetail.Value.GGR;
                        currencyItem.PlayerWinLoss += currencyDetail.Value.PlayerWinLoss;
                        currencyItem.BetCount += currencyDetail.Value.BetCount;
                        currencyItem.AvgBetAmount += currencyDetail.Value.AvgBetAmount;
                        currencyItem.UniquePlayers += currencyDetail.Value.UniquePlayers;
                        currencyItem.Margin += currencyDetail.Value.Margin;
                        currencyItem.DepositCount += currencyDetail.Value.DepositCount;
                        currencyItem.TotalDepositAmount += currencyDetail.Value.TotalDepositAmount;
                        currencyItem.WithDrawCount += currencyDetail.Value.WithDrawCount;
                        currencyItem.TotalWithDrawAmount += currencyDetail.Value.TotalWithDrawAmount;
                    }
                    else
                    {
                        temp.CurrencyDetail[currencyDetail.Key] = new MpaWinLossByDayCurrencyDetailDto()
                        {
                            TurnOver = currencyDetail.Value.TurnOver,
                            ValidTurnOver = currencyDetail.Value.ValidTurnOver,
                            GGR = currencyDetail.Value.GGR,
                            PlayerWinLoss = currencyDetail.Value.PlayerWinLoss,
                            BetCount = currencyDetail.Value.BetCount,
                            AvgBetAmount = currencyDetail.Value.AvgBetAmount,
                            UniquePlayers = currencyDetail.Value.UniquePlayers,
                            Margin = currencyDetail.Value.Margin,
                            DepositCount = currencyDetail.Value.DepositCount,
                            TotalDepositAmount = currencyDetail.Value.TotalDepositAmount,
                            WithDrawCount = currencyDetail.Value.WithDrawCount,
                            TotalWithDrawAmount = currencyDetail.Value.TotalWithDrawAmount,
                        };
                    }
                }
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

        private static Dictionary<string, MpaWinLossByDayCurrencyDetailDto> Compose_CurrencyDetail_ByDate(MpaWinLossByDateDto dayItem)
        {
            var dayCurrencyDetail = new Dictionary<string, MpaWinLossByDayCurrencyDetailDto>();
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
                    dayCurrencyDetail[currencyDetailKeyPair.Currency] = new MpaWinLossByDayCurrencyDetailDto()
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


        private static void ComposeByTpGameType_ResultItem(Dictionary<string, MpaWinLossByTpGameTypeResponse> responseResult,
          IGrouping<string, MpaWinLossByTpGameTypeDto> byGameType,
          MpaWinLossByTpGameTypeDto dayItem,
          Dictionary<string, MpaWinLossByTpGameTypeCurrencyDetailDto> dayCurrencyDetail)
        {
            if (responseResult.ContainsKey(byGameType.Key))
            {
                var temp = responseResult[byGameType.Key];
                temp.TurnOver += dayItem.TurnOver;
                temp.ValidTurnOver += dayItem.ValidTurnOver;
                temp.GGR += dayItem.GGR;
                temp.PlayerWinLoss += dayItem.PlayerWinLoss;
                temp.BetCount += dayItem.BetCount;
                temp.AvgBetAmount += dayItem.AvgBetAmount;
                temp.UniquePlayers += dayItem.UniquePlayers;
                temp.Margin += dayItem.Margin;


                foreach (var currencyDetail in dayCurrencyDetail)
                {
                    if (temp.CurrencyDetail.ContainsKey(currencyDetail.Key))
                    {
                        var currencyItem = temp.CurrencyDetail[currencyDetail.Key];
                        currencyItem.TurnOver += currencyDetail.Value.TurnOver;
                        currencyItem.ValidTurnOver += currencyDetail.Value.ValidTurnOver;
                        currencyItem.GGR += currencyDetail.Value.GGR;
                        currencyItem.PlayerWinLoss += currencyDetail.Value.PlayerWinLoss;
                        currencyItem.BetCount += currencyDetail.Value.BetCount;
                        currencyItem.AvgBetAmount += currencyDetail.Value.AvgBetAmount;
                        currencyItem.UniquePlayers += currencyDetail.Value.UniquePlayers;
                        currencyItem.Margin += currencyDetail.Value.Margin;
                    }
                    else
                    {
                        temp.CurrencyDetail[currencyDetail.Key] = new MpaWinLossByTpGameTypeCurrencyDetailDto()
                        {
                            TurnOver = currencyDetail.Value.TurnOver,
                            ValidTurnOver = currencyDetail.Value.ValidTurnOver,
                            GGR = currencyDetail.Value.GGR,
                            PlayerWinLoss = currencyDetail.Value.PlayerWinLoss,
                            BetCount = currencyDetail.Value.BetCount,
                            AvgBetAmount = currencyDetail.Value.AvgBetAmount,
                            UniquePlayers = currencyDetail.Value.UniquePlayers,
                            Margin = currencyDetail.Value.Margin,
                        };
                    }
                }
            }
            else
            {
                responseResult[byGameType.Key] = new MpaWinLossByTpGameTypeResponse()
                {
                    TurnOver = dayItem.TurnOver,
                    ValidTurnOver = dayItem.ValidTurnOver,
                    GGR = dayItem.GGR,
                    PlayerWinLoss = dayItem.PlayerWinLoss,
                    BetCount = dayItem.BetCount,
                    AvgBetAmount = dayItem.AvgBetAmount,
                    UniquePlayers = dayItem.UniquePlayers,
                    Margin = dayItem.Margin,
                    CurrencyDetail = dayCurrencyDetail
                };
            }
        }

        private static Dictionary<string, MpaWinLossByTpGameTypeCurrencyDetailDto> Compose_CurrencyDetail_ByTpGameType(MpaWinLossByTpGameTypeDto gameTypeItem)
        {
            var dayCurrencyDetail = new Dictionary<string, MpaWinLossByTpGameTypeCurrencyDetailDto>();
            foreach (var currencyDetailKeyPair in gameTypeItem.CurrencyDetail)
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
                }
                else
                {
                    dayCurrencyDetail[currencyDetailKeyPair.Currency] = new MpaWinLossByTpGameTypeCurrencyDetailDto()
                    {
                        TurnOver = currencyDetailKeyPair.TurnOver,
                        ValidTurnOver = currencyDetailKeyPair.ValidTurnOver,
                        GGR = currencyDetailKeyPair.GGR,
                        PlayerWinLoss = currencyDetailKeyPair.PlayerWinLoss,
                        BetCount = currencyDetailKeyPair.BetCount,
                        AvgBetAmount = currencyDetailKeyPair.AvgBetAmount,
                        UniquePlayers = currencyDetailKeyPair.UniquePlayers,
                        Margin = currencyDetailKeyPair.Margin,
                    };
                }
            }

            return dayCurrencyDetail;
        }

        #endregion

        #region Common
        /// <summary>
        /// 取得幣別匯率
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<int, decimal>> GetRateMapper()
        {
            var usdRateResult = await GetMpaUSdRate();
            return usdRateResult
                .ToDictionary(k => k.CurrencyId, v => v.USDRate);
        }

        /// <summary>
        /// 取得 Currency 的 IsoCode
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<int, string>> GetCurrencyIsoCodeMapper()
        {
            var usdRateResult = await GetMpaUSdRate();
            return usdRateResult
                .ToDictionary(k => k.CurrencyId, v => v.IsoCode);
        }

        /// <summary>
        /// 取得每個幣別資訊 + Usd 匯率
        /// </summary>
        /// <returns></returns>
        private Task<IEnumerable<MpaUSdRateDto>> GetMpaUSdRate()
        {
            const string redisKey = "MpaUsdRate";
            return _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromHours(12),
                () => _mpaWinLossService.GetMpaUsdRate());
        }


        public async Task<Dictionary<int, string>> GetTpGameTypeMapper()
        {
            const string redisKey = "MpaTpGameTypeMapper";
            var data = await _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromHours(12),
                () => _mpaWinLossService.GetTpGameTypeOption());

            return data.ToDictionary(x => x.Type, v => v.TypeName);
        }



        /// <summary>
        /// 取得 TpBetRecordRelay by Date
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private Task<IEnumerable<MpaTpBetRecordByDateDto>> GetMapTpBetRecordByDate(MpaWinLossByDataRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var redisKey =
                $"MapTpBetRecord:{groupByType}:SiteId:{request.SiteId}:{request.SettledStarTime:yyyy-MM-dd HH:mm:ss}:{request.SettledEndTime:yyyy-MM-dd HH:mm:ss}";

            return _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromMinutes(15),
                () => _mpaWinLossService.GetMapTpBetRecordByDate<MpaTpBetRecordByDateDto>(request.SiteId, request.SettledStarTime, request.SettledEndTime, format));
        }

        /// <summary>
        ///  取得 Deposit And Withdrawal Relay By Date
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private Task<IEnumerable<TransactionRelayDto>> GetMapTransactionByDate(MpaWinLossByDataRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var redisKey =
                $"MapTransaction:{groupByType}:SiteId:{request.SiteId}:{request.SettledStarTime:yyyy-MM-dd HH:mm:ss}:{request.SettledEndTime:yyyy-MM-dd HH:mm:ss}";

            return _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromMinutes(15),
                () => _mpaWinLossService.GetMapTransactionByDate<TransactionRelayDto>(request.SiteId, request.SettledStarTime, request.SettledEndTime, format));
        }

        private Task<int> GetCustIdByUserName(int siteId, string userName)
        {
            var redisKey = $"MapCustId:SiteId:{siteId}:UserName:{userName}";

            return _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromMinutes(2),
                () => _mpaWinLossService.GetCustIdByUserName(siteId, userName));
        }

        private static string MapMpaWinLossGroupByTypeEnumFormat(MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = groupByType switch
            {
                MpaWinLossGroupByTypeEnum.Date => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.Month => "%Y-%m",
                _ => string.Empty
            };
            return format;
        }

        #endregion
    }
}
