using Newtonsoft.Json;

namespace BackendSite.Service.Model.Mpa
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

        [JsonProperty(Order = 4)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 5)]
        public decimal AvgBetAmount { get; set; }

        [JsonProperty(Order = 6)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 7)]
        public decimal Margin { get; set; }

        [JsonProperty(Order = 8)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 9)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 10)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 11)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 12)]
        public decimal TotalBonus { get; set; }

        public Dictionary<string,MpaWinLossByDayCurrencyDetailDto> CurrencyDetail { get; set; }
    }

    //public class MpaWinLossByTpGameTypeDto : MpaWinLossBaseDto
    //{
    //    public string TpGameTypeName { get; set; }
    //}
}
