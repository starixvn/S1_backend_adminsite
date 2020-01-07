using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class WithdrawalRequest
    {
        public int WithdrawalId { set; get; }
        public string TransId { set; get; }
        public int WithdrawalType { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public string Email { set; get; }
        public string WithdrawalInfo { set; get; }
        public int CurrencyId { set; get; }
        public decimal Amount { set; get; }
        public decimal ChargeFee { set; get; }
        public string Memo { set; get; }
        public string Comment { set; get; }
        public DateTime CreateTime { set; get; }
        public int Status { set; get; }
        public string SiteName { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public string FromBankName { set; get; }
        public string FromBankCode { set; get; }
        public string PayName { set; get; }
        public int SysId { set; get; }
        public int PayId { set; get; }
        public int BankId { set; get; }
        public string AccountNo { set; get; }
        public string AccountName { set; get; }
        public string RefId { set; get; }

        //public virtual IEnumerable<Comments> Comments { get; set; }
    }
    public class WithdrawalInfo
    {
        public int SysId { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        //[JsonIgnore]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? FromSysId { get; set; }
        //[JsonIgnore]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FromAccountName { get; set; }
        //[JsonIgnore]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FromAccountNo { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PayId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? BankId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? CurrencyId { set; get; }
    }
}
