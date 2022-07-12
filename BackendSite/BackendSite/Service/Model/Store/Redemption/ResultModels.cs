using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model.Store.Redemption
{
    public class RedemptionListResult
    {
        public int SiteId { get; set; }
        public string IsoCode { get; set; }
        public int RedemptionId { get; set; }
        public string TransId { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
        public int VipLevel { get; set; }
        public string VipLevelName { get; set; }
        public string ItemName { get; set; }
        public int Point { get; set; }
        public decimal ActualCost { get; set; }
        public string Operator { get; set; }
    }
    
    public class ExportRedemptionListResult
    {
        public IEnumerable<RedemptionOverviewResult> Overview { get; set; }
        public IEnumerable<RedemptionListResult> List { get; set; }
    }

    public class RedemptionOverviewResult
    {
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public decimal TotalPoint { get; set; }
        public decimal TotalCost { get; set; }
        public int TotalRecord { get; set; }
    }
    
    public class UploadReceiptResult
    {
        public List<string> ReceiptUrls { get; set; }
       
    }

}