using System;
using System.Collections.Generic;
using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model.Store.StorePoint
{
    public class GetMemberListRequest : ApiRequestPageModel
    {
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public string UserName { get; set; }
        public string TpUserName { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
    
    public class GetMemberRewardPointLogRequest : ApiRequestPageModel
    {
        public int CustId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
    
    public class CreateManualRewardPointInfoRequest
    {
        public int SiteId { get; set; }
        public string UserName { get; set; }
        public decimal Point { get; set; }
        public int CategoryId { get; set; }
        public string Memo { get; set; }
    }
    
    public class CreateManualRewardPointInfoBatchRequest
    {
        public int SiteId { get; set; }
        public IEnumerable<ManualRewardPointInfo> Infos { get; set; }
        public string Operator { get; set; }
    }
}