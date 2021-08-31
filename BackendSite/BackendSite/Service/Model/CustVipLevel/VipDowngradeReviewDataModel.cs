using System;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.CustVipLevel
{
    public class VipDowngradeReviewDataModel
    {
        public int SiteId { get; set; }

        public string SiteName { get; set; }

        public int CustId { get; set; }

        public string UserName { get; set; }

        public int CurrencyId { get; set; }

        public string IsoCode { get; set; }

        public int CurrentVipLevel { get; set; }

        public string CurrentVipName { get; set; }

        public int DowngradeVipLevel { get; set; }

        public string DowngradeVipName { get; set; }

        public decimal ThresholdDeposit { get; set; }
        
        public decimal DepositAmount { get; set; }

        public decimal ThresholdTurnOver { get; set; }

        public decimal TurnOverAmount { get; set; }

        public VipStatus Status { get; set; }

        public string CalculateTime { get; set; }

        public string SettleTime { get; set; }

        public string QualifiedTime { get; set; }

        public string LastUpdateUser { get; set; }
    }
}