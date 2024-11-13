using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    #region Lottery
    
    public class GetLotteryEventRequest
    {
        public int SiteId { get; set; }
        public DateTime StartDate{ get; set; }
        public DateTime EndDate{ get; set; }
    }
    
    public class GetLotteryEventRateRequest
    {
        public int Sq { get; set; }
    }
    
    public class GetLotteryEventRateResponse
    {
        public DateTime RateDate { get; set; }
        public decimal Rate { get; set; }
        /// <summary>
        /// 1 : 乘 , -1 : 除
        /// </summary>
        public int RateFunction { get; set; } = 1;
        public int TargetCurrencyId { get; set; }
        public string IsoCode { get; set; }
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
        public List<string> ExcludeSettingList => JsonConvert.DeserializeObject<List<string>>(ExcludeSetting);
        public string ExcludeTagSetting { get; set; }
        public List<string> ExcludeTagList => JsonConvert.DeserializeObject<List<string>>(ExcludeTagSetting);
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
        public IEnumerable<int> ExcludeTagList { get; set; } = new List<int>();
    }
    
    public class AddLotteryEventRequest
    {
        public int SiteId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DrawDate { get; set; }
        public int FirstPrize { get; set; }
        public int SecondPrize { get; set; }
        public int ThirdPrize { get; set; }
        public int Consolation { get; set; }
        public int Small { get; set; }
        public IEnumerable<int> ExcludeTagList { get; set; } = new List<int>();
    }
    
    public class LotteryTicketHistoryRequest
    {
        public int SiteId { get; set; }
        public int RoundId { get; set; }
    }
    
    public class LotteryTicketHistory
    {
        public int SiteId { get; set; }
        public int RoundId { get; set; }
        public string FirstPrize { get; set; }
        public string SecondPrize { get; set; }
        public string ThirdPrize { get; set; }
        public string ConsolationPrize { get; set; }
        public string SmallPrize { get; set; }
        public List<string> FirstPrizeList => JsonConvert.DeserializeObject<List<string>>(FirstPrize);
        public List<string> SecondPrizeList => JsonConvert.DeserializeObject<List<string>>(SecondPrize);
        public List<string> ThirdPrizeList => JsonConvert.DeserializeObject<List<string>>(ThirdPrize);
        public List<string> ConsolationPrizeList => JsonConvert.DeserializeObject<List<string>>(ConsolationPrize);
        public List<string> SmallPrizeList => JsonConvert.DeserializeObject<List<string>>(SmallPrize);
        public int StickerId { get; set; }
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
    
    public class GetStickerCountRequest
    {
        public int SiteId { get; set; }
    }
    
    public class GetStickerCountResponse
    {
        public int StickerCount { get; set; }
    }
    
    public class SetStickerCountRequest
    {
        public int SiteId { get; set; }
        public int StickerCount { get; set; }
    }
    
    public class RaffleTicketStickerResponse
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public decimal StickerPrize { get; set; }
        public int StickerCount { get; set; }
        public decimal NextTurnOver { get; set; }
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

    public class GetStickerReportRequest : ApiRequestPageModel
    {
        public IEnumerable<int> SiteIds { get; set; }
        public string UserName { get; set; }
        public string RoundId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public DateTime StartDate{ get; set; }
        public DateTime EndDate{ get; set; }
    }
    
    public class GetStickerReportExportRequest
    {
        public IEnumerable<int> SiteIds { get; set; }
        public string UserName { get; set; }
        public string RoundId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public DateTime StartDate{ get; set; }
        public DateTime EndDate{ get; set; }
    }
    
    public class GetStickerReportResponse
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
        public string SiteName { get; set; }
        public int RoundId { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public ChineseZodiacStickerId StickerId { get; set; }
        public string StickerName => StickerId.ToString();
        public decimal StickerAmount { get; set; }
        private DateTimeOffset _purchaseTime { set; get; }
        public string PurchaseTime
        {
            // get => _purchaseTime;
            // set => _purchaseTime = new DateTimeOffset(value.DateTime, new TimeSpan(8, 0, 0));
            get; set;
        }
        private DateTimeOffset _roundStartDate { set; get; }
        public string RoundStartDate
        {
            // get => _roundStartDate;
            // set => _roundStartDate = new DateTimeOffset(value.DateTime, new TimeSpan(8, 0, 0));
            get; set;
        }
        private DateTimeOffset _roundEndDate { set; get; }
        public string RoundEndDate
        {
            // get => _roundEndDate;
            // set => _roundEndDate = new DateTimeOffset(value.DateTime, new TimeSpan(8, 0, 0));
            get; set;
        }
    }
    
    public class StickerReportCurrencyTotal
    {
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public int CurrencyCount { get; set; }
        public decimal CurrencyAmount { get; set; }
    }
    
    public class GetCustLotteryTicketRequest
    {
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public string UserName { get; set; }
        public int RoundId { get; set; }
        public string TicketNo { get; set; }
    }
    
    public class GetCustLotteryTicket
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public string UserName { get; set; }
        public int RoundId { get; set; }
        public string TicketNo { get; set; }
        private DateTimeOffset _periodStartTime { set; get; }
        private DateTimeOffset _periodEndTime { set; get; }
        private DateTimeOffset _receivedTime { set; get; }
        public DateTimeOffset PeriodStartTime
        {
            get => _periodStartTime;
            set => _periodStartTime = new DateTimeOffset(value.DateTime, new TimeSpan(8, 0, 0));
        }
        public DateTimeOffset PeriodEndTime
        {
            get => _periodEndTime;
            set => _periodEndTime = new DateTimeOffset(value.DateTime, new TimeSpan(8, 0, 0));
        }
        public DateTimeOffset ReceivedTime
        {
            get => _receivedTime;
            set => _receivedTime = new DateTimeOffset(value.DateTime, new TimeSpan(8, 0, 0));
        }
    }
    
    public class LotteryMaintainModel
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsMaintain { get; set; }
        public string UpdateUser { get; set; }
    }
    
    public class LotteryMaintainCreateRequest
    {
        private DateTimeOffset _beginDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
        public int SiteId { get; set; }
        public DateTimeOffset BeginDate
        {
            get => _beginDate;
            set => _beginDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndDate
        {
            get => _endDate;
            set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public bool IsMaintain { get; set; }
    }
    
    #endregion
}
