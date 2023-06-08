using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model.enums;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.BLL.Mpa.Extension;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model.Mpa.WinLoss;
using System.Diagnostics;

namespace BackendSite.Service.BLL.Mpa
{
    public class MpaWinLossBLL : IMpaWinLossBLL
    {
        private readonly IMpaWinLossService _mpaWinLossService;
        private readonly IMpaCommonService _mpaCommonService;
        private readonly ICacheHelper _redisHelper;
        private readonly ILogger<MpaWinLossBLL> _logger;

        public MpaWinLossBLL(IMpaWinLossService mpaWinLossService, ICacheHelper redisHelper, IMpaCommonService mpaCommonService, ILogger<MpaWinLossBLL> logger)
        {
            _mpaWinLossService = mpaWinLossService;
            _mpaCommonService = mpaCommonService;
            _redisHelper = redisHelper;
            _logger = logger;
        }

        #region GropuByDate、Month

        /// <summary>
        /// excel Export
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MpaWinLossByDateDto>> GetMpaWinLossByDateExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料 & 存提款 & Bonus
            var bonusTask = Task.Run(() => GetBonusByDateResult(request, groupByType));
            var betRecordTask = Task.Run(() => GetTpBetRecordGroupByDateResult(request, groupByType));
            var transactionTask = Task.Run(() => GetTransactionByDateResult(request, groupByType));
            await Task.WhenAll(betRecordTask, transactionTask, bonusTask);
            var betRecord = betRecordTask.Result;
            var transaction = transactionTask.Result;
            var bonus = bonusTask.Result;

            // 組合結果
            var originCombineResult = ByDateCombineResult(request, betRecord.OriginTpBetRecordResult, transaction.OriginTransactionResult, bonus.OriginBonusResult).ToList();
            var alreadyMultiplyRateCombineResult = ByDateCombineResult(request, betRecord.AlreadyMultiplyRateTpBetRecordResult, transaction.AlreadyMultiplyRateTransactionResult, bonus.AlreadyMultiplyRateBonusResult).ToList();

            var exportData = new List<MpaWinLossByDateDto>();
            exportData.AddRange(request.DisplayGroupBreakdownInOriginalCurrency
            ? originCombineResult
            : alreadyMultiplyRateCombineResult);

            var keys = originCombineResult.Select(x => x.Date).Distinct();
            foreach (var key in keys)
            {
                var dateSummary = alreadyMultiplyRateCombineResult.Where(x => x.Date == key).ToList();
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
                        ? dateSummary.Sum(x => x.TurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                };

                exportData.Add(result);
            }

