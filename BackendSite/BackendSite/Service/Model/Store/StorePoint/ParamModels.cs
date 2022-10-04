using System.Collections.Generic;

namespace BackendSite.Service.Model.Store.StorePoint
{
    public class CreateManualRewardPointInfoParams
    {
        public CreateManualRewardPointInfoParams(CreateManualRewardPointInfoRequest req, string @operator)
        {
            SiteId = req.SiteId;
            UserName = req.UserName;
            Point = req.Point;
            CategoryId = req.CategoryId;
            Memo = req.Memo;
            Operator = @operator;
        }
    
        public int SiteId { get; set; }
        public string UserName { get; set; }
        public decimal Point { get; set; }
        public int CategoryId { get; set; }
        public string Memo { get; set; }
        public string Operator { get; set; }
    }
    
    public class CreateManualRewardPointInfoBatchParams
    {
        public CreateManualRewardPointInfoBatchParams(int siteId, IEnumerable<ManualRewardPointInfo> infos, string @operator)
        {
            SiteId = siteId;
            Infos = infos;
            Operator = @operator;
        }
    
        public int SiteId { get; set; }
        public IEnumerable<ManualRewardPointInfo> Infos { get; set; }
        public string Operator { get; set; }
    }
    
    public class ManualRewardPointInfo
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int CategoryId { get; set; }
        public decimal Point { get; set; }
        public string Memo { get; set; }
    }
    
    public class ManualRewardPointItem
    {
        public string MemberId { get; set; }
        public string Category { get; set; }
        public decimal Point { get; set; }
        public string Memo { get; set; }
    }
}