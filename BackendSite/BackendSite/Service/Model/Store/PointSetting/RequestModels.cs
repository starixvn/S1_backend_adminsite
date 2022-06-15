using System;
using System.Collections.Generic;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.Store.PointSetting
{
    public class GetInactivePointExpirySettingRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
    }

    public class SetInactivePointExpirySettingRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int Days { get; set; }
        public GeneralStatusEnum Status { get; set; }
    }

    public class GetTurnoverPointSettingRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int VipLevel { get; set; }
    }

    public class GetTurnoverPointSettingListRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
    }

    public class SetTurnoverPointSettingRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public IEnumerable<TurnoverPointSettingParams> Infos { get; set; }
    }

    public class GetDepositPointSettingRequest
    {
        public int SiteId { get; set; }
    }

    public class SetDepositPointSettingRequest
    {
        private DateTimeOffset _startTime { get; set; }
        private DateTimeOffset _endTime { get; set; }

        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal PointRate { get; set; }
        public GeneralStatusEnum PromotionStatus { get; set; }
        public decimal PromotionPointRate { get; set; }

        public DateTimeOffset PromotionStartTime
        {
            get => _startTime;
            set => _startTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        public DateTimeOffset PromotionEndTime
        {
            get => _endTime;
            set => _endTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }

    public class GetTurnoverPointStatusSettingRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int TpCategoryId { get; set; }
    }

    public class SetTurnoverPointStatusSettingRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public IEnumerable<TurnoverPointStatusParams> Infos { get; set; }
    }
}