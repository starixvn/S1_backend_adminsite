using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.BLL.Mpa.Extension;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using NuGet.Packaging;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.ExpressionGraph;
using System.Linq;
using Org.BouncyCastle.Asn1.Ocsp;

namespace BackendSite.Service.BLL.Mpa
{
    public class MpaWinLossBLL
    {
        private readonly IMpaWinLossService _mpaWinLossService;
        private readonly ICacheHelper _redisHelper;

        public MpaWinLossBLL(IMpaWinLossService mpaWinLossService, ICacheHelper redisHelper)
        {
            _mpaWinLossService = mpaWinLossService;
            _redisHelper = redisHelper;
        }


        #region GropuByDate、Month

        public async Task<IEnumerable<MpaWinLossByDateDto>> GetMpaWinLossByDateExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料
            var data = await GetTpBetRecordGroupByDateResult(request, groupByType);
            var tpBetRecordResult = data.TpBetRecordResult;

            // 存提款 
            var transactionResult = await GetTransactionByDateResult(request, groupByType);

            // Bonus
            var bonusResult = await GetBonusByDateResult(request, groupByType);

            // 組合結果
            var combineResult = ByDateCombineResult(request, tpBetRecordResult, transactionResult, bonusResult);

            var exportData = new List<MpaWinLossByDateDto>();
            exportData.AddRange(combineResult);

            var keys = combineResult.Select(x => x.Date).Distinct();
            var rateMap = await GetCurrencyNameRateMapper();

            foreach (var key in keys)
            {
                // 決定原本的資料要不要 轉換匯率
                var details = exportData.Where(x => x.Date == key).ToList();
                foreach (var detail in details)
                {
                    var rate = request.DisplayGroupBreakdownInOriginalCurrency
                        ? 1
                        : rateMap[detail.Currency];

                    detail.TurnOver = detail.TurnOver / rate;
                    detail.ValidTurnOver = detail.ValidTurnOver / rate;
                    detail.GGR = detail.GGR / rate;
                    detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                    detail.ProgressiveWin = detail.ProgressiveWin / rate;
                    detail.ProgressiveBet = detail.ProgressiveBet / rate;
                    detail.Commission = detail.Commission / rate;
                    detail.AvgBetAmount = detail.AvgBetAmount / rate;
                    detail.Margin = detail.Margin / rate;
                    detail.TotalDepositAmount = detail.TotalDepositAmount / rate;
                    detail.TotalWithDrawAmount = detail.TotalWithDrawAmount / rate;
                    detail.TotalBonus = detail.TotalBonus / rate;
                }

                var dateSummary = combineResult.Where(x => x.Date == key).ToList();
                foreach (var detail in dateSummary)
                {
                    var rate = rateMap[detail.Currency];
                    detail.TurnOver = detail.TurnOver / rate;
                    detail.ValidTurnOver = detail.ValidTurnOver / rate;
                    detail.GGR = detail.GGR / rate;
                    detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                    detail.ProgressiveWin = detail.ProgressiveWin / rate;
                    detail.ProgressiveBet = detail.ProgressiveBet / rate;
                    detail.Commission = detail.Commission / rate;
                    detail.AvgBetAmount = detail.AvgBetAmount / rate;
                    detail.Margin = detail.Margin / rate;
                    detail.TotalDepositAmount = detail.TotalDepositAmount / rate;
                    detail.TotalWithDrawAmount = detail.TotalWithDrawAmount / rate;
                    detail.TotalBonus = detail.TotalBonus / rate;
                }

                var result = new MpaWinLossByDateDto
                {
                    Date = key,
                    Currency = "All",
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = dateSummary.Sum(x => x.GGR),
                    PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = dateSummary.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = dateSummary.Sum(x => x.ProgressiveBet),
                    Commission = dateSummary.Sum(x => x.Commission),
                    BetCount = dateSummary.Sum(x => x.BetCount),
                    UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                    DepositCount = dateSummary.Sum(x => x.DepositCount),
                    TotalDepositAmount = dateSummary.Sum(x => x.TotalDepositAmount),
                    WithDrawCount = dateSummary.Sum(x => x.WithDrawCount),
                    TotalWithDrawAmount = dateSummary.Sum(x => x.TotalWithDrawAmount),
                    AvgBetAmount = dateSummary.Sum(x => x.BetCount) != 0
                        ? dateSummary.Sum(x => x.ValidTurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                };

                exportData.Add(result);
            }

            exportData = request.OrderBy
                ? exportData.OrderBy(request)
                : exportData.OrderBy(x => x.Date).ThenBy(x => x.Currency).ToList();

            // total
            exportData.Add(new MpaWinLossByDateDto()
            {
                Date = "Total",
                TurnOver = exportData.Sum(x => x.TurnOver),
                ValidTurnOver = exportData.Sum(x => x.ValidTurnOver),
                GGR = exportData.Sum(x => x.GGR),
                PlayerWinLoss = exportData.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = exportData.Sum(x => x.ProgressiveWin),
                ProgressiveBet = exportData.Sum(x => x.ProgressiveBet),
                Commission = exportData.Sum(x => x.Commission),
                BetCount = exportData.Sum(x => x.BetCount),
                UniquePlayers = data.UniquePlayers,
                DepositCount = exportData.Sum(x => x.DepositCount),
                TotalDepositAmount = exportData.Sum(x => x.TotalDepositAmount),
                WithDrawCount = exportData.Sum(x => x.WithDrawCount),
                TotalWithDrawAmount = exportData.Sum(x => x.TotalWithDrawAmount),
                TotalBonus = exportData.Sum(x => x.TotalBonus),
                AvgBetAmount = exportData.Sum(x => x.BetCount) != 0
                    ? (exportData.Sum(x => x.ValidTurnOver) / exportData.Sum(x => x.BetCount))
                    : 0.00m,
                Margin = exportData.Sum(x => x.ValidTurnOver) != 0
                    ? exportData.Sum(x => x.GGR) / exportData.Sum(x => x.ValidTurnOver)
                    : 0.00m,
            });

            return exportData;
        }

        public async Task<Dictionary<string, MpaWinLossByDateResponse>> GetMapWinLossByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料
            var data = await GetTpBetRecordGroupByDateResult(request, groupByType);
            var tpBetRecordResult = data.TpBetRecordResult;

            // 存提款 
            var transactionResult = await GetTransactionByDateResult(request, groupByType);

            // Bonus
            var bonusResult = await GetBonusByDateResult(request, groupByType);

            // 組合結果
            var combineResult = ByDateCombineResult(request, tpBetRecordResult, transactionResult, bonusResult);

            var exportData = new Dictionary<string, MpaWinLossByDateResponse>();
            var keys = combineResult.Select(x => x.Date).Distinct();
            var rateMap = await GetCurrencyNameRateMapper();

            foreach (var key in keys)
            {
                var dateSummary = combineResult.Where(x => x.Date == key).ToList();
                foreach (var detail in dateSummary)
                {
                    var rate = rateMap[detail.Currency];
                    detail.TurnOver = detail.TurnOver / rate;
                    detail.ValidTurnOver = detail.ValidTurnOver / rate;
                    detail.GGR = detail.GGR / rate;
                    detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                    detail.ProgressiveWin = detail.ProgressiveWin / rate;
                    detail.ProgressiveBet = detail.ProgressiveBet / rate;
                    detail.Commission = detail.Commission / rate;
                    detail.AvgBetAmount = detail.AvgBetAmount / rate;
                    detail.Margin = detail.Margin / rate;
                    detail.TotalDepositAmount = detail.TotalDepositAmount / rate;
                    detail.TotalWithDrawAmount = detail.TotalWithDrawAmount / rate;
                    detail.TotalBonus = detail.TotalBonus / rate;
                }

                var result = new MpaWinLossByDateResponse
                {
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = dateSummary.Sum(x => x.GGR),
                    PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = dateSummary.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = dateSummary.Sum(x => x.ProgressiveBet),
                    Commission = dateSummary.Sum(x => x.Commission),
                    BetCount = dateSummary.Sum(x => x.BetCount),
                    UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                    DepositCount = dateSummary.Sum(x => x.DepositCount),
                    TotalDepositAmount = dateSummary.Sum(x => x.TotalDepositAmount),
                    WithDrawCount = dateSummary.Sum(x => x.WithDrawCount),
                    TotalWithDrawAmount = dateSummary.Sum(x => x.TotalWithDrawAmount),
                    AvgBetAmount = dateSummary.Sum(x => x.BetCount) != 0
                        ? dateSummary.Sum(x => x.ValidTurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                    CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByDate(combineResult, key)
                    : GetCurrencyDetailByDate(dateSummary)
                };

                exportData.Add(key, result);
            }

            // total
            exportData.Add("total", new MpaWinLossByDateResponse()
            {
                TurnOver = exportData.Sum(x => x.Value.TurnOver),
                ValidTurnOver = exportData.Sum(x => x.Value.ValidTurnOver),
                GGR = exportData.Sum(x => x.Value.GGR),
                PlayerWinLoss = exportData.Sum(x => x.Value.PlayerWinLoss),
                ProgressiveWin = exportData.Sum(x => x.Value.ProgressiveWin),
                ProgressiveBet = exportData.Sum(x => x.Value.ProgressiveBet),
                Commission = exportData.Sum(x => x.Value.Commission),
                BetCount = exportData.Sum(x => x.Value.BetCount),
                UniquePlayers = data.UniquePlayers,
                DepositCount = exportData.Sum(x => x.Value.DepositCount),
                TotalDepositAmount = exportData.Sum(x => x.Value.TotalDepositAmount),
                WithDrawCount = exportData.Sum(x => x.Value.WithDrawCount),
                TotalWithDrawAmount = exportData.Sum(x => x.Value.TotalWithDrawAmount),
                TotalBonus = exportData.Sum(x => x.Value.TotalBonus),
                AvgBetAmount = exportData.Sum(x => x.Value.BetCount) != 0
                    ? (exportData.Sum(x => x.Value.ValidTurnOver) / exportData.Sum(x => x.Value.BetCount))
                    : 0.00m,

                Margin = exportData.Sum(x => x.Value.ValidTurnOver) != 0
                    ? exportData.Sum(x => x.Value.GGR) / exportData.Sum(x => x.Value.ValidTurnOver)
                    : 0.00m,
            });

            return exportData;
        }