            exportData = request.OrderBy
                ? exportData.OrderBy(request).ToList()
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
                UniquePlayers = betRecord.UniquePlayers,
                DepositCount = exportData.Sum(x => x.DepositCount),
                TotalDepositAmount = exportData.Sum(x => x.TotalDepositAmount),
                WithDrawCount = exportData.Sum(x => x.WithDrawCount),
                TotalWithDrawAmount = exportData.Sum(x => x.TotalWithDrawAmount),
                TotalBonus = exportData.Sum(x => x.TotalBonus),
                AvgBetAmount = exportData.Sum(x => x.BetCount) != 0
                    ? (exportData.Sum(x => x.TurnOver) / exportData.Sum(x => x.BetCount))
                    : 0.00m,
                Margin = exportData.Sum(x => x.ValidTurnOver) != 0
                    ? exportData.Sum(x => x.GGR) / exportData.Sum(x => x.ValidTurnOver)
                    : 0.00m,
            });

            return exportData.ToExcelFormat();
        }

        /// <summary>
        /// api 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MpaWinLossByDateResponseToFrontEnd>> GetMapWinLossByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料 & 存提款 & Bonus
            var sw = new Stopwatch();
            sw.Start();

            var bonusTask = Task.Run(() => GetBonusByDateResult(request, groupByType));
            var betRecordTask = Task.Run(() => GetTpBetRecordGroupByDateResult(request, groupByType));
            var transactionTask = Task.Run(() => GetTransactionByDateResult(request, groupByType));
            await Task.WhenAll(betRecordTask, transactionTask, bonusTask);

            sw.Stop();
            _logger.LogInformation($"MickeyTest Task.WhenAll ms:{sw.Elapsed.TotalMilliseconds}");

            sw.Restart();

            var betRecord = betRecordTask.Result;
            var transaction = transactionTask.Result;
            var bonus = bonusTask.Result;

            // 組合結果
            var originCombineResult = ByDateCombineResult(request, betRecord.OriginTpBetRecordResult, transaction.OriginTransactionResult, bonus.OriginBonusResult).ToList();
            var alreadyMultiplyRateCombineResult = ByDateCombineResult(request, betRecord.AlreadyMultiplyRateTpBetRecordResult, transaction.AlreadyMultiplyRateTransactionResult, bonus.AlreadyMultiplyRateBonusResult).ToList();

            sw.Stop();
            _logger.LogInformation($"MickeyTest CombineResult ms:{sw.Elapsed.TotalMilliseconds}");

            sw.Restart();

            var exportData = new Dictionary<string, MpaWinLossByDateResponse>();
            var keys = originCombineResult.Select(x => x.Date).Distinct();

            foreach (var key in keys)
            {
                var dateSummary = alreadyMultiplyRateCombineResult.Where(x => x.Date == key).ToList();
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
                        ? dateSummary.Sum(x => x.TurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                    CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByDate(originCombineResult, key)
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
                UniquePlayers = betRecord.UniquePlayers,
                DepositCount = exportData.Sum(x => x.Value.DepositCount),
                TotalDepositAmount = exportData.Sum(x => x.Value.TotalDepositAmount),
                WithDrawCount = exportData.Sum(x => x.Value.WithDrawCount),
                TotalWithDrawAmount = exportData.Sum(x => x.Value.TotalWithDrawAmount),
                TotalBonus = exportData.Sum(x => x.Value.TotalBonus),
                AvgBetAmount = exportData.Sum(x => x.Value.BetCount) != 0
                    ? (exportData.Sum(x => x.Value.TurnOver) / exportData.Sum(x => x.Value.BetCount))
                    : 0.00m,

                Margin = exportData.Sum(x => x.Value.ValidTurnOver) != 0
                    ? exportData.Sum(x => x.Value.GGR) / exportData.Sum(x => x.Value.ValidTurnOver)
                    : 0.00m,
            });
            var rs = exportData.ToApiFormat();

            sw.Stop();
            _logger.LogInformation($"MickeyTest exportData&ToApiFormat ms:{sw.Elapsed.TotalMilliseconds}");

            return rs;
        }

        /// <summary>
        /// tpBetRecord + payment + bonus
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tpBetRecordResult"></param>
        /// <param name="transactionResult"></param>
        /// <param name="bonusResult"></param>
        /// <returns></returns>
        private static IEnumerable<MpaWinLossByDateDto> ByDateCombineResult(MpaWinLossRequest request, IEnumerable<MpaWinLossByDateDto> tpBetRecordResult,
            IEnumerable<MpaWinLossByDateDto> transactionResult, IEnumerable<MpaWinLossByDateDto> bonusResult)
        {
            // 注單 + 存提款資料合併 + bonus
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
                AvgBetAmount = r.Sum(x => x.BetCount) != 0 ? r.Sum(x => x.TurnOver) / r.Sum(x => x.BetCount) : 0.00m,
                Margin = r.Sum(x => x.ValidTurnOver) != 0 ? r.Sum(x => x.GGR) / r.Sum(x => x.ValidTurnOver) : 0.00m,
                TotalBonus = r.Sum(x => x.TotalBonus)
            });

            return request.OrderBy ? combineResult.OrderBy(request) : combineResult.OrderBy(x => x.Date).ThenBy(x => x.Currency);
        }

        /// <summary>
        /// db 拿注單
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<TpBetRecordByDateWrapper> GetTpBetRecordGroupByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var betRecordResult = await GetTpBetRecord(request, groupByType);
            var originTpBetRecordResult = GetGroupResult(betRecordResult.OriginBetRecord);
            var alreadyMultiplyRateTpBetRecordResult = GetGroupResult(betRecordResult.AlreadyMultiplyRateBetRecord);

            List<MpaWinLossByDateDto> GetGroupResult(List<MpaTpBetRecordDto> source)
            {
                return source.GroupBy(x => new
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
                    AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.StakeSummary) / r.Sum(x => x.RowCount) : 0.00m,
                    Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                    UniquePlayers = r.DistinctBy(x => x.CustId).Count()
                }).ToList();
            }

            var wrapperResult = new TpBetRecordByDateWrapper()
            {
                OriginTpBetRecordResult = originTpBetRecordResult,
                AlreadyMultiplyRateTpBetRecordResult = alreadyMultiplyRateTpBetRecordResult,
                UniquePlayers = betRecordResult.OriginBetRecord.DistinctBy(x => x.CustId).Count()
            };

            return wrapperResult;
        }

        private static void BetRecordSetDateToOffSet(MpaWinLossRequest request, MpaTpBetRecordWrapper betRecordResult,
            MpaWinLossGroupByTypeEnum groupByType)
        {
            if (betRecordResult == null) return;
            if (groupByType != MpaWinLossGroupByTypeEnum.Date && groupByType != MpaWinLossGroupByTypeEnum.Month) return;

            var offset = request.StarTime.Offset;

            foreach (var item in betRecordResult.OriginBetRecord)
            {
                item.Date = item.Date.ToOffSetReturnDate(offset);
            }

            foreach (var item in betRecordResult.AlreadyMultiplyRateBetRecord)
            {
                item.Date = item.Date.ToOffSetReturnDate(offset);
            }
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

        #endregion

        #region GroupBy TpGameType

        /// <summary>
        /// excel export
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MpaWinLossByTpGameTypeDto>> GetMapWinLossByTpGameTypeExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料
            var data = await GetTpBetRecordGroupByTpGameTypeResult(request, groupByType);
            var keys = data.OriginTpBetRecordResult.Select(x => x.TpGameTypeName).Distinct();
            var exportData = new List<MpaWinLossByTpGameTypeDto>();
            exportData.AddRange(request.DisplayGroupBreakdownInOriginalCurrency
                ? data.OriginTpBetRecordResult
                : data.AlreadyMultiplyRateTpBetRecordResult);

            foreach (var key in keys)
            {
                var dateSummary = data.AlreadyMultiplyRateTpBetRecordResult.Where(x => x.TpGameTypeName == key).ToList();
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
                        ? dateSummary.Sum(x => x.TurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m
                };

                exportData.Add(result);
            }

            exportData = request.OrderBy
                ? exportData.OrderBy(request).ToList()
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
                    ? exportData.Sum(x => x.GGR) / exportData.Sum(x => x.ValidTurnOver)
                    : 0.00m
            });

            return exportData.ToExcelFormat();
        }

        /// <summary>
        /// api
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MpaWinLossByTpGameTypeResponseToFrontEnd>> GetMapWinLossByTpGameTypeResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var responseResult = new Dictionary<string, MpaWinLossByTpGameTypeResponse>();

            // 注單資料
            var data = await GetTpBetRecordGroupByTpGameTypeResult(request, groupByType);
            var tpBetRecordResult = data.OriginTpBetRecordResult;

            var combineResult = request.OrderBy
                ? tpBetRecordResult.OrderBy(request)
                : tpBetRecordResult.OrderBy(r => r.TpGameTypeName).ThenBy(x => x.Currency).AsEnumerable();

            var keys = combineResult.Select(x => x.TpGameTypeName).Distinct();

            foreach (var key in keys)
            {
                var dateSummary = data.AlreadyMultiplyRateTpBetRecordResult.Where(x => x.TpGameTypeName == key).ToList();
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
                        ? dateSummary.Sum(x => x.TurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByTpGameType(data.OriginTpBetRecordResult, key)
                    : GetCurrencyDetailByTpGameType(dateSummary)
                };

                responseResult.Add(key, result);
            }

            // total
            responseResult.Add("total", new MpaWinLossByTpGameTypeResponse()
            {
                TurnOver = responseResult.Sum(x => x.Value.TurnOver),
                ValidTurnOver = responseResult.Sum(x => x.Value.ValidTurnOver),
                GGR = responseResult.Sum(x => x.Value.GGR),
                PlayerWinLoss = responseResult.Sum(x => x.Value.PlayerWinLoss),
                ProgressiveWin = responseResult.Sum(x => x.Value.ProgressiveWin),
                ProgressiveBet = responseResult.Sum(x => x.Value.ProgressiveBet),
                Commission = responseResult.Sum(x => x.Value.Commission),
                BetCount = responseResult.Sum(x => x.Value.BetCount),
                AvgBetAmount = (responseResult.Sum(x => x.Value.BetCount) != 0 && responseResult.Sum(x => x.Value.TurnOver) != 0)
                    ? (responseResult.Sum(x => x.Value.TurnOver) / responseResult.Sum(x => x.Value.BetCount))
                    : 0.00m,
                UniquePlayers = data.UniquePlayers,
                Margin = (responseResult.Sum(x => x.Value.GGR) != 0 && responseResult.Sum(x => x.Value.TurnOver) != 0)
                        ? responseResult.Sum(x => x.Value.GGR) / responseResult.Sum(x => x.Value.ValidTurnOver)
                        : 0.00m
            });

            return responseResult.ToApiFormat();
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
        /// 拿注單資料 + GroupBy
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<TpBetRecordByTpGameTypeWrapper> GetTpBetRecordGroupByTpGameTypeResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var tpGameTypeMap = await GetTpGameTypeMapper();
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var betRecordResult = await GetTpBetRecord(request, groupByType);
            var originTpBetRecordResult = GetGroupResult(betRecordResult.OriginBetRecord);
            var alreadyMultiplyRateTpBetRecordResult = GetGroupResult(betRecordResult.AlreadyMultiplyRateBetRecord);

            List<MpaWinLossByTpGameTypeDto> GetGroupResult(List<MpaTpBetRecordDto> source)
            {
                return source.GroupBy(x => new
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
                    AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.StakeSummary) / r.Sum(x => x.RowCount) : 0.00m,
                    Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                    UniquePlayers = r.DistinctBy(x => x.CustId).Count()
                }).ToList();
            }

            var wrapperResult = new TpBetRecordByTpGameTypeWrapper()
            {
                OriginTpBetRecordResult = originTpBetRecordResult,
                AlreadyMultiplyRateTpBetRecordResult = alreadyMultiplyRateTpBetRecordResult,
                UniquePlayers = betRecordResult.OriginBetRecord.DistinctBy(x => x.CustId).Count()
            };

            return wrapperResult;
        }



        #endregion

        #region GroupBy TpCategory

        /// <summary>
        /// excel export
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MpaWinLossByTpCategoryDto>> GetMapWinLossByTpCategoryExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料
            var data = await GetTpBetRecordGroupByTpCategoryResult(request, groupByType);
            var exportData = new List<MpaWinLossByTpCategoryDto>();
            exportData.AddRange(request.DisplayGroupBreakdownInOriginalCurrency
                ? data.OriginTpBetRecordResult
                : data.AlreadyMultiplyRateTpBetRecordResult);
            exportData.ForEach(x => x.GGR = x.GGR + (x.ProgressiveWin - x.ProgressiveBet));

            var keys = data.OriginTpBetRecordResult.Select(x => x.TpCategoryName).Distinct();

            foreach (var key in keys)
            {
                var dateSummary = data.AlreadyMultiplyRateTpBetRecordResult.Where(x => x.TpCategoryName == key).ToList();
                var progressiveWin = dateSummary.Sum(x => x.ProgressiveWin);
                var progressiveBet = dateSummary.Sum(x => x.ProgressiveBet);
                var progressiveDiff = progressiveWin - progressiveBet;

                var result = new MpaWinLossByTpCategoryDto
                {
                    TpCategoryName = key,
                    Currency = "All",
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = dateSummary.Sum(x => x.GGR) + progressiveDiff,
                    PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = progressiveWin,
                    ProgressiveBet = progressiveBet,
                    Commission = dateSummary.Sum(x => x.Commission),
                    BetCount = dateSummary.Sum(x => x.BetCount),
                    UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                    AvgBetAmount = dateSummary.Sum(x => x.BetCount) != 0
                        ? dateSummary.Sum(x => x.TurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                };

                exportData.Add(result);
            }

            exportData = request.OrderBy
                ? exportData.OrderBy(request).ToList()
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
                    ? exportData.Sum(x => x.GGR) / exportData.Sum(x => x.ValidTurnOver)
                    : 0.00m
            });

            return exportData.ToExcelFormat();
        }

        /// <summary>
        /// api
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MpaWinLossByTpCategoryResponseToFrontEnd>> GetMapWinLossByTpCategoryResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var exportData = new Dictionary<string, MpaWinLossByTpCategoryResponse>();

            //// 注單資料
            var data = await GetTpBetRecordGroupByTpCategoryResult(request, groupByType);
            var tpBetRecordResult = data.OriginTpBetRecordResult;

            var combineResult = request.OrderBy
                ? tpBetRecordResult.OrderBy(request)
                : tpBetRecordResult;

            var keys = combineResult.Select(x => x.TpCategoryName).Distinct();

            foreach (var key in keys)
            {
                var dateSummary = data.AlreadyMultiplyRateTpBetRecordResult.Where(x => x.TpCategoryName == key).ToList();
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
                        ? dateSummary.Sum(x => x.TurnOver) / dateSummary.Sum(x => x.BetCount)
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
                        ? exportData.Sum(x => x.Value.GGR) / exportData.Sum(x => x.Value.ValidTurnOver)
                        : 0.00m
            });

            return exportData.ToApiFormat();
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
        /// db 拿注單
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<TpBetRecordByTpCategoryWrapper> GetTpBetRecordGroupByTpCategoryResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var tpCategoryMap = await GetTpCategoryMapper();
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var betRecordResult = await GetTpBetRecord(request, groupByType);
            var originTpBetRecordResult = GetGroupResult(betRecordResult.OriginBetRecord);
            var alreadyMultiplyRateTpBetRecordResult = GetGroupResult(betRecordResult.AlreadyMultiplyRateBetRecord);

            List<MpaWinLossByTpCategoryDto> GetGroupResult(List<MpaTpBetRecordDto> source)
            {
                return source.GroupBy(x => new
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
                    AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.StakeSummary) / r.Sum(x => x.RowCount) : 0.00m,
                    Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                    UniquePlayers = r.DistinctBy(x => x.CustId).Count()
                }).ToList();
            }

            var wrapperResult = new TpBetRecordByTpCategoryWrapper()
            {
                OriginTpBetRecordResult = originTpBetRecordResult,
                AlreadyMultiplyRateTpBetRecordResult = alreadyMultiplyRateTpBetRecordResult,
                UniquePlayers = betRecordResult.OriginBetRecord.DistinctBy(x => x.CustId).Count()
            };

            return wrapperResult;
        }

        #endregion

        #region GroupBy Player Master

        /// <summary>
        /// excel export
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MpaWinLossByPlayerDto>> GetMpaWinLossByPlayerExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var winLossByPlayer = await GetWinLossByPlayer(request, groupByType);
            var excludeUserName = new List<string>();
            var exportData = new List<MpaWinLossByPlayerDto>();
            exportData.AddRange(request.DisplayGroupBreakdownInOriginalCurrency
                ? winLossByPlayer.OriginData
                : winLossByPlayer.AlreadyMultiplyRateData);

            var userNames = winLossByPlayer.OriginData.Select(x => x.UserName).Distinct();

            foreach (var userName in userNames)
            {
                var dateSummary = winLossByPlayer.AlreadyMultiplyRateData.Where(x => x.UserName == userName).ToList();
                var validTurnOver = dateSummary.Sum(x => x.ValidTurnOver);
                var turnOver = dateSummary.Sum(x => x.TurnOver);
                var ggr = dateSummary.Sum(x => x.GGR);
                var betCount = dateSummary.Sum(x => x.BetCount);
                if (betCount == 0) { excludeUserName.Add(userName); }

                var responseItem = new MpaWinLossByPlayerDto
                {
                    TpUserName = dateSummary.Select(r => r.TpUserName).FirstOrDefault(),
                    Currency = "All",
                    UserName = dateSummary.Select(r => r.UserName).FirstOrDefault(),
                    TurnOver = turnOver,
                    ValidTurnOver = validTurnOver,
                    GGR = ggr,
                    PlayerWinLoss = dateSummary.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = dateSummary.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = dateSummary.Sum(x => x.ProgressiveBet),
                    Commission = dateSummary.Sum(x => x.Commission),
                    BetCount = betCount,
                    UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                    ClosingBalance = dateSummary.Sum(x => x.ClosingBalance),
                    DepositCount = dateSummary.Sum(x => x.DepositCount),
                    TotalDepositAmount = dateSummary.Sum(x => x.TotalDepositAmount),
                    WithDrawCount = dateSummary.Sum(x => x.WithDrawCount),
                    TotalWithDrawAmount = dateSummary.Sum(x => x.TotalWithDrawAmount),
                    AvgBetAmount = betCount != 0
                        ? turnOver / betCount
                        : decimal.Zero,

                    Margin = validTurnOver != 0
                        ? ggr / validTurnOver
                        : decimal.Zero,
                    TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                };

                exportData.Add(responseItem);
            }

            exportData = request.OrderBy
                ? exportData.OrderBy(request).ToList()
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
                UniquePlayers = exportData.Where(x => !excludeUserName.Contains(x.UserName)).DistinctBy(x => x.UserName).Count(),
                DepositCount = exportData.Sum(x => x.DepositCount),
                TotalDepositAmount = exportData.Sum(x => x.TotalDepositAmount),
                WithDrawCount = exportData.Sum(x => x.WithDrawCount),
                TotalWithDrawAmount = exportData.Sum(x => x.TotalWithDrawAmount),
                TotalBonus = exportData.Sum(x => x.TotalBonus),
                AvgBetAmount = exportData.Sum(x => x.BetCount) != 0
                    ? (exportData.Sum(x => x.TurnOver) / exportData.Sum(x => x.BetCount))
                    : 0.00m,

                Margin = exportData.Sum(x => x.ValidTurnOver) != 0
                    ? exportData.Sum(x => x.GGR) / exportData.Sum(x => x.ValidTurnOver)
                    : 0.00m,
            });

            return exportData.ToExcelFormat();
        }

        /// <summary>
        /// api
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<MpaWinLossByPlayerResponseToFrontEndWrapper> GetMpaWinLossByPlayerResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            IPageInfo pageInfo = new PageInfo();
            var key = request.GetWinLossByPlayerRedisKey();
            var masterKey = $"MpaWinLossByPlayer:{key}";
            var cacheTime = TimeSpan.FromMinutes(15);
            var apiData = new List<MpaWinLossByPlayerResponse>();
            apiData.Clear();
            var rowCount = 0;
            var excludeUserName = new List<string>();
            MpaWinLossByPlayerResponse totalItem;

            if (await _redisHelper.KeyExistsAsync(masterKey))
            {
                var data = await _redisHelper.GetListByRangeAsync<MpaWinLossByPlayerResponse>(masterKey, 0, -1);
                apiData.AddRange(data);
                rowCount = apiData.Count;
                apiData = request.OrderBy ? apiData.OrderBy(request).ToList() : apiData;
                excludeUserName.AddRange(apiData.Where(x => x.BetCount == 0).Select(x => x.UserName).ToList());
                totalItem = GetMpaWinLossByPlayerTotal(apiData.ToDictionary(k => k.UserName, v => v), excludeUserName);
                if (request.PageNumber != -1)
                {
                    var (index, count) = pageInfo.ListPagination(apiData.Count, request.PageNumber, request.PageSize);
                    apiData = apiData.GetRange(index, count);
                }
            }
            else
            {
                var result = await GetWinLossByPlayer(request, groupByType);
                var userNames = result.OriginData.Select(x => x.UserName).Distinct();
                foreach (var userName in userNames)
                {
                    var dateSummary = result.AlreadyMultiplyRateData.Where(x => x.UserName == userName).ToList();
                    var betCount = dateSummary.Sum(x => x.BetCount);
                    if (betCount == 0) { excludeUserName.Add(userName); }

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
                        BetCount = betCount,
                        UniquePlayers = dateSummary.Sum(x => x.UniquePlayers),
                        ClosingBalance = dateSummary.Sum(x => x.ClosingBalance),
                        DepositCount = dateSummary.Sum(x => x.DepositCount),
                        TotalDepositAmount = dateSummary.Sum(x => x.TotalDepositAmount),
                        WithDrawCount = dateSummary.Sum(x => x.WithDrawCount),
                        TotalWithDrawAmount = dateSummary.Sum(x => x.TotalWithDrawAmount),
                        AvgBetAmount = betCount != 0
                            ? dateSummary.Sum(x => x.TurnOver) / betCount
                            : 0.00m,

                        Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                            ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                            : 0.00m,
                        TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                        CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                            ? GetOriginalCurrencyDetailByPlayer(result.OriginData, userName)
                            : GetCurrencyDetailByPlayer(dateSummary)
                    };
                    apiData.Add(responseItem);
                }

                rowCount = apiData.Count;
                apiData = request.OrderBy ? apiData.OrderBy(request).ToList() : apiData;
                AddCache(apiData, masterKey, cacheTime, request.OrderBy);
                totalItem = GetMpaWinLossByPlayerTotal(apiData.ToDictionary(k => k.UserName, v => v), excludeUserName);

                if (request.PageNumber != -1)
                {
                    var (index, count) = pageInfo.ListPagination(apiData.Count, request.PageNumber, request.PageSize);
                    apiData = apiData.GetRange(index, count);
                }
            }

            var response = apiData.ToDictionary(k => k.UserName, v => v);
            response.Add("total", totalItem);
            return response.ToApiFormat(rowCount);
        }

        private static MpaWinLossByPlayerResponse GetMpaWinLossByPlayerTotal(Dictionary<string, MpaWinLossByPlayerResponse> response, List<string> excludeUserName)
        {
            var validTurnOver = response.Sum(x => x.Value.ValidTurnOver);
            var turnOver = response.Sum(x => x.Value.TurnOver);
            var betCount = response.Sum(x => x.Value.BetCount);
            var ggr = response.Sum(x => x.Value.GGR);

            return new MpaWinLossByPlayerResponse()
            {
                TurnOver = turnOver,
                ValidTurnOver = validTurnOver,
                GGR = ggr,
                PlayerWinLoss = response.Sum(x => x.Value.PlayerWinLoss),
                ProgressiveWin = response.Sum(x => x.Value.ProgressiveWin),
                ProgressiveBet = response.Sum(x => x.Value.ProgressiveBet),
                Commission = response.Sum(x => x.Value.Commission),
                BetCount = betCount,
                ClosingBalance = response.Sum(x => x.Value.ClosingBalance),
                UniquePlayers = response.Where(x => !excludeUserName.Contains(x.Key)).DistinctBy(x => x.Value.UserName).Count(),
                DepositCount = response.Sum(x => x.Value.DepositCount),
                TotalDepositAmount = response.Sum(x => x.Value.TotalDepositAmount),
                WithDrawCount = response.Sum(x => x.Value.WithDrawCount),
                TotalWithDrawAmount = response.Sum(x => x.Value.TotalWithDrawAmount),
                TotalBonus = response.Sum(x => x.Value.TotalBonus),
                AvgBetAmount = betCount != 0
                    ? (turnOver / betCount)
                    : decimal.Zero,

                Margin = validTurnOver != 0
                    ? ggr / validTurnOver
                    : decimal.Zero,
            };
        }

        /// <summary>
        /// BetRecord + payment + bonus
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<MpaWinLossByPlayerWrapper> GetWinLossByPlayer(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料 & 存提款 & bonus
            var betRecordTask = Task.Run(() => GetTpBetRecordGroupByPlayerResult(request, groupByType));
            var transactionTask = Task.Run(() => GetTransactionByPlayerResult(request, groupByType));
            var bonusTask = Task.Run(() => GetBonusByPlayerResult(request, groupByType));
            await Task.WhenAll(betRecordTask, transactionTask, bonusTask);
            var betRecord = betRecordTask.Result;
            var transaction = transactionTask.Result;
            var bonus = bonusTask.Result;

            // 注單 + 存提款 + bonus 資料合併
            var originCombineData = betRecord.OriginTpBetRecordResult.Concat(transaction.OriginTransactionResult).Concat(bonus.OriginBonusResult).ToList();
            var alreadyMultiplyCombineData = betRecord.AlreadyMultiplyRateTpBetRecordResult.Concat(transaction.AlreadyMultiplyRateTransactionResult).Concat(bonus.AlreadyMultiplyRateBonusResult).ToList();

            var originData = GetGroupResult(originCombineData);
            var alreadyMultiplyRateData = GetGroupResult(alreadyMultiplyCombineData);

            List<MpaWinLossByPlayerDto> GetGroupResult(List<MpaWinLossByPlayerDto> source)
            {
                return source.GroupBy(x => new
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
                    AvgBetAmount = r.Sum(x => x.BetCount) != 0 ? r.Sum(x => x.TurnOver) / r.Sum(x => x.BetCount) : 0.00m,
                    Margin = r.Sum(x => x.ValidTurnOver) != 0 ? r.Sum(x => x.GGR) / r.Sum(x => x.ValidTurnOver) : 0.00m,
                    TotalBonus = r.Sum(x => x.TotalBonus)
                }).ToList();
            }

            var wrapperResult = new MpaWinLossByPlayerWrapper
            {
                OriginData = originData,
                AlreadyMultiplyRateData = alreadyMultiplyRateData
            };

            return wrapperResult;
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

        /// <summary>
        /// db 拿注單
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<TpBetRecordByPlayerWrapper> GetTpBetRecordGroupByPlayerResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var betRecord = await GetTpBetRecord(request, groupByType);
            var originBetRecord = GetGroupResult(betRecord.OriginBetRecord);
            var alreadyMultiplyRateBetRecord = GetGroupResult(betRecord.AlreadyMultiplyRateBetRecord);

            List<MpaWinLossByPlayerDto> GetGroupResult(List<MpaTpBetRecordDto> source)
            {
                return source.GroupBy(x => new
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
                    AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.StakeSummary) / r.Sum(x => x.RowCount) : 0.00m,
                    Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                    UniquePlayers = r.DistinctBy(x => x.CustId).Count()
                }).ToList();
            }

            var wrapperResult = new TpBetRecordByPlayerWrapper()
            {
                OriginTpBetRecordResult = originBetRecord,
                AlreadyMultiplyRateTpBetRecordResult = alreadyMultiplyRateBetRecord,
                UniquePlayers = betRecord.OriginBetRecord.DistinctBy(x => x.CustId).Count()
            };

            return wrapperResult;
        }



        #endregion

        #region GroupBy Player Detail

        public async Task<IEnumerable<MpaWinLossByPlayerDetailResponseFrontEnd>> GetMpaWinLossByPlayerDetailResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單
            var betRecord = await GetTpBetRecordGroupByPlayerDetailResult(request, groupByType);
            var keys = betRecord.OriginTpBetRecordResult.Select(x => x.GameTypeName).Distinct();
            var apiData = new Dictionary<string, MpaWinLossByPlayerDetailResponse>();

            foreach (var key in keys)
            {
                var dateSummary = request.DisplayGroupBreakdownInOriginalCurrency
                    ? betRecord.OriginTpBetRecordResult.Where(x => x.GameTypeName == key).ToList()
                    : betRecord.AlreadyMultiplyRateTpBetRecordResult.Where(x => x.GameTypeName == key).ToList();
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
                        ? dateSummary.Sum(x => x.TurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                };

                apiData.Add(key, result);

            }

            return apiData.ToApiFormat();
        }


        private async Task<TpBetRecordByPlayerWrapper> GetTpBetRecordGroupByPlayerDetailResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var betRecord = await GetTpBetRecord(request, groupByType);
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var tpGameTypeMap = await GetTpGameTypeMapper();
            var originTpBetRecordResult = GetGroupResult(betRecord.OriginBetRecord);
            var alreadyMultiplyRateBetRecord = GetGroupResult(betRecord.AlreadyMultiplyRateBetRecord);

            List<MpaWinLossByPlayerDto> GetGroupResult(List<MpaTpBetRecordDto> source)
            {
                return source.GroupBy(x => new
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
                    AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.StakeSummary) / r.Sum(x => x.RowCount) : 0.00m,
                    Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                    UniquePlayers = r.DistinctBy(x => x.CustId).Count()
                }).ToList();
            }

            var wrapperResult = new TpBetRecordByPlayerWrapper()
            {
                OriginTpBetRecordResult = originTpBetRecordResult,
                AlreadyMultiplyRateTpBetRecordResult = alreadyMultiplyRateBetRecord,
                UniquePlayers = betRecord.OriginBetRecord.DistinctBy(x => x.CustId).Count()
            };

            return wrapperResult;
        }

        #endregion

        #region GroupBy Currency

        /// <summary>
        /// excel export
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MpaWinLossByCurrencyDto>> GetMapWinLossByCurrencyExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料
            var betRecordTask = Task.Run(() => GetTpBetRecordGroupByCurrencyResult(request, groupByType));
            var transactionTask = Task.Run(() => GetTransactionByCurrencyResult(request, groupByType));
            var bonusTask = Task.Run(() => GetBonusByCurrencyResult(request, groupByType));
            await Task.WhenAll(betRecordTask, transactionTask, bonusTask);
            var betRecord = betRecordTask.Result;
            var transaction = transactionTask.Result;
            var bonus = bonusTask.Result;

            var originBetRecord = betRecord.OriginBetRecord;
            var alreadyMultiplyRateBetRecord = betRecord.AlreadyMultiplyRateBetRecord;

            // 存提款 
            var originTransaction = transaction.OriginTransaction;
            var alreadyMultiplyRateTransaction = transaction.AlreadyMultiplyRateTransaction;

            // bonus
            var originBonus = bonus.OriginBonus;
            var alreadyMultiplyRateBonus = bonus.AlreadyMultiplyRateBonus;

            // 注單 + 存提款資料合併
            var originCombineData = request.OrderBy
                ? originBetRecord
                    .Concat(originTransaction)
                    .Concat(originBonus)
                    .OrderBy(request)
                    .ToList()
                : originBetRecord
                    .Concat(originTransaction)
                    .Concat(originBonus)
                    .ToList();

            var alreadyMultiplyRateCombineData = request.OrderBy
                ? alreadyMultiplyRateBetRecord
                    .Concat(alreadyMultiplyRateTransaction)
                    .Concat(alreadyMultiplyRateBonus)
                    .OrderBy(request)
                    .ToList()
                : alreadyMultiplyRateBetRecord
                    .Concat(alreadyMultiplyRateTransaction)
                    .Concat(alreadyMultiplyRateBonus)
                    .ToList();


            var originCombineResult = GetGroupResult(originCombineData);
            var alreadyMultiplyRateCombineResult = GetGroupResult(alreadyMultiplyRateCombineData);

            List<MpaWinLossByCurrencyDto> GetGroupResult(List<MpaWinLossByCurrencyDto> source)
            {
                return source.GroupBy(x => new
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
                        AvgBetAmount = r.Sum(x => x.BetCount) != 0 ? r.Sum(x => x.TurnOver) / r.Sum(x => x.BetCount) : 0.00m,
                        Margin = r.Sum(x => x.ValidTurnOver) != 0 ? r.Sum(x => x.GGR) / r.Sum(x => x.ValidTurnOver) : 0.00m,
                        TotalBonus = r.Sum(x => x.TotalBonus)
                    }).ToList();
            }

            var exportData = new List<MpaWinLossByCurrencyDto>();
            exportData.AddRange(request.DisplayGroupBreakdownInOriginalCurrency
            ? originCombineResult
            : alreadyMultiplyRateCombineResult);

            var keys = originCombineResult.Select(x => x.Currency).Distinct();

            foreach (var key in keys)
            {
                var dateSummary = alreadyMultiplyRateCombineResult.Where(x => x.Currency == key).ToList();
                var result = new MpaWinLossByCurrencyDto
                {
                    Currency = $"{key}_All",
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
                        ? dateSummary.Sum(x => x.TurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                };

                exportData.Add(result);
            }

            exportData = request.OrderBy
                ? exportData.OrderBy(request).ToList()
                : exportData.OrderByDescending(r => r.Currency).ToList();

            // total
            exportData.Add(new MpaWinLossByCurrencyDto()
            {
                Currency = "total",
                TurnOver = exportData.Sum(x => x.TurnOver),
                ValidTurnOver = exportData.Sum(x => x.ValidTurnOver),
                GGR = exportData.Sum(x => x.GGR),
                PlayerWinLoss = exportData.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = exportData.Sum(x => x.ProgressiveWin),
                ProgressiveBet = exportData.Sum(x => x.ProgressiveBet),
                Commission = exportData.Sum(x => x.Commission),
                BetCount = exportData.Sum(x => x.BetCount),
                UniquePlayers = betRecord.UniquePlayers,
                DepositCount = exportData.Sum(x => x.DepositCount),
                TotalDepositAmount = exportData.Sum(x => x.TotalDepositAmount),
                WithDrawCount = exportData.Sum(x => x.WithDrawCount),
                TotalWithDrawAmount = exportData.Sum(x => x.TotalWithDrawAmount),
                TotalBonus = exportData.Sum(x => x.TotalBonus),
                AvgBetAmount = exportData.Sum(x => x.BetCount) != 0
                    ? (exportData.Sum(x => x.TurnOver) / exportData.Sum(x => x.BetCount))
                    : 0.00m,

                Margin = exportData.Sum(x => x.ValidTurnOver) != 0
                    ? exportData.Sum(x => x.GGR) / exportData.Sum(x => x.ValidTurnOver)
                    : 0.00m,
            });

            return exportData.ToExcelFormat();
        }

        /// <summary>
        /// api
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MpaWinLossByCurrencyResponseFrontEnd>> GetMapWinLossByCurrencyResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料 & 存提款 & bonus
            var betRecordTask = Task.Run(() => GetTpBetRecordGroupByCurrencyResult(request, groupByType));
            var transactionTask = Task.Run(() => GetTransactionByCurrencyResult(request, groupByType));
            var bonusTask = Task.Run(() => GetBonusByCurrencyResult(request, groupByType));
            await Task.WhenAll(betRecordTask, transactionTask, bonusTask);
            var betRecord = betRecordTask.Result;
            var transaction = transactionTask.Result;
            var bonus = bonusTask.Result;

            var originBetRecord = betRecord.OriginBetRecord;
            var alreadyMultiplyRateBetRecord = betRecord.AlreadyMultiplyRateBetRecord;

            var originTransaction = transaction.OriginTransaction;
            var alreadyMultiplyRateTransaction = transaction.AlreadyMultiplyRateTransaction;

            var originBonus = bonus.OriginBonus;
            var alreadyMultiplyRateBonus = bonus.AlreadyMultiplyRateBonus;

            // 注單 + 存提款資料合併
            var originCombineData = request.OrderBy
                ? originBetRecord
                    .Concat(originTransaction)
                    .Concat(originBonus)
                    .OrderBy(request)
                    .ToList()
                : originBetRecord
                    .Concat(originTransaction)
                    .Concat(originBonus)
                    .ToList();

            var alreadyMultiplyRateCombineData = request.OrderBy
                ? alreadyMultiplyRateBetRecord
                    .Concat(alreadyMultiplyRateTransaction)
                    .Concat(alreadyMultiplyRateBonus)
                    .OrderBy(request)
                    .ToList()
                : alreadyMultiplyRateBetRecord
                    .Concat(alreadyMultiplyRateTransaction)
                    .Concat(alreadyMultiplyRateBonus)
                    .ToList();

            var originCombineResult = GetGroupResult(originCombineData);
            var alreadyMultiplyRateCombineResult = GetGroupResult(alreadyMultiplyRateCombineData);

            List<MpaWinLossByCurrencyDto> GetGroupResult(List<MpaWinLossByCurrencyDto> source)
            {
                return source.GroupBy(x => new
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
                        AvgBetAmount = r.Sum(x => x.BetCount) != 0 ? r.Sum(x => x.TurnOver) / r.Sum(x => x.BetCount) : 0.00m,
                        Margin = r.Sum(x => x.ValidTurnOver) != 0 ? r.Sum(x => x.GGR) / r.Sum(x => x.ValidTurnOver) : 0.00m,
                        TotalBonus = r.Sum(x => x.TotalBonus)
                    }).ToList();
            }

            var apiData = new Dictionary<string, MpaWinLossByCurrencyResponse>();
            var keys = originCombineResult.Select(x => x.Currency).Distinct();
            foreach (var key in keys)
            {
                var dateSummary = alreadyMultiplyRateCombineResult.Where(x => x.Currency == key).ToList();
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
                        ? dateSummary.Sum(x => x.TurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                    CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByCurrency(originCombineResult, key)
                    : GetCurrencyDetailByCurrency(dateSummary)
                };

                apiData.Add(key, result);
            }

            // total
            apiData.Add("total", new MpaWinLossByCurrencyResponse()
            {
                TurnOver = apiData.Sum(x => x.Value.TurnOver),
                ValidTurnOver = apiData.Sum(x => x.Value.ValidTurnOver),
                GGR = apiData.Sum(x => x.Value.GGR),
                PlayerWinLoss = apiData.Sum(x => x.Value.PlayerWinLoss),
                ProgressiveWin = apiData.Sum(x => x.Value.ProgressiveWin),
                ProgressiveBet = apiData.Sum(x => x.Value.ProgressiveBet),
                Commission = apiData.Sum(x => x.Value.Commission),
                BetCount = apiData.Sum(x => x.Value.BetCount),
                UniquePlayers = betRecord.UniquePlayers,
                DepositCount = apiData.Sum(x => x.Value.DepositCount),
                TotalDepositAmount = apiData.Sum(x => x.Value.TotalDepositAmount),
                WithDrawCount = apiData.Sum(x => x.Value.WithDrawCount),
                TotalWithDrawAmount = apiData.Sum(x => x.Value.TotalWithDrawAmount),
                TotalBonus = apiData.Sum(x => x.Value.TotalBonus),
                AvgBetAmount = apiData.Sum(x => x.Value.BetCount) != 0
                    ? (apiData.Sum(x => x.Value.TurnOver) / apiData.Sum(x => x.Value.BetCount))
                    : 0.00m,

                Margin = apiData.Sum(x => x.Value.ValidTurnOver) != 0
                    ? apiData.Sum(x => x.Value.GGR) / apiData.Sum(x => x.Value.ValidTurnOver)
                    : 0.00m,
            });

            return apiData.ToApiFormat();
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

        /// <summary>
        /// db 拿注單
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<TpBetRecordByCurrencyWrapper> GetTpBetRecordGroupByCurrencyResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var betRecordResult = await GetTpBetRecord(request, groupByType);
            var originData = GetGroupResult(betRecordResult.OriginBetRecord);
            var alreadyMultiplyRateData = GetGroupResult(betRecordResult.AlreadyMultiplyRateBetRecord);

            List<MpaWinLossByCurrencyDto> GetGroupResult(List<MpaTpBetRecordDto> source)
            {
                return source.GroupBy(x => new
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
                    AvgBetAmount = r.Sum(x => x.RowCount) != 0 ? r.Sum(x => x.StakeSummary) / r.Sum(x => x.RowCount) : 0.00m,
                    Margin = r.Sum(x => x.ValidBetSummary) != 0 ? (r.Sum(x => x.WinLossSummary) * -1) / r.Sum(x => x.ValidBetSummary) : 0.00m,
                    UniquePlayers = r.DistinctBy(x => x.CustId).Count()
                }).ToList();
            }

            var tpBetRecordRelayResultWrapper = new TpBetRecordByCurrencyWrapper()
            {
                OriginBetRecord = originData,
                AlreadyMultiplyRateBetRecord = alreadyMultiplyRateData,
                UniquePlayers = betRecordResult.OriginBetRecord.DistinctBy(x => x.CustId).Count()
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
        private async Task<TransactionByDateWrapper> GetTransactionByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var transactionResult = await GetMapTransactionByDate(request, groupByType);
            var originTransactionResult = GetGroupResult(transactionResult.OriginTransaction);
            var alreadyMultiplyRateTransactionResult = GetGroupResult(transactionResult.AlreadyMultiplyRateTransaction);

            List<MpaWinLossByDateDto> GetGroupResult(List<TransactionRelayDto> source)
            {
                return source.GroupBy(x => new
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

            }

            return new TransactionByDateWrapper()
            {
                OriginTransactionResult = originTransactionResult,
                AlreadyMultiplyRateTransactionResult = alreadyMultiplyRateTransactionResult
            };
        }

        /// <summary>
        /// 取得 (充值單/提款單) 結果 by Player
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<TransactionByPlayerWrapper> GetTransactionByPlayerResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var transaction = await GetMapTransactionByDate(request, groupByType);
            var originTransaction = GetGroupResult(transaction.OriginTransaction);
            var alreadyMultiplyRateTransaction = GetGroupResult(transaction.AlreadyMultiplyRateTransaction);

            List<MpaWinLossByPlayerDto> GetGroupResult(List<TransactionRelayDto> source)
            {
                return source.GroupBy(x => new
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
            }


            var wrapperResult = new TransactionByPlayerWrapper()
            {
                OriginTransactionResult = originTransaction,
                AlreadyMultiplyRateTransactionResult = alreadyMultiplyRateTransaction
            };

            return wrapperResult;
        }

        private static void TransactionSetDateToOffSet(MpaWinLossRequest request, TransactionRelayWrapper transaction,
            MpaWinLossGroupByTypeEnum groupByType)
        {
            if (transaction == null) return;
            if (groupByType != MpaWinLossGroupByTypeEnum.Date && groupByType != MpaWinLossGroupByTypeEnum.Month) return;
            var offset = request.StarTime.Offset;

            foreach (var item in transaction.OriginTransaction)
            {
                item.Date = item.Date.ToOffSetReturnDate(offset);
            }

            foreach (var item in transaction.AlreadyMultiplyRateTransaction)
            {
                item.Date = item.Date.ToOffSetReturnDate(offset);
            }
        }

        /// <summary>
        /// 取得 (充值單/提款單) 結果 By Currency
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<TransactionByCurrencyWrapper> GetTransactionByCurrencyResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var transaction = await GetMapTransactionByDate(request, groupByType);
            var originTransaction = GetGroupResult(transaction.OriginTransaction);
            var alreadyMultiplyRateTransaction = GetGroupResult(transaction.AlreadyMultiplyRateTransaction);

            List<MpaWinLossByCurrencyDto> GetGroupResult(List<TransactionRelayDto> source)
            {
                return source.GroupBy(x => new
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
            }

            var wrapperResult = new TransactionByCurrencyWrapper()
            {
                OriginTransaction = originTransaction,
                AlreadyMultiplyRateTransaction = alreadyMultiplyRateTransaction
            };

            return wrapperResult;
        }

        #endregion

        #region Bouns 

        /// <summary>
        /// 取得 Bonus by 日期
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<BonusByDateWrapper> GetBonusByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var bonus = await GetMapWinLossBonusByDate(request, groupByType);
            var originBonusResult = GetGroupResult(bonus.OriginBonus);
            var alreadyMultiplyRateBonusResult = GetGroupResult(bonus.AlreadyMultiplyRateBonus);

            List<MpaWinLossByDateDto> GetGroupResult(List<MpaWinLossBonusDto> source)
            {
                return source.GroupBy(x => new
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
            }

            var wrapperResult = new BonusByDateWrapper()
            {
                OriginBonusResult = originBonusResult,
                AlreadyMultiplyRateBonusResult = alreadyMultiplyRateBonusResult
            };

            return wrapperResult;
        }

        private static void BonusSetDateToOffSet(MpaWinLossRequest request, MpaWinLossBonusWrapper bonus,
            MpaWinLossGroupByTypeEnum groupByType)
        {
            if (bonus == null) return;
            if (groupByType != MpaWinLossGroupByTypeEnum.Date && groupByType != MpaWinLossGroupByTypeEnum.Month) return;
            var offset = request.StarTime.Offset;

            foreach (var item in bonus.OriginBonus)
            {
                item.Date = item.Date.ToOffSetReturnDate(offset);
            }

            foreach (var item in bonus.AlreadyMultiplyRateBonus)
            {
                item.Date = item.Date.ToOffSetReturnDate(offset);
            }
        }

        /// <summary>
        /// 取得 Bonus by player
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<BonusByPlayerWrapper> GetBonusByPlayerResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var bonus = await GetMapWinLossBonusByDate(request, groupByType);
            var originBonus = GetGroupResult(bonus.OriginBonus);
            var alreadyMultiplyRateBonus = GetGroupResult(bonus.AlreadyMultiplyRateBonus);

            List<MpaWinLossByPlayerDto> GetGroupResult(List<MpaWinLossBonusDto> source)
            {
                return source.GroupBy(x => new
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
            }

            var wrapperResult = new BonusByPlayerWrapper()
            {
                OriginBonusResult = originBonus,
                AlreadyMultiplyRateBonusResult = alreadyMultiplyRateBonus
            };

            return wrapperResult;
        }

        /// <summary>
        /// 取得 Bonus by Currency
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<BonusByCurrencyWrapper> GetBonusByCurrencyResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var currencyNameMapper = await GetCurrencyIsoCodeMapper();
            var bonus = await GetMapWinLossBonusByDate(request, groupByType);
            var originBonus = GetGroupResult(bonus.OriginBonus);
            var alreadyMultiplyRateBonus = GetGroupResult(bonus.AlreadyMultiplyRateBonus);

            List<MpaWinLossByCurrencyDto> GetGroupResult(List<MpaWinLossBonusDto> source)
            {
                return source.GroupBy(x => new
                {
                    x.CurrencyId,
                })
                    .Select(r => new MpaWinLossByCurrencyDto
                    {
                        Currency = currencyNameMapper[r.Key.CurrencyId],
                        TotalBonus = r.Sum(x => x.BonusAmountClaim)
                    }).ToList();
            }

            var wrapperResult = new BonusByCurrencyWrapper()
            {
                OriginBonus = originBonus,
                AlreadyMultiplyRateBonus = alreadyMultiplyRateBonus
            };

            return wrapperResult;
        }


        #endregion

        #region Common

        /// <summary>
        /// 取得 Currency 的 IsoCode
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<int, string>> GetCurrencyIsoCodeMapper()
        {
            return await _mpaCommonService.GetCurrencyIsoCodeMapper();
        }

        /// <summary>
        /// 取得每個TpGameType + Name
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<int, string>> GetTpGameTypeMapper()
        {
            const string redisKey = "MpaTpGameTypeMapper";
            var data = await _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromHours(12),
                () => _mpaWinLossService.GetTpGameTypeOption());

            return data.ToDictionary(x => x.Type, v => v.TypeName);
        }


        private async Task<Dictionary<int, string>> GetTpCategoryMapper()
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
            #region cache code
            //var redisKey = $"MapTpBetRecord:{groupByType}:SiteId:{request.SiteId}:DateType:{request.DateType}:{request.StarTime:yyyy-MM-dd HH:mm:ss.fffzzz}:{request.EndTime:yyyy-MM-dd HH:mm:ss.fffzzz}";
            //if (await _redisHelper.KeyExistsAsync(redisKey))
            //{

            //    var sw = new Stopwatch();
            //    sw.Start();
            //    var redisData = await _redisHelper.GetListByRangeAsync<MpaTpBetRecordDto>(redisKey, 0, -1);
            //    sw.Stop();
            //    _logger.LogInformation($"MickeyTest UseRedis Get TpBetRecord ms:{sw.Elapsed.TotalMilliseconds}");
            //    return redisData;
            //    var _ = _redisHelper.InsertList(redisKey, TimeSpan.FromMinutes(15), data);
            //}
            #endregion
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var startDate = request.StarTime;
            var endDate = request.EndTime;
            var interval = TimeSpan.FromDays(16);
            var segmentCount = (int)Math.Ceiling((endDate - startDate).TotalDays / interval.TotalDays);
            var data = new List<MpaTpBetRecordDto>();
            var tasks = new List<Task<IEnumerable<MpaTpBetRecordDto>>>();
            for (var i = 0; i < segmentCount; i++)
            {
                var segmentStart = startDate.AddDays(i * interval.TotalDays);
                var segmentEnd = segmentStart.AddDays(interval.TotalDays);
                if (segmentEnd > endDate) segmentEnd = endDate;
                tasks.Add(Task.Run(() => _mpaWinLossService.GetMapTpBetRecord<MpaTpBetRecordDto>(request.SiteId, segmentStart, segmentEnd.AddSeconds(-1), format, request.DateType)));
            }

            await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                data.AddRange(task.Result);
            }

            data = data.Where(x => !string.IsNullOrEmpty(x.UserName)).ToList();

            return data;
        }

        private async Task<TransactionRelayWrapper> GetMapTransactionByDate(MpaWinLossRequest request,
            MpaWinLossGroupByTypeEnum groupByType)
        {
            #region cache code

            //var redisKey = $"MapTransaction:{groupByType}:SiteId:{request.SiteId}:{request.StarTime:yyyy-MM-dd HH:mm:ss.fffzzz}:{request.EndTime:yyyy-MM-dd HH:mm:ss.fffzzz}";
            //List<TransactionRelayDto> originTransaction;

            //if (await _redisHelper.KeyExistsAsync(redisKey))
            //if (false)
            //{
            //    //originTransaction = await _redisHelper.GetListByRangeAsync<TransactionRelayDto>(redisKey, 0, -1);
            //}
            //else
            //{
            //    //originTransaction = (await _mpaWinLossService.GetMapTransaction<TransactionRelayDto>(request.SiteId, request.StarTime, request.EndTime, format)).ToList();
            //    //var _ = _redisHelper.InsertList(redisKey, TimeSpan.FromMinutes(15), originTransaction);
            //}

            #endregion
            var sw = new Stopwatch();
            sw.Start();
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var originTransaction = (await _mpaWinLossService.GetMapTransaction<TransactionRelayDto>(request.SiteId, request.StarTime, request.EndTime, format)).ToList();
            originTransaction = FilterMapWinLossTransaction(request, originTransaction);
            sw.Stop();
            _logger.LogInformation($"MickeyTest GetMapTransaction GetData And filter data ms:{sw.Elapsed.TotalMilliseconds}");


            sw.Restart();
            var mapRate = await GetMapRate(request);
            sw.Stop();
            _logger.LogInformation($"MickeyTest GetMapTransaction Get mapRate data ms:{sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            var alreadyMultiplyRateTransaction = (from item in originTransaction
                                                  let rate = mapRate[$"{item.Date.DateMapperToRateDate()}{item.CurrencyId}{request.BaseCurrencyId}"]
                                                  select new TransactionRelayDto()
                                                  {
                                                      Date = item.Date,
                                                      SiteId = item.SiteId,
                                                      CurrencyId = item.CurrencyId,
                                                      CustId = item.CustId,
                                                      UserName = item.UserName,
                                                      TpUserName = item.TpUserName,
                                                      SummaryType = item.SummaryType,
                                                      TransactionType = item.TransactionType,
                                                      Amount = item.Amount * rate,
                                                      RowCount = item.RowCount
                                                  }).ToList();
            sw.Stop();
            _logger.LogInformation($"MickeyTest alreadyMultiplyRateTransaction ms:{sw.Elapsed.TotalMilliseconds}");

            var wrapperResult = new TransactionRelayWrapper()
            {
                OriginTransaction = originTransaction.ToList(),
                AlreadyMultiplyRateTransaction = alreadyMultiplyRateTransaction
            };
            TransactionSetDateToOffSet(request, wrapperResult, groupByType);

            return wrapperResult;
        }

        private static List<TransactionRelayDto> FilterMapWinLossTransaction(MpaWinLossRequest request, List<TransactionRelayDto> originTransaction)
        {
            originTransaction = originTransaction.Where(r => request.CurrencyIds.Contains(r.CurrencyId)).ToList();
            if (!string.IsNullOrEmpty(request.UserName))
            {
                originTransaction = originTransaction
                    .Where(r => r.UserName == request.UserName || r.TpUserName == request.UserName).ToList();
            }

            return originTransaction;
        }

        private async Task<MpaWinLossBonusWrapper> GetMapWinLossBonusByDate(MpaWinLossRequest request,
            MpaWinLossGroupByTypeEnum groupByType)
        {
            #region cache code

            //var redisKey = $"MapBonus:{groupByType}:SiteId:{request.SiteId}:{request.StarTime:yyyy-MM-dd HH:mm:ss.fffzzz}:{request.EndTime:yyyy-MM-dd HH:mm:ss.fffzzz}";
            //var originBonus = new List<MpaWinLossBonusDto>();

            //if (await _redisHelper.KeyExistsAsync(redisKey))
            //if (false)
            //{
            //    //originBonus = await _redisHelper.GetListByRangeAsync<MpaWinLossBonusDto>(redisKey, 0, -1);
            //}
            //else
            //{
            //    //originBonus = (await _mpaWinLossService.GetMpaWinLossBonus(request.SiteId, request.StarTime, request.EndTime, format)).ToList();
            //    //var _ = _redisHelper.InsertList(redisKey, TimeSpan.FromMinutes(15), originBonus);
            //}

            #endregion

            var sw = new Stopwatch();
            sw.Start();
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var originBonus = (await _mpaWinLossService.GetMpaWinLossBonus(request.SiteId, request.StarTime, request.EndTime, format)).ToList();
            originBonus = FilterMpaWinLossBonus(request, originBonus);
            sw.Stop();
            _logger.LogInformation($"MickeyTest Get MapWinLossBonus GetData And filter Data  ms:{sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            var mapRate = await GetMapRate(request);
            sw.Stop();
            _logger.LogInformation($"MickeyTest Get WinLossBonus Get mapRate data ms:{sw.Elapsed.TotalMilliseconds}");


            sw.Restart();
            var alreadyMultiplyRateBonus = (from item in originBonus
                                            let rate = mapRate[$"{item.Date.DateMapperToRateDate()}{item.CurrencyId}{request.BaseCurrencyId}"]
                                            select new MpaWinLossBonusDto()
                                            {
                                                Date = item.Date,
                                                CustId = item.CustId,
                                                UserName = item.UserName,
                                                TpUserName = item.TpUserName,
                                                CurrencyId = item.CurrencyId,
                                                BonusAmountClaim = item.BonusAmountClaim * rate
                                            }).ToList();
            sw.Stop();
            _logger.LogInformation($"MickeyTest Get alreadyMultiplyRateBonus ms:{sw.Elapsed.TotalMilliseconds}");

            var wrapperResult = new MpaWinLossBonusWrapper()
            {
                OriginBonus = originBonus.ToList(),
                AlreadyMultiplyRateBonus = alreadyMultiplyRateBonus
            };
            BonusSetDateToOffSet(request, wrapperResult, groupByType);


            return wrapperResult;
        }

        private static List<MpaWinLossBonusDto> FilterMpaWinLossBonus(MpaWinLossRequest request, List<MpaWinLossBonusDto> originBonus)
        {
            originBonus = originBonus.Where(r => request.CurrencyIds.Contains(r.CurrencyId)).ToList();
            if (!string.IsNullOrEmpty(request.UserName))
            {
                originBonus = originBonus.Where(r => r.UserName == request.UserName || r.TpUserName == request.UserName)
                    .ToList();
            }

            return originBonus;
        }

        private void AddCache(IEnumerable<MpaWinLossByPlayerResponse> list, string masterKey, TimeSpan cacheTime, bool orderBy)
        {
            // Add Cache by List      
            _ = Task.Run(() =>
            {
                _redisHelper.InsertList(masterKey, cacheTime, list);
            });
        }

        private static string MapMpaWinLossGroupByTypeEnumFormat(MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = groupByType switch
            {
                MpaWinLossGroupByTypeEnum.Date => "%m/%d/%Y %H:%i",
                MpaWinLossGroupByTypeEnum.Month => "%m/%d/%Y %H:%i",
                MpaWinLossGroupByTypeEnum.GameType => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.TpCategory => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.Player => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.Currency => "%Y-%m-%d",
                _ => string.Empty
            };
            return format;
        }

        private async Task<MpaTpBetRecordWrapper> GetTpBetRecord(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var wrapperResult = new MpaTpBetRecordWrapper();

            // 注單過濾
            var sw = new Stopwatch();
            sw.Start();

            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var data = await GetMapTpBetRecordByDate(request, groupByType);
            var tpBetRecordRelayResult = data.Filter(request)
                .SetGgRByOneGamingPoker()
                .SetGgRByV8Poker()
                .SetCommissionByNineW(request, format, _mpaWinLossService.Get9WCommission)
                .ToList();
            sw.Stop();
            _logger.LogInformation($"MickeyTest GetTpBetRecord GetData And filter data ms:{sw.Elapsed.TotalMilliseconds}");


            sw.Restart();
            var mapRate = await GetMapRate(request);
            sw.Stop();
            _logger.LogInformation($"MickeyTest TpBetRecord Get mapRate data ms:{sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            wrapperResult.AlreadyMultiplyRateBetRecord = (from item in tpBetRecordRelayResult
                                                          let rate = mapRate[$"{item.Date.DateMapperToRateDate()}{item.CurrencyId}{request.BaseCurrencyId}"]
                                                          select new MpaTpBetRecordDto()
                                                          {
                                                              Date = item.Date,
                                                              TpId = item.TpId,
                                                              CategoryId = item.CategoryId,
                                                              GameType = item.GameType,
                                                              SiteId = item.SiteId,
                                                              CurrencyId = item.CurrencyId,
                                                              CustId = item.CustId,
                                                              UserName = item.UserName,
                                                              TpUserName = item.TpUserName,
                                                              SettlementType = item.SettlementType,
                                                              BonusId = item.BonusId,
                                                              PromotionType = item.PromotionType,
                                                              TicketStatus = item.TicketStatus,
                                                              StakeSummary = item.StakeSummary * rate,
                                                              ValidBetSummary = item.ValidBetSummary * rate,
                                                              WinLossSummary = item.WinLossSummary * rate,
                                                              RevenueSummary = item.RevenueSummary * rate,
                                                              ProgressiveWinSummary = item.ProgressiveWinSummary * rate,
                                                              ProgressiveBetSummary = item.ProgressiveBetSummary * rate,
                                                              ClosingBalance = item.ClosingBalance * rate,
                                                              RowCount = item.RowCount,
                                                          }).ToList();
            sw.Stop();
            _logger.LogInformation($"MickeyTest AlreadyMultiplyRateBetRecord data ms:{sw.Elapsed.TotalMilliseconds}");
            wrapperResult.OriginBetRecord = tpBetRecordRelayResult;
            BetRecordSetDateToOffSet(request, wrapperResult, groupByType);
            return wrapperResult;
        }

        private async Task<Dictionary<string, decimal>> GetMapRate(MpaWinLossRequest request)
        {
            return (await _mpaCommonService.GetMpaExchangeRate(request.StarTime, request.EndTime, request.BaseCurrencyId))
                .Where(x => x.TargetCurrencyId == request.BaseCurrencyId)
                .ToDictionary(k => $"{k.RateDate.ToString("yyyy-MM-dd")}{k.CurrencyId}{k.TargetCurrencyId}", v => v.Rate);
        }

        #endregion
    }
}
