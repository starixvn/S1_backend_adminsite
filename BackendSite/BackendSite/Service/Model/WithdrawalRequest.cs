using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class WithdrawalRequest
    {
        public string Name { set; get; }
        public string BankName { set; get; }
        public int  BankNumber { set; get; }

    }

    public class WithdrawalList
    {
        public int WithdrawalId { set; get; }
        public string TransId { set; get; }
        public int WithdrawalType { set; get; }
        public string UserName { set; get; }
        public string WithdrawalInfo { set; get; }
        public decimal Amount { set; get; }
        public decimal ChargeFee { set; get; }
        public string Memo { set; get; }
        public string CreateTime { set; get; }
        public int Status { set; get; }

        public virtual IEnumerable<Comments> Comments { get; set; }
    }
}
