using System;

namespace BackendSite.Service.Model
{
    public class AdjustWithdrawalLimit
    {
        private DateTimeOffset _createTime { get; set; }
        public int CustId { get; set; }
        public int AdjustType { get; set; }
        public string SiteName { get; set; }
        public string TransId { get; set; }
        public decimal Amount { get; set; }
        public string Operator { get; set; }
        public int Status { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string Memo { get; set; }
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
