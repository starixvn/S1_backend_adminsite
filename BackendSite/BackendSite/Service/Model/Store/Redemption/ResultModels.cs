using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model.Store.Redemption
{
    public class GetRedemptionListResult
    {
        public int SiteId { get; set; }
        public string IsoCode { get; set; }
        public string TransId { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
        public int VipLevel { get; set; }
        public string VipLevelName { get; set; }
        public string ItemName { get; set; }
        public int Point { get; set; }
        public decimal ActualCost { get; set; }
        public string Operator { get; set; }
    }
    
    public class UploadReceiptResult
    {
        public List<string> ReceiptUrls { get; set; }
       
    }

}