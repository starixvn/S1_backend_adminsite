using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.BLL.Mpa.Extension;

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

        #region Public BLL Method

        public async Task<Dictionary<string, MpaWinLossByDateResponse>> GetMapWinLossByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單資料
            var data = await GetTpBetRecordByDateResult(request, groupByType);
            var tpBetRecordResult = data.TpBetRecordResult;

            // 存提款 
            var transactionResult = await GetTransactionResult(request, groupByType);

            // 注單 + 存提款資料合併
            var combineData = request.OrderBy
                ? tpBetRecordResult
                    .Concat(transactionResult)
                    .OrderBy(request)
                    .ToList()
                : tpBetRecordResult
                    .Concat(transactionResult)
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
            });

            var responseResult = new Dictionary<string, MpaWinLossByDateResponse>();
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
                    CurrencyDetail = request.DisplayGroupBreakdownInOriginalCurrency
                    ? GetOriginalCurrencyDetailByDate(combineResult, key)
                    : GetCurrencyDetailByDate(dateSummary)
                };

                responseResult.Add(key, result);
            }

            // total
            responseResult.Add("total", new MpaWinLossByDateResponse()
            {
                TurnOver = responseResult.Sum(x => x.Value.TurnOver),
                ValidTurnOver = responseResult.Sum(x => x.Value.ValidTurnOver),
                GGR = responseResult.Sum(x => x.Value.GGR),
                PlayerWinLoss = responseResult.Sum(x => x.Value.PlayerWinLoss),
                ProgressiveWin = responseResult.Sum(x => x.Value.ProgressiveWin),
                ProgressiveBet = responseResult.Sum(x => x.Value.ProgressiveBet),
                Commission = responseResult.Sum(x => x.Value.Commission),
                BetCount = responseResult.Sum(x => x.Value.BetCount),
                UniquePlayers = data.UniquePlayers,
                DepositCount = responseResult.Sum(x => x.Value.DepositCount),
                TotalDepositAmount = responseResult.Sum(x => x.Value.TotalDepositAmount),
                WithDrawCount = responseResult.Sum(x => x.Value.WithDrawCount),
                TotalWithDrawAmount = responseResult.Sum(x => x.Value.TotalWithDrawAmount),
                TotalBonus = responseResult.Sum(x => x.Value.TotalBonus),
                AvgBetAmount = responseResult.Sum(x => x.Value.BetCount) != 0
                    ? (responseResult.Sum(x => x.Value.ValidTurnOver) / responseResult.Sum(x => x.Value.BetCount))
                    : 0.00m,

                Margin = responseResult.Sum(x => x.Value.ValidTurnOver) != 0
                    ? responseResult.Sum(x => x.Value.GGR) / responseResult.Sum(x => x.Value.ValidTurnOver)
                    : 0.00m,
            });

            return responseResult;
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
                TotalWithDrawAmount = v.TotalWithDrawAmount
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
                    TotalWithDrawAmount = v.TotalWithDrawAmount
                });
        }

        public async Task<Dictionary<string, MpaWinLossByTpGameTypeResponse>> GetMapWinLossByTpGameTypeResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var responseResult = new Dictionary<string, MpaWinLossByTpGameTypeResponse>();

            // 注單資料
            var data = await GetTpBetRecordByTpGameTypeResult(request, groupByType);
            var tpBetRecordResult = data.TpBetRecordResult;

            var combineResult = request.OrderBy
                ? tpBetRecordResult.OrderBy(request)
                : tpBetRecordResult;

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
                    ? GetOriginalCurrecnyDetailByTpGameType(combineResult, key)
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
                        ? responseResult.Sum(x => x.Value.GGR) / responseResult.Sum(x => x.Value.TurnOver)
                        : 0.00m
            });

            return responseResult;
        }

        private static Dictionary<string, MpaWinLossByTpGameTypeCurrencyDetailDto> GetOriginalCurrecnyDetailByTpGameType(IEnumerable<MpaWinLossByTpGameTypeDto> combineResult, string key)
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


        #endregion

        #region TpBetRecodr

        /// <summary>
        /// 取得 GroupBy 日期、月份注單結果
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<TpBetRecordRelayResultWrapper> GetTpBetRecordByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單過濾
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);

            var data = await GetMapTpBetRecordByDate(request, groupByType);
            var tpBetRecordRelayResult = data.Filter(request, GetCustIdByUserName)
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

            var tpBetRecordRelayResultWrapper = new TpBetRecordRelayResultWrapper()
            {
                TpBetRecordResult = tpBetRecordResult,
                UniquePlayers = tpBetRecordRelayResult.DistinctBy(x => x.CustId).Count()
            };

            return tpBetRecordRelayResultWrapper;
        }

        /// <summary>
        /// 取得 GroupBy TpGameType 注單結果
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<TpBetRecordByTpGameTypeWrapper> GetTpBetRecordByTpGameTypeResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            // 注單過濾
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);

            var data = await GetMapTpBetRecordByDate(request, groupByType);
            var tpBetRecordRelayResult = data.Filter(request, GetCustIdByUserName)
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

            var responseResult = new TpBetRecordByTpGameTypeWrapper()
            {
                TpBetRecordResult = tpBetRecordResult,
                UniquePlayers = tpBetRecordRelayResult.DistinctBy(x => x.CustId).Count()
            };

            return responseResult;
        }

        #endregion

        #region Transaction

        /// <summary>
        /// 取得 (充值單/提款單) 結果
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private async Task<IEnumerable<MpaWinLossByDateDto>> GetTransactionResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var transactionRelayResult = (await GetMapTransactionByDate(request, groupByType))
                .Where(r => request.CurrencyIds.Contains(r.CurrencyId));

            if (!string.IsNullOrEmpty(request.UserName))
            {
                var custId = GetCustIdByUserName(request.SiteId, request.UserName);
                transactionRelayResult = transactionRelayResult.Where(r => r.CustId == custId);
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
        private Task<IEnumerable<MpaTpBetRecordByDateDto>> GetMapTpBetRecordByDate(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var redisKey =
                $"MapTpBetRecord:{groupByType}:SiteId:{request.SiteId}:DateType:{request.DateType}:{request.StarTime:yyyy-MM-dd HH:mm:ss}:{request.EndTime:yyyy-MM-dd HH:mm:ss}";

            return _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromMinutes(15),
                () => _mpaWinLossService.GetMapTpBetRecord<MpaTpBetRecordByDateDto>(request.SiteId, request.StarTime, request.EndTime, format, request.DateType));
        }

        /// <summary>
        ///  取得 Deposit And Withdrawal Relay By Date
        /// </summary>
        /// <param name="request"></param>
        /// <param name="groupByType"></param>
        /// <returns></returns>
        private Task<IEnumerable<TransactionRelayDto>> GetMapTransactionByDate(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = MapMpaWinLossGroupByTypeEnumFormat(groupByType);
            var redisKey =
                $"MapTransaction:{groupByType}:SiteId:{request.SiteId}:{request.StarTime:yyyy-MM-dd HH:mm:ss}:{request.EndTime:yyyy-MM-dd HH:mm:ss}";

            return _redisHelper.TryGetOrCreateAsync(redisKey,
                TimeSpan.FromMinutes(15),
                () => _mpaWinLossService.GetMapTransaction<TransactionRelayDto>(request.SiteId, request.StarTime, request.EndTime, format));
        }

        private int GetCustIdByUserName(int siteId, string userName)
        {
            var redisKey = $"MapCustId:SiteId:{siteId}:UserName:{userName}";

            return _redisHelper.TryGetOrCreate(redisKey,
                TimeSpan.FromMinutes(2),
                () => _mpaWinLossService.GetCustIdByUserName(siteId, userName));
        }

        private static string MapMpaWinLossGroupByTypeEnumFormat(MpaWinLossGroupByTypeEnum groupByType)
        {
            var format = groupByType switch
            {
                MpaWinLossGroupByTypeEnum.Date => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.GameType => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.TpCategory => "%Y-%m-%d",
                MpaWinLossGroupByTypeEnum.Month => "%Y-%m",
                _ => string.Empty
            };
            return format;
        }

        #endregion
    }
}
