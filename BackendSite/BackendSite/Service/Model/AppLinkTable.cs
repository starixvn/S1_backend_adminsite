using BackendSite.Extensions;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class AppLinkTable
    {
        private DateTimeOffset _lastUpdateTime { get; set; }
        private string _appType { get; set; }
        private string _protocolType { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string Domain { get; set; }
        public AppTypeEnum AppType { get; set; }
        public ProtocolTypeEnum ProtocolType { get; set; }

        public string LastUpdateUser { get; set; }

        public string AppTypeName => AppType.GetDescription();

        public string ProtocolTypeName => ProtocolType.GetDescription();
        public DateTimeOffset LastUpdateTime
        {
            get => _lastUpdateTime;
            set => _lastUpdateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }


    }
}
