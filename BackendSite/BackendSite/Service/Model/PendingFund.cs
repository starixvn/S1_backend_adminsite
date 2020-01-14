using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class PendingFund
    {
        public string UserName { set; get; }
        public int CustId { set; get; }
        public int CurrencyId { set; get; }
        public int Status { set; get; }
        public decimal Balance { set; get; }
        public int SiteId { set; get; }
    }
}
