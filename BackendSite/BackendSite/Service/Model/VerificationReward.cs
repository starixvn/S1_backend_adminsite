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
        private DateTimeOffset _createTime;
        private DateTimeOffset? _lastUpdateTime;
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public int VerificationType { get; set; }
        public decimal PhoneReward { get; set; }
        public decimal EmailReward { get; set; }
        public int Status { get; set; }

        public DateTimeOffset CreateTime
        {
            get => _createTime; 
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string LastUpdateUser { get; set; }

        public DateTimeOffset? LastUpdateTime
        {
            get => _lastUpdateTime;
            set 
            { 
                if(value.HasValue)
                    _lastUpdateTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero); 
            }
        }
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