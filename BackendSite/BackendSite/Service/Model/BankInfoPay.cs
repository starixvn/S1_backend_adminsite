namespace BackendSite.Service.Model
{
    public class BankInfoPay
    {
        public int SysId { get; set; }
        public int CurrencyId { get; set; }
        public string SysBankCode { get; set; }
        public string SysBankName { get; set; }
        public string BankLogo { get; set; }
        public int PayId { get; set; }
        public string PayName { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }
        public int Mode { get; set; }
        public int PointLevel { get; set; }
        public string PayCode { get; set; }
        public int MSort { get; set; }
        public int SSort { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
    }
    
    public class EWalletPay
    {
        public int PayId { get; set; }
        public string PayName { get; set; }
        public int CurrencyId { get; set; }
    }
    
    public class Pay
    {
        /// <summary>
        /// 付款渠道ID
        /// </summary>
        public int PayId { get; set; }

        /// <summary>
        /// 付款名稱
        /// </summary>
        public string PayName { get; set; }

        /// <summary>
        /// 渠道ID
        /// </summary>
        public int ChannelId { get; set; }
    }
    
    public class PayComparer : IEqualityComparer<Pay>
    {
        public bool Equals(Pay t1, Pay t2)
        {
            return (t1.PayId == t2.PayId);
        }

        public int GetHashCode(Pay t)
        {
            return t.ToString().GetHashCode();
        }
    }
}
