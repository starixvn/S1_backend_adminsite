namespace BackendSite.Service.Model
{
    public class PointLevelInfo
    {
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }

        public int PointLevel { get; set; }

        public int DepositCnt { get; set; }

        public int WithdrawalCnt { get; set; }
        public int IsKyc { get; set; }
    }
}