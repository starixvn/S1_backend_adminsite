using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class DomainTable
    {
        public int DId { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public string Domain { get; set; }
        public int Sort { get; set; }
        public int Status { get; set; }
        public string LastUpdateUser { get; set; }
    }
    public class GetAppLinkTableReq 
    {
        public int SiteId { get; set; }
    }
    public class SetAppLinkReq
    {
        public int SiteId { get; set; }
        public string Domain { get; set; }
        public AppTypeEnum AppType { get; set; }
        public ProtocolTypeEnum ProtocolType { get; set; }
    }
}
