namespace BackendSite.Service.Model.Mpa.Marketing
{
    public class DashboardBase
    {
        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
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
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
    }

    public class GetDataRequest : DashboardBase
    {
        private DateTimeOffset _startDateCompared { get; set; }
        private DateTimeOffset _endDateCompared { get; set; }
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
    
    public class GetDataRequestExport : DashboardBase
    {
        public int BaseCurrencyId { get; set; }
    }
    
    public class GetUserActivitiesDataRequest : DashboardBase
    {
        public int ViewType { get; set; }
    }
    
    public class GetRegistrationDetailsRequest : DashboardBase
    {
        
    }
    
    public class GetRevenueAvgDataRequest : DashboardBase
    {
        public IEnumerable<int> TpIds { get; set; }
        public int BaseCurrencyId { get; set; }
    }
    
    public class PerformanceHighlightData
    {
        public int TotalUsers { get; set; }
        public int TotalDepositUsers { get; set; }
        
        public int DepositUsers { get; set; }
        public int FirstDepositCount { get; set; }
        public decimal DepositUsersComparedRate { get; set; }
        
        public decimal DepositAmount { get; set; }
        public decimal FirstDepositAmount { get; set; }
        public decimal DepositAmountComparedRate { get; set; }
        
        public int ActiveUsers { get; set; }
        public decimal ActiveUsersComparedRate { get; set; }
        
        public int NRU { get; set; }
        public decimal NRUComparedRate { get; set; }
        
        public int ActiveNRU { get; set; }
        public decimal ActiveNRUComparedRate { get; set; }
        
        
        public decimal WithdrawalUsers { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public decimal WithdrawalAmountComparedRate { get; set; }
        
        public decimal NetCashAmount { get; set; }
        public decimal NetCashAmountComparedRate { get; set; }
        
        public decimal BonusAmountClaimed { get; set; }
        public int TotalBonusClaimants { get; set; }
        public decimal BonusAmountClaimedComparedRate { get; set; }
        
        public decimal ChurnRate { get; set; }
        public decimal ChurnComparedRate { get; set; }
        
        public decimal RetentionRate { get; set; }
        public decimal RetentionComparedRate { get; set; }
        
        public int NoBetUsers { get; set; }
        public int HasBetUsers { get; set; }
        public int BasedOnReportTotalUsers { get; set; }
    }
    
    public class PerformanceHighlightDataExportModel
    {
        //NRU - Newly Registered Users
        public int NRU { get; set; }
        
        // Active Users
        public int ActiveUsers { get; set; }
        
        //Active NRU
        public int ActiveNRU { get; set; }
        
        // Total bonus claimants 筆數
        public decimal TotalBonusClaimants { get; set; }
        
        //  Total bonus amount claimed
        public decimal BonusAmountClaimed { get; set; }
        
        //No. of deposit users  存款人數
        public int DepositUsers { get; set; }
        
        //No. of FTD users
        public int FirstDepositUsers { get; set; }
        
        //No. of withdrawal users 提款人數
        public int WithdrawalUsers { get; set; }
        
        //Total deposit amount
        public decimal DepositAmount { get; set; }
        
        //Total FTD amount
        public decimal FirstDepositAmount { get; set; }
        
        //Total withdrawal amount
        public decimal WithdrawalAmount { get; set; }
        
        public decimal ChurnRate { get; set; }

        public decimal RetentionRate { get; set; }
        
    }
    
    public class PerformanceHighlightDataExport
    {
        public string Menu { get; set; }
        public string Origin { get; set; }
        public string MTD { get; set; }
    }
    
    public class UserActivitiesData
    {
        public DateTime RecordDate { get; set; }
        public int ActiveUsersCount { get; set; }
        public int NRUCount { get; set; }
        public int DayOfYear => (RecordDate.DayOfYear - 1) / 7 + 1;
    }
    
    public class UserActivitiesResult
    {
        public string RecordDate { get; set; }
        public int ActiveUsersCount { get; set; }
        public int NRUCount { get; set; }
    }
    
    
    
    public class RegistrationDetailsResult
    {
        public string SignupUrl { get; set; }
        public int Count { get; set; }
        public bool IsMTD { get; set; }
    }

    public class MarketingNRUExport
    {
        public string SignupUrl { get; set; }
        public int Count { get; set; }
        public int MTDCount { get; set; }
    }
    
    public class RevenueAvgData
    {
        public int TpId { get; set; }
        public string TpName { get; set; }
        public decimal AllUsersRevenue { get; set; }
        public decimal PaidUsersRevenue { get; set; }
    }
}

