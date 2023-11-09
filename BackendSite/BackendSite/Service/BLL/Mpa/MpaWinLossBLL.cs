using BackendSite.Extensions;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.BLL.Mpa.Extension;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL.Mpa
{
    public class MpaWinLossBLL : IMpaWinLossBLL
    {
        private readonly IMpaWinLossService _mpaWinLossService;
        private readonly IMpaCommonService _mpaCommonService;
        private readonly ICacheHelper _redisHelper;
        private readonly ILogger<MpaWinLossBLL> _logger;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly Common _common;

        public MpaWinLossBLL(IMpaWinLossService mpaWinLossService, ICacheHelper redisHelper, IMpaCommonService mpaCommonService, ILogger<MpaWinLossBLL> logger, ServerInfoBLL serverInfoBll, Common common)
        {
            _mpaWinLossService = mpaWinLossService;
            _mpaCommonService = mpaCommonService;
            _redisHelper = redisHelper;
            _logger = logger;
            _serverInfoBll = serverInfoBll;
            _common = common;
        }

        #region GropuByDate、Month

        /// <summary>
        /// excel Export
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MpaWinLossByDateDtoExport>> GetMpaWinLossByDateExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
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

            var allCurrencySummary = new List<MpaWinLossByDateDto>();
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

                allCurrencySummary.Add(result);
            }

            #region Sort

            allCurrencySummary = request.OrderBy
                ? allCurrencySummary.OrderBy(request).ToList()
                : allCurrencySummary;
            var currencyDetails = request.DisplayGroupBreakdownInOriginalCurrency
                ? originCombineResult
                : alreadyMultiplyRateCombineResult;
            var exportData = new List<MpaWinLossByDateDto>();
            foreach (var currencySummary in allCurrencySummary)
            {
                var currencyDetail = request.OrderBy
                    ? currencyDetails.Where(r => r.Date == currencySummary.Date).OrderBy(request).ToList()
                    : currencyDetails.Where(r => r.Date == currencySummary.Date).ToList();
                exportData.Add(currencySummary);
                exportData.AddRange(currencyDetail);
            }

            #endregion

            #region Total

            var exportDataWithoutAll = exportData.Where(r => r.Currency != "All").ToList();
            exportData.Add(new MpaWinLossByDateDto()
            {
                Date = "Total",
                TurnOver = exportDataWithoutAll.Sum(x => x.TurnOver),
                ValidTurnOver = exportDataWithoutAll.Sum(x => x.ValidTurnOver),
                GGR = exportDataWithoutAll.Sum(x => x.GGR),
                PlayerWinLoss = exportDataWithoutAll.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = exportDataWithoutAll.Sum(x => x.ProgressiveWin),
                ProgressiveBet = exportDataWithoutAll.Sum(x => x.ProgressiveBet),
                Commission = exportDataWithoutAll.Sum(x => x.Commission),
                BetCount = exportDataWithoutAll.Sum(x => x.BetCount),
                UniquePlayers = betRecord.UniquePlayers,
                DepositCount = exportDataWithoutAll.Sum(x => x.DepositCount),
                TotalDepositAmount = exportDataWithoutAll.Sum(x => x.TotalDepositAmount),
                WithDrawCount = exportDataWithoutAll.Sum(x => x.WithDrawCount),
                TotalWithDrawAmount = exportDataWithoutAll.Sum(x => x.TotalWithDrawAmount),
                TotalBonus = exportDataWithoutAll.Sum(x => x.TotalBonus),
                AvgBetAmount = exportDataWithoutAll.Sum(x => x.BetCount) != 0
                    ? (exportDataWithoutAll.Sum(x => x.TurnOver) / exportDataWithoutAll.Sum(x => x.BetCount))
                    : 0.00m,
                Margin = exportDataWithoutAll.Sum(x => x.ValidTurnOver) != 0
                    ? exportDataWithoutAll.Sum(x => x.GGR) / exportDataWithoutAll.Sum(x => x.ValidTurnOver)
                    : 0.00m,
            });

            #endregion

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
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
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

            var exportData = new Dictionary<string, MpaWinLossByDateResponse>();
            var keys = originCombineResult.Select(x => x.Date).Distinct();

            foreach (var key in keys)
            {
                var dateSummary = alreadyMultiplyRateCombineResult.Where(x => x.Date == key).ToList();
                var ggr = dateSummary.Sum(x => x.GGR);

                var result = new MpaWinLossByDateResponse
                {
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = ggr,
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
                        ? ggr / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                    //CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                    //? GetOriginalCurrencyDetailByDate(originCombineResult, key, _common)
                    //: GetCurrencyDetailByDate(dateSummary, _common),
                    CurrencyDetailGroupByCurrency = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByDate(originCombineResult, key)
                    : GetCurrencyDetailByDate(dateSummary)
                };

                exportData.Add(key, result);
            }

            #region Sort Data

            var orderByProperty = request.OrderByProperty.ToLower() == "date"
                ? "key"
                : request.OrderByProperty;

            exportData = request.OrderBy
                ? _mpaCommonService.SortApiData(exportData, orderByProperty, request.OrderByDirection)
                : exportData;

            foreach (var item in exportData)
            {
                var currencyDetail = request.OrderBy
                    ? _mpaCommonService.SortApiData(item.Value.CurrencyDetail, orderByProperty, request.OrderByDirection)
                    : item.Value.CurrencyDetail;
                item.Value.CurrencyDetail = currencyDetail;
            }

            #endregion

            #region total

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

            #endregion

            return exportData.ToApiFormat();
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
                x.SiteId,
            })
            .Select(r => new MpaWinLossByDateDto
            {
                Date = r.Key.Date,
                Currency = r.Key.Currency,
                SiteId = r.Key.SiteId,
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

            return combineResult;
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
                return source
                    .GroupBy(x => new
                    {
                        x.Date,
                        x.CurrencyId,
                        x.SiteId,
                        x.GameType
                    })
                    .Select(r =>
                    {
                        // 預先計算常用的總和，以避免多次重複計算
                        var totalStakeSummary = r.Sum(x => x.StakeSummary);
                        var totalValidBetSummary = r.Sum(x => x.ValidBetSummary);
                        //Progressive Win already included in PlayerWinLoss. 
                        var totalWinLossSummary = r.Sum(x => x.WinLossSummary)+ r.Sum(x => x.ProgressiveWinSummary);
                        var totalRevenueSummary = r.Sum(x => x.RevenueSummary);
                        var totalProgressiveWinSummary = r.Sum(x => x.ProgressiveWinSummary);
                        var totalProgressiveBetSummary = r.Sum(x => x.ProgressiveBetSummary);
                        var totalRowCount = r.Sum(x => x.RowCount);
                        var gameType = r.Key.GameType;
                        var totalRevenueSummaryfor9w = r.Sum(x => CalculateCommission(x));


                        // 建立新的MpaWinLossByDateDto物件並返回
                        return new MpaWinLossByDateDto
                        {
                            Date = groupByType == MpaWinLossGroupByTypeEnum.Date
                                ? r.Key.Date.ToString("yyyy-MM-dd")
                                : r.Key.Date.ToString("yyyy-MM"),
                            Currency = currencyNameMapper[r.Key.CurrencyId],
                            TurnOver = totalStakeSummary,
                            ValidTurnOver = totalValidBetSummary,
                            GGR = CalculateGGR(gameType, totalWinLossSummary, totalProgressiveWinSummary, totalProgressiveBetSummary, totalRevenueSummaryfor9w),
                            PlayerWinLoss = totalWinLossSummary,
                            ProgressiveWin = totalProgressiveWinSummary,
                            ProgressiveBet = totalProgressiveBetSummary,
                            Commission = r.Sum(x => CalculateCommission(x)),
                            BetCount = totalRowCount,
                            AvgBetAmount = totalRowCount != 0 ? totalStakeSummary / totalRowCount : 0.00m,
                            Margin = totalValidBetSummary != 0
                                ? CalculateGGR(gameType, totalWinLossSummary, totalProgressiveWinSummary, totalProgressiveBetSummary, totalRevenueSummaryfor9w) / totalValidBetSummary
                                : 0.00m,
                            UniquePlayers = r.DistinctBy(x => x.CustId).Count(),
                            SiteId = r.Key.SiteId
                        };
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
        /// <summary>
        /// 計算GGR的輔助方法
        /// </summary>
        /// <param name="gameType"></param>
        /// <param name="winLoss"></param>
        /// <param name="progressiveWin"></param>
        /// <param name="progressiveBet"></param>
        /// <param name="revenue"></param>
        /// <returns></returns>
        private decimal CalculateGGR(int gameType, decimal winLoss, decimal progressiveWin, decimal progressiveBet, decimal revenue)
        {
            var winLossSummary = winLoss;
            if (gameType == (int)MpaGGRCustomizationGameTypeEnum.NineW)
            {
                //GGR for 9W Sports = - PlayerWinLoss + Commission
                //因經由CalculateCommission方法計算出的RevenueSummary已經帶負號，故這邊要revenue*-1
                winLossSummary = winLoss + (revenue * -1);
            }
            return GetGGR(winLossSummary, progressiveWin, progressiveBet);
        }
        /// <summary>
        /// 計算佣金的輔助方法
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private decimal CalculateCommission(MpaTpBetRecordDto x)
        {
            if (isOnegameorV8(x.GameType))
            {
                return 0;
            }

            if (is9w(x.GameType))
            {
                return x.RevenueSummary * -1;
            }

            return x.RevenueSummary;
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

        private static Dictionary<string, MpaWinLossByDayCurrencyDetailDto> GetCurrencyDetailByDate(List<MpaWinLossByDateDto> dateSummary, Common _common)
        {
            return dateSummary.ToDictionary(k => _common.GetSiteName(k.SiteId) + k.Currency, v => new MpaWinLossByDayCurrencyDetailDto
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
        private IEnumerable<MpaWinLossByDayCurrencyDetailDto> GetCurrencyDetailByDate(List<MpaWinLossByDateDto> dateSummary)
        {
            var result = dateSummary.GroupBy(x => x.Currency)
                .Select(r => new MpaWinLossByDayCurrencyDetailDto
                {
                    Currency = r.Key,
                    TurnOver = r.Sum(x => x.TurnOver),
                    ValidTurnOver = r.Sum(x => x.ValidTurnOver),
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
                    TotalBonus = r.Sum(x => x.TotalBonus),
                });
            return result;

        }

        private static Dictionary<string, MpaWinLossByDayCurrencyDetailDto> GetOriginalCurrencyDetailByDate(IEnumerable<MpaWinLossByDateDto> combineResult, string key, Common _common)
        {
            return combineResult.Where(x => x.Date == key)
                .ToDictionary(k => _common.GetSiteName(k.SiteId) + k.Currency, v => new MpaWinLossByDayCurrencyDetailDto
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
        private IEnumerable<MpaWinLossByDayCurrencyDetailDto> GetOriginalCurrencyDetailByDate(List<MpaWinLossByDateDto> combineResult, string key)
        {
            return combineResult.Where(x => x.Date == key)
                .GroupBy(x => x.Currency)
                .Select(r => new MpaWinLossByDayCurrencyDetailDto
                {
                    Currency = r.Key,
                    TurnOver = r.Sum(x => x.TurnOver),
                    ValidTurnOver = r.Sum(x => x.ValidTurnOver),
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
                    TotalBonus = r.Sum(x => x.TotalBonus),
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
        public async Task<IEnumerable<MpaWinLossByTpGameTypeDtoExport>> GetMapWinLossByTpGameTypeExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            // 注單資料
            var data = await GetTpBetRecordGroupByTpGameTypeResult(request, groupByType);
            var keys = data.OriginTpBetRecordResult.Select(x => x.TpGameTypeName).Distinct();

            var allCurrencySummary = new List<MpaWinLossByTpGameTypeDto>();
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

                allCurrencySummary.Add(result);
            }

            #region sort

            allCurrencySummary = request.OrderBy
                ? allCurrencySummary.OrderBy(request).ToList()
                : allCurrencySummary;

            var currencyDetails = request.DisplayGroupBreakdownInOriginalCurrency
                ? data.OriginTpBetRecordResult
                : data.AlreadyMultiplyRateTpBetRecordResult;

            var exportData = new List<MpaWinLossByTpGameTypeDto>();

            foreach (var currencySummary in allCurrencySummary)
            {
                var currencyDetail = request.OrderBy
                    ? currencyDetails.Where(r => r.TpGameTypeName == currencySummary.TpGameTypeName).OrderBy(request).ToList()
                    : currencyDetails.Where(r => r.TpGameTypeName == currencySummary.TpGameTypeName).ToList();
                exportData.Add(currencySummary);
                exportData.AddRange(currencyDetail);
            }

            #endregion

            #region total

            var exportDataWithoutAll = exportData.Where(r => r.Currency != "All").ToList();
            exportData.Add(new MpaWinLossByTpGameTypeDto()
            {
                TpGameTypeName = "total",
                TurnOver = exportDataWithoutAll.Sum(x => x.TurnOver),
                ValidTurnOver = exportDataWithoutAll.Sum(x => x.ValidTurnOver),
                GGR = exportDataWithoutAll.Sum(x => x.GGR),
                PlayerWinLoss = exportDataWithoutAll.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = exportDataWithoutAll.Sum(x => x.ProgressiveWin),
                ProgressiveBet = exportDataWithoutAll.Sum(x => x.ProgressiveBet),
                Commission = exportDataWithoutAll.Sum(x => x.Commission),
                BetCount = exportDataWithoutAll.Sum(x => x.BetCount),
                AvgBetAmount = (exportDataWithoutAll.Sum(x => x.BetCount) != 0 && exportDataWithoutAll.Sum(x => x.TurnOver) != 0)
                    ? (exportDataWithoutAll.Sum(x => x.TurnOver) / exportDataWithoutAll.Sum(x => x.BetCount))
                    : 0.00m,
                UniquePlayers = data.UniquePlayers,
                Margin = (exportDataWithoutAll.Sum(x => x.GGR) != 0 && exportDataWithoutAll.Sum(x => x.TurnOver) != 0)
                    ? exportDataWithoutAll.Sum(x => x.GGR) / exportDataWithoutAll.Sum(x => x.ValidTurnOver)
                    : 0.00m
            });

            #endregion

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
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            // 注單資料
            var data = await GetTpBetRecordGroupByTpGameTypeResult(request, groupByType);
            var tpBetRecordResult = data.OriginTpBetRecordResult;

            var keys = tpBetRecordResult.Select(x => x.TpGameTypeName).Distinct();
            var responseResult = new Dictionary<string, MpaWinLossByTpGameTypeResponse>();
            foreach (var key in keys)
            {
                var dateSummary = data.AlreadyMultiplyRateTpBetRecordResult.Where(x => x.TpGameTypeName == key).ToList();
                var ggr = dateSummary.Sum(x => x.GGR);
                var result = new MpaWinLossByTpGameTypeResponse
                {
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = ggr,
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
                        ? ggr / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByTpGameType(data.OriginTpBetRecordResult, key)
                    : GetCurrencyDetailByTpGameType(dateSummary)
                };

                responseResult.Add(key, result);
            }

            #region Sort Data

            var orderByProperty = request.OrderByProperty.ToLower() == "tpgametypename"
                ? "key"
                : request.OrderByProperty;

            responseResult = request.OrderBy
                ? _mpaCommonService.SortApiData(responseResult, orderByProperty, request.OrderByDirection)
                : responseResult;

            foreach (var item in responseResult)
            {
                var currencyDetail = request.OrderBy
                    ? _mpaCommonService.SortApiData(item.Value.CurrencyDetail, orderByProperty, request.OrderByDirection)
                    : item.Value.CurrencyDetail;
                item.Value.CurrencyDetail = currencyDetail;
            }

            #endregion

            #region total

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

            #endregion

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
                        GGR = v.GGR,
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
                    GGR = v.GGR,
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
                return source
                    .GroupBy(x => new
                    {
                        x.CurrencyId,
                        x.GameType
                    }).Select(r =>
                    {
                        // 預先計算常用的總和，以避免多次重複計算
                        var totalStakeSummary = r.Sum(x => x.StakeSummary);
                        var totalValidBetSummary = r.Sum(x => x.ValidBetSummary);
                        var totalWinLossSummary = r.Sum(x => x.WinLossSummary);
                        var totalRevenueSummary = r.Sum(x => x.RevenueSummary);
                        var totalProgressiveWinSummary = r.Sum(x => x.ProgressiveWinSummary);
                        var totalProgressiveBetSummary = r.Sum(x => x.ProgressiveBetSummary);
                        var totalRowCount = r.Sum(x => x.RowCount);
                        var gameType = r.Key.GameType;
                        var totalRevenueSummaryfor9w = r.Sum(x => CalculateCommission(x));


                        // 建立新的MpaWinLossByTpGameTypeDto物件並返回
                        return new MpaWinLossByTpGameTypeDto
                        {
                            TpGameTypeName = tpGameTypeMap[r.Key.GameType],
                            Currency = currencyNameMapper[r.Key.CurrencyId],
                            TurnOver = totalStakeSummary,
                            ValidTurnOver = totalValidBetSummary,
                            GGR = CalculateGGR(gameType, totalWinLossSummary, totalProgressiveWinSummary, totalProgressiveBetSummary, totalRevenueSummaryfor9w),
                            PlayerWinLoss = totalWinLossSummary,
                            ProgressiveWin = totalProgressiveWinSummary,
                            ProgressiveBet = totalProgressiveBetSummary,
                            Commission = r.Sum(x => CalculateCommission(x)),
                            BetCount = totalRowCount,
                            AvgBetAmount = totalRowCount != 0 ? totalStakeSummary / totalRowCount : 0.00m,
                            Margin = totalValidBetSummary != 0
                                ? CalculateGGR(gameType, totalWinLossSummary, totalProgressiveWinSummary, totalProgressiveBetSummary, totalRevenueSummaryfor9w) / totalValidBetSummary
                                : 0.00m,
                            UniquePlayers = r.DistinctBy(x => x.CustId).Count(),
                        };
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
        public async Task<IEnumerable<MpaWinLossByTpCategoryDtoExport>> GetMapWinLossByTpCategoryExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            // 注單資料
            var data = await GetTpBetRecordGroupByTpCategoryResult(request, groupByType);
            var allCurrencySummary = new List<MpaWinLossByTpCategoryDto>();
            var keys = data.OriginTpBetRecordResult.Select(x => x.TpCategoryName).Distinct();

            foreach (var key in keys)
            {
                var dateSummary = data.AlreadyMultiplyRateTpBetRecordResult.Where(x => x.TpCategoryName == key).ToList();
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
                        ? dateSummary.Sum(x => x.TurnOver) / dateSummary.Sum(x => x.BetCount)
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? dateSummary.Sum(x => x.GGR) / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                };

                allCurrencySummary.Add(result);
            }

            #region Sort

            allCurrencySummary = request.OrderBy
                ? allCurrencySummary.OrderBy(request).ToList()
                : allCurrencySummary;

            var currencyDetails = request.DisplayGroupBreakdownInOriginalCurrency
                ? data.OriginTpBetRecordResult
                : data.AlreadyMultiplyRateTpBetRecordResult;

            var exportData = new List<MpaWinLossByTpCategoryDto>();
            foreach (var currencySummary in allCurrencySummary)
            {
                var currencyDetail = request.OrderBy
                        ? currencyDetails.Where(r => r.TpCategoryName == currencySummary.TpCategoryName).OrderBy(request).ToList()
                        : currencyDetails.Where(r => r.TpCategoryName == currencySummary.TpCategoryName).ToList();

                exportData.Add(currencySummary);
                exportData.AddRange(currencyDetail);
            }

            #endregion

            #region total
            var exportDataWithoutAll = exportData.Where(r => r.Currency != "All").ToList();
            exportData.Add(new MpaWinLossByTpCategoryDto()
            {
                TpCategoryName = "total",
                TurnOver = exportDataWithoutAll.Sum(x => x.TurnOver),
                ValidTurnOver = exportDataWithoutAll.Sum(x => x.ValidTurnOver),
                GGR = exportDataWithoutAll.Sum(x => x.GGR),
                PlayerWinLoss = exportDataWithoutAll.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = exportDataWithoutAll.Sum(x => x.ProgressiveWin),
                ProgressiveBet = exportDataWithoutAll.Sum(x => x.ProgressiveBet),
                Commission = exportDataWithoutAll.Sum(x => x.Commission),
                BetCount = exportDataWithoutAll.Sum(x => x.BetCount),
                AvgBetAmount = (exportDataWithoutAll.Sum(x => x.BetCount) != 0 && exportDataWithoutAll.Sum(x => x.TurnOver) != 0)
                    ? (exportDataWithoutAll.Sum(x => x.TurnOver) / exportDataWithoutAll.Sum(x => x.BetCount))
                    : 0.00m,
                UniquePlayers = data.UniquePlayers,
                Margin = (exportDataWithoutAll.Sum(x => x.GGR) != 0 && exportDataWithoutAll.Sum(x => x.TurnOver) != 0)
                    ? exportDataWithoutAll.Sum(x => x.GGR) / exportDataWithoutAll.Sum(x => x.ValidTurnOver)
                    : 0.00m
            });

            #endregion

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
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
            var exportData = new Dictionary<string, MpaWinLossByTpCategoryResponse>();

            // 注單資料
            var data = await GetTpBetRecordGroupByTpCategoryResult(request, groupByType);
            var tpBetRecordResult = data.OriginTpBetRecordResult;
            var keys = tpBetRecordResult.Select(x => x.TpCategoryName).Distinct();

            foreach (var key in keys)
            {
                var dateSummary = data.AlreadyMultiplyRateTpBetRecordResult.Where(x => x.TpCategoryName == key).ToList();
                var ggr = dateSummary.Sum(x => x.GGR);

                var result = new MpaWinLossByTpCategoryResponse
                {
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
                    GGR = ggr,
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
                        ? ggr / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    CurrencyDetailByCategory = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByTpCategory(tpBetRecordResult, key)
                    : GetCurrencyDetailByTpCategory(dateSummary),
                };

                exportData.Add(key, result);
            }

            #region Sort

            var orderByProperty = request.OrderByProperty.ToLower() == "tpcategoryname"
                ? "key"
                : request.OrderByProperty;

            exportData = request.OrderBy
                ? _mpaCommonService.SortApiData(exportData, orderByProperty, request.OrderByDirection)
                : exportData;

            foreach (var item in exportData)
            {
                var currencyDetail = request.OrderBy
                    ? _mpaCommonService.SortApiData(item.Value.CurrencyDetail, orderByProperty, request.OrderByDirection)
                    : item.Value.CurrencyDetail;
                item.Value.CurrencyDetail = currencyDetail;
            }

            #endregion

            #region total

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

            #endregion

            return exportData.ToApiFormat();
        }
        public IEnumerable<MpaWinLossByTpCategoryCurrencyDetailDto> GetOriginalCurrencyDetailByTpCategory(IEnumerable<MpaWinLossByTpCategoryDto> combineResult,string key)
        {
            return combineResult.Where(x => x.TpCategoryName == key)
                .GroupBy(x => x.Currency)
                .Select(r => new MpaWinLossByTpCategoryCurrencyDetailDto
                {
                    Currency = r.Key,
                    TurnOver = r.Sum(x => x.TurnOver),
                    ValidTurnOver = r.Sum(x => x.ValidTurnOver),
                    GGR = r.Sum(x =>x.GGR),
                    PlayerWinLoss = r.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = r.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = r.Sum(x => x.ProgressiveBet),
                    Commission = r.Sum(x => x.Commission),
                    BetCount = r.Sum(x => x.BetCount),
                    UniquePlayers = r.Sum(x => x.UniquePlayers)
                });
        }
        public IEnumerable<MpaWinLossByTpCategoryCurrencyDetailDto> GetCurrencyDetailByTpCategory(IEnumerable<MpaWinLossByTpCategoryDto> dateSummary)
        {
            return dateSummary.GroupBy(x => x.Currency)
                .Select(r => new MpaWinLossByTpCategoryCurrencyDetailDto
                {
                    Currency = r.Key,
                    TurnOver = r.Sum(x=> x.TurnOver),
                    GGR = r.Sum(x=> x.GGR),
                    ValidTurnOver = r.Sum(x => x.ValidTurnOver),
                    PlayerWinLoss = r.Sum(x => x.PlayerWinLoss),
                    ProgressiveWin = r.Sum(x => x.ProgressiveWin),
                    ProgressiveBet = r.Sum(x => x.ProgressiveBet),
                    Commission = r.Sum(x => x.Commission),
                    BetCount = r.Sum(x => x.BetCount),
                    UniquePlayers = r.Sum(x =>x.UniquePlayers)
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
                return source
                    .GroupBy(x => new
                    {
                        x.CategoryId,
                        x.CurrencyId,
                        x.GameType
                    }).Select(r =>
                    {
                        // 預先計算常用的總和，以避免多次重複計算
                        var totalStakeSummary = r.Sum(x => x.StakeSummary);
                        var totalValidBetSummary = r.Sum(x => x.ValidBetSummary);
                        //Progressive Win already included in PlayerWinLoss. 
                        var totalWinLossSummary = r.Sum(x => x.WinLossSummary) + r.Sum(x => x.ProgressiveWinSummary);
                        var totalRevenueSummary = r.Sum(x => x.RevenueSummary);
                        var totalProgressiveWinSummary = r.Sum(x => x.ProgressiveWinSummary);
                        var totalProgressiveBetSummary = r.Sum(x => x.ProgressiveBetSummary);
                        var totalRowCount = r.Sum(x => x.RowCount);
                        var gameType = r.Key.GameType;
                        var totalRevenueSummaryfor9w = r.Sum(x => CalculateCommission(x));


                        // 建立新的MpaWinLossByTpCategoryDto物件並返回
                        return new MpaWinLossByTpCategoryDto
                        {
                            TpCategoryName = tpCategoryMap[r.Key.CategoryId],
                            Currency = currencyNameMapper[r.Key.CurrencyId],
                            TurnOver = totalStakeSummary,
                            ValidTurnOver = totalValidBetSummary,
                            GGR = CalculateGGR(gameType, totalWinLossSummary, totalProgressiveWinSummary, totalProgressiveBetSummary, totalRevenueSummaryfor9w),
                            PlayerWinLoss = totalWinLossSummary,
                            ProgressiveWin = totalProgressiveWinSummary,
                            ProgressiveBet = totalProgressiveBetSummary,
                            Commission = r.Sum(x => CalculateCommission(x)),
                            BetCount = totalRowCount,
                            AvgBetAmount = totalRowCount != 0 ? totalStakeSummary / totalRowCount : 0.00m,
                            Margin = totalValidBetSummary != 0
                                ? CalculateGGR(gameType, totalWinLossSummary, totalProgressiveWinSummary, totalProgressiveBetSummary, totalRevenueSummaryfor9w) / totalValidBetSummary
                                : 0.00m,
                            UniquePlayers = r.DistinctBy(x => x.CustId).Count(),
                        };
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

        private decimal GetGGR(decimal winLossSummary, decimal progressiveWinSummary, decimal progressiveBetSummary)
        {
            return (winLossSummary * -1) + (progressiveWinSummary - progressiveBetSummary);
        }

        #endregion

        #region GroupBy Player Master

        /// <summary>
        /// excel export
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MpaWinLossByPlayerDtoExport>> GetMpaWinLossByPlayerExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
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

            #region total
            var exportDataWithoutAll = exportData.Where(r => r.Currency != "All").ToList();
            exportData.Add(new MpaWinLossByPlayerDto()
            {
                TpUserName = "total",
                TurnOver = exportDataWithoutAll.Sum(x => x.TurnOver),
                ValidTurnOver = exportDataWithoutAll.Sum(x => x.ValidTurnOver),
                GGR = exportDataWithoutAll.Sum(x => x.GGR),
                PlayerWinLoss = exportDataWithoutAll.Sum(x => x.PlayerWinLoss),
                ProgressiveWin = exportDataWithoutAll.Sum(x => x.ProgressiveWin),
                ProgressiveBet = exportDataWithoutAll.Sum(x => x.ProgressiveBet),
                Commission = exportDataWithoutAll.Sum(x => x.Commission),
                BetCount = exportDataWithoutAll.Sum(x => x.BetCount),
                ClosingBalance = exportDataWithoutAll.Sum(x => x.ClosingBalance),
                UniquePlayers = exportDataWithoutAll.Where(x => !excludeUserName.Contains(x.UserName)).DistinctBy(x => x.UserName).Count(),
                DepositCount = exportDataWithoutAll.Sum(x => x.DepositCount),
                TotalDepositAmount = exportDataWithoutAll.Sum(x => x.TotalDepositAmount),
                WithDrawCount = exportDataWithoutAll.Sum(x => x.WithDrawCount),
                TotalWithDrawAmount = exportDataWithoutAll.Sum(x => x.TotalWithDrawAmount),
                TotalBonus = exportDataWithoutAll.Sum(x => x.TotalBonus),
                AvgBetAmount = exportDataWithoutAll.Sum(x => x.BetCount) != 0
                    ? (exportDataWithoutAll.Sum(x => x.TurnOver) / exportDataWithoutAll.Sum(x => x.BetCount))
                    : 0.00m,

                Margin = exportDataWithoutAll.Sum(x => x.ValidTurnOver) != 0
                    ? exportDataWithoutAll.Sum(x => x.GGR) / exportDataWithoutAll.Sum(x => x.ValidTurnOver)
                    : 0.00m,
            });

            #endregion

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
            var apiData = new List<MpaWinLossByPlayerResponse>();
            apiData.Clear();
            var rowCount = 0;
            var excludeUserName = new List<string>();
            MpaWinLossByPlayerResponse totalItem;

            var result = await GetWinLossByPlayer(request, groupByType);
            var userNames = result.OriginData.Select(x => new { x.UserName, x.SiteId }).Distinct();
            foreach (var userName in userNames)
            {
                var dateSummary = result.AlreadyMultiplyRateData.Where(x => x.UserName == userName.UserName & x.SiteId == userName.SiteId).ToList();
                var ggr = dateSummary.Sum(x => x.GGR);
                var betCount = dateSummary.Sum(x => x.BetCount);
                if (betCount == 0) { excludeUserName.Add(userName.UserName); }

                var responseItem = new MpaWinLossByPlayerResponse
                {
                    SabaId = dateSummary.Select(r => r.TpUserName).FirstOrDefault(),
                    UserName = dateSummary.Select(r => r.UserName).FirstOrDefault(),
                    TurnOver = dateSummary.Sum(x => x.TurnOver),
                    ValidTurnOver = dateSummary.Sum(x => x.ValidTurnOver),
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
                        ? dateSummary.Sum(x => x.TurnOver) / betCount
                        : 0.00m,

                    Margin = dateSummary.Sum(x => x.ValidTurnOver) != 0
                        ? ggr / dateSummary.Sum(x => x.ValidTurnOver)
                        : 0.00m,
                    TotalBonus = dateSummary.Sum(x => x.TotalBonus),
                    CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                        ? GetOriginalCurrencyDetailByPlayer(result.OriginData, userName.UserName, _common)
                        : GetCurrencyDetailByPlayer(dateSummary, _common),
                };
                apiData.Add(responseItem);
            }

            rowCount = apiData.Count;
            apiData = request.OrderBy ? apiData.OrderBy(request).ToList() : apiData;
            totalItem = GetMpaWinLossByPlayerTotal(apiData.ToDictionary(k => $"{k.UserName}-{k.SabaId}", v => v), excludeUserName);

            if (request.PageNumber != -1)
            {
                var (index, count) = pageInfo.ListPagination(apiData.Count, request.PageNumber, request.PageSize);
                apiData = apiData.GetRange(index, count);
            }

            var response = apiData.ToDictionary(k => new UserNameKey { UserName = k.UserName, SabaId = k.SabaId }, v => v);
            response.Add(new UserNameKey { Total = "total" }, totalItem);
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
        public class UserNameKey
        {
            public string UserName { get; set; }
            public string SabaId { get; set; }
            public string Total { get; set; }
        }

        /// <summary>
        /// BetRecord + payment + bonus
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<MpaWinLossByPlayerWrapper> GetWinLossByPlayer(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
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
                    x.SiteId,
                })
                .Select(r => new MpaWinLossByPlayerDto
                {
                    UserName = r.Key.UserName,
                    TpUserName = r.Key.TpUserName,
                    Currency = r.Key.Currency,
                    SiteId = r.Key.SiteId,
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

        private static Dictionary<string, MpaWinLossByPlayerCurrencyDetailDto> GetCurrencyDetailByPlayer(List<MpaWinLossByPlayerDto> dateSummary, Common _common)
        {
            return dateSummary.ToDictionary(k => k.Currency, v => new MpaWinLossByPlayerCurrencyDetailDto
            {
                UserName = v.UserName,
                SabaId = v.TpUserName,
                Currency = v.Currency,
                TurnOver = v.TurnOver,
                ValidTurnOver = v.ValidTurnOver,
                GGR = v.GGR,
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

        private static Dictionary<string, MpaWinLossByPlayerCurrencyDetailDto> GetOriginalCurrencyDetailByPlayer(IEnumerable<MpaWinLossByPlayerDto> combineResult, string key, Common _common)
        {
            return combineResult.Where(x => x.UserName == key)
                .ToDictionary(k => k.Currency, v => new MpaWinLossByPlayerCurrencyDetailDto
                {
                    UserName = v.UserName,
                    SabaId = v.TpUserName,
                    Currency = v.Currency,
                    TurnOver = v.TurnOver,
                    ValidTurnOver = v.ValidTurnOver,
                    GGR = v.GGR,
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
                return source
                    .GroupBy(x => new
                    {
                        x.UserName,
                        x.TpUserName,
                        x.CurrencyId,
                        x.SiteId,
                        x.GameType
                    })
                    .Select(r =>
                    {
                        // 預先計算常用的總和，以避免多次重複計算
                        var totalStakeSummary = r.Sum(x => x.StakeSummary);
                        var totalValidBetSummary = r.Sum(x => x.ValidBetSummary);
                        //Progressive Win already included in PlayerWinLoss. 
                        var totalWinLossSummary = r.Sum(x => x.WinLossSummary) + r.Sum(x => x.ProgressiveWinSummary);
                        var totalRevenueSummary = r.Sum(x => x.RevenueSummary);
                        var totalProgressiveWinSummary = r.Sum(x => x.ProgressiveWinSummary);
                        var totalProgressiveBetSummary = r.Sum(x => x.ProgressiveBetSummary);
                        var totalRowCount = r.Sum(x => x.RowCount);
                        var gameType = r.Key.GameType;
                        var totalRevenueSummaryfor9w = r.Sum(x => CalculateCommission(x));


                        // 建立新的MpaWinLossByPlayerDto物件並返回
                        return new MpaWinLossByPlayerDto
                        {
                            UserName = r.Key.UserName,
                            TpUserName = r.Key.TpUserName,
                            Currency = currencyNameMapper[r.Key.CurrencyId],
                            SiteId = r.Key.SiteId,
                            ClosingBalance = r.Sum(x => x.ClosingBalance),
                            TurnOver = totalStakeSummary,
                            ValidTurnOver = totalValidBetSummary,
                            GGR = CalculateGGR(gameType, totalWinLossSummary, totalProgressiveWinSummary, totalProgressiveBetSummary, totalRevenueSummaryfor9w),
                            PlayerWinLoss = totalWinLossSummary,
                            ProgressiveWin = totalProgressiveWinSummary,
                            ProgressiveBet = totalProgressiveBetSummary,
                            Commission = r.Sum(x => CalculateCommission(x)),
                            BetCount = totalRowCount,
                            AvgBetAmount = totalRowCount != 0 ? totalStakeSummary / totalRowCount : 0.00m,
                            Margin = totalValidBetSummary != 0
                                ? CalculateGGR(gameType, totalWinLossSummary, totalProgressiveWinSummary, totalProgressiveBetSummary, totalRevenueSummaryfor9w) / totalValidBetSummary
                                : 0.00m,
                            UniquePlayers = r.DistinctBy(x => x.CustId).Count()
                        };
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

        private bool isOnegameorV8(int gameType)
        {
            return (gameType == (int)MpaGGRCustomizationGameTypeEnum.OneGamingPoker) || (gameType == (int)MpaGGRCustomizationGameTypeEnum.V8Poker);
        }
        private bool is9w(int gameType)
        {
            return (gameType == (int)MpaGGRCustomizationGameTypeEnum.NineW);
        }



        #endregion

        #region GroupBy Player Detail

        public async Task<IEnumerable<MpaWinLossByPlayerDetailResponseFrontEnd>> GetMpaWinLossByPlayerDetailResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
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

            var orderByProperty = request.OrderByProperty.ToLower() == "username"
                ? "key"
                : request.OrderByProperty;


            apiData = request.OrderBy
                ? _mpaCommonService.SortApiData(apiData, request.OrderByProperty, request.OrderByDirection)
                : apiData;


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
        public async Task<IEnumerable<MpaWinLossByCurrencyDtoExport>> GetMapWinLossByCurrencyExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
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
            var originCombineData = originBetRecord
                    .Concat(originTransaction)
                    .Concat(originBonus)
                    .ToList();

            var alreadyMultiplyRateCombineData = alreadyMultiplyRateBetRecord
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

            var keys = originCombineResult.Select(x => x.Currency).Distinct();
            var allCurrencySummary = new List<MpaWinLossByCurrencyDto>();
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

                allCurrencySummary.Add(result);
            }

            #region Sort

            allCurrencySummary = request.OrderBy
                ? allCurrencySummary.OrderBy(request).ToList()
                : allCurrencySummary;
            var currencyDetails = request.DisplayGroupBreakdownInOriginalCurrency
                ? originCombineResult
                : alreadyMultiplyRateCombineResult;

            var exportData = new List<MpaWinLossByCurrencyDto>();
            foreach (var currencySummary in allCurrencySummary)
            {
                var currencyDetail = request.OrderBy
                    ? currencyDetails.Where(r => r.Currency == currencySummary.Currency.Replace("_All", "")).OrderBy(request).ToList()
                    : currencyDetails.Where(r => r.Currency == currencySummary.Currency.Replace("_All", "")).ToList();
                exportData.Add(currencySummary);
                exportData.AddRange(currencyDetail);
            }

            #endregion

            #region total

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

            #endregion

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
            var userServerInfo = _serverInfoBll.GetSiteListByLoginUserV2();
            ((IApiRequest)request).ConvertToMultiSiteParam(userServerInfo);
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
            var originCombineData = originBetRecord
                .Concat(originTransaction)
                .Concat(originBonus)
                .ToList();

            var alreadyMultiplyRateCombineData = alreadyMultiplyRateBetRecord
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

            #region Sort

            var orderByProperty = request.OrderByProperty.ToLower() == "currency"
                ? "key"
                : request.OrderByProperty;

            apiData = request.OrderBy
                ? _mpaCommonService.SortApiData(apiData, orderByProperty, request.OrderByDirection)
                : apiData;

            foreach (var item in apiData)
            {
                var currencyDetail = request.OrderBy
                    ? _mpaCommonService.SortApiData(item.Value.CurrencyDetail, orderByProperty, request.OrderByDirection)
                    : item.Value.CurrencyDetail;
                item.Value.CurrencyDetail = currencyDetail;
            }

            #endregion

            #region total

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

            #endregion

            return apiData.ToApiFormat();
        }

        private static Dictionary<string, MpaWinLossByCurrencyCurrencyDetailDto> GetCurrencyDetailByCurrency(List<MpaWinLossByCurrencyDto> dateSummary)
        {
            return dateSummary.ToDictionary(k => k.Currency, v => new MpaWinLossByCurrencyCurrencyDetailDto
            {
                TurnOver = v.TurnOver,
                ValidTurnOver = v.ValidTurnOver,
                GGR = v.GGR,
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
                    GGR = v.GGR,
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
                return source
                    .GroupBy(x => new
                    {
                        x.CurrencyId,
                        x.GameType
                    }).Select(r =>
                    {
                        // 預先計算常用的總和，以避免多次重複計算
                        var totalStakeSummary = r.Sum(x => x.StakeSummary);
                        var totalValidBetSummary = r.Sum(x => x.ValidBetSummary);
                        //Progressive Win already included in PlayerWinLoss. 
                        var totalWinLossSummary = r.Sum(x => x.WinLossSummary) + r.Sum(x => x.ProgressiveWinSummary);
                        var totalRevenueSummary = r.Sum(x => x.RevenueSummary);
                        var totalProgressiveWinSummary = r.Sum(x => x.ProgressiveWinSummary);
                        var totalProgressiveBetSummary = r.Sum(x => x.ProgressiveBetSummary);
                        var totalRowCount = r.Sum(x => x.RowCount);
                        var gameType = r.Key.GameType;
                        var totalRevenueSummaryfor9w = r.Sum(x => CalculateCommission(x));


                        // 建立新的MpaWinLossByCurrencyDto物件並返回
                        return new MpaWinLossByCurrencyDto
                        {
                            Currency = currencyNameMapper[r.Key.CurrencyId],
                            TurnOver = totalStakeSummary,
                            ValidTurnOver = totalValidBetSummary,
                            GGR = CalculateGGR(gameType, totalWinLossSummary, totalProgressiveWinSummary, totalProgressiveBetSummary, totalRevenueSummaryfor9w),
                            PlayerWinLoss = totalWinLossSummary,
                            ProgressiveWin = totalProgressiveWinSummary,
                            ProgressiveBet = totalProgressiveBetSummary,
                            Commission = r.Sum(x => CalculateCommission(x)),
                            BetCount = totalRowCount,
                            AvgBetAmount = totalRowCount != 0 ? totalStakeSummary / totalRowCount : 0.00m,
                            Margin = totalValidBetSummary != 0
                                ?CalculateGGR(gameType, totalWinLossSummary, totalProgressiveWinSummary, totalProgressiveBetSummary, totalRevenueSummaryfor9w) / totalValidBetSummary
                                : 0.00m,
                            UniquePlayers = r.DistinctBy(x => x.CustId).Count(),
                        };
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
                    x.SiteId
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
                            : 0,
                        SiteId = r.Key.SiteId
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
                    x.SiteId
                })
                    .Select(r => new MpaWinLossByPlayerDto
                    {
                        UserName = r.Key.UserName,
                        TpUserName = r.Key.TpUserName,
                        Currency = currencyNameMapper[r.Key.CurrencyId],
                        SiteId = r.Key.SiteId,
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
                    x.CurrencyId,
                    x.SiteId
                }).Select(r => new MpaWinLossByDateDto
                {
                    Date = groupByType == MpaWinLossGroupByTypeEnum.Date
                        ? r.Key.Date.ToString("yyyy-MM-dd")
                        : r.Key.Date.ToString("yyyy-MM"),
                    Currency = currencyNameMapper[r.Key.CurrencyId],
                    TotalBonus = r.Sum(x => x.BonusAmountClaim),
                    SiteId = r.Key.SiteId
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
                    x.SiteId
                }).Select(r => new MpaWinLossByPlayerDto
                {
                    UserName = r.Key.UserName,
                    TpUserName = r.Key.TpUserName,
                    Currency = currencyNameMapper[r.Key.CurrencyId],
                    SiteId = r.Key.SiteId,
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
                tasks.Add(Task.Run(() => _mpaWinLossService.GetMapTpBetRecord<MpaTpBetRecordDto>(request.SiteIds, segmentStart, segmentEnd.AddSeconds(-1), format, request.MemberTypeJson(), request.DateType)));
            }

            await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                data.AddRange(task.Result);
            }

            data = data.Where(x => !string.IsNullOrEmpty(x.UserName)).ToList();
            
            //IDR轉KIDR，VND轉KVND
            decimal rate = 0.001m;
            foreach (var item in data)
            {
                if (item.CurrencyId == 15 || item.CurrencyId == 51)
                {
                    item.StakeSummary = item.StakeSummary * rate;
                    item.ValidBetSummary = item.ValidBetSummary * rate;
                    item.WinLossSummary = item.WinLossSummary * rate;
                    item.RevenueSummary = item.RevenueSummary * rate;
                    item.ProgressiveWinSummary = item.ProgressiveWinSummary * rate;
                    item.ProgressiveBetSummary = item.ProgressiveBetSummary * rate;
                    item.ClosingBalance = item.ClosingBalance * rate;
                }
            }

            return data;
        }

        private async Task<TransactionRelayWrapper> GetMapTransactionByDate(MpaWinLossRequest request,
            MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var originTransaction = (await _mpaWinLossService.GetMapTransaction<TransactionRelayDto>(request.SiteIds, request.StarTime, request.EndTime, format, request.MemberTypeJson())).ToList();
            originTransaction = FilterMapWinLossTransaction(request, originTransaction);

            var mapRate = await GetMapRate(request);
            var alreadyMultiplyRateTransaction = (from item in originTransaction
                                                  //若為VND與IDR則匯率轉為1
                                                  let rate = (item.CurrencyId == 15 && request.BaseCurrencyId == 15) || (item.CurrencyId == 51 && request.BaseCurrencyId == 51) ?
                                                  1
                                                  : mapRate[$"{item.Date.DateMapperToRateDate()}{item.CurrencyId}{request.BaseCurrencyId}"]
                                                  //baseCurrency選VND與IDR時，顯示千單位
                                                  let baseCurrencyRate = (item.CurrencyId != 15 && request.BaseCurrencyId == 15) || (item.CurrencyId != 51 && request.BaseCurrencyId == 51) ?
                                                  0.001m
                                                  : 1
                                                  //Currency選VND與IDR時，轉換其他幣別時顯示正常單位
                                                  let currencyRate = (item.CurrencyId == 15 && request.BaseCurrencyId != 15) || (item.CurrencyId == 51 && request.BaseCurrencyId != 51) ?
                                                  1000m
                                                  : 1
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
                                                      Amount = item.Amount * rate * baseCurrencyRate * currencyRate,
                                                      RowCount = item.RowCount
                                                  }).ToList();

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

            if (!request.IncludeTest)
            {
                originTransaction = originTransaction.Where(x => x.IsTest == 0).ToList();
            }

            //IDR轉KIDR，VND轉KVND
            decimal rate = 0.001m;
            foreach (var item in originTransaction)
            {
                if (item.CurrencyId == 15 || item.CurrencyId == 51)
                {
                    item.Amount = item.Amount * rate;
                }
            }

            return originTransaction;
        }

        private async Task<MpaWinLossBonusWrapper> GetMapWinLossBonusByDate(MpaWinLossRequest request,
            MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var originBonus = (await _mpaWinLossService.GetMpaWinLossBonus(request.SiteIds, request.StarTime, request.EndTime, format, request.MemberTypeJson())).ToList();
            originBonus = FilterMpaWinLossBonus(request, originBonus);

            var mapRate = await GetMapRate(request);
            var alreadyMultiplyRateBonus = (from item in originBonus
                                            //若為VND與IDR則匯率轉為1
                                            let rate = (item.CurrencyId == 15 && request.BaseCurrencyId == 15) || (item.CurrencyId == 51 && request.BaseCurrencyId == 51) ?
                                            1
                                            : mapRate[$"{item.Date.DateMapperToRateDate()}{item.CurrencyId}{request.BaseCurrencyId}"]
                                            //baseCurrency選VND與IDR時，顯示千單位
                                            let baseCurrencyRate = (item.CurrencyId != 15 && request.BaseCurrencyId == 15) || (item.CurrencyId != 51 && request.BaseCurrencyId == 51) ?
                                            0.001m
                                            : 1
                                            //Currency選VND與IDR時，轉換其他幣別時顯示正常單位
                                            let currencyRate = (item.CurrencyId == 15 && request.BaseCurrencyId != 15) || (item.CurrencyId == 51 && request.BaseCurrencyId != 51) ?
                                            1000m
                                            : 1
                                            select new MpaWinLossBonusDto()
                                            {
                                                Date = item.Date,
                                                CustId = item.CustId,
                                                UserName = item.UserName,
                                                TpUserName = item.TpUserName,
                                                CurrencyId = item.CurrencyId,
                                                BonusAmountClaim = item.BonusAmountClaim * rate * baseCurrencyRate * currencyRate,
                                                SiteId = item.SiteId
                                            }).ToList();

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

            if (!request.IncludeTest)
            {
                originBonus = originBonus.Where(x => x.IsTest == 0).ToList();
            }

            //IDR轉KIDR，VND轉KVND
            decimal rate = 0.001m;
            foreach (var item in originBonus)
            {
                if (item.CurrencyId == 15 || item.CurrencyId == 51)
                {
                    item.BonusAmountClaim = item.BonusAmountClaim * rate;
                }
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
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var data = await GetMapTpBetRecordByDate(request, groupByType);
            var tpBetRecordRelayResult = data.Filter(request)
                .SetCommissionByNineW(request, format, _mpaWinLossService.Get9WCommission)
                .ToList();

            var mapRate = await GetMapRate(request);
            wrapperResult.AlreadyMultiplyRateBetRecord = (from item in tpBetRecordRelayResult
                                                          //若為VND與IDR則匯率轉為1
                                                          let rate = (item.CurrencyId == 15 && request.BaseCurrencyId == 15) || (item.CurrencyId == 51 && request.BaseCurrencyId == 51)?
                                                          1
                                                          :mapRate[$"{item.Date.DateMapperToRateDate()}{item.CurrencyId}{request.BaseCurrencyId}"]
                                                          //baseCurrency選VND與IDR時，顯示千單位
                                                          let  baseCurrencyRate = (item.CurrencyId != 15 && request.BaseCurrencyId ==15) || (item.CurrencyId != 51 && request.BaseCurrencyId == 51) ? 
                                                          0.001m
                                                          :1
                                                          //Currency選VND與IDR時，轉換其他幣別時顯示正常單位
                                                          let currencyRate = (item.CurrencyId == 15 && request.BaseCurrencyId != 15) || (item.CurrencyId == 51 && request.BaseCurrencyId != 51) ?
                                                          1000m
                                                          :1
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
                                                              StakeSummary = item.StakeSummary * rate * baseCurrencyRate * currencyRate,
                                                              ValidBetSummary = item.ValidBetSummary * rate * baseCurrencyRate * currencyRate,
                                                              WinLossSummary = item.WinLossSummary * rate * baseCurrencyRate * currencyRate,
                                                              RevenueSummary = item.RevenueSummary * rate * baseCurrencyRate * currencyRate,
                                                              ProgressiveWinSummary = item.ProgressiveWinSummary * rate * baseCurrencyRate * currencyRate,
                                                              ProgressiveBetSummary = item.ProgressiveBetSummary * rate * baseCurrencyRate * currencyRate,
                                                              ClosingBalance = item.ClosingBalance * rate * baseCurrencyRate * currencyRate,
                                                              RowCount = item.RowCount,
                                                          }).ToList();

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
