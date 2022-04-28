using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class WithdrawalRequest
    {
        private DateTimeOffset _flowEndTime { set; get; }

        private DateTimeOffset _createTime { set; get; }

        private DateTimeOffset? _updateTime { set; get; }

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
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
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
        public string IFSCCode { set; get; }
        public string NickName { set; get; }
        public string AccountType { set; get; }
        public string Network { set; get; }
        public int WalletCurrencyID { set; get; }
        public string CryptoCurrency { set; get; }
        public string RefId { set; get; }
        public bool HasPhotoId { set; get; }
        public string VipLevelName { set; get; }
        public int VipLevel { set; get; }
        public string Operator { set; get; }
        public bool SameOperator { get; set; }
        public string CalCode => WithdrawalType switch
        {
            (int)enums.WithdrawalTypeEnum.LocalBank => $"{SiteName}-{FromBankCode}-{FromAccountName?.Replace(" ", string.Empty)}-{FromAccountNo?.Substring(FromAccountNo.Length > 4 ? FromAccountNo.Length - 4 : 0)}",
            (int)enums.WithdrawalTypeEnum.OnlinePayment => $"{SiteName}-{FromBankCode}",
            _ => null
        };
        public int IsKyc { set; get; }
        public int IsRisk { set; get; }
        public int WithdrawlCnt { set; get; }
        public int Flag { set; get; }
        public DateTimeOffset? UpdateTime
        {
            get => _updateTime;
            set
            {
                if (value != null)
                    _updateTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public bool HasPic { get; set; }
        public int BankSlipPicCount { get; set; }

        public DateTimeOffset FlowEndTime
        {
            get => _flowEndTime;
            set => _flowEndTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string ChannelName { set; get; }
    }
    public class WithdrawalInfo
    {
        public int SysId { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public string IFSCCode { get; set; }
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
        public int? ChannelId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? BankId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? CurrencyId { set; get; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AccountType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NickName { get; set; }
        public string Network { set; get; }
    }
    public class WithdrawalInfoForExport
    {

        private DateTimeOffset? _flowEndTime { set; get; }

        private DateTimeOffset _createTime { set; get; }

        private DateTimeOffset? _updateTime { set; get; }
        public int WithdrawalId { set; get; }
        public string TransId { set; get; }
        public int WithdrawalType { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public string NetWork { set; get; }
        public decimal CryptoAmount { set; get; }
        public int CurrencyId { set; get; }
        public bool IsTest { set; get; }
        public decimal Amount { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public string AccountNo { set; get; }
        public string AccountName { set; get; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
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
            (int)enums.WithdrawalTypeEnum.LocalBank => $"{SiteName}-{FromBankCode}-{FromAccountName?.Replace(" ", string.Empty)}-{FromAccountNo?.Substring(FromAccountNo.Length > 4 ? FromAccountNo.Length - 4 : 0)}",
            (int)enums.WithdrawalTypeEnum.OnlinePayment => $"{SiteName}-{FromBankCode}",
            _ => null
        };
        public int IsKyc { set; get; }
        public int IsRisk { set; get; }
        public int WithdrawlCnt { set; get; }
        public string Operator { set; get; }
        public DateTimeOffset? UpdateTime
        {
            get => _updateTime;
            set
            {
                if (value != null)
                    _updateTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public DateTimeOffset? FlowEndTime
        {
            get => _flowEndTime;
            set
            {
                if (value != null)
                    _flowEndTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
    }

    public class WithdrawalLimit
    {
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }
    }

    public class WithdrawalLimitInfoManage
    {
        private DateTimeOffset _updateTime { set; get; }
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }

        public int VipLevel { get; set; }

        public double WithdrawalMin { get; set; }

        public double WithdrawalMax { get; set; }

        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        public string UpdateOperator { get; set; }
        
        public string LevelName { get; set; }

        public int Times { get; set; }
    }
    
    public class SetWithdrawalLimitInfo
    {
        public int VipLevel { get; set; }
        public int WithdrawalMin { get; set; }
        public int WithdrawalMax { get; set; }
        public int Times { get; set; }
    }

    public class WithdrawalKYC
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
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
    public class WithdrawalTimeReportOldReq
    {
        public string WithdrawalId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public IEnumerable<int> SelectSite { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class WithdrawalTimeReportPara
    {
        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
        public string WithdrawalId { get; set; }
        public DateTimeOffset StartDate
        {
            get => _startDate;
            set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndDate
        {
            get => _endDate;
            set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public IEnumerable<int> SiteIds { get; set; }
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
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public DateTimeOffset? VerifyTime { get; set; }
        public DateTimeOffset? ProcessTime { get; set; }
        public DateTimeOffset FlowEndTime { get; set; }
        public string TransId { get; set; }
        public int WithdrawalType { get; set; }
        public string UserName { get; set; }
        public int CustId { get; set; }
        public string IsoCode { get; set; }
        public DateTimeOffset? DepositTime { get; set; }
        public string CompleteTime { get; set; }
        public string WithdrawalTypeName => Enum.GetName(typeof(WithdrawalTypeEnum), WithdrawalType);
        public string StatusName => Enum.GetName(typeof(WithdrawalStatusEnum), Status);
        public int WithdrawalId { get; set; }
        public string ToVerifyTime { get; set; }
        public string ToApprovedTime { get; set; }
        public string Operator { get; set; }
        public string VerifiedBy { get; set; }
        public string ProcessedBy { get; set; }
        public string ApprovedBy { get; set; }
    }

    public class WithdrawalTimeExportData
    {
        public string SiteName { get; set; }
        public int WithdrawalId { get; set; }
        public string WithdrawalType { get; set; }
        public string UserName { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string OperateBy { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public DateTimeOffset? FlowEndTime { get; set; }
        public string ToVerifyTime { get; set; }
        public string VerifiedBy { get; set; }
        public string ToProcessTime { get; set; }
        public string ProcessedBy { get; set; }
        public string ToApprovedTime { get; set; }
        public string ApprovedBy { get; set; }
        public string CompleteTime { get; set; }
    }
    public class USDTWithdrawalLimit
    {
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }
        public int NetworkType { get; set; }
    }

    public class USDTWithdrawalLimitInfoManage
    {
        private DateTimeOffset _updateTime { get; set; }
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }

        public int VipLevel { get; set; }

        public double WithdrawalMin { get; set; }

        public double WithdrawalMax { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string UpdateOperator { get; set; }
        public string Network { get; set; }
        public int NetworkType { get; set; }
    }
}
