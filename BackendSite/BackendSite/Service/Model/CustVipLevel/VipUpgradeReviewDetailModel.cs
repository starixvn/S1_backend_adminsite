namespace BackendSite.Service.Model.CustVipLevel
{
    public class VipUpgradeReviewDetailModel: VipUpgradeReviewDataModel
    {
        public VipUpgradeReviewDetailModel(VipUpgradeReviewDataModel data)
        {
            SiteId = data.SiteId;
            SiteName = data.SiteName;
            CustId = data.CustId;
            UserName = data.UserName;
            CurrencyId = data.CurrencyId;
            IsoCode = data.IsoCode;
            CurrentVipLevel = data.CurrentVipLevel;
            CurrentVipName = data.CurrentVipName;
            UpgradeVipLevel = data.UpgradeVipLevel;
            UpgradeVipName = data.UpgradeVipName;
            ThresholdDeposit = data.ThresholdDeposit;
            DepositAmount = data.DepositAmount;
            UpgradeBonus = data.UpgradeBonus;
            Status = data.Status;
            CalculateTime = data.CalculateTime;
            SettleTime = data.SettleTime;
            LastUpdateUser = data.LastUpdateUser;

            CanUpdateStatus = false; 
        }

        public bool CanUpdateStatus
        {
            get; set;
        }
    }
}