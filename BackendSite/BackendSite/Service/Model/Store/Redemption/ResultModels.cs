using System;
using System.Collections.Generic;
using System.ComponentModel;
using BackendSite.Attributes;

namespace BackendSite.Service.Model.Store.Redemption
{
    public class RedemptionListResult
    {
        [ExportIgnore]
        public int SiteId { get; set; }
        [DisplayName("Currency")]
        public string IsoCode { get; set; }
        [ExportIgnore]
        public int RedemptionId { get; set; }
        [DisplayName("InvoiceID")]
        public string TransId { get; set; }
        [ExportIgnore]
        public int Status { get; set; }
        [DisplayName("Status")]
        public string StatusName { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        [DisplayName("MemberID")]
        public string UserName { get; set; }
        public string SabaId { get; set; }
        [ExportIgnore]
        public int VipLevel { get; set; }
        [DisplayName("VipLevel")]
        public string VipLevelName { get; set; }
        public string ItemName { get; set; }
        [DisplayName("ItemPoint")]
        public int Point { get; set; }
        [DisplayName("ItemCost")]
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
        [ExportIgnore]
        public int CurrencyId { get; set; }
        [DisplayName("Currency")]
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