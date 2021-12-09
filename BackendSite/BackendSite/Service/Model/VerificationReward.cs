namespace BackendSite.Service.Model
{
    public class VerificationRewardModel
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int VerificationType { get; set; }
        public decimal PhoneReward { get; set; }
        public decimal EmailReward { get; set; }
    }
}