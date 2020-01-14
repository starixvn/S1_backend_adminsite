using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class AdjustmentInfo
    {
        public string TransId { set; get; }
        public int AdjustType { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public int CurrencyId { set; get; }
        public int SiteId { set; get; }
        public string AdjustIdInfo { set; get; }
        public decimal Amount { set; get; }
        public DateTime CreateTime { set; get; }
        public bool FirstDeposit { set; get; }
        public string RefId { set; get; }
        public string Memo { set; get; }
        public string Comment { set; get; }
        public int Status { set; get; }
    }
}
