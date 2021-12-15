using System;

namespace BackendSite.Service.Model
{
    public class GetVerificationRewardRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
    }
    
    public class VerificationRewardModel
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public int VerificationType { get; set; }
        public decimal PhoneReward { get; set; }
        public decimal EmailReward { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
        public string LastUpdateUser { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }
    
    public class UpdateVerificationRewardStatusReq
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
    }
    
    public class SetVerificationRewardRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int VerificationType { get; set; }
        public decimal PhoneReward { get; set; }
        public decimal EmailReward { get; set; }
    }
}