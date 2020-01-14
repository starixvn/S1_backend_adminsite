using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class TpWallet
    {
        public int TpId { set; get; }
        public string TpName { set; get; }
        public string TpUserName { set; get; }
        public decimal Balance { set; get; }
    }
    public class TpInfo
    {
        public int TpId { set; get; }
        public int CustId { set; get; }
        public string LangId { set; get; }
        public int AppType { set; get; }
        public Dictionary<string, string> inputs { set; get; }
    }
    public class TpTransfer
    {
        public int TpId { set; get; }
        public int CustId { set; get; }
        public decimal Amount { set; get; }
        public TpDirection direction { set; get; }
        public Dictionary<string, string> inputs { set; get; }
    }
    public enum TpDirection
    {
        ToTp = 1,
        ToMain = 2
    }
    public class TransferResult
    {
        public TransferResult()
        {
            Status = TransferStatus.Pending;
        }
        public TransferStatus Status { get; set; }
        public string ThirdId { get; set; }
        public decimal Difference { get; set; }
    }
    public enum TransferStatus
    {
        Success = 0, Fail = 1, Pending = 2, PendingD = 3
    }
    public class UserBalance
    {
        public decimal balance { get; set; }
        public decimal outstanding { get; set; }
        public byte Currency { get; set; }
    }
}
