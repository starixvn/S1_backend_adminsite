using Newtonsoft.Json;

namespace BackendSite.Service.Model.Mpa.WinLoss
{
    public class MpaWinLossByDateResponse
    {
        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }
        public decimal BAPlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 6)]
        public decimal Commission { get; set; }
        public decimal BACommission { get; set; }

        [JsonProperty(Order = 7)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 8)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin { get; set; }

        [JsonProperty(Order = 11)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 12)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 13)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 14)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 15)]
        public decimal TotalBonus { get; set; }

        public Dictionary<string, MpaWinLossByDayCurrencyDetailDto> CurrencyDetail { get; set; }
    }

    public class MpaWinLossByTpGameTypeResponse
    {
        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 6)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 7)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 8)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin { get; set; }

        public Dictionary<string, MpaWinLossByTpGameTypeCurrencyDetailDto> CurrencyDetail { get; set; }
        public decimal BAPlayerWinLoss { get; set; }
        public decimal BACommission { get; set; }
    }

    public class MpaWinLossByTpCategoryResponse
    {
        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 6)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 7)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 8)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin { get; set; }

        public Dictionary<string, MpaWinLossByTpCategoryCurrencyDetailDto> CurrencyDetail { get; set; }
        public IEnumerable<MpaWinLossByTpCategoryCurrencyDetailDto> CurrencyDetailByCategory { get; set; }
        public decimal BAPlayerWinLoss { get; set; }
        public decimal BACommission { get; set; }
    }

    public class MpaWinLossByPlayerResponse
    {
        [JsonProperty(Order = 0)]
        public string SabaId { get; set; }

        [JsonProperty(Order = 1)]
        public string UserName { get; set; }

        [JsonProperty(Order = 2)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 3)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 4)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 5)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 6)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 7)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 8)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 9)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 10)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 11)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 12)]
        public decimal Margin { get; set; }

        [JsonProperty(Order = 13)]
        public decimal ClosingBalance { get; set; }

        [JsonProperty(Order = 14)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 15)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 16)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 17)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 18)]
        public decimal TotalBonus { get; set; }

        public Dictionary<string, MpaWinLossByPlayerCurrencyDetailDto> CurrencyDetail { get; set; }
        public IEnumerable<MpaWinLossByPlayerCurrencyDetailDto> CurrencyDetailByplayer { get; set; }

        public decimal BAPlayerWinLoss { get; set; }
        public decimal BACommission { get; set; }
    }

    public class MpaWinLossByPlayerDetailResponse
    {
        [JsonProperty(Order = 0)]
        public string SabaId { get; set; }

        [JsonProperty(Order = 1)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 3)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 4)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 6)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 7)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 8)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 9)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 10)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 11)]
        public decimal Margin { get; set; }
    }

    public class MpaWinLossByCurrencyResponse
    {
        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 6)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 7)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 8)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin { get; set; }

        [JsonProperty(Order = 11)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 12)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 13)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 14)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 15)]
        public decimal TotalBonus { get; set; }

        public Dictionary<string, MpaWinLossByCurrencyCurrencyDetailDto> CurrencyDetail { get; set; }
        public decimal BAPlayerWinLoss { get; set; }
        public decimal BACommission { get; set; }
    }

    public class MpaWinLossByBrandResponse
    {
        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal BAPlayerWinLoss { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 6)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 7)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 8)]
        public decimal BACommission { get; set; }

        [JsonProperty(Order = 9)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 10)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 11)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 12)]
        public decimal Margin { get; set; }

        [JsonProperty(Order = 13)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 14)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 15)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 16)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 17)]
        public decimal TotalBonus { get; set; }

        public Dictionary<string, MpaWinLossByBrandCurrencyDetailDto> CurrencyDetail { get; set; }
    }

    #region To FrontEnd API 格式

    public class MpaWinLossByDateResponseToFrontEnd
    {
        [JsonProperty(Order = -1)]
        public string DisplayKey { get; set; }

        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal BAPlayerWinLoss { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 6)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 7)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 8)]
        public decimal BACommission { get; set; }

        [JsonProperty(Order = 9)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 10)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 11)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 12)]
        public decimal Margin { get; set; }

        [JsonProperty(Order = 13)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 14)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 15)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 16)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 17)]
        public decimal TotalBonus { get; set; }

        public List<MpaWinLossByDayCurrencyDetailDtoToFrontEnd> Children { get; set; }
    }

    public class MpaWinLossByTpCategoryResponseToFrontEnd
    {
        [JsonProperty(Order = -1)]
        public string DisplayKey { get; set; }

        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal BAPlayerWinLoss { get; set;}

        [JsonProperty(Order = 5)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 6)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 7)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 8)]
        public decimal BACommission { get; set; }

        [JsonProperty(Order = 9)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 10)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 11)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 12)]
        public decimal Margin { get; set; }

        public List<MpaWinLossByTpCategoryCurrencyDetailDtoFrontEnd> Children { get; set; }
    }

    public class MpaWinLossByTpGameTypeResponseToFrontEnd
    {
        [JsonProperty(Order = -1)]
        public string DisplayKey { get; set; }

        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal BAPlayerWinLoss { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 6)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 7)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 8)]
        public decimal BACommission { get; set; }

        [JsonProperty(Order = 9)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 10)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 11)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 12)]
        public decimal Margin { get; set; }

        public List<MpaWinLossByTpGameTypeCurrencyDetailDtoFrontEnd> Children { get; set; }
    }

    public class MpaWinLossByPlayerResponseToFrontEndWrapper
    {
        public IEnumerable<MpaWinLossByPlayerResponseToFrontEnd> Data { get; set; }

        public int RowCount { get; set; }
    }

    public class MpaWinLossByPlayerResponseToFrontEnd
    {
        [JsonProperty(Order = -1)]
        public string DisplayKey { get; set; }

        [JsonProperty(Order = 0)]
        public string SabaId { get; set; }

        [JsonProperty(Order = 1)]
        public string UserName { get; set; }

        [JsonProperty(Order = 2)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 3)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 4)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 5)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 6)]
        public decimal BAPlayerWinLoss { get; set; }

        [JsonProperty(Order = 7)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 8)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 9)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 10)]
        public decimal BACommission { get; set; }

        [JsonProperty(Order = 11)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 12)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 13)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 14)]
        public decimal Margin { get; set; }

        [JsonProperty(Order = 15)]
        public decimal ClosingBalance { get; set; }

        [JsonProperty(Order = 16)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 17)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 18)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 19)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 20)]
        public decimal TotalBonus { get; set; }

        public List<MpaWinLossByPlayerCurrencyDetailDtoFrontEnd> Children { get; set; }
    }

    public class MpaWinLossByPlayerDetailResponseFrontEnd
    {
        [JsonProperty(Order = -1)]
        public string DisplayKey { get; set; }

        [JsonProperty(Order = 0)]
        public string SabaId { get; set; }

        [JsonProperty(Order = 1)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 3)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 4)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 6)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 7)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 8)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 9)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 10)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 11)]
        public decimal Margin { get; set; }

        [JsonProperty(Order = 12)]
        public decimal ClosingBalance => 0.0m;

        [JsonProperty(Order = 13)]
        public int DepositCount => 0;

        [JsonProperty(Order = 14)]
        public decimal TotalDepositAmount => 0.0m;

        [JsonProperty(Order = 15)]
        public int WithDrawCount => 0;

        [JsonProperty(Order = 16)]
        public decimal TotalWithDrawAmount => 0.0m;

        [JsonProperty(Order = 17)]
        public decimal TotalBonus => 0.0m;
    }

    public class MpaWinLossByCurrencyResponseFrontEnd
    {
        [JsonProperty(Order = -1)]
        public string DisplayKey { get; set; }

        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal BAPlayerWinLoss { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 6)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 7)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 8)]
        public decimal BACommission { get; set; }

        [JsonProperty(Order = 9)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 10)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 11)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 12)]
        public decimal Margin { get; set; }

        [JsonProperty(Order = 13)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 14)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 15)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 16)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 17)]
        public decimal TotalBonus { get; set; }

        public List<MpaWinLossByCurrencyCurrencyDetailDtoFrontEnd> Children { get; set; }
    }

    public class MpaWinLossByBrandResponseToFrontEnd
    {
        [JsonProperty(Order = -1)]
        public string DisplayKey { get; set; }

        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR { get; set; }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal BAPlayerWinLoss { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 6)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 7)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 8)]
        public decimal BACommission { get; set; }

        [JsonProperty(Order = 9)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 10)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 11)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 12)]
        public decimal Margin { get; set; }

        [JsonProperty(Order = 13)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 14)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 15)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 16)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 17)]
        public decimal TotalBonus { get; set; }

        public List<MpaWinLossByBrandCurrencyDetailDtoToFrontEnd> Children { get; set; }
    }

    #endregion
}


