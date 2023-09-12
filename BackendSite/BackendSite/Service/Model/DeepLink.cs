using BackendSite.Extensions;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class DeepLink
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class GetDeepLinkReq
    {
        public int SiteId { get; set; }
        public int DeepLinkId { get; set; }
    }
    public class GetDeepLinkRep
    {
        private DateTimeOffset _lastUpdateTime;
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int DeepLinkId { get; set; }
        public string DeepLinkName { get; set; }
        public DeepLinkTableTypeEnum Type { get; set; }
        public string TypeName => Type.GetDescription();
        public DeepLinkTableProtocolTypeEnum ProtocolType { get; set; }
        public string ProtocolTypeName => ProtocolType.GetDescription();
        public string Domain { get; set; }
        public string LastUpdateUser { get; set; }
        public DateTimeOffset LastUpdateTime
        {
            get => _lastUpdateTime;
            set => _lastUpdateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
    public class SetDeepLinkReq
    {
        public int SiteId { get; set; }
        public string Domain { get; set; }
        public DeepLinkTableTypeEnum Type { get; set; }
        public DeepLinkTableProtocolTypeEnum ProtocolType { get; set; }
        public int DeepLinkId { get; set; }
    }
}
