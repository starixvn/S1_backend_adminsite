using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class WithdrawRequest
    {
        public string Name { set; get; }
        public string BankName { set; get; }
        public int  BankNumber { set; get; }
    }
}
