namespace BackendSite.Service.Model.CustVipLevel
{
    public class VipDowngradeReviewDetailModel : VipDowngradeReviewDataModel
    {
        public VipDowngradeReviewDetailModel(VipDowngradeReviewDataModel data)
        {
            SiteId = data.SiteId;
            SiteName = data.SiteName;
            CustId = data.CustId;
            UserName = data.UserName;
            CurrencyId = data.CurrencyId;
            IsoCode = data.IsoCode;
            CurrentVipLevel = data.CurrentVipLevel;
            CurrentVipName = data.CurrentVipName;
            DowngradeVipLevel = data.DowngradeVipLevel;
            DowngradeVipName = data.DowngradeVipName;
            ThresholdDeposit = data.ThresholdDeposit;
            DepositAmount = data.DepositAmount;
            ThresholdTurnOver = data.ThresholdTurnOver;
            TurnOverAmount = data.TurnOverAmount;
            Status = data.Status;
            CalculateTime = data.CalculateTime;
            SettleTime = data.SettleTime;
            QualifiedTime = data.QualifiedTime;
            LastUpdateUser = data.LastUpdateUser;

            CanUpdateStatus = false; 
        }

        public bool CanUpdateStatus
        {
            get; set;
        }
    }
}