        private static IEnumerable<MpaWinLossByDateDto> ByDateCombineResult(MpaWinLossRequest request, IEnumerable<MpaWinLossByDateDto> tpBetRecordResult,
            IEnumerable<MpaWinLossByDateDto> transactionResult, IEnumerable<MpaWinLossByDateDto> bonusResult)
        {
            // 注單 + 存提款資料合併
            var combineData = tpBetRecordResult
                    .Concat(transactionResult)
                    .Concat(bonusResult)
                    .ToList();

            var combineResult = combineData.GroupBy(x => new
            {
                x.Date,
                x.Currency,
            })
                .Select(r => new MpaWinLossByDateDto
                {
                    Date = r.Key.Date,
                    Currency = r.Key.Currency,
                    TurnOver = r.Sum(x => x.TurnOver),
                    ValidTurnOver = r.Sum(x => x.ValidTurnOver),
                    GGR = r.Sum(x => x.GGR),
                    PlayerWinLoss = r.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = r.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = r.Sum(x => x.ProgressiveBet),
                    Commission = r.Sum(x => x.Commission),
                    BetCount = r.Sum(x => x.BetCount),
                    UniquePlayers = r.Sum(x => x.UniquePlayers),
                    DepositCount = r.Sum(x => x.DepositCount),
                    TotalDepositAmount = r.Sum(x => x.TotalDepositAmount),
                    WithDrawCount = r.Sum(x => x.WithDrawCount),
                    TotalWithDrawAmount = r.Sum(x => x.TotalWithDrawAmount),
                    AvgBetAmount = r.Sum(x => x.BetCount) != 0 ? r.Sum(x => x.ValidTurnOver) / r.Sum(x => x.BetCount) : 0.00m,
                    Margin = r.Sum(x => x.ValidTurnOver) != 0 ? r.Sum(x => x.GGR) / r.Sum(x => x.ValidTurnOver) : 0.00m,
                    TotalBonus = r.Sum(x => x.TotalBonus)
                });

            return request.OrderBy ? combineResult.OrderBy(request) : combineResult.OrderBy(x => x.Date).ThenBy(x => x.Currency);
        }

        private static Dictionary<string, MpaWinLossByDayCurrencyDetailDto> GetCurrencyDetailByDate(List<MpaWinLossByDateDto> dateSummary)
        {
            return dateSummary.ToDictionary(k => k.Currency, v => new MpaWinLossByDayCurrencyDetailDto
            {
                TurnOver = v.TurnOver,
                ValidTurnOver = v.ValidTurnOver,
                PlayerWinLoss = v.PlayerWinLoss,
                ProgressiveWin = v.ProgressiveWin,
                ProgressiveBet = v.ProgressiveBet,
                Commission = v.Commission,
                BetCount = v.BetCount,
                UniquePlayers = v.UniquePlayers,
                DepositCount = v.DepositCount,
                TotalDepositAmount = v.TotalDepositAmount,
                WithDrawCount = v.WithDrawCount,
                TotalWithDrawAmount = v.TotalWithDrawAmount,
                TotalBonus = v.TotalBonus
            });
        }

        private static Dictionary<string, MpaWinLossByDayCurrencyDetailDto> GetOriginalCurrencyDetailByDate(IEnumerable<MpaWinLossByDateDto> combineResult, string key)
        {
            return combineResult.Where(x => x.Date == key)
                .ToDictionary(k => k.Currency, v => new MpaWinLossByDayCurrencyDetailDto
                {
                    TurnOver = v.TurnOver,
                    ValidTurnOver = v.ValidTurnOver,
                    PlayerWinLoss = v.PlayerWinLoss,
                    ProgressiveWin = v.ProgressiveWin,
                    ProgressiveBet = v.ProgressiveBet,
                    Commission = v.Commission,
                    BetCount = v.BetCount,
                    UniquePlayers = v.UniquePlayers,
                    DepositCount = v.DepositCount,
                    TotalDepositAmount = v.TotalDepositAmount,
                    WithDrawCount = v.WithDrawCount,
                    TotalWithDrawAmount = v.TotalWithDrawAmount,
                    TotalBonus = v.TotalBonus
                });
        }

        private async Task<TpBetRecordByDateWrapper> GetTpBetRecordGroupByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單過濾
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);

            var data = await GetMapTpBetRecordByDate(request, groupByType);
            var tpBetRecordRelayResult = data.Filter(request)
                .SetGgRByOneGamingPoker()
                .SetGgRByV8Poker()
                .SetCommissionByNineW(request, format, _mpaWinLossService.Get9WCommission)
                .ToList();

            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var tpBetRecordResult = tpBetRecordRelayResult.GroupBy(x => new
            {
                x.Date,
                x.CurrencyId
            }).Select(r => new MpaWinLossByDateDto
            {
                Date = groupByType == MpaWinLossGroupByTypeEnum.Date
                    ? r.Key.Date.ToString("yyyy-MM-dd")
                    : r.Key.Date.ToString("yyyy-MM"),
                Currency = currencyNameMapper[r.Key.CurrencyId],
                TurnOver = r.Sum(x => x.StakeSummary),
                ValidTurnOver = r.Sum(x => x.ValidBetSummary),
                GGR = r.Sum(x => x.WinLossSummary) * -1,
                PlayerWinLoss = r.Sum(x => x.WinLossSummary),
                ProgressiveWin = r.Sum(x => x.ProgressiveWinSummary),
                ProgressiveBet = r.Sum(x => x.ProgressiveBetSummary),
                Commission = r.Sum(x => x.RevenueSummary),
                BetCount = r.Sum(x => x.RowCount),
                AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.ValidBetSummary) / r.Sum(x => x.RowCount) : 0.00m,
                Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                UniquePlayers = r.DistinctBy(x => x.CustId).Count()
            });

            var tpBetRecordRelayResultWrapper = new TpBetRecordByDateWrapper()
            {
                TpBetRecordResult = tpBetRecordResult,
                UniquePlayers = tpBetRecordRelayResult.DistinctBy(x => x.CustId).Count()
            };

            return tpBetRecordRelayResultWrapper;
        }


        #endregion

        #region GroupBy TpGameType

        public async Task<IEnumerable<MpaWinLossByTpGameTypeDto>> GetMapWinLossByTpGameTypeExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料
            var data = await GetTpBetRecordGroupByTpGameTypeResult(request, groupByType);
            var tpBetRecordResult = data.TpBetRecordResult;

            var combineResult = tpBetRecordResult;
            var rateMap = await GetCurrencyNameRateMapper();
            var keys = combineResult.Select(x => x.TpGameTypeName).Distinct();
            var exportData = new List<MpaWinLossByTpGameTypeDto>();
            exportData.AddRange(combineResult);

            foreach (var key in keys)
            {
                var dateSummary = combineResult.Where(x => x.TpGameTypeName == key).ToList();
                foreach (var detail in dateSummary)
                {
                    var rate = rateMap[detail.Currency];
                    detail.TurnOver = detail.TurnOver / rate;
                    detail.ValidTurnOver = detail.ValidTurnOver / rate;
                    detail.GGR = detail.GGR / rate;
                    detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                    detail.ProgressiveWin = detail.ProgressiveWin / rate;
                    detail.ProgressiveBet = detail.ProgressiveBet / rate;
                    detail.Commission = detail.Commission / rate;
                    detail.AvgBetAmount = detail.AvgBetAmount / rate;
                    detail.Margin = detail.Margin / rate;
                }

                var result = new MpaWinLossByTpGameTypeDto
                {
                    TpGameTypeName = key,
                    Currency = "All",
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = dateSummary.Sum(x => x.GGR),
                    PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = dateSummary.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = dateSummary.Sum(x => x.ProgressiveBet),
                    Commission = dateSummary.Sum(x => x.Commission),
                    BetCount = dateSummary.Sum(x => x.BetCount),
                    UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                    AvgBetAmount = dateSummary.Sum(x => x.BetCount) != 0
                        ? dateSummary.Sum(x => x.ValidTurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m
                };

                exportData.Add(result);
            }

            exportData = request.OrderBy
                ? exportData.OrderBy(request)
                : exportData.OrderBy(x => x.TpGameTypeName).ThenBy(x => x.Currency).ToList();

            // total
            exportData.Add(new MpaWinLossByTpGameTypeDto()
            {
                TpGameTypeName = "total",
                TurnOver = exportData.Sum(x => x.TurnOver),
                ValidTurnOver = exportData.Sum(x => x.ValidTurnOver),
                GGR = exportData.Sum(x => x.GGR),
                PlayerWinLoss = exportData.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = exportData.Sum(x => x.ProgressiveWin),
                ProgressiveBet = exportData.Sum(x => x.ProgressiveBet),
                Commission = exportData.Sum(x => x.Commission),
                BetCount = exportData.Sum(x => x.BetCount),
                AvgBetAmount = (exportData.Sum(x => x.BetCount) != 0 && exportData.Sum(x => x.TurnOver) != 0)
                    ? (exportData.Sum(x => x.TurnOver) / exportData.Sum(x => x.BetCount))
                    : 0.00m,
                UniquePlayers = data.UniquePlayers,
                Margin = (exportData.Sum(x => x.GGR) != 0 && exportData.Sum(x => x.TurnOver) != 0)
                    ? exportData.Sum(x => x.GGR) / exportData.Sum(x => x.TurnOver)
                    : 0.00m
            });

            return exportData;
        }

        public async Task<Dictionary<string, MpaWinLossByTpGameTypeResponse>> GetMapWinLossByTpGameTypeResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var exportData = new Dictionary<string, MpaWinLossByTpGameTypeResponse>();

            // 注單資料
            var data = await GetTpBetRecordGroupByTpGameTypeResult(request, groupByType);
            var tpBetRecordResult = data.TpBetRecordResult;

            var combineResult = request.OrderBy
                ? tpBetRecordResult.OrderBy(request)
                : tpBetRecordResult.OrderBy(r => r.TpGameTypeName).ThenBy(x => x.Currency).ToList();

            var rateMap = await GetCurrencyNameRateMapper();
            var keys = combineResult.Select(x => x.TpGameTypeName).Distinct();

            foreach (var key in keys)
            {
                var dateSummary = combineResult.Where(x => x.TpGameTypeName == key).ToList();
                foreach (var detail in dateSummary)
                {
                    var rate = rateMap[detail.Currency];
                    detail.TurnOver = detail.TurnOver / rate;
                    detail.ValidTurnOver = detail.ValidTurnOver / rate;
                    detail.GGR = detail.GGR / rate;
                    detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                    detail.ProgressiveWin = detail.ProgressiveWin / rate;
                    detail.ProgressiveBet = detail.ProgressiveBet / rate;
                    detail.Commission = detail.Commission / rate;
                    detail.AvgBetAmount = detail.AvgBetAmount / rate;
                    detail.Margin = detail.Margin / rate;
                }

                var result = new MpaWinLossByTpGameTypeResponse
                {
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = dateSummary.Sum(x => x.GGR),
                    PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = dateSummary.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = dateSummary.Sum(x => x.ProgressiveBet),
                    Commission = dateSummary.Sum(x => x.Commission),
                    BetCount = dateSummary.Sum(x => x.BetCount),
                    UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                    AvgBetAmount = dateSummary.Sum(x => x.BetCount) != 0
                        ? dateSummary.Sum(x => x.ValidTurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByTpGameType(combineResult, key)
                    : GetCurrencyDetailByTpGameType(dateSummary)
                };

                exportData.Add(key, result);
            }

            // total
            exportData.Add("total", new MpaWinLossByTpGameTypeResponse()
            {
                TurnOver = exportData.Sum(x => x.Value.TurnOver),
                ValidTurnOver = exportData.Sum(x => x.Value.ValidTurnOver),
                GGR = exportData.Sum(x => x.Value.GGR),
                PlayerWinLoss = exportData.Sum(x => x.Value.PlayerWinLoss),
                ProgressiveWin = exportData.Sum(x => x.Value.ProgressiveWin),
                ProgressiveBet = exportData.Sum(x => x.Value.ProgressiveBet),
                Commission = exportData.Sum(x => x.Value.Commission),
                BetCount = exportData.Sum(x => x.Value.BetCount),
                AvgBetAmount = (exportData.Sum(x => x.Value.BetCount) != 0 && exportData.Sum(x => x.Value.TurnOver) != 0)
                    ? (exportData.Sum(x => x.Value.TurnOver) / exportData.Sum(x => x.Value.BetCount))
                    : 0.00m,
                UniquePlayers = data.UniquePlayers,
                Margin = (exportData.Sum(x => x.Value.GGR) != 0 && exportData.Sum(x => x.Value.TurnOver) != 0)
                        ? exportData.Sum(x => x.Value.GGR) / exportData.Sum(x => x.Value.TurnOver)
                        : 0.00m
            });

            return exportData;
        }

        private static Dictionary<string, MpaWinLossByTpGameTypeCurrencyDetailDto> GetOriginalCurrencyDetailByTpGameType(IEnumerable<MpaWinLossByTpGameTypeDto> combineResult, string key)
        {
            return combineResult.Where(x => x.TpGameTypeName == key)
                .ToDictionary(k => k.Currency,
                    v => new MpaWinLossByTpGameTypeCurrencyDetailDto
                    {
                        TurnOver = v.TurnOver,
                        ValidTurnOver = v.ValidTurnOver,
                        PlayerWinLoss = v.PlayerWinLoss,
                        ProgressiveWin = v.ProgressiveWin,
                        ProgressiveBet = v.ProgressiveBet,
                        Commission = v.Commission,
                        BetCount = v.BetCount,
                        UniquePlayers = v.UniquePlayers,
                    });
        }

        private static Dictionary<string, MpaWinLossByTpGameTypeCurrencyDetailDto> GetCurrencyDetailByTpGameType(IEnumerable<MpaWinLossByTpGameTypeDto> dateSummary)
        {
            return dateSummary.ToDictionary(k => k.Currency,
                v => new MpaWinLossByTpGameTypeCurrencyDetailDto
                {
                    TurnOver = v.TurnOver,
                    ValidTurnOver = v.ValidTurnOver,
                    PlayerWinLoss = v.PlayerWinLoss,
                    ProgressiveWin = v.ProgressiveWin,
                    ProgressiveBet = v.ProgressiveBet,
                    Commission = v.Commission,
                    BetCount = v.BetCount,
                    UniquePlayers = v.UniquePlayers,
                });
        }

        /// <summary>
        /// 取得 GroupBy TpGameType 注單結果
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<TpBetRecordByTpGameTypeWrapper> GetTpBetRecordGroupByTpGameTypeResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單過濾
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);

            var data = await GetMapTpBetRecordByDate(request, groupByType);
            var tpBetRecordRelayResult = data.Filter(request)
                .SetGgRByOneGamingPoker()
                .SetGgRByV8Poker()
                .SetCommissionByNineW(request, format, _mpaWinLossService.Get9WCommission)
                .ToList();

            var tpGameTypeMap = await GetTpGameTypeMapper();
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var tpBetRecordResult = tpBetRecordRelayResult.GroupBy(x => new
            {
                x.GameType,
                x.CurrencyId
            })
            .Select(r => new MpaWinLossByTpGameTypeDto
            {
                TpGameTypeName = tpGameTypeMap[r.Key.GameType],
                Currency = currencyNameMapper[r.Key.CurrencyId],
                TurnOver = r.Sum(x => x.StakeSummary),
                ValidTurnOver = r.Sum(x => x.ValidBetSummary),
                GGR = r.Sum(x => x.WinLossSummary) * -1,
                PlayerWinLoss = r.Sum(x => x.WinLossSummary),
                ProgressiveWin = r.Sum(x => x.ProgressiveWinSummary),
                ProgressiveBet = r.Sum(x => x.ProgressiveBetSummary),
                Commission = r.Sum(x => x.RevenueSummary),
                BetCount = r.Sum(x => x.RowCount),
                AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.ValidBetSummary) / r.Sum(x => x.RowCount) : 0.00m,
                Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                UniquePlayers = r.DistinctBy(x => x.CustId).Count()
            });

            var exportData = new TpBetRecordByTpGameTypeWrapper()
            {
                TpBetRecordResult = tpBetRecordResult,
                UniquePlayers = tpBetRecordRelayResult.DistinctBy(x => x.CustId).Count()
            };

            return exportData;
        }

        #endregion

        #region GroupBy TpCategory

        public async Task<IEnumerable<MpaWinLossByTpCategoryDto>> GetMapWinLossByTpCategoryExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料
            var data = await GetTpBetRecordGroupByTpCategoryResult(request, groupByType);
            var tpBetRecordResult = data.TpBetRecordResult;
            var combineResult = tpBetRecordResult;
            var exportData = new List<MpaWinLossByTpCategoryDto>();
            exportData.AddRange(combineResult);


            var rateMap = await GetCurrencyNameRateMapper();
            var keys = combineResult.Select(x => x.TpCategoryName).Distinct();

            foreach (var key in keys)
            {
                var dateSummary = combineResult.Where(x => x.TpCategoryName == key).ToList();
                foreach (var detail in dateSummary)
                {
                    var rate = request.DisplayGroupBreakdownInOriginalCurrency
                        ? 1
                        : rateMap[detail.Currency];

                    detail.TurnOver = detail.TurnOver / rate;
                    detail.ValidTurnOver = detail.ValidTurnOver / rate;
                    detail.GGR = detail.GGR / rate;
                    detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                    detail.ProgressiveWin = detail.ProgressiveWin / rate;
                    detail.ProgressiveBet = detail.ProgressiveBet / rate;
                    detail.Commission = detail.Commission / rate;
                    detail.AvgBetAmount = detail.AvgBetAmount / rate;
                    detail.Margin = detail.Margin / rate;
                }

                var result = new MpaWinLossByTpCategoryDto
                {
                    TpCategoryName = key,
                    Currency = "All",
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = dateSummary.Sum(x => x.GGR),
                    PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = dateSummary.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = dateSummary.Sum(x => x.ProgressiveBet),
                    Commission = dateSummary.Sum(x => x.Commission),
                    BetCount = dateSummary.Sum(x => x.BetCount),
                    UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                    AvgBetAmount = dateSummary.Sum(x => x.BetCount) != 0
                        ? dateSummary.Sum(x => x.ValidTurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                };

                exportData.Add(result);
            }

            exportData = request.OrderBy
                ? exportData.OrderBy(request)
                : exportData.OrderBy(x => x.TpCategoryName).ThenBy(x => x.Currency).ToList();

            // total
            exportData.Add(new MpaWinLossByTpCategoryDto()
            {
                TpCategoryName = "total",
                TurnOver = exportData.Sum(x => x.TurnOver),
                ValidTurnOver = exportData.Sum(x => x.ValidTurnOver),
                GGR = exportData.Sum(x => x.GGR),
                PlayerWinLoss = exportData.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = exportData.Sum(x => x.ProgressiveWin),
                ProgressiveBet = exportData.Sum(x => x.ProgressiveBet),
                Commission = exportData.Sum(x => x.Commission),
                BetCount = exportData.Sum(x => x.BetCount),
                AvgBetAmount = (exportData.Sum(x => x.BetCount) != 0 && exportData.Sum(x => x.TurnOver) != 0)
                    ? (exportData.Sum(x => x.TurnOver) / exportData.Sum(x => x.BetCount))
                    : 0.00m,
                UniquePlayers = data.UniquePlayers,
                Margin = (exportData.Sum(x => x.GGR) != 0 && exportData.Sum(x => x.TurnOver) != 0)
                    ? exportData.Sum(x => x.GGR) / exportData.Sum(x => x.TurnOver)
                    : 0.00m
            });

            return exportData;
        }


        public async Task<Dictionary<string, MpaWinLossByTpCategoryResponse>> GetMapWinLossByTpCategoryResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var exportData = new Dictionary<string, MpaWinLossByTpCategoryResponse>();

            //// 注單資料
            var data = await GetTpBetRecordGroupByTpCategoryResult(request, groupByType);
            var tpBetRecordResult = data.TpBetRecordResult;

            var combineResult = request.OrderBy
                ? tpBetRecordResult.OrderBy(request)
                : tpBetRecordResult;

            var rateMap = await GetCurrencyNameRateMapper();
            var keys = combineResult.Select(x => x.TpCategoryName).Distinct();

            foreach (var key in keys)
            {
                var dateSummary = combineResult.Where(x => x.TpCategoryName == key).ToList();
                foreach (var detail in dateSummary)
                {
                    var rate = rateMap[detail.Currency];
                    detail.TurnOver = detail.TurnOver / rate;
                    detail.ValidTurnOver = detail.ValidTurnOver / rate;
                    detail.GGR = detail.GGR / rate;
                    detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                    detail.ProgressiveWin = detail.ProgressiveWin / rate;
                    detail.ProgressiveBet = detail.ProgressiveBet / rate;
                    detail.Commission = detail.Commission / rate;
                    detail.AvgBetAmount = detail.AvgBetAmount / rate;
                    detail.Margin = detail.Margin / rate;
                }

                var result = new MpaWinLossByTpCategoryResponse
                {
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = dateSummary.Sum(x => x.GGR),
                    PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = dateSummary.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = dateSummary.Sum(x => x.ProgressiveBet),
                    Commission = dateSummary.Sum(x => x.Commission),
                    BetCount = dateSummary.Sum(x => x.BetCount),
                    UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                    AvgBetAmount = dateSummary.Sum(x => x.BetCount) != 0
                        ? dateSummary.Sum(x => x.ValidTurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByTpCategory(combineResult, key)
                    : GetCurrencyDetailByTpCategory(dateSummary)
                };

                exportData.Add(key, result);
            }

            // total
            exportData.Add("total", new MpaWinLossByTpCategoryResponse()
            {
                TurnOver = exportData.Sum(x => x.Value.TurnOver),
                ValidTurnOver = exportData.Sum(x => x.Value.ValidTurnOver),
                GGR = exportData.Sum(x => x.Value.GGR),
                PlayerWinLoss = exportData.Sum(x => x.Value.PlayerWinLoss),
                ProgressiveWin = exportData.Sum(x => x.Value.ProgressiveWin),
                ProgressiveBet = exportData.Sum(x => x.Value.ProgressiveBet),
                Commission = exportData.Sum(x => x.Value.Commission),
                BetCount = exportData.Sum(x => x.Value.BetCount),
                AvgBetAmount = (exportData.Sum(x => x.Value.BetCount) != 0 && exportData.Sum(x => x.Value.TurnOver) != 0)
                    ? (exportData.Sum(x => x.Value.TurnOver) / exportData.Sum(x => x.Value.BetCount))
                    : 0.00m,
                UniquePlayers = data.UniquePlayers,
                Margin = (exportData.Sum(x => x.Value.GGR) != 0 && exportData.Sum(x => x.Value.TurnOver) != 0)
                        ? exportData.Sum(x => x.Value.GGR) / exportData.Sum(x => x.Value.TurnOver)
                        : 0.00m
            });

            return exportData;
        }

        private static Dictionary<string, MpaWinLossByTpCategoryCurrencyDetailDto> GetOriginalCurrencyDetailByTpCategory(IEnumerable<MpaWinLossByTpCategoryDto> combineResult, string key)
        {
            return combineResult.Where(x => x.TpCategoryName == key)
                .ToDictionary(k => k.Currency,
                    v => new MpaWinLossByTpCategoryCurrencyDetailDto
                    {
                        TurnOver = v.TurnOver,
                        ValidTurnOver = v.ValidTurnOver,
                        PlayerWinLoss = v.PlayerWinLoss,
                        ProgressiveWin = v.ProgressiveWin,
                        ProgressiveBet = v.ProgressiveBet,
                        Commission = v.Commission,
                        BetCount = v.BetCount,
                        UniquePlayers = v.UniquePlayers,
                    });
        }

        private static Dictionary<string, MpaWinLossByTpCategoryCurrencyDetailDto> GetCurrencyDetailByTpCategory(IEnumerable<MpaWinLossByTpCategoryDto> dateSummary)
        {
            return dateSummary.ToDictionary(k => k.Currency,
                v => new MpaWinLossByTpCategoryCurrencyDetailDto
                {
                    TurnOver = v.TurnOver,
                    ValidTurnOver = v.ValidTurnOver,
                    PlayerWinLoss = v.PlayerWinLoss,
                    ProgressiveWin = v.ProgressiveWin,
                    ProgressiveBet = v.ProgressiveBet,
                    Commission = v.Commission,
                    BetCount = v.BetCount,
                    UniquePlayers = v.UniquePlayers,
                });
        }


        /// <summary>
        /// 取得 GroupBy TpGameType 注單結果
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<TpBetRecordByTpCategoryWrapper> GetTpBetRecordGroupByTpCategoryResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單過濾
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);

            var data = await GetMapTpBetRecordByDate(request, groupByType);
            var tpBetRecordRelayResult = data.Filter(request)
                .SetGgRByOneGamingPoker()
                .SetGgRByV8Poker()
                .SetCommissionByNineW(request, format, _mpaWinLossService.Get9WCommission)
                .ToList();

            var tpCategoryMap = await GetTpCategoryMapper();
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var tpBetRecordResult = tpBetRecordRelayResult.GroupBy(x => new
            {
                x.CategoryId,
                x.CurrencyId
            })
            .Select(r => new MpaWinLossByTpCategoryDto
            {
                TpCategoryName = tpCategoryMap[r.Key.CategoryId],
                Currency = currencyNameMapper[r.Key.CurrencyId],
                TurnOver = r.Sum(x => x.StakeSummary),
                ValidTurnOver = r.Sum(x => x.ValidBetSummary),
                GGR = r.Sum(x => x.WinLossSummary) * -1,
                PlayerWinLoss = r.Sum(x => x.WinLossSummary),
                ProgressiveWin = r.Sum(x => x.ProgressiveWinSummary),
                ProgressiveBet = r.Sum(x => x.ProgressiveBetSummary),
                Commission = r.Sum(x => x.RevenueSummary),
                BetCount = r.Sum(x => x.RowCount),
                AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.ValidBetSummary) / r.Sum(x => x.RowCount) : 0.00m,
                Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                UniquePlayers = r.DistinctBy(x => x.CustId).Count()
            });

            var exportData = new TpBetRecordByTpCategoryWrapper()
            {
                TpBetRecordResult = tpBetRecordResult,
                UniquePlayers = tpBetRecordRelayResult.DistinctBy(x => x.CustId).Count()
            };

            return exportData;
        }

        #endregion

        #region GroupBy Player Master

        public async Task<IEnumerable<MpaWinLossByPlayerDto>> GetMpaWinLossByPlayerExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料
            var data = await GetTpBetRecordGroupByPlayerResult(request, groupByType);
            var tpBetRecordResult = data.TpBetRecordResult;

            // 存提款 
            var transactionResult = await GetTransactionByPlayerResult(request, groupByType);

            // bonus
            var bonusResult = await GetBonusByPlayerResult(request, groupByType);

            // 注單 + 存提款資料合併
            var combineData = tpBetRecordResult
                .Concat(transactionResult)
                .Concat(bonusResult)
                .ToList();

            var combineResult = ComposeMpaWinLossByPlayer(combineData);
            var exportData = new List<MpaWinLossByPlayerDto>();
            exportData.AddRange(combineResult);

            var userNames = combineResult.Select(x => x.UserName).Distinct();
            var rateMap = await GetCurrencyNameRateMapper();
            
            foreach (var userName in userNames)
            {
                // 決定原本的資料要不要 轉換匯率
                var details = exportData.Where(x => x.UserName == userName).ToList();
                foreach (var detail in details)
                {
                    var rate = request.DisplayGroupBreakdownInOriginalCurrency
                    ? 1
                    : rateMap[detail.Currency];

                    detail.ClosingBalance = detail.ClosingBalance / rate;
                    detail.TurnOver = detail.TurnOver / rate;
                    detail.ValidTurnOver = detail.ValidTurnOver / rate;
                    detail.GGR = detail.GGR / rate;
                    detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                    detail.ProgressiveWin = detail.ProgressiveWin / rate;
                    detail.ProgressiveBet = detail.ProgressiveBet / rate;
                    detail.Commission = detail.Commission / rate;
                    detail.AvgBetAmount = detail.AvgBetAmount / rate;
                    detail.Margin = detail.Margin / rate;
                    detail.TotalDepositAmount = detail.TotalDepositAmount / rate;
                    detail.TotalWithDrawAmount = detail.TotalWithDrawAmount / rate;
                    detail.TotalBonus = detail.TotalBonus / rate;
                }

                var dateSummary = combineResult.Where(x => x.UserName == userName).ToList();
                foreach (var detail in dateSummary)
                {
                    var rate = rateMap[detail.Currency];
                    detail.ClosingBalance = detail.ClosingBalance / rate;
                    detail.TurnOver = detail.TurnOver / rate;
                    detail.ValidTurnOver = detail.ValidTurnOver / rate;
                    detail.GGR = detail.GGR / rate;
                    detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                    detail.ProgressiveWin = detail.ProgressiveWin / rate;
                    detail.ProgressiveBet = detail.ProgressiveBet / rate;
                    detail.Commission = detail.Commission / rate;
                    detail.AvgBetAmount = detail.AvgBetAmount / rate;
                    detail.Margin = detail.Margin / rate;
                    detail.TotalDepositAmount = detail.TotalDepositAmount / rate;
                    detail.TotalWithDrawAmount = detail.TotalWithDrawAmount / rate;
                    detail.TotalBonus = detail.TotalBonus / rate;
                }

                var responseItem = new MpaWinLossByPlayerDto
                {
                    TpUserName = dateSummary.Select(r => r.TpUserName).FirstOrDefault(),
                    Currency = "All",
                    UserName = dateSummary.Select(r => r.UserName).FirstOrDefault(),
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = dateSummary.Sum(x => x.GGR),
                    PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = dateSummary.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = dateSummary.Sum(x => x.ProgressiveBet),
                    Commission = dateSummary.Sum(x => x.Commission),
                    BetCount = dateSummary.Sum(x => x.BetCount),
                    UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                    ClosingBalance = dateSummary.Sum(x => x.ClosingBalance),
                    DepositCount = dateSummary.Sum(x => x.DepositCount),
                    TotalDepositAmount = dateSummary.Sum(x => x.TotalDepositAmount),
                    WithDrawCount = dateSummary.Sum(x => x.WithDrawCount),
                    TotalWithDrawAmount = dateSummary.Sum(x => x.TotalWithDrawAmount),
                    AvgBetAmount = dateSummary.Sum(x => x.BetCount) != 0
                        ? dateSummary.Sum(x => x.ValidTurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                };

                exportData.Add(responseItem);
            }

            exportData = request.OrderBy
                ? exportData.OrderBy(request)
                : exportData.OrderBy(x => x.UserName).ThenBy(x => x.Currency).ToList();

            // total
            exportData.Add(new MpaWinLossByPlayerDto()
            {
                TpUserName = "total",
                TurnOver = exportData.Sum(x => x.TurnOver),
                ValidTurnOver = exportData.Sum(x => x.ValidTurnOver),
                GGR = exportData.Sum(x => x.GGR),
                PlayerWinLoss = exportData.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = exportData.Sum(x => x.ProgressiveWin),
                ProgressiveBet = exportData.Sum(x => x.ProgressiveBet),
                Commission = exportData.Sum(x => x.Commission),
                BetCount = exportData.Sum(x => x.BetCount),
                ClosingBalance = exportData.Sum(x => x.ClosingBalance),
                UniquePlayers = exportData.DistinctBy(x => x.UserName).Count(),
                DepositCount = exportData.Sum(x => x.DepositCount),
                TotalDepositAmount = exportData.Sum(x => x.TotalDepositAmount),
                WithDrawCount = exportData.Sum(x => x.WithDrawCount),
                TotalWithDrawAmount = exportData.Sum(x => x.TotalWithDrawAmount),
                TotalBonus = exportData.Sum(x => x.TotalBonus),
                AvgBetAmount = exportData.Sum(x => x.BetCount) != 0
                    ? (exportData.Sum(x => x.ValidTurnOver) / exportData.Sum(x => x.BetCount))
                    : 0.00m,

                Margin = exportData.Sum(x => x.ValidTurnOver) != 0
                    ? exportData.Sum(x => x.GGR) / exportData.Sum(x => x.ValidTurnOver)
                    : 0.00m,
            });

            return exportData;
        }

        public async Task<Dictionary<string, MpaWinLossByPlayerResponse>> GetMpaWinLossByPlayerResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            IPageInfo pageInfo = new PageInfo();
            var key = request.GetWinLossByPlayerRedisKey();
            var masterKey = $"MpaWinLossByPlayer:{key}";
            var sortKey = $"{masterKey}:{request.OrderByProperty}:{request.OrderByDirection}";
            var cacheTime = TimeSpan.FromMinutes(15);
            var responseData = new List<MpaWinLossByPlayerResponse>();
            responseData.Clear();

            if (_redisHelper.KeyExists(masterKey))
            {
                if (request.OrderBy && !_redisHelper.KeyExists(sortKey))
                {
                    // 取出全部資料重新排序
                    var sortData = (await _redisHelper.GetListByRangeAsync<MpaWinLossByPlayerResponse>(masterKey, 0, -1)).OrderBy(request);

                    // 刪除舊資料重設cache
                    if (_redisHelper.KeyDeleteByPattern($"{masterKey}*") > 0)
                    {
                        AddCache(sortData, masterKey, sortKey, cacheTime, request.OrderBy);
                    }
                    var (sortIndex, sortCount) = pageInfo.ListPagination(sortData.Count, request.PageNumber, request.PageSize);
                    responseData.AddRange(sortData.GetRange(sortIndex, sortCount));
                }
                else
                {
                    if (request.PageNumber == -1) //取全部 for excel export
                    {
                        var data = await _redisHelper.GetListByRangeAsync<MpaWinLossByPlayerResponse>(masterKey, 0, request.PageNumber);
                        responseData.AddRange(data);
                    }
                    else
                    {
                        //Redis Pagination
                        var total = (int)_redisHelper.GetListLength(masterKey);
                        var (startIndex, endIndex) = pageInfo.RedisPagination(total, request.PageNumber, request.PageSize);
                        var data = await _redisHelper.GetListByRangeAsync<MpaWinLossByPlayerResponse>(masterKey, startIndex, endIndex);
                        responseData.AddRange(data);
                    }
                }
            }
            else
            {
                // 注單資料
                var data = await GetTpBetRecordGroupByPlayerResult(request, groupByType);
                var tpBetRecordResult = data.TpBetRecordResult;

                // 存提款 
                var transactionResult = await GetTransactionByPlayerResult(request, groupByType);

                // bonus
                var bonusResult = await GetBonusByPlayerResult(request, groupByType);

                // 注單 + 存提款資料合併
                var combineData = tpBetRecordResult
                        .Concat(transactionResult)
                        .Concat(bonusResult)
                        .ToList();

                var combineResult = ComposeMpaWinLossByPlayer(combineData);
                var userNames = combineResult.Select(x => x.UserName).Distinct();
                var rateMap = await GetCurrencyNameRateMapper();

                foreach (var userName in userNames)
                {
                    var dateSummary = combineResult.Where(x => x.UserName == userName).ToList();
                    foreach (var detail in dateSummary)
                    {
                        var rate = rateMap[detail.Currency];
                        detail.ClosingBalance = detail.ClosingBalance / rate;
                        detail.TurnOver = detail.TurnOver / rate;
                        detail.ValidTurnOver = detail.ValidTurnOver / rate;
                        detail.GGR = detail.GGR / rate;
                        detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                        detail.ProgressiveWin = detail.ProgressiveWin / rate;
                        detail.ProgressiveBet = detail.ProgressiveBet / rate;
                        detail.Commission = detail.Commission / rate;
                        detail.AvgBetAmount = detail.AvgBetAmount / rate;
                        detail.Margin = detail.Margin / rate;
                        detail.TotalDepositAmount = detail.TotalDepositAmount / rate;
                        detail.TotalWithDrawAmount = detail.TotalWithDrawAmount / rate;
                        detail.TotalBonus = detail.TotalBonus / rate;
                    }

                    var responseItem = GetMpaWinLossByPlayerResponseItem(request, dateSummary, combineResult, userName);
                    responseData.Add(responseItem);
                }

                responseData = request.OrderBy ? responseData.OrderBy(request) : responseData;
                AddCache(responseData, masterKey, sortKey, cacheTime, request.OrderBy);

                if (request.PageNumber != -1) //取全部 for excel export
                {
                    var (index, count) = pageInfo.ListPagination(responseData.Count, request.PageNumber, request.PageSize);
                    responseData = responseData.GetRange(index, count);
                }
            }

            var exportData = new Dictionary<string, MpaWinLossByPlayerResponse>();
            exportData.AddRange(responseData.ToDictionary(k => k.UserName, v => v));

            // total
            exportData.Add("total", new MpaWinLossByPlayerResponse()
            {
                TurnOver = exportData.Sum(x => x.Value.TurnOver),
                ValidTurnOver = exportData.Sum(x => x.Value.ValidTurnOver),
                GGR = exportData.Sum(x => x.Value.GGR),
                PlayerWinLoss = exportData.Sum(x => x.Value.PlayerWinLoss),
                ProgressiveWin = exportData.Sum(x => x.Value.ProgressiveWin),
                ProgressiveBet = exportData.Sum(x => x.Value.ProgressiveBet),
                Commission = exportData.Sum(x => x.Value.Commission),
                BetCount = exportData.Sum(x => x.Value.BetCount),
                ClosingBalance = exportData.Sum(x => x.Value.ClosingBalance),
                UniquePlayers = exportData.DistinctBy(x => x.Value.UserName).Count(),
                DepositCount = exportData.Sum(x => x.Value.DepositCount),
                TotalDepositAmount = exportData.Sum(x => x.Value.TotalDepositAmount),
                WithDrawCount = exportData.Sum(x => x.Value.WithDrawCount),
                TotalWithDrawAmount = exportData.Sum(x => x.Value.TotalWithDrawAmount),
                TotalBonus = exportData.Sum(x => x.Value.TotalBonus),
                AvgBetAmount = exportData.Sum(x => x.Value.BetCount) != 0
                    ? (exportData.Sum(x => x.Value.ValidTurnOver) / exportData.Sum(x => x.Value.BetCount))
                    : 0.00m,

                Margin = exportData.Sum(x => x.Value.ValidTurnOver) != 0
                    ? exportData.Sum(x => x.Value.GGR) / exportData.Sum(x => x.Value.ValidTurnOver)
                    : 0.00m,
            });

            return exportData;
        }

        private static MpaWinLossByPlayerResponse GetMpaWinLossByPlayerResponseItem(MpaWinLossRequest request, List<MpaWinLossByPlayerDto> dateSummary,
            IEnumerable<MpaWinLossByPlayerDto> combineResult, string userName)
        {
            var responseItem = new MpaWinLossByPlayerResponse
            {
                SabaId = dateSummary.Select(r => r.TpUserName).FirstOrDefault(),
                UserName = dateSummary.Select(r => r.UserName).FirstOrDefault(),
                TurnOver = dateSummary.Sum(x => x.TurnOver),
                ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                GGR = dateSummary.Sum(x => x.GGR),
                PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = dateSummary.Sum(x => x.ProgressiveWin),
                ProgressiveBet = dateSummary.Sum(x => x.ProgressiveBet),
                Commission = dateSummary.Sum(x => x.Commission),
                BetCount = dateSummary.Sum(x => x.BetCount),
                UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                ClosingBalance = dateSummary.Sum(x => x.ClosingBalance),
                DepositCount = dateSummary.Sum(x => x.DepositCount),
                TotalDepositAmount = dateSummary.Sum(x => x.TotalDepositAmount),
                WithDrawCount = dateSummary.Sum(x => x.WithDrawCount),
                TotalWithDrawAmount = dateSummary.Sum(x => x.TotalWithDrawAmount),
                AvgBetAmount = dateSummary.Sum(x => x.BetCount) != 0
                    ? dateSummary.Sum(x => x.ValidTurnOver) / dateSummary.Sum(x => x.BetCount)
                    : 0.00m,

                Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                    ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                    : 0.00m,
                TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByPlayer(combineResult, userName)
                    : GetCurrencyDetailByPlayer(dateSummary)
            };
            return responseItem;
        }

        private static IEnumerable<MpaWinLossByPlayerDto> ComposeMpaWinLossByPlayer(List<MpaWinLossByPlayerDto> combineData)
        {
            return combineData.GroupBy(x => new
            {
                x.UserName,
                x.TpUserName,
                x.Currency,
            })
            .Select(r => new MpaWinLossByPlayerDto
            {
                UserName = r.Key.UserName,
                TpUserName = r.Key.TpUserName,
                Currency = r.Key.Currency,
                ClosingBalance = r.Sum(x => x.ClosingBalance),
                TurnOver = r.Sum(x => x.TurnOver),
                ValidTurnOver = r.Sum(x => x.ValidTurnOver),
                GGR = r.Sum(x => x.GGR),
                PlayerWinLoss = r.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = r.Sum(x => x.ProgressiveWin),
                ProgressiveBet = r.Sum(x => x.ProgressiveBet),
                Commission = r.Sum(x => x.Commission),
                BetCount = r.Sum(x => x.BetCount),
                UniquePlayers = r.Sum(x => x.UniquePlayers),
                DepositCount = r.Sum(x => x.DepositCount),
                TotalDepositAmount = r.Sum(x => x.TotalDepositAmount),
                WithDrawCount = r.Sum(x => x.WithDrawCount),
                TotalWithDrawAmount = r.Sum(x => x.TotalWithDrawAmount),
                AvgBetAmount = r.Sum(x => x.BetCount) != 0 ? r.Sum(x => x.ValidTurnOver) / r.Sum(x => x.BetCount) : 0.00m,
                Margin = r.Sum(x => x.ValidTurnOver) != 0 ? r.Sum(x => x.GGR) / r.Sum(x => x.ValidTurnOver) : 0.00m,
                TotalBonus = r.Sum(x => x.TotalBonus)
            });
        }

        private static Dictionary<string, MpaWinLossByPlayerCurrencyDetailDto> GetCurrencyDetailByPlayer(List<MpaWinLossByPlayerDto> dateSummary)
        {
            return dateSummary.ToDictionary(k => k.Currency, v => new MpaWinLossByPlayerCurrencyDetailDto
            {
                UserName = v.UserName,
                SabaId = v.TpUserName,
                TurnOver = v.TurnOver,
                ValidTurnOver = v.ValidTurnOver,
                PlayerWinLoss = v.PlayerWinLoss,
                ProgressiveWin = v.ProgressiveWin,
                ProgressiveBet = v.ProgressiveBet,
                Commission = v.Commission,
                BetCount = v.BetCount,
                UniquePlayers = v.UniquePlayers,
                ClosingBalance = v.ClosingBalance,
                DepositCount = v.DepositCount,
                TotalDepositAmount = v.TotalDepositAmount,
                WithDrawCount = v.WithDrawCount,
                TotalWithDrawAmount = v.TotalWithDrawAmount,
                TotalBonus = v.TotalBonus
            });
        }

        private static Dictionary<string, MpaWinLossByPlayerCurrencyDetailDto> GetOriginalCurrencyDetailByPlayer(IEnumerable<MpaWinLossByPlayerDto> combineResult, string key)
        {
            return combineResult.Where(x => x.UserName == key)
                .ToDictionary(k => k.Currency, v => new MpaWinLossByPlayerCurrencyDetailDto
                {
                    UserName = v.UserName,
                    SabaId = v.TpUserName,
                    TurnOver = v.TurnOver,
                    ValidTurnOver = v.ValidTurnOver,
                    PlayerWinLoss = v.PlayerWinLoss,
                    ProgressiveWin = v.ProgressiveWin,
                    ProgressiveBet = v.ProgressiveBet,
                    Commission = v.Commission,
                    BetCount = v.BetCount,
                    UniquePlayers = v.UniquePlayers,
                    ClosingBalance = v.ClosingBalance,
                    DepositCount = v.DepositCount,
                    TotalDepositAmount = v.TotalDepositAmount,
                    WithDrawCount = v.WithDrawCount,
                    TotalWithDrawAmount = v.TotalWithDrawAmount,
                    TotalBonus = v.TotalBonus
                });
        }

        private async Task<TpBetRecordByPlayerWrapper> GetTpBetRecordGroupByPlayerResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單過濾
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);

            var data = await GetMapTpBetRecordByPlayer(request, groupByType);
            var tpBetRecordRelayResult = data.Filter(request)
                .SetGgRByOneGamingPoker()
                .SetGgRByV8Poker()
                .SetCommissionByNineW(request, format, _mpaWinLossService.Get9WCommission)
                .ToList();

            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var tpBetRecordResult = tpBetRecordRelayResult.GroupBy(x => new
            {
                x.UserName,
                x.TpUserName,
                x.CurrencyId
            }).Select(r => new MpaWinLossByPlayerDto
            {
                UserName = r.Key.UserName,
                TpUserName = r.Key.TpUserName,
                Currency = currencyNameMapper[r.Key.CurrencyId],
                TurnOver = r.Sum(x => x.StakeSummary),
                ClosingBalance = r.Select(x => x.ClosingBalance).FirstOrDefault(),
                ValidTurnOver = r.Sum(x => x.ValidBetSummary),
                GGR = r.Sum(x => x.WinLossSummary) * -1,
                PlayerWinLoss = r.Sum(x => x.WinLossSummary),
                ProgressiveWin = r.Sum(x => x.ProgressiveWinSummary),
                ProgressiveBet = r.Sum(x => x.ProgressiveBetSummary),
                Commission = r.Sum(x => x.RevenueSummary),
                BetCount = r.Sum(x => x.RowCount),
                AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.ValidBetSummary) / r.Sum(x => x.RowCount) : 0.00m,
                Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                UniquePlayers = r.DistinctBy(x => x.CustId).Count()
            });

            var tpBetRecordRelayResultWrapper = new TpBetRecordByPlayerWrapper()
            {
                TpBetRecordResult = tpBetRecordResult,
                UniquePlayers = tpBetRecordRelayResult.DistinctBy(x => x.CustId).Count()
            };

            return tpBetRecordRelayResultWrapper;
        }


        #endregion

        #region GroupBy Player Detail

        public async Task<Dictionary<string, MpaWinLossByPlayerDetailResponse>> GetMpaWinLossByPlayerDetailResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料
            var data = await GetTpBetRecordGroupByPlayerDetailResult(request, groupByType);
            var tpBetRecordResult = data.TpBetRecordResult;

            // 注單
            var combineData = tpBetRecordResult.ToList();
            var rateMap = await GetCurrencyNameRateMapper();
            var keys = combineData.Select(x => x.GameTypeName).Distinct();
            var exportData = new Dictionary<string, MpaWinLossByPlayerDetailResponse>();

            foreach (var key in keys)
            {
                var dateSummary = combineData.Where(x => x.GameTypeName == key).ToList();
                foreach (var detail in dateSummary)
                {
                    var rate = request.DisplayGroupBreakdownInOriginalCurrency ? 1 : rateMap[detail.Currency];
                    detail.TurnOver = detail.TurnOver / rate;
                    detail.ValidTurnOver = detail.ValidTurnOver / rate;
                    detail.GGR = detail.GGR / rate;
                    detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                    detail.ProgressiveWin = detail.ProgressiveWin / rate;
                    detail.ProgressiveBet = detail.ProgressiveBet / rate;
                    detail.Commission = detail.Commission / rate;
                    detail.AvgBetAmount = detail.AvgBetAmount / rate;
                    detail.Margin = detail.Margin / rate;
                }

                var result = new MpaWinLossByPlayerDetailResponse
                {
                    SabaId = dateSummary.Select(r => r.TpUserName).FirstOrDefault(),
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = dateSummary.Sum(x => x.GGR),
                    PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = dateSummary.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = dateSummary.Sum(x => x.ProgressiveBet),
                    Commission = dateSummary.Sum(x => x.Commission),
                    BetCount = dateSummary.Sum(x => x.BetCount),
                    UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                    AvgBetAmount = dateSummary.Sum(x => x.BetCount) != 0
                        ? dateSummary.Sum(x => x.ValidTurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                };

                exportData.Add(key, result);

            }

            return exportData;
        }


        private async Task<TpBetRecordByPlayerWrapper> GetTpBetRecordGroupByPlayerDetailResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單過濾
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);

            var data = await GetMapTpBetRecordByDate(request, groupByType);
            var tpBetRecordRelayResult = data.Filter(request)
                .SetGgRByOneGamingPoker()
                .SetGgRByV8Poker()
                .SetCommissionByNineW(request, format, _mpaWinLossService.Get9WCommission)
                .ToList();

            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var tpGameTypeMap = await GetTpGameTypeMapper();

            var tpBetRecordResult = tpBetRecordRelayResult.GroupBy(x => new
            {
                x.UserName,
                x.TpUserName,
                x.CurrencyId,
                x.GameType
            }).Select(r => new MpaWinLossByPlayerDto
            {
                UserName = r.Key.UserName,
                TpUserName = r.Key.TpUserName,
                Currency = currencyNameMapper[r.Key.CurrencyId],
                GameTypeName = tpGameTypeMap[r.Key.GameType],
                TurnOver = r.Sum(x => x.StakeSummary),
                ValidTurnOver = r.Sum(x => x.ValidBetSummary),
                GGR = r.Sum(x => x.WinLossSummary) * -1,
                PlayerWinLoss = r.Sum(x => x.WinLossSummary),
                ProgressiveWin = r.Sum(x => x.ProgressiveWinSummary),
                ProgressiveBet = r.Sum(x => x.ProgressiveBetSummary),
                Commission = r.Sum(x => x.RevenueSummary),
                BetCount = r.Sum(x => x.RowCount),
                AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.ValidBetSummary) / r.Sum(x => x.RowCount) : 0.00m,
                Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                UniquePlayers = r.DistinctBy(x => x.CustId).Count()
            });

            var tpBetRecordRelayResultWrapper = new TpBetRecordByPlayerWrapper()
            {
                TpBetRecordResult = tpBetRecordResult,
                UniquePlayers = tpBetRecordRelayResult.DistinctBy(x => x.CustId).Count()
            };

            return tpBetRecordRelayResultWrapper;
        }

        #endregion

        #region GroupBy Currency

        public async Task<Dictionary<string, MpaWinLossByCurrencyResponse>> GetMapWinLossByCurrencyResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料
            var data = await GetTpBetRecordGroupByCurrencyResult(request, groupByType);
            var tpBetRecordResult = data.TpBetRecordResult;

            // 存提款 
            var transactionResult = await GetTransactionByCurrencyResult(request, groupByType);

            // bonus
            var bonusResult = await GetBonusByCurrencyResult(request, groupByType);

            // 注單 + 存提款資料合併
            var combineData = request.OrderBy
                ? tpBetRecordResult
                    .Concat(transactionResult)
                    .Concat(bonusResult)
                    .OrderBy(request)
                    .ToList()
                : tpBetRecordResult
                    .Concat(transactionResult)
                    .Concat(bonusResult)
                    .ToList();

            var combineResult = combineData.GroupBy(x => new
            {
                x.Currency,
            })
            .Select(r => new MpaWinLossByCurrencyDto
            {
                Currency = r.Key.Currency,
                TurnOver = r.Sum(x => x.TurnOver),
                ValidTurnOver = r.Sum(x => x.ValidTurnOver),
                GGR = r.Sum(x => x.GGR),
                PlayerWinLoss = r.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = r.Sum(x => x.ProgressiveWin),
                ProgressiveBet = r.Sum(x => x.ProgressiveBet),
                Commission = r.Sum(x => x.Commission),
                BetCount = r.Sum(x => x.BetCount),
                UniquePlayers = r.Sum(x => x.UniquePlayers),
                DepositCount = r.Sum(x => x.DepositCount),
                TotalDepositAmount = r.Sum(x => x.TotalDepositAmount),
                WithDrawCount = r.Sum(x => x.WithDrawCount),
                TotalWithDrawAmount = r.Sum(x => x.TotalWithDrawAmount),
                AvgBetAmount = r.Sum(x => x.BetCount) != 0 ? r.Sum(x => x.ValidTurnOver) / r.Sum(x => x.BetCount) : 0.00m,
                Margin = r.Sum(x => x.ValidTurnOver) != 0 ? r.Sum(x => x.GGR) / r.Sum(x => x.ValidTurnOver) : 0.00m,
                TotalBonus = r.Sum(x => x.TotalBonus)
            });

            var exportData = new Dictionary<string, MpaWinLossByCurrencyResponse>();
            var keys = combineResult.Select(x => x.Currency).Distinct();
            var rateMap = await GetCurrencyNameRateMapper();

            foreach (var key in keys)
            {
                var dateSummary = combineResult.Where(x => x.Currency == key).ToList();
                foreach (var detail in dateSummary)
                {
                    var rate = rateMap[detail.Currency];
                    detail.TurnOver = detail.TurnOver / rate;
                    detail.ValidTurnOver = detail.ValidTurnOver / rate;
                    detail.GGR = detail.GGR / rate;
                    detail.PlayerWinLoss = detail.PlayerWinLoss / rate;
                    detail.ProgressiveWin = detail.ProgressiveWin / rate;
                    detail.ProgressiveBet = detail.ProgressiveBet / rate;
                    detail.Commission = detail.Commission / rate;
                    detail.AvgBetAmount = detail.AvgBetAmount / rate;
                    detail.Margin = detail.Margin / rate;
                    detail.TotalDepositAmount = detail.TotalDepositAmount / rate;
                    detail.TotalWithDrawAmount = detail.TotalWithDrawAmount / rate;
                    detail.TotalBonus = detail.TotalBonus / rate;
                }

                var result = new MpaWinLossByCurrencyResponse
                {
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = dateSummary.Sum(x => x.GGR),
                    PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = dateSummary.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = dateSummary.Sum(x => x.ProgressiveBet),
                    Commission = dateSummary.Sum(x => x.Commission),
                    BetCount = dateSummary.Sum(x => x.BetCount),
                    UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                    DepositCount = dateSummary.Sum(x => x.DepositCount),
                    TotalDepositAmount = dateSummary.Sum(x => x.TotalDepositAmount),
                    WithDrawCount = dateSummary.Sum(x => x.WithDrawCount),
                    TotalWithDrawAmount = dateSummary.Sum(x => x.TotalWithDrawAmount),
                    AvgBetAmount = dateSummary.Sum(x => x.BetCount) != 0
                        ? dateSummary.Sum(x => x.ValidTurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                    CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByCurrency(combineResult, key)
                    : GetCurrencyDetailByCurrency(dateSummary)
                };

                exportData.Add(key, result);
            }

            // total
            exportData.Add("total", new MpaWinLossByCurrencyResponse()
            {
                TurnOver = exportData.Sum(x => x.Value.TurnOver),
                ValidTurnOver = exportData.Sum(x => x.Value.ValidTurnOver),
                GGR = exportData.Sum(x => x.Value.GGR),
                PlayerWinLoss = exportData.Sum(x => x.Value.PlayerWinLoss),
                ProgressiveWin = exportData.Sum(x => x.Value.ProgressiveWin),
                ProgressiveBet = exportData.Sum(x => x.Value.ProgressiveBet),
                Commission = exportData.Sum(x => x.Value.Commission),
                BetCount = exportData.Sum(x => x.Value.BetCount),
                UniquePlayers = data.UniquePlayers,
                DepositCount = exportData.Sum(x => x.Value.DepositCount),
                TotalDepositAmount = exportData.Sum(x => x.Value.TotalDepositAmount),
                WithDrawCount = exportData.Sum(x => x.Value.WithDrawCount),
                TotalWithDrawAmount = exportData.Sum(x => x.Value.TotalWithDrawAmount),
                TotalBonus = exportData.Sum(x => x.Value.TotalBonus),
                AvgBetAmount = exportData.Sum(x => x.Value.BetCount) != 0
                    ? (exportData.Sum(x => x.Value.ValidTurnOver) / exportData.Sum(x => x.Value.BetCount))
                    : 0.00m,

                Margin = exportData.Sum(x => x.Value.ValidTurnOver) != 0
                    ? exportData.Sum(x => x.Value.GGR) / exportData.Sum(x => x.Value.ValidTurnOver)
                    : 0.00m,
            });

            return exportData;
        }

        private static Dictionary<string, MpaWinLossByCurrencyCurrencyDetailDto> GetCurrencyDetailByCurrency(List<MpaWinLossByCurrencyDto> dateSummary)
        {
            return dateSummary.ToDictionary(k => k.Currency, v => new MpaWinLossByCurrencyCurrencyDetailDto
            {
                TurnOver = v.TurnOver,
                ValidTurnOver = v.ValidTurnOver,
                PlayerWinLoss = v.PlayerWinLoss,
                ProgressiveWin = v.ProgressiveWin,
                ProgressiveBet = v.ProgressiveBet,
                Commission = v.Commission,
                BetCount = v.BetCount,
                UniquePlayers = v.UniquePlayers,
                DepositCount = v.DepositCount,
                TotalDepositAmount = v.TotalDepositAmount,
                WithDrawCount = v.WithDrawCount,
                TotalWithDrawAmount = v.TotalWithDrawAmount,
                TotalBonus = v.TotalBonus
            });
        }

        private static Dictionary<string, MpaWinLossByCurrencyCurrencyDetailDto> GetOriginalCurrencyDetailByCurrency(IEnumerable<MpaWinLossByCurrencyDto> combineResult, string key)
        {
            return combineResult.Where(x => x.Currency == key)
                .ToDictionary(k => k.Currency, v => new MpaWinLossByCurrencyCurrencyDetailDto
                {
                    TurnOver = v.TurnOver,
                    ValidTurnOver = v.ValidTurnOver,
                    PlayerWinLoss = v.PlayerWinLoss,
                    ProgressiveWin = v.ProgressiveWin,
                    ProgressiveBet = v.ProgressiveBet,
                    Commission = v.Commission,
                    BetCount = v.BetCount,
                    UniquePlayers = v.UniquePlayers,
                    DepositCount = v.DepositCount,
                    TotalDepositAmount = v.TotalDepositAmount,
                    WithDrawCount = v.WithDrawCount,
                    TotalWithDrawAmount = v.TotalWithDrawAmount,
                    TotalBonus = v.TotalBonus
                });
        }

        private async Task<TpBetRecordByCurrencyWrapper> GetTpBetRecordGroupByCurrencyResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {

            // 注單過濾
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);

            var data = await GetMapTpBetRecordByDate(request, groupByType);
            var tpBetRecordRelayResult = data.Filter(request)
                .SetGgRByOneGamingPoker()
                .SetGgRByV8Poker()
                .SetCommissionByNineW(request, format, _mpaWinLossService.Get9WCommission)
                .ToList();

            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var tpBetRecordResult = tpBetRecordRelayResult.GroupBy(x => new
            {
                x.CurrencyId
            }).Select(r => new MpaWinLossByCurrencyDto
            {
                Currency = currencyNameMapper[r.Key.CurrencyId],
                TurnOver = r.Sum(x => x.StakeSummary),
                ValidTurnOver = r.Sum(x => x.ValidBetSummary),
                GGR = r.Sum(x => x.WinLossSummary) * -1,
                PlayerWinLoss = r.Sum(x => x.WinLossSummary),
                ProgressiveWin = r.Sum(x => x.ProgressiveWinSummary),
                ProgressiveBet = r.Sum(x => x.ProgressiveBetSummary),
                Commission = r.Sum(x => x.RevenueSummary),
                BetCount = r.Sum(x => x.RowCount),
                AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.ValidBetSummary) / r.Sum(x => x.RowCount) : 0.00m,
                Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                UniquePlayers = r.DistinctBy(x => x.CustId).Count()
            });

            var tpBetRecordRelayResultWrapper = new TpBetRecordByCurrencyWrapper()
            {
                TpBetRecordResult = tpBetRecordResult,
                UniquePlayers = tpBetRecordRelayResult.DistinctBy(x => x.CustId).Count()
            };

            return tpBetRecordRelayResultWrapper;
        }


        #endregion

        #region Transaction (存/提)款

        /// <summary>
        /// 取得 (充值單/提款單) 結果 By 日期
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<IEnumerable<MpaWinLossByDateDto>> GetTransactionByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var transactionRelayResult = (await GetMapTransactionByDate(request, groupByType))
                .Where(r => request.CurrencyIds.Contains(r.CurrencyId));

            if (!string.IsNullOrEmpty(request.UserName))
            {
                transactionRelayResult = transactionRelayResult.Where(r => r.UserName == request.UserName || r.TpUserName == request.UserName);
            }

            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var transactionResult = transactionRelayResult.GroupBy(x => new
            {
                x.Date,
                x.CurrencyId,
                x.SummaryType,
            })
            .Select(r => new MpaWinLossByDateDto
            {
                Date = groupByType == MpaWinLossGroupByTypeEnum.Date
                        ? r.Key.Date.ToString("yyyy-MM-dd")
                        : r.Key.Date.ToString("yyyy-MM"),
                Currency = currencyNameMapper[r.Key.CurrencyId],
                DepositCount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Deposit
                    ? r.Sum(x => x.RowCount)
                    : 0,
                TotalDepositAmount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Deposit
                    ? r.Sum(x => x.Amount)
                    : 0,
                WithDrawCount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Withdrawal
                    ? r.Sum(x => x.RowCount)
                    : 0,
                TotalWithDrawAmount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Withdrawal
                    ? r.Sum(x => x.Amount)
                    : 0
            }).ToList();

            return transactionResult;
        }

        /// <summary>
        /// 取得 (充值單/提款單) 結果 by Player
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<IEnumerable<MpaWinLossByPlayerDto>> GetTransactionByPlayerResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var transactionRelayResult = (await GetMapTransactionByDate(request, groupByType))
                .Where(r => request.CurrencyIds.Contains(r.CurrencyId));

            if (!string.IsNullOrEmpty(request.UserName))
            {
                transactionRelayResult = transactionRelayResult.Where(r => r.UserName == request.UserName || r.TpUserName == request.UserName);
            }

            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var transactionResult = transactionRelayResult.GroupBy(x => new
            {
                x.UserName,
                x.TpUserName,
                x.CurrencyId,
                x.SummaryType,
            })
            .Select(r => new MpaWinLossByPlayerDto
            {
                UserName = r.Key.UserName,
                TpUserName = r.Key.TpUserName,
                Currency = currencyNameMapper[r.Key.CurrencyId],
                DepositCount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Deposit
                    ? r.Sum(x => x.RowCount)
                    : 0,
                TotalDepositAmount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Deposit
                    ? r.Sum(x => x.Amount)
                    : 0,
                WithDrawCount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Withdrawal
                    ? r.Sum(x => x.RowCount)
                    : 0,
                TotalWithDrawAmount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Withdrawal
                    ? r.Sum(x => x.Amount)
                    : 0
            }).ToList();

            return transactionResult;
        }

        /// <summary>
        /// 取得 (充值單/提款單) 結果 By Currency
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<IEnumerable<MpaWinLossByCurrencyDto>> GetTransactionByCurrencyResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var transactionRelayResult = (await GetMapTransactionByDate(request, groupByType))
                .Where(r => request.CurrencyIds.Contains(r.CurrencyId));

            if (!string.IsNullOrEmpty(request.UserName))
            {
                transactionRelayResult = transactionRelayResult.Where(r => r.UserName == request.UserName || r.TpUserName == request.UserName);
            }

            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var transactionResult = transactionRelayResult.GroupBy(x => new
            {
                x.CurrencyId,
                x.SummaryType,
            })
                .Select(r => new MpaWinLossByCurrencyDto
                {
                    Currency = currencyNameMapper[r.Key.CurrencyId],
                    DepositCount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Deposit
                        ? r.Sum(x => x.RowCount)
                        : 0,
                    TotalDepositAmount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Deposit
                        ? r.Sum(x => x.Amount)
                        : 0,
                    WithDrawCount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Withdrawal
                        ? r.Sum(x => x.RowCount)
                        : 0,
                    TotalWithDrawAmount = r.Key.SummaryType == MpaTransactionRelaySummaryTypeEnum.Withdrawal
                        ? r.Sum(x => x.Amount)
                        : 0
                }).ToList();

            return transactionResult;
        }



        #endregion

        #region Bouns 

        private async Task<IEnumerable<MpaWinLossByDateDto>> GetBonusByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {

            var bonusRelayResult = (await GetMapWinLossBonusByDate(request, groupByType))
                .Where(r => request.CurrencyIds.Contains(r.CurrencyId));

            if (!string.IsNullOrEmpty(request.UserName))
            {
                bonusRelayResult = bonusRelayResult.Where(r => r.UserName == request.UserName || r.TpUserName == request.UserName);
            }

            var currencyNameMapper = await GetCurrencyIsoCodeMapper();

            var bonusResult = bonusRelayResult.GroupBy(x => new
            {
                x.Date,
                x.CurrencyId
            }).Select(r => new MpaWinLossByDateDto
            {
                Date = groupByType == MpaWinLossGroupByTypeEnum.Date
                            ? r.Key.Date.ToString("yyyy-MM-dd")
                            : r.Key.Date.ToString("yyyy-MM"),
                Currency = currencyNameMapper[r.Key.CurrencyId],
                TotalBonus = r.Sum(x => x.BonusAmountClaim)
            }).ToList();


            return bonusResult;
        }

        private async Task<IEnumerable<MpaWinLossByPlayerDto>> GetBonusByPlayerResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {

            var bonusRelayResult = (await GetMapWinLossBonusByDate(request, groupByType))
                .Where(r => request.CurrencyIds.Contains(r.CurrencyId));

            if (!string.IsNullOrEmpty(request.UserName))
            {
                bonusRelayResult = bonusRelayResult.Where(r => r.UserName == request.UserName || r.TpUserName == request.UserName);
            }

            var currencyNameMapper = await GetCurrencyIsoCodeMapper();

            var bonusResult = bonusRelayResult.GroupBy(x => new
            {
                x.UserName,
                x.TpUserName,
                x.CurrencyId,
            }).Select(r => new MpaWinLossByPlayerDto
            {
                UserName = r.Key.UserName,
                TpUserName = r.Key.TpUserName,
                Currency = currencyNameMapper[r.Key.CurrencyId],
                TotalBonus = r.Sum(x => x.BonusAmountClaim)
            }).ToList();

            return bonusResult;
        }


        private async Task<IEnumerable<MpaWinLossByCurrencyDto>> GetBonusByCurrencyResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var bonusRelayResult = (await GetMapWinLossBonusByDate(request, groupByType))
                .Where(r => request.CurrencyIds.Contains(r.CurrencyId));

            if (!string.IsNullOrEmpty(request.UserName))
            {
                bonusRelayResult = bonusRelayResult.Where(r => r.UserName == request.UserName || r.TpUserName == request.UserName);
            }

            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var bonusResult = bonusRelayResult.GroupBy(x => new
            {
                x.CurrencyId,
            })
                .Select(r => new MpaWinLossByCurrencyDto
                {
                    Currency = currencyNameMapper[r.Key.CurrencyId],
                    TotalBonus = r.Sum(x => x.BonusAmountClaim)
                }).ToList();

            return bonusResult;
        }


        #endregion

        #region Common

        /// <summary>
        /// 取得幣別對應
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string, decimal>> GetCurrencyNameRateMapper()
        {
            var usdRateResult = await GetMpaUSdRate();
            return usdRateResult
                .ToDictionary(k => k.IsoCode, v => v.USDRate);
        }

        /// <summary>
        /// 取得幣別對應
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<int, decimal>> GetCurrencyIdRateMapper()
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


        /// <summary>
        /// 取得每個TpGameType + Name
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<int, string>> GetTpGameTypeMapper()
        {
            const string redisKey = "MpaTpGameTypeMapper";
            var data = await _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromHours(12),
                () => _mpaWinLossService.GetTpGameTypeOption());

            return data.ToDictionary(x => x.Type, v => v.TypeName);
        }


        public async Task<Dictionary<int, string>> GetTpCategoryMapper()
        {
            const string redisKey = "MpaTpCategoryMapper";
            var data = await _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromHours(12),
                () => _mpaWinLossService.GetTpCategory());

            return data.ToDictionary(x => x.CategoryId, v => v.CategoryName);
        }

        /// <summary>
        /// 取得 TpBetRecordRelay by Date
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<IEnumerable<MpaTpBetRecordDto>> GetMapTpBetRecordByDate(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var redisKey =
                $"MapTpBetRecord:{groupByType}:SiteId:{request.SiteId}:DateType:{request.DateType}:{request.StarTime:yyyy-MM-dd HH:mm:ss}:{request.EndTime:yyyy-MM-dd HH:mm:ss}";

            if (_redisHelper.KeyExists(redisKey))
                return await _redisHelper.GetListByRangeAsync<MpaTpBetRecordDto>(redisKey, 0, -1);
            var data = await _mpaWinLossService.GetMapTpBetRecord<MpaTpBetRecordDto>(request.SiteId, request.StarTime,
                request.EndTime, format, request.DateType);
            var _ = _redisHelper.InsertList(redisKey, TimeSpan.FromMinutes(15), data);
            return data;
        }

        /// <summary>
        /// 取得 TpBetRecordRelay by Player
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<IEnumerable<MpaTpBetRecordDto>> GetMapTpBetRecordByPlayer(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var redisKey =
                $"MapTpBetRecordByPlayer:{groupByType}:SiteId:{request.SiteId}:DateType:{request.DateType}:{request.StarTime:yyyy-MM-dd HH:mm:ss}:{request.EndTime:yyyy-MM-dd HH:mm:ss}";

            if (_redisHelper.KeyExists(redisKey))
                return await _redisHelper.GetListByRangeAsync<MpaTpBetRecordDto>(redisKey, 0, -1);
            var data = await _mpaWinLossService.GetMapTpBetRecordByPlayer<MpaTpBetRecordDto>(request.SiteId, request.StarTime,
                request.EndTime, format, request.DateType);
            var _ = _redisHelper.InsertList(redisKey, TimeSpan.FromMinutes(15), data);
            return data;
        }



        /// <summary>
        ///  取得 Deposit And Withdrawal Relay By Date
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<IEnumerable<TransactionRelayDto>> GetMapTransactionByDate(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var redisKey =
                $"MapTransaction:{groupByType}:SiteId:{request.SiteId}:{request.StarTime:yyyy-MM-dd HH:mm:ss}:{request.EndTime:yyyy-MM-dd HH:mm:ss}";

            if (_redisHelper.KeyExists(redisKey))
                return await _redisHelper.GetListByRangeAsync<TransactionRelayDto>(redisKey, 0, -1);
            var data = await _mpaWinLossService.GetMapTransaction<TransactionRelayDto>(request.SiteId, request.StarTime,
                request.EndTime, format);
            var _ = _redisHelper.InsertList(redisKey, TimeSpan.FromMinutes(15), data);
            return data;
        }

        private async Task<IEnumerable<MpaWinLossBonusDto>> GetMapWinLossBonusByDate(MpaWinLossRequest request,
            MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var redisKey =
                $"MapBonus:{groupByType}:SiteId:{request.SiteId}:{request.StarTime:yyyy-MM-dd HH:mm:ss}:{request.EndTime:yyyy-MM-dd HH:mm:ss}";
            if (_redisHelper.KeyExists(redisKey))
                return await _redisHelper.GetListByRangeAsync<MpaWinLossBonusDto>(redisKey, 0, -1);

            var data = await _mpaWinLossService.GetMpaWinLossBonus(request.SiteId, request.StarTime, request.EndTime, format);
            var _ = _redisHelper.InsertList(redisKey, TimeSpan.FromMinutes(15), data);
            return data;
        }

        private void AddCache(List<MpaWinLossByPlayerResponse> list, string masterKey, string sortKey, TimeSpan cacheTime, bool orderBy)
        {
            // Add Cache by List      
            _ = Task.Run(() =>
            {
                _redisHelper.InsertList(masterKey, cacheTime, list);
                if (orderBy)
                {
                    _redisHelper.StringSetAsync(sortKey, sortKey, cacheTime);
                }
            });
        }


        private static string MapMpaWinLossGroupByTypeEnumFormat(MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = groupByType switch
            {
                MpaWinLossGroupByTypeEnum.Date => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.GameType => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.TpCategory => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.Player => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.Currency => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.Month => "%Y-%m",
                _ => string.Empty
            };
            return format;
        }

        #endregion
    }
}


