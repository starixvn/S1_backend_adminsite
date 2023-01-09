namespace BackendSite.Service.Model.GameStatisticalReport
{
    public class GameStatisticalDao
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int GameType { get; set; }
        public string GameTypeName { get; set; }
        public string GameName { get; set; }
        public int UniquePlayers { get; set; }
        public decimal ValidBetSummary { get; set; }
        public decimal WinLossSummary { get; set; }
    }
}
