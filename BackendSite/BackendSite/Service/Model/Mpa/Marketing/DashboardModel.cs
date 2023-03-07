namespace BackendSite.Service.Model.Mpa.Marketing
{
    public class GetDataRequest
    {
        private DateTimeOffset _startDateCompared { get; set; }
        private DateTimeOffset _endDateCompared { get; set; }
        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
        public int SiteId { get; set; }
        public DateTimeOffset StartDate
        {
            get => _startDate;
            set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndDate
        {
            get => _endDate;
            set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset StartDateCompared
        {
            get => _startDateCompared;
            set => _startDateCompared = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndDateCompared
        {
            get => _endDateCompared;
            set => _endDateCompared = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public IEnumerable<int> CurrencyIds { get; set; }
        public int BaseCurrencyId { get; set; }
        public bool IsCompared { get; set; }
        
        public DateTimeOffset GetEndDate()
        {
            //因為過去日期要查月底最後天，假設今天 12/1 去查  11/1 ~ 11/18 的資料，一律用 11/30的匯率 
            var endDate = EndDate.AddMonths(1).AddDays(EndDate.AddMonths(1).Day);
            return endDate;
        }
        
        public DateTimeOffset GetCompareEndDate()
        {
            //因為過去日期要查月底最後天，假設今天 12/1 去查  11/1 ~ 11/18 的資料，一律用 11/30的匯率 
            var endDate = EndDateCompared.AddMonths(1).AddDays(EndDateCompared.AddMonths(1).Day);
            return endDate;
        }
    }
    
    
    public class PerformanceHighlightData
    {
        public int TotalUsers { get; set; }
        public int TotalDepositUsers { get; set; }
        
        public int DepositUsers { get; set; }
        public decimal DepositUsersComparedRate { get; set; }
        
        public int ActiveUsers { get; set; }
        public decimal ActiveUsersComparedRate { get; set; }
        
        public int NRU { get; set; }
        public decimal NRUComparedRate { get; set; }
        
        public int ActiveNRU { get; set; }
        public decimal ActiveNRUComparedRate { get; set; }
        
        public decimal DepositAmount { get; set; }
        public decimal DepositAmountComparedRate { get; set; }
        
        public decimal WithdrawalAmount { get; set; }
        public decimal WithdrawalAmountComparedRate { get; set; }
        
        public decimal NetCashAmount { get; set; }
        public decimal NetCashAmountComparedRate { get; set; }
        
        public decimal BonusAmountClaimed { get; set; }
        public decimal BonusAmountClaimedComparedRate { get; set; }
        
        public decimal ChurnRate { get; set; }
        public decimal ChurnComparedRate { get; set; }
        
        public decimal RetentionRate { get; set; }
        public decimal RetentionComparedRate { get; set; }
        
        public int NoBetUsers { get; set; }
        public int HasBetUsers { get; set; }
        public int BasedOnReportTotalUsers { get; set; }
    }
    
    
}

