using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
}
