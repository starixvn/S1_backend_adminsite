using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class DepositRequest
    {
        public string TransId { set; get; }
        public string DispositType { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public int SiteId { set; get; }
        public string DispositInfo { set; get; }
        public decimal Amount { set; get; }
        public decimal ActualDeposit { set; get; }
        public DateTime CreateTime { set; get; }
        public int FirstDeposit { set; get; }
        public string Memo { set; get; }

    }
}
