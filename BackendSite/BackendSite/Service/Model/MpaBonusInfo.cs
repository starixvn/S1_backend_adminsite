namespace BackendSite.Service.Model
{
    #region MpaBonus
    
    public class MpaBonusInfo : MpaBonusInfoSummary
    {
        public string RecordDate { get; set; }
        public string BonusName { get; set; }
        public string BonusCode { get; set; }
        public int BonusType { get; set; }
        public string TypeName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string BonusStatus { get; set; }
    }
    
    public class MpaBonusInfoSummary
    {
        public decimal GGR { get; set; }
        public int RunningCount { get; set; }
        public int CancelCount { get; set; }
        public int FulFillCount { get; set; }
        public int OtherPlatformCount { get; set; }
        public int TotalClaimants{ get; set; }
        public decimal TotalClaimedAmount { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalWithdrawal { get; set; }
    }

    public class MpaBonusDetail
    {
        public string UserName { get; set; }
        public string BonusCode { get; set; }
        public DateTime ClaimDate { get; set; }
        public decimal BonusAmountClaim { get; set; }
        public int TransactionType{ get; set; }
        public int ClaimStatus{ get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalWithdrawal { get; set; }
        public decimal Completion { get; set; }
    }
    
    #endregion
    
    #region MpaMarketing
    
    public class MpaMarketingBonusInfo
    {
        public MpaMarketingBonusGroupByCurrency GroupByCurrencySum{ get; set; }
        public MpaMarketingBonusGroupByBonusCode GroupByBonusCodeSum{ get; set; }
        public IEnumerable<MpaMarketingBonusGroupByCurrency> GroupByCurrency { get; set; }
        public IEnumerable<MpaMarketingBonusGroupByBonusCodeInfo> GroupByBonusCode { get; set; }
        public int TotalBonusClaimants{ get; set; }

    }
    
    public class MpaMarketingBonusGroupByCurrency
    {
        public int CurrencyId{ get; set; }
        public string Currency { get; set; }
        public decimal BonusAmountClaim { get; set; }
        public decimal BaseBonusAmountClaim { get; set; }
        public decimal ComparedBonusAmountClaim { get; set; }
        public decimal ChangeRate { get; set; }
    }
    
    public class MpaMarketingBonusGroupByBonusCodeInfo
    {
        public MpaMarketingBonusGroupByBonusCode GroupInfo { get; set; }
        public IEnumerable<MpaMarketingBonus> Children { get; set; }
    }
    
    public class MpaMarketingBonusGroupByBonusCode
    {
        public string BonusCode { get; set; }
        public string BonusName { get; set; }
        public int NumberOfBonusUser{ get; set; }
        public decimal BonusAmountClaim { get; set; }
        public decimal BaseBonusAmountClaim { get; set; }
    }
    
    public class MpaMarketingBonus : MpaMarketingBonusGroupByBonusCode
    {
        public int SiteId{ get; set; }
        public int CurrencyId{ get; set; }
        public string Currency { get; set; }
    }
    
    public class MpaMarketingBonusDetail
    {
        public int SiteId{ get; set; }
        public string BonusCode { get; set; }
        public string CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId{ get; set; }
        public string Currency { get; set; }
        public DateTime RegisteredDate { get; set; }
        public string SignupUrl { get; set; }
        public int AffCode{ get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalWithdrawal { get; set; }
        public decimal BonusAmountClaim { get; set; }
    }
    
    #endregion
    
}