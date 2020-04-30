using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class AdjustWithdrawalLimit
    {
        public int AdjustType { get; set; }
        public string SiteName { get; set; }
        public string TransId { get; set; }
        public decimal Amount { get; set; }
        public string Operator { get; set; }
        public int Status { get; set; }
        
        public DateTime CreateTime { get; set; }
        public string Memo { get; set; }
        public int CustId { get; set; }
    }

    public class WithdrawalLimitInfo
    {
        public decimal WithdrawalLimit { get; set; }
        public decimal TurnOver { get; set; }
        public decimal Balance { get; set; }
    }

    public class WalletInfo
    {
        public decimal Balance { get; set; }
        public decimal DifferenceAmount { get; set; }
        public decimal AvailableBalance { get; set; }
    }
}
