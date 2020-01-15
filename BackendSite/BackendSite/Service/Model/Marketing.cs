namespace BackendSite.Service.Model
{
    public class PointLevelInfo
    {
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }

        public int PointLevel { get; set; }

        public double DepositLeast { get; set; }

        public double BetLeast { get; set; }
    }
}