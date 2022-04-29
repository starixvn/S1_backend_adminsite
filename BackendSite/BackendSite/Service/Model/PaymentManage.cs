using System.Collections.Generic;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class PayList
    {
        public int Id { get; set; }
        public string PayName { get; set; }
        public int Status { get; set; }
        public string Sort { get; set; }

        public IEnumerable<PayChannel> ChannelList { get; set; }
    }

    public class PayChannel
    {
        public int Id { get; set; }
        public int PayId { get; set; }
        public int GroupId { get; set; }
        public int Type { get; set; }
        public string ChannelName { get; set; }
        public int Mode { get; set; }
        public int PointLevel { get; set; }
        public int Sort { get; set; }
        public int Status { get; set; }

        public IEnumerable<PayCurrency> CurrencyList { get; set; }
    }

    public class PayCurrency
    {
        public int ChannelId { get; set; }
        public int CurrencyId { get; set; }
        public string PayCurrencyId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int AmountRule { get; set; }
        public string AmountShow { get; set; }
    }

    public class FrontEndDatas
    {
        public IEnumerable<PayList> PayList { get; set; }
        public IEnumerable<PayChannel> ChannelList { get; set; }
        public IEnumerable<PayCurrency> CurrencyList { get; set; }
    }

    public class PayListBankInfoSys
    {
        public int Id { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public int Status { get; set; }

        public int CurrencyId { get; set; }
    }
    
    public class UpdatePayInfo
    {
        public int PayId { get; set; }
        // log用
        public string PayName { get; set; }

        /// <summary>
        /// 狀態(1=Enable、2=Disable)
        /// </summary>
        public PayListStatusEnum Status { get; set; }

        public string StatusName => Status.ToString();
    }
    
    public class UpdatePayChannel
    {
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }

        public int Sort { get; set; }

        /// <summary>
        /// 狀態(1=Enable、2=Disable)
        /// </summary>
        public PayChannelStatusEnum Status { get; set; }
        public string StatusName => Status.ToString();

        public int PointLevel { get; set; }
    }

    public class UpdatePayCurrency
    {
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }

        public decimal MinAmount { get; set; }

        public decimal MaxAmount { get; set; }

        /// <summary>
        /// 數字規則(1=Integer、2=Decimal、3=Fixed)
        /// </summary>
        public AmountRuleEnum AmountRule { get; set; }
        public string AmountRuleName => AmountRule.ToString();

        public IEnumerable<decimal> FixedAmountList { get; set; }
    }
}
