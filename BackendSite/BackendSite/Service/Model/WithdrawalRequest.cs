using BackendSite.Service.Model.enums;
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
        public int SiteId { set; get; }
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
        public string FromAccountNo { set; get; }
        public string FromAccountName { set; get; }
        public string PayName { set; get; }
        public int SysId { set; get; }
        public int PayId { set; get; }
        public int BankId { set; get; }
        public string AccountNo { set; get; }
        public string AccountName { set; get; }
        public string RefId { set; get; }
        public bool HasPhotoId { set; get; }
        public string VipLevelName { set; get; }
        public int VipLevel { set; get; }
        public string Operator { set; get; }
        public bool SameOperator { get; set; }
        public string CalCode =>
    WithdrawalType switch
    {
        (int)enums.WithdrawalType.LocalBank => $"{SiteName}-{FromBankCode}-{FromAccountName?.Replace(" ", string.Empty)}-{FromAccountNo?.Substring(FromAccountNo.Length > 4 ? FromAccountNo.Length - 4 : 0)}",
        (int)enums.WithdrawalType.OnlinePayment => $"{SiteName}-{FromBankCode}",
        _ => null
    };
        public int IsKyc { set; get; }
        public int IsRisk { set; get; }
        public int WithdrawlCnt { set; get; }
        public int Flag { set; get; }
        public DateTime? UpdateTime { set; get; }
        //public virtual IEnumerable<Comments> Comments { get; set; }
        public bool HasPic { get; set; }
        public int BankSlipPicCount { get; set; }
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
    public class WithdrawalInfoForExport
    {
        public int WithdrawalId { set; get; }
        public string TransId { set; get; }
        public int WithdrawalType { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public int CurrencyId { set; get; }
        public bool IsTest { set; get; }
        public decimal Amount { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public string AccountNo { set; get; }
        public string AccountName { set; get; }
        public DateTime CreateTime { set; get; }
        public int Status { set; get; }
        public string SiteName { set; get; }
        public string FromBankName { set; get; }
        public string FromBankCode { set; get; }
        public string FromAccountNo { set; get; }
        public string FromAccountName { set; get; }
        public string PayName { set; get; }
        public string RefId { set; get; }

        public string CalCode =>
        WithdrawalType switch
        {
            (int)enums.WithdrawalType.LocalBank => $"{SiteName}-{FromBankCode}-{FromAccountName?.Replace(" ", string.Empty)}-{FromAccountNo?.Substring(FromAccountNo.Length > 4 ? FromAccountNo.Length - 4 : 0)}",
            (int)enums.WithdrawalType.OnlinePayment => $"{SiteName}-{FromBankCode}",
            _ => null
        };
        public int IsKyc { set; get; }
        public int IsRisk { set; get; }
        public int WithdrawlCnt { set; get; }
        public string Operator { set; get; }
        public DateTime? UpdateTime { set; get; }
    }

    public class WithdrawalLimit
    {
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }
    }

    public class WithdrawalLimitInfoManage
    {
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }

        public int VipLevel { get; set; }

        public double WithdrawalMin { get; set; }

        public double WithdrawalMax { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateOperator { get; set; }
    }

    public class WithdrawalKYC
    {
        public int CurrencyId { get; set; }
        public int SiteId { get; set; }
        public double Amount { get; set; }

    }
    public class WithdrawalData
    {
        public bool HasDetail { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public int WithdrawalMin { get; set; }
    }
    public class WithdrawalMinInfo
    {
        public int CurrencyId { get; set; }
        public int WithdrawalMin { get; set; }
    }
    public class BankSlipPic
    {
        public int BankSlipPicCount { get; set; }
    }
    public class WithdrawalTimeReportPara
    {
        public string WithdrawalID { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public IEnumerable<int> SelectSite { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class WithdrawalTimeData
    {
        public List<WithdrawalTimeList> WithdrawalTimeList { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class WithdrawalTimeList
    {
        public string SiteName { get; set; }
        public int SiteId { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime VerifyTime { get; set; }
        public string TransId { get; set; }
        public int WithdrawalType { get; set; }
        public string UserName { get; set; }
        public int CustId { get; set; }
        public string IsoCode { get; set; }
        public DateTime DepositTime { get; set; }
        public string CompleteTime { get; set; }
        public string WithdrawalTypeName => Enum.GetName(typeof(WithdrawalType), WithdrawalType);
        public string StatusName => Enum.GetName(typeof(WithdrawalStatus), Status);
        public int WithdrawalId { get; set; }
        public string ToVerifyTime { get; set; }
        public string ToApprovedTime { get; set; }
    }

    public class WithdrawalTimeExportData
    {
        public string SiteName { get; set; }
        public int WithdrawalId { get; set; }
        public string WithdrawalType { get; set; }
        public string UserName { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string ToVerifyTime { get; set; }
        public string ToApprovedTime { get; set; }
        public string CompleteTime { get; set; }
    }
}
