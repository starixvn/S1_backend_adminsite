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
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public IEnumerable<int> Statuses { get; set; }
        public string TransId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
    }

    
    public class ExportRedemptionListRequest 
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int SiteId { get; set; }
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
    
    public class SetRedemptionInfoRequest 
    {
        public int SiteId { get; set; }
        public string TransId { get; set; }
        public int Action { get; set; }
        public string Comment { get; set; }
        public decimal? ActualCost { get; set; }
        public IEnumerable<string> ReceiptUrls { get; set; } 
        public string Operator { get; set; }
    }
    
    public class GetRedemptionLogRequest 
    {
        public string TransId { get; set; }
    }
}