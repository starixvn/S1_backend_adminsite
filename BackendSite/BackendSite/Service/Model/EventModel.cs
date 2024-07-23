namespace BackendSite.Service.Model
{
    #region Lottery
    
    public class GetLotteryEventRequest
    {
        public int SiteId { get; set; }
        public DateTime StartDate{ get; set; }
        public DateTime EndDate{ get; set; }
    }
    
    public class LotteryEventResponse
    {
        public int Sq { get; set; }
        public int SiteId { get; set; }
        public int RoundId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DrawDate { get; set; }
        public int Status { get; set; }
        public string ExcludeSetting { get; set; }
        public string Updator { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
    
    public class SetLotteryEventRequest
    {
        public int Sq { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DrawDate { get; set; }
        public int IsSuspend { get; set; }
        public int FirstPrize { get; set; }
        public int SecondPrize { get; set; }
        public int ThirdPrize { get; set; }
        public int Consolation { get; set; }
        public int Small { get; set; }
    }
    
    public class GetRaffleTicketStickerRequest
    {
        public int SiteId { get; set; }
    }
    
    public class SetRaffleTicketStickerRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public decimal StickerPrize { get; set; }
        public decimal TurnOver { get; set; }
    }
    
    public class RaffleTicketStickerResponse
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public decimal StickerPrize { get; set; }
        public decimal TurnOver { get; set; }
        public string Updator { get; set; }
        private DateTimeOffset _updateTime { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
    
    public class GetEventRestrictionRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId{ get; set; }
        public string UserName{ get; set; }
    }
    
    public class EventRestrictionResponse
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
        public string Reason { get; set; }
        public string Creator { get; set; }
        private DateTimeOffset _taggingTime { get; set; }
        public DateTimeOffset TaggingTime
        {
            get => _taggingTime;
            set => _taggingTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
    
    public class SetEventRestrictionRequest
    {
        public string UserName { get; set; }
        public string Reason { get; set; }
    }
    
    public class DelEventRestrictionRequest
    {
        public string UserName { get; set; }
    }

    public class GetStickerReportRequest
    {
        public IEnumerable<int> SiteIds { get; set; }
        public string UserName { get; set; }
        public int RoundId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public DateTime StartDate{ get; set; }
        public DateTime EndDate{ get; set; }
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
    }
    
    public class GetStickerReportReponse
    {
        public List<GetStickerReportDto> StickerReportData { get; set; }
        public List<StickerReportCurrencyTotal> StickerReportCurrencyTotal { get; set; }
        public int PageNumber { get; set; }
        public int RowCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class GetStickerReportDto
    {
        public int SiteId { get; set; }
        public int Round { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
        public int CurrencyId { get; set; }
        public int StickerId { get; set; }
        public decimal StickerAmount { get; set; }
        public DateTime PurchaseTime { get; set; }
        public DateTime RoundStartDate{ get; set; }
        public DateTime RoundEndDate{ get; set; }
    }
    
    public class StickerReportCurrencyTotal
    {
        public int CurrencyId { get; set; }
        public decimal CurrencyAmount { get; set; }
    }
    
    #endregion
}
