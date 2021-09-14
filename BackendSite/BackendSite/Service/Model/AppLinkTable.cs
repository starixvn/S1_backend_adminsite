using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class AppLinkTable
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string Domain { get; set; }
        public int AppType { get; set; }
        public int ProtocolType { get; set; }
    }
}
