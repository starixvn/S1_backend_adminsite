using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class Promotion
    {
        private DateTimeOffset _expiryTime { get; set; }
        public DateTimeOffset _claimDate { get; set; }
        public DateTimeOffset _updateTime { get; set; }
        public int BonusId { get; set; }
        public string BonusCode { get; set; }
        public string Description { get; set; }
        public PromotionTypeEnum PromotionType { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ExpiryDays { get; set; }
        public int PeriodTo { get; set; }
        public int CurrencyId { get; set; }
        public int RollOver { get; set; }
        public int FullFillType { get; set; }
        public decimal Bonus { get; set; }
        public BonusTypeEnum BonusType { get; set; }
        public decimal MaxBonus { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public DateTimeOffset ExpiryTime
        {
            get => _expiryTime;
            set => _expiryTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int BonusInfoStatus { get; set; }
        public int Sort { get; set; }
        public CustRollOver custRollOver { get; set; }
        public DateTimeOffset ClaimDate
        {
            get => _claimDate;
            set => _claimDate = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int AtPage { get; set; }

        public decimal CancelBonusAmount { get; set; }
        public bool ExpiredCancel { get; set; }
    }
    public class PromotionListData
    {
        public List<Promotion> PromotionList { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
        public List<string> CurrencyAmount { get; set; }
    }

    public class PromotionResponseWrapper
    {
        public List<PromotionDto> PromotionList { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
        public List<string> CurrencyAmount { get; set; }
    }

    public class PromotionDto
    {
        private DateTimeOffset _expiryTime { get; set; }
        public DateTimeOffset _claimDate { get; set; }
        public DateTimeOffset _updateTime { get; set; }

        public int BonusId { get; set; }
        public string BonusCode { get; set; }
        public string Description { get; set; }
        public PromotionTypeEnum PromotionType { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ExpiryDays { get; set; }
        public int PeriodTo { get; set; }
        public int CurrencyId { get; set; }
        public int RollOver { get; set; }
        public int FullFillType { get; set; }
        public decimal Bonus { get; set; }
        public BonusTypeEnum BonusType { get; set; }
        public decimal MaxBonus { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public DateTimeOffset ExpiryTime
        {
            get => _expiryTime;
            set => _expiryTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int BonusInfoStatus { get; set; }
        public int Sort { get; set; }
        public CustRollOver custRollOver { get; set; }
        public DateTimeOffset ClaimDate
        {
            get => _claimDate;
            set => _claimDate = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int AtPage { get; set; }

        public decimal CancelBonusAmount { get; set; }
        public bool ExpiredCancel { get; set; }
        public string DepositInfo { get; set; }
    }

    public class GetPromotionCashbackRequest
    {
        public int CustId { set; get; }
    }

    public class GetPromotionCashbackResponse
    {
        public string BonusCode { set; get; }
        public string Description { set; get; }
    }

    public class GetPromotionDetailRequest
    {
        public string BonusCode { set; get; }
        public int CustId { set; get; }
    }

    public class GetPromotionDetailResponse
    {
        public CashBackDetails CashBackDetails { get; set; }
    }

    public class CashBackDetails
    {
        public decimal TotalBonus { get; set; }
        public decimal MaxBonus { get; set; }
        public decimal TotalLossAmount { get; set; }
        public decimal MinLossAmount { get; set; }
        public decimal OriginalTotalBonus { get; set; }
        public string Percentage { get; set; }
        public IEnumerable<CashBackDetail> CashBackDetail { get; set; }
    }

    public class CashBackDetail
    {
        public string TpName { get; set; }
        public decimal LossAmount { get; set; }
        public decimal LossAmountWithBonus { get; set; }
    }

    public class CustPromotionRequest
    {
        public int SiteId { set; get; }
        public int CustId { set; get; }
        public int CurrencyId { set; get; }
        public int DeviceType { set; get; }
        public bool IsLogin { set; get; }
        public string LangId { set; get; }
        public string DefLang { set; get; }
        public int PromotionType { set; get; }
    }

    public class PromotionDetailRequest
    {
        public int SiteId { set; get; }
        public int CustId { set; get; }
        public int CurrencyId { set; get; }
        public int DeviceType { set; get; }
        public bool IsLogin { set; get; }
        public string LangId { set; get; }
        public string DefLang { set; get; }
        public string BonusCode { set; get; }
    }

    public class CancelBonusRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
        public int BonusId { get; set; }
    }
}
