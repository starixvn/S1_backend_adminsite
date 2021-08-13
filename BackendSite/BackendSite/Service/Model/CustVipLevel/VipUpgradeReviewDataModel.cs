using System;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.CustVipLevel
{
    public class VipUpgradeReviewDataModel
    {
        public int SiteId { get; set; }

        public string SiteName { get; set; }

        public int CustId { get; set; }

        public string UserName { get; set; }

        public int CurrencyId { get; set; }

        public string IsoCode { get; set; }

        public int CurrentVipLevel { get; set; }

        public string CurrentVipName { get; set; }

        public int UpgradeVipLevel { get; set; }

        public string UpgradeVipName { get; set; }

        public decimal ThresholdDeposit { get; set; }

        public decimal DepositAmount { get; set; }

        public decimal UpgradeBonus { get; set; }

        public VipStatus Status { get; set; }

        public string CalculateTime { get; set; }

        public string SettleTime { get; set; }

        public string LastUpdateUser { get; set; }
    }
}