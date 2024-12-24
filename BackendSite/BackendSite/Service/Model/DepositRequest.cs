using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

using OfficeOpenXml.Attributes;

namespace BackendSite.Service.Model
{
    public class DepositRequest
    {
        public string TransId { set; get; }
        public int DepositId { set; get; }
        public DepositTypeEnum DepositType { set; get; }
        public string DepositTypeName => DepositType.ToString();
        public int CurrencyId { get; set; }
        public string IsoCode { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public string Email { set; get; }
        public string RealName { set; get; }
        public int SiteId { set; get; }
        public string SiteName { set; get; }
        public string DepositInfo { set; get; }
        public decimal Amount { set; get; }
        public decimal ActualDeposit { set; get; }
        public string Comment { set; get; }
        private DateTimeOffset _createTime { set; get; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        [System.Text.Json.Serialization.JsonIgnore]
        public IDepositInfo oDepositInfo =>
                DepositType switch
                {
                    DepositTypeEnum.OnlineBanking => JsonConvert.DeserializeObject<DepositInfo1_1>(DepositInfo),
                    DepositTypeEnum.MobilePay => JsonConvert.DeserializeObject<DepositInfo2_1>(DepositInfo),
                    DepositTypeEnum.LocalBanking => JsonConvert.DeserializeObject<DepositInfo3_1>(DepositInfo),
                    _ => null
                };
        public int FirstDeposit { set; get; }
        public DepositStatusEnum Status { set; get; }
        public string StatusName => Status.ToString();
        public IEnumerable<DepositActionEnum> UsableActionButton { set; get; }
        public bool Receipt { set; get; }
        public string RefId { set; get; }
        private DateTimeOffset? _depositTime;
        public DateTimeOffset? DepositTime
        {
            get => _depositTime;
            set
            {
                if (value != null)
                    _depositTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public int BankId { set; get; }
        public string AccountName { set; get; }
        public string AccountNo { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public string PayName { set; get; }
        public string PicFileName { set; get; }
        public string DisplayName { set; get; }
        public string CalCode =>
            DepositType switch
            {
                DepositTypeEnum.OnlineBanking => $"{SiteName}-{PayName}-{BankCode}",
                DepositTypeEnum.MobilePay => $"{SiteName}-{PayName}-{Network}",
                DepositTypeEnum.LocalBanking => DisplayName,//$"{SiteName}-{BankCode}-{AccountName?.Replace(" ", string.Empty)}-{AccountNo?.Substring(AccountNo.Length > 4 ? AccountNo.Length - 4 : 0)}",
                _ => null
            };
        public int IsKyc { set; get; }
        public int IsRisk { set; get; }
        public string VipLevelName { set; get; }
        public int DepositCnt { set; get; }
        public int Flag { set; get; }
        private DateTimeOffset? _updateTime;
        public DateTimeOffset? UpdateTime
        {
            get => _updateTime;
            set
            {
                if (value != null)
                    _updateTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        private DateTimeOffset? _statementTime;
        public DateTimeOffset? StatementTime
        {
            get => _statementTime;
            set
            {
                if (value != null)
                    _statementTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        private DateTimeOffset? _flowEndTime;
        public DateTimeOffset? FlowEndTime
        {
            get => _flowEndTime;
            set
            {
                if (value != null)
                    _flowEndTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public string ChannelName { set; get; }
        public int VipLevel { set; get; }
        public string Operator { set; get; }
        public string BonusCode { set; get; }
        public string BonusDescription { set; get; }
        public string Agent { set; get; }
        public string DepositFrom { set; get; }
        public string FromAccountNo { set; get; }
        public string FromBankCode { set; get; }
        public string FromBankName { set; get; }
        public int CheckReceipt { set; get; }
        public string CheckReceiptBy { set; get; }
        public string Network { set; get; }
        public string Remark { set; get; }
        public IEnumerable<DepositMessage> DepositMessageList { set; get; }
        
        public int AgentId { set; get; }
        public PaymentAgentTypeEnum AgentType { set; get; }
        public PaymentAgentStatusEnum AgentStatus { set; get; }
    }

    public class DepositInfoForExport
    {
        [EpplusIgnore]
        public int DepositId { set; get; }
        public string TransId { set; get; }
        [EpplusIgnore]
        public int DepositType { set; get; }
        public string DepositTypeName => Enum.GetName(typeof(DepositTypeEnum), DepositType);
        [EpplusIgnore]
        public int CurrencyId { get; set; }
        public string Currency { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public string Network { set; get; }
        public decimal CryptoAmount { set; get; }
        public string Agent { set; get; }
        public string BankAgent { set; get; }
        public string SiteName { set; get; }
        public bool IsTest { set; get; }
        public decimal Amount { set; get; }
        public decimal ActualDeposit { set; get; }
        public string Comment { set; get; }
        public bool FirstDeposit { set; get; }
        public string RefId { set; get; }
        [EpplusIgnore]
        public int Status { set; get; }
        public string StatusName => Enum.GetName(typeof(DepositStatusEnum), Status);
        public string AccountName { set; get; }
        public string AccountNo { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public string PayName { set; get; }
        public string ChannelName { set; get; }
        private string DisplayName { set; get; }
        public string CalCode =>
            DepositType switch
            {
                (int)DepositTypeEnum.OnlineBanking => $"{SiteName}-{PayName}-{BankCode}",
                (int)DepositTypeEnum.MobilePay => $"{SiteName}-{PayName}-{Network}",
                (int)DepositTypeEnum.LocalBanking => DisplayName,//$"{SiteName}-{BankCode}-{AccountName?.Replace(" ", string.Empty)}-{AccountNo?.Substring(AccountNo.Length > 4 ? AccountNo.Length - 4 : 0)}",
                _ => null
            };
        public int IsKyc { set; get; }
        public int IsRisk { set; get; }
        public int DepositCnt { set; get; }
        public string Operator { set; get; }
        private DateTimeOffset _createTime { set; get; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
        }
        private DateTimeOffset? _depositTime;
        public DateTimeOffset? DepositTime
        {
            get => _depositTime;
            set
            {
                if (value != null)
                    _depositTime = new DateTimeOffset(value.Value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }
        }
        private DateTimeOffset? _updateTime;
        public DateTimeOffset? UpdateTime
        {
            get => _updateTime;
            set
            {
                if (value != null)
                    _updateTime = new DateTimeOffset(value.Value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }
        }
        private DateTimeOffset? _statementTime;
        public DateTimeOffset? StatementTime
        {
            get => _statementTime;
            set
            {
                if (value != null)
                    _statementTime = new DateTimeOffset(value.Value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }
        }
        private DateTimeOffset? _flowEndTime;
        public DateTimeOffset? FlowEndTime
        {
            get => _flowEndTime;
            set
            {
                if (value != null)
                    _flowEndTime = new DateTimeOffset(value.Value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }
        }
        public string AffiliateCode { get; set; }
        public int CheckReceipt { set; get; }
        public string CheckReceiptBy { set; get; }
        public string Memo { set; get; }
        public string SendMessages { set; get; }
    }

    public class DepositDetailSubmission
    {
        public DepositDetailInfo Detail { get; set; }
        public IFormFile Receipt { get; set; }
    }

    public class DepositDetailInfo
    {
        private DateTimeOffset _depositTime { get; set; }
        [LogIgnore]
        public int CustId { get; set; }
        public string UserName { get; set; }
        [LogIgnore]
        public int CurrencyId { get; set; }
        [LogIgnore]
        public int SiteId { get; set; }
        [LogIgnore]
        public int SysId { get; set; }
        [LogIgnore]
        public string DepositInfo { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }
        [LogPropertyName("ReferenceNo")]
        public string RefId { get; set; }
        public DateTimeOffset DepositTime
        {
            get => _depositTime;
            set => _depositTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string Operator { get; set; }
        public string BankName { get; set; }
        public string AccountHolderName { get; set; }
        public string AccountNumber { get; set; }
        public string SabaId { get; set; }
    }

    public interface IDepositInfo
    {
        int Ver { get; set; }
        int CurrencyId { get; set; }
    }
    public interface IPay
    {
        int PayId { get; set; }
        int ChannelId { get; set; }
        int CurrencyId { get; set; }
    }
    public class DepositInfo1_1 : IDepositInfo, IPay
    {
        public int Ver { get; set; }
        public int SysId { get; set; }
        public int CurrencyId { get; set; }
        public string SysBankCode { get; set; }
        public string SysBankName { get; set; }
        public string BankLogo { get; set; }
        public int PayId { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public int ChannelId { get; set; }
        public int Mode { get; set; }
        public int PointLevel { get; set; }
        public string PayCode { get; set; }
    }
    public class DepositInfo2_1 : IDepositInfo, IPay
    {
        public int Ver { get; set; }
        public int PayId { get; set; }
        public int ChannelId { get; set; }
        public int CurrencyId { get; set; }
    }
    public class DepositInfo3_1 : IDepositInfo
    {
        public int Ver { get; set; }
        public int SysId { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public int CurrencyId { get; set; }
        public bool IsLocalPay { get; set; }
        public int GroupId { get; set; }
        public string Code { get; set; }
    }
    public class DepositComments
    {
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string Comment { get; set; }
        public int Action { get; set; }
        public string ActionName => Enum.GetName(typeof(DepositStatusEnum), Action);
        public decimal ActualDeposit { get; set; }
        public string Operator { get; set; }
    }

    public class DepositReceipt
    {
        public int DepositId { get; set; }
        public int ReceiptNo { get; set; }
        public string RefNo { get; set; }
        public string PicFileName { get; set; }
        public byte[] DepositPic { get; set; }
        public int CreateFrom { get; set; }
        public string Creator { get; set; }
        public DateTime DepositTime { get; set; }
        public DateTime CreateTime { get; set; }

    }

    public class DepositReceiptModel
    {
        public string Receipt { get; set; }
        public int ReceiptNo { get; set; }
        public int CreateFrom { get; set; }
        public string Creator { get; set; }
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }

    public class DepositLimit
    {
        public int SiteId { get; set; }
        public int DepositType { get; set; }
        public int CurrencyId { get; set; }
    }

    public class DepositLimitInfo : DepositLimit
    {
        public int VipLevel { get; set; }

        public double DepositMin { get; set; }

        public double DepositMax { get; set; }

        public int DepositStatus { get; set; }
    }

    public class DepositLimitModel
    {
        public int VipLevel { get; set; }

        public double DepositMin { get; set; }

        public double DepositMax { get; set; }
        public DepositTypeEnum DepositType { get; set; }
        public DepositStatusEnum DepositStatus { get; set; }
        public string DepositTypeName => DepositType.ToString();
        public string DepositStatusName => DepositStatus.ToString();
    }

    public class DepositLimitCurrencyInfo
    {
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public string Note { get; set; }
    }

    public class DepositTimeList
    {
        public string SiteName { get; set; }
        public int SiteId { get; set; }
        public int Status { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string TransId { get; set; }
        public int DepositType { get; set; }
        public string UserName { get; set; }
        public int CustId { get; set; }
        public string IsoCode { get; set; }
        public DateTimeOffset? DepositTime { get; set; }
        public string CompleteTime { get; set; }
        public string DepositTypeName => Enum.GetName(typeof(DepositTypeEnum), DepositType);
        public string StatusName => Enum.GetName(typeof(DepositStatusEnum), Status);
        public int DepositId { get; set; }
        public DateTimeOffset? ApprovedTime { get; set; }
        public string ToApprovedTime { get; set; }
        public string ApprovedBy { get; set; }
        public DateTimeOffset? StatementTime { get; set; }
        public string Operator { get; set; }
        public string DepositMessage { get; set; }

        public IEnumerable<DepositMessage> DepositMessageList { set; get; }
    }

    public class DepositTimeReportOldPara : ApiRequestPageModel
    {
        public string DepositId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public IEnumerable<int> SelectSite { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
    }

    public class DepositTimeReportPara : ApiRequestPageModel
    {
        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
        public string DepositId { get; set; }
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
        public IEnumerable<DepositStatusEnum> Status { get; set; }
        public string MemberId { get; set; }
        public int CustId { set; get; }
    }

    public class DepositTimeExportData
    {
        public string SiteName { get; set; }
        public int DepositId { get; set; }
        public string DepositType { get; set; }
        public string UserName { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string CompleteTime { get; set; }
        public DateTimeOffset? EndFlowTime { get; set; }
        public string ToApprovedTime { get; set; }
        public string ApprovedBy { get; set; }
        public DateTimeOffset? StatementTime { get; set; }
        public string Operator { get; set; }
        public string SendMessage { get; set; }
    }
    public class SetAmountRequest
    {
        public decimal Amount { get; set; }
    }

    public class DepositManualRequest
    {
        public string TransId { get; set; }
        public decimal Amount { get; set; }
        public string ThirdId { get; set; }
        public string Operator { get; set; }
    }

    public class DepositManualResponse
    {
        public int ErrorCode { get; set; }

        public string Message { get; set; }

        public decimal Amount { get; set; }
    }

    public class DepositCalCodeSetRequest
    {
        public int? BankId { get; set; }
        public string DisplayName { get; set; }
        public DateTimeOffset? StatementTime { get; set; }
    }

    public class DepositMessageRequest
    {
        public DepositMessageStatus Status { get; set; }
        public string StatusName { get; set; }
        public int DepositId { get; set; }
        public Decimal ActualDeposit { get; set; }
        public DepositStatusEnum Action { get; set; }
        public string ActionName { get; set; }
    }

    public class DepositReceiptFile
    {
        public int SiteId { get; set; }
        public int CustId { get; set; }
        public int ReceiptNo { get; set; }
        public string TransId { get; set; }
        public string Receipt { get; set; }
    }

    public class CheckDepositReceiptRequest
    {
        public string TransId { get; set; }
    }

    public class DeleteReceiptRequest
    {
        public int SiteId { get; set; }
        public int CustId { get; set; }
        public int ReceiptNo { get; set; }
        public string TransId { get; set; }
    }

    public class DepositMessage
    {
        private DateTimeOffset _createTime { set; get; }
        public DepositMessageStatus Status { get; set; }
        public string Message => Status.GetDescription();
        public int DepositId { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
}
