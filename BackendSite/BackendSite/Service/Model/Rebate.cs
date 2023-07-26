namespace BackendSite.Service.Model
{
    public class TPCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
    public class RebateCategoryList
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public IEnumerable<RebateLevelData> CategoryLevelList { get; set; }

    }

    public class RebatePeriod
    {
        public int CurrencyId { get; set; }
        public int? PeriodType { get; set; }
        public int? PeriodDay { get; set; }
        public int? RedeemHours { get; set; }
    }
    public class RebateLevelData
    {
        public int Level { get; set; }
        public decimal Percentage { set; get; }
    }
    public class RebatePayoutLimitData
    {
        public int CategoryId { get; set; }
        public decimal MaxPayout { set; get; }
        public decimal MinPayout { set; get; }
    }

    public class RebateCurrencyData
    {
        public int CurrencyId { get; set; }
        public IEnumerable<RebatePayoutLimitData> PayoutLimitList { get; set; }
        public IEnumerable<RebateCategoryList> CategoryDataList { get; set; }

    }

    public class RebateGameTypeDetail
    {
        public int GameType { get; set; }
        public string GameTypeName { get; set; }
        public int Level { get; set; }
        public decimal? Percentage { get; set; }
    }

    public class AdjustmentInfoLogParam
    {
        public string Menu { get; set; }
        public int ActionType { get; set; }// ActionType 1. insert/update 2. delete
        public string DetailInfo { get; set; }
        public string Operator { get; set; }
    }

    public class RebateExcludeGame
    {
        public int TpId { get; set; }
        public string TpName { get; set; }
        public int GameType { get; set; }
        public string GameTypeName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string GameId { get; set; }
        public string GameName { get; set; }
        public string AppId { get; set; }
    }
}
