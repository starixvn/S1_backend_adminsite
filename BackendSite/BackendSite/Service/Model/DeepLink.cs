using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string Domain { get; set; }
        public DeepLinkTableTypeEnum Type { get; set; }
        public DeepLinkTableProtocolTypeEnum ProtocolType { get; set; }
        public int DeepLinkId { get; set; }
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
