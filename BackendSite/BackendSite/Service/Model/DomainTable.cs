using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class DomainTable
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public string Domain { get; set; }
        public int Sort { get; set; }
        public int Status { get; set; }
        public string LastUpdateUser { get; set; }
    }
}
