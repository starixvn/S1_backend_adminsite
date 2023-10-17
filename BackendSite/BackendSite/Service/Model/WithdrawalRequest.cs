using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class GetByTransIdRequest
    {
        public string TransId { get; set; }
    }

    public class SendToNextRequest : GetByTransIdRequest
    {
        public string Comment { set; get; }
        public WithdrawalStatusEnum Action { set; get; }
        public string WithdrawalInfo { set; get; }
        public WithdrawalTypeEnum? WithdrawalType { set; get; }
        public int? SelId { set; get; }
        public int? ChannelId { set; get; }
        public IEnumerable<WithdrawalAccountingModel> WithdrawalAccountingModels { set; get; }
    }

    public class ReqWithdrawalOnline
    {
        public int SysId { get; set; }
        public string TransId { get; set; }
        public int PayId { get; set; }
        public int ChannelId { get; set; }
        public int CustId { get; set; }
        public decimal Amount { get; set; }
        public string AccountName { set; get; }
        public string AccountNo { set; get; }
        public string IFSCCode { set; get; }
    }

    public class WithdrawalRequest
    {
        private DateTimeOffset _flowEndTime { set; get; }
        private DateTimeOffset _createTime { set; get; }
        private DateTimeOffset? _updateTime { set; get; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset FlowEndTime
        {
            get => _flowEndTime;
            set => _flowEndTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset? UpdateTime
        {
            get => _updateTime;
            set
            {
                if (value != null)
                    _updateTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public int WithdrawalId { set; get; }
        public string TransId { set; get; }
        public WithdrawalTypeEnum WithdrawalType { set; get; }
        public string WithdrawalTypeName => WithdrawalType.ToString();
        public int CustId { set; get; }
        public string UserName { set; get; }
        public string Email { set; get; }
        public string WithdrawalInfo { set; get; }
        public int CurrencyId { set; get; }
        public string IsoCode { get; set; }
        public int SiteId { set; get; }
        public decimal Amount { set; get; }
        public decimal ChargeFee { set; get; }
        public string Memo { set; get; }
        public string Comment { set; get; }
        public WithdrawalStatusEnum Status { set; get; }
        public string StatusName => Status.ToString();
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
        private string FromAccountDisplayName { set; get; }
        public string CalCode => WithdrawalType switch
        {
            WithdrawalTypeEnum.LocalBank => FromAccountDisplayName,//$"{SiteName}-{FromBankCode}-{FromAccountName?.Replace(" ", string.Empty)}-{FromAccountNo?.Substring(FromAccountNo.Length > 4 ? FromAccountNo.Length - 4 : 0)}",
            WithdrawalTypeEnum.OnlineBanking => $"{SiteName}-{FromBankCode}",
            _ => null
        };
        public int IsKyc { set; get; }
        public int IsRisk { set; get; }
        public int WithdrawlCnt { set; get; }
        public int Flag { set; get; }
        public bool HasPic { get; set; }
        public int BankSlipPicCount { get; set; }
        public string PayChannelName { get; set; }

        public string ChannelName
        {
            get
            {
                if (!string.IsNullOrEmpty(CryptoCurrency))
                {
                    return CryptoCurrency == Network
                        ? CryptoCurrency
                        : $"{CryptoCurrency}-{Network}";
                }
                else
                {
                    return PayChannelName;
                }
            }
        }

        public string EWalletName { get; set; }
        public int EWalletSysId { get; set; }
        public int? DeviceScore { set; get; }
        public string Agent { get; set; }
        public List<WithdrawalActionEnum> UsableActionButton { set; get; }
        public string TransferPin { get; set; }
        public string LoginPin { get; set; }
        public string MobilePin { get; set; }
        public string CorporateID { get; set; }
        public IEnumerable<WithdrawalAccountingModel> WithdrawalAccountingModels { set; get; }

        [JsonIgnore]
        public WithdrawalInfoProcessWayEnum ProcessWay { get; set; }

        public string ProcessWayName => ProcessWay.ToString();

        public string OnUser { get; set; }
        public IEnumerable<WithdrawalMessage> WithdrawalMessageList { set; get; }
    }
    public class WithdrawalInfo
    {
        public int SysId { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public string IFSCCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? FromSysId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FromAccountName { get; set; }
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
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Network { set; get; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? EWalletSysId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? TempExchangeRate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? TempExchangeAmount { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ExchangeRate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ExchangeAmount { get; set; }
    }
    public class WithdrawalInfoForExport
    {
        private DateTimeOffset? _flowEndTime { set; get; }
        public DateTimeOffset? FlowEndTime
        {
            get => _flowEndTime;
            set
            {
                if (value != null)
                    _flowEndTime = new DateTimeOffset(value.Value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }
        }
        private DateTimeOffset _createTime { set; get; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
        }
        private DateTimeOffset? _updateTime { set; get; }
        public DateTimeOffset? UpdateTime
        {
            get => _updateTime;
            set
            {
                if (value != null)
                    _updateTime = new DateTimeOffset(value.Value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }
        }
        public int WithdrawalId { set; get; }
        public string TransId { set; get; }
        public int WithdrawalType { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public string NetWork { set; get; }
        public decimal CryptoAmount { set; get; }
        public int CurrencyId { set; get; }
        public string IsoCode { get; set; }
        public bool IsTest { set; get; }
        public decimal Amount { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public string AccountNo { set; get; }
        public string AccountName { set; get; }
        public int Status { set; get; }
        public string SiteName { set; get; }
        public string FromBankName { set; get; }
        public string FromBankCode { set; get; }
        public string FromAccountNo { set; get; }
        public string FromAccountName { set; get; }
        public string PayName { set; get; }
        public string RefId { set; get; }
        private string FromAccountDisplayName { set; get; }
        public string CalCode => WithdrawalType switch
        {
            (int)WithdrawalTypeEnum.LocalBank => FromAccountDisplayName,//$"{SiteName}-{FromBankCode}-{FromAccountName?.Replace(" ", string.Empty)}-{FromAccountNo?.Substring(FromAccountNo.Length > 4 ? FromAccountNo.Length - 4 : 0)}",
            (int)WithdrawalTypeEnum.OnlineBanking => $"{SiteName}-{FromBankCode}",
            _ => null
        };
        public int IsKyc { set; get; }
        public int IsRisk { set; get; }
        public int WithdrawlCnt { set; get; }
        public string Operator { set; get; }
        public string EWalletName { get; set; }
        public string CurrencyName { get; set; }
    }

    public class WithdrawalComments
    {
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string Comment { get; set; }
        public int Action { get; set; }
        public string ActionName => Enum.GetName(typeof(WithdrawalStatusEnum), Action);
        public decimal ActualDeposit { get; set; }
        public string Operator { get; set; }
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

        public decimal WithdrawalMin { get; set; }

        public decimal WithdrawalMax { get; set; }

        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        public string UpdateOperator { get; set; }

        public string LevelName { get; set; }

        public int Times { get; set; }
        public decimal TotalWithdrawalMax { get; set; }
    }

    public class SetWithdrawalLimitInfo
    {
        [LogIgnore]
        public int VipLevel { get; set; }
        public string VipLevelName { get; set; }
        public int WithdrawalMin { get; set; }
        public int WithdrawalMax { get; set; }
        public int TotalWithdrawalMax { get; set; }
        [LogPropertyName("WithdrawalFrequency")]
        public int Times { get; set; }
    }

    public class WithdrawalKYC
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public double Amount { get; set; }

    }
    public class WithdrawalKYCAmount
    {
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
        public IEnumerable<int> CurrencyId { get; set; }
        public IEnumerable<int> Status { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string MemberId { get; set; }
        public int CustId { set; get; }
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
        public IEnumerable<WithdrawalMessage> WithdrawalMessageList { set; get; }
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
        public string WithdrawalMessage { set; get; }
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

    public class EWalletWithdrawalLimitInfoManage
    {
        private DateTimeOffset _updateTime { set; get; }
        public int SiteId { get; set; }

        public int CurrencyId { get; set; }

        public double WithdrawalMin { get; set; }

        public double WithdrawalMax { get; set; }

        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        public string UpdateOperator { get; set; }
        public int Status { get; set; }
        public string IsoCode { get; set; }
    }
    public class WithdrawalInfoForExportData
    {
        public int WithdrawalId { set; get; }
        public string TransId { set; get; }
        public string WithdrawalType { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public string NetWork { set; get; }
        public decimal CryptoAmount { set; get; }
        public string CurrencyId { set; get; }
        public bool IsTest { set; get; }
        public decimal Amount { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public string AccountNo { set; get; }
        public string AccountName { set; get; }
        public DateTimeOffset CreateTime { set; get; }
        public int Status { set; get; }
        public string SiteName { set; get; }
        public string FromBankName { set; get; }
        public string FromBankCode { set; get; }
        public string FromAccountNo { set; get; }
        public string FromAccountName { set; get; }
        public string PayName { set; get; }
        public string RefId { set; get; }
        public string CalCode { set; get; }
        public int IsKyc { set; get; }
        public int IsRisk { set; get; }
        public int WithdrawlCnt { set; get; }
        public string Operator { set; get; }
        public DateTimeOffset? UpdateTime { set; get; }
        public DateTimeOffset? FlowEndTime { set; get; }
        public string WithdrawalMessage { set; get; }
    }
    public class WithdrawalAccountingModel
    {
        public int Seq { set; get; }
        public int BankId { set; get; }
        public string DisplayName { set; get; }
        public decimal Amount { set; get; }
        public decimal ChargeFee { set; get; }
    }

    public class WithdrawalManualRequest
    {
        public string TransId { set; get; }
        public string ThirdId { get; set; }
        public string Operator { get; set; }
    }
    public class WithdrawalManualResponse
    {
        public int ErrorCode { get; set; }

        public string Message { get; set; }
    }

    public class WithdrawalMessage
    {
        private DateTimeOffset _createTime { set; get; }
        public WithdrawalMessageStatus Status { get; set; }
        public string Message => Status.GetDescription();
        public int WithdrawalId { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
}
