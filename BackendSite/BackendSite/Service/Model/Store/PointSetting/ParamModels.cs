using System;
using System.Collections.Generic;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.Store.PointSetting
{
    public class GetInactivePointExpirySettingParams
    {
        public GetInactivePointExpirySettingParams(GetInactivePointExpirySettingRequest req)
        {
            SiteId = req.SiteId;
        }

        public int SiteId { get; set; }
    }

    public class SetInactivePointExpirySettingParams
    {
        public SetInactivePointExpirySettingParams(SetInactivePointExpirySettingRequest req, string userName)
        {
            SiteId = req.SiteId;
            Days = req.Days;
            Status = (int) req.Status;
            Operator = userName;
        }

        public int SiteId { get; set; }
        public int Days { get; set; }
        public int Status { get; set; }
        public string Operator { get; set; }
    }

    public class SetTurnoverPointSettingParams
    {
        public SetTurnoverPointSettingParams(SetTurnoverPointSettingRequest req, string userName)
        {
            SiteId = req.SiteId;
            CurrencyId = req.CurrencyId;
            Infos = req.Infos;
            Operator = userName;
        }

        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public IEnumerable<TurnoverPointSettingParams> Infos { get; set; }
        public string Operator { get; set; }
    }

    public class SetDepositPointSettingParams
    {
        public SetDepositPointSettingParams(SetDepositPointSettingRequest req, string userName)
        {
            SiteId = req.SiteId;
            CurrencyId = req.CurrencyId;
            DepositAmount = req.DepositAmount;
            //PointRate = req.PointRate;
            PromotionStatus = (int) req.PromotionStatus;
            PromotionPointRate = req.PromotionPointRate;
            PromotionStartTime = req.PromotionStartTime;
            PromotionEndTime = req.PromotionEndTime;
            Operator = userName;
        }

        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public decimal DepositAmount { get; set; }
        //public decimal PointRate { get; set; }

        /// <summary>
        /// 0: Enable, 1: Disabled
        /// </summary>
        public int PromotionStatus { get; set; }

        public decimal PromotionPointRate { get; set; }
        public DateTimeOffset PromotionStartTime { get; set; }
        public DateTimeOffset PromotionEndTime { get; set; }
        public string Operator { get; set; }
    }

    public class SetTurnoverPointStatusSettingParams
    {
        public SetTurnoverPointStatusSettingParams(SetTurnoverPointStatusSettingRequest req, string userName)
        {
            SiteId = req.SiteId;
            CurrencyId = req.CurrencyId;
            Infos = req.Infos;
            Operator = userName;
        }

        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public IEnumerable<TurnoverPointStatusParams> Infos { get; set; }
        public string Operator { get; set; }
    }

    public class TurnoverPointStatusParams
    {
        public int TpGameTypeId { get; set; }
        public GeneralStatusEnum Status { get; set; }
    }


    public class TurnoverPointSettingParams
    {
        private DateTimeOffset? _startTime { get; set; }
        private DateTimeOffset? _endTime { get; set; }

        public int TpCategoryId { get; set; }
        public int VipLevel { get; set; }
        public decimal Turnover { get; set; }
        public decimal PointRate { get; set; }
        public GeneralStatusEnum PromotionStatus { get; set; }
        public decimal PromotionPointRate { get; set; }

        public DateTimeOffset? PromotionStartTime
        {
            get => _startTime;
            set
            {
                if (value != null)
                    _startTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }

        public DateTimeOffset? PromotionEndTime
        {
            get => _endTime;
            set
            {
                if (value != null)
                    _endTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
    }
}
