using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class PaymentAdjustment
    {
        public string TransId { get; set; }
        public int AdjustType { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int SiteId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreateTime { get; set; }
        public int Status { get; set; }
    }

    public class PaymentAdjustmentFrontEnd
    {
        public string SiteName { get; set; }
        public string TransId { get; set; }

        public string AdjustTypeName { get; set; }

        public string UserName { get; set; }

        public string CurrencyName { get; set; }
        
        public decimal Amount { get; set; }
        public DateTime CreateTime { get; set; }

        public string StatusName { get; set; }
    }
}
