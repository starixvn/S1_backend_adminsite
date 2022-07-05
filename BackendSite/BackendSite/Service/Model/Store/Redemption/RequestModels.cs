using System;
using System.Collections.Generic;
using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model.Store.Redemption
{
    public class CreateRedemptionRequest
    {
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int GiftId { get; set; }
        public int Point { get; set; }
    }

    public class GetRedemptionListRequest : ApiRequestPageModel
    {
        private DateTimeOffset? _startTime;
        private DateTimeOffset? _endTime;
        public DateTimeOffset? StartTime 
        {
            get => _startTime;
            set
            {
                if (value != null) 
                    _startTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public DateTimeOffset? EndTime 
        {
            get => _endTime;
            set
            {
                if (value != null) 
                    _endTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public IEnumerable<int> SiteIds { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public IEnumerable<int> Statuses { get; set; }
        public string TransId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
    }

    public class GetRedemptionDetailRequest 
    {
        public int RedemptionId { get; set; }
    }
}