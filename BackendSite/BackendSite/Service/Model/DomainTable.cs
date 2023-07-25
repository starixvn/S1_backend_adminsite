using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class DomainTable
    {
        public int DId { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public string Domain { get; set; }
        public int Sort { get; set; }
        public int Status { get; set; }
        public string LastUpdateUser { get; set; }
        private DateTimeOffset _lastUpdateTime { get; set; }
        public DateTimeOffset LastUpdateTime
        {
            get => _lastUpdateTime;
            set => _lastUpdateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
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
