using System;
using System.Collections.Generic;
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
}
