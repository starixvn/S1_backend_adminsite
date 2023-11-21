using BackendSite.Attributes;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

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
        public BankInfoPayStatusEnum Status { get; set; }
        public int CurrencyId { get; set; }
    }

    public class BankInfoPayForSetting : PayListBankInfoSys
    {
        public BankInfoPayMethodEnum Method { get; set; }
    }

    public class UpdatePayInfo
    {
        [LogIgnore]
        public int PayId { get; set; }
        // log用
        public string PayName { get; set; }

        /// <summary>
        /// 狀態(1=Enable、2=Disable)
        /// </summary>
        [LogIgnore]
        public PayListStatusEnum Status { get; set; }

        public string StatusName => Status.ToString();
    }

    public class UpdatePayChannel
    {
        [LogIgnore]
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }

        public int Sort { get; set; }

        /// <summary>
        /// 狀態(1=Enable、2=Disable)
        /// </summary>
        [LogIgnore]
        public PayChannelStatusEnum Status { get; set; }
        public string StatusName => Status.ToString();

        [LogPropertyName("VipLevel")]
        public int PointLevel { get; set; }
    }

    public class UpdatePayCurrency
    {
        [LogIgnore]
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }

        public decimal MinAmount { get; set; }

        public decimal MaxAmount { get; set; }

        /// <summary>
        /// 數字規則(1=Integer、2=Decimal、3=Fixed)
        /// </summary>
        [LogIgnore]
        public AmountRuleEnum AmountRule { get; set; }
        public string AmountRuleName => AmountRule.ToString();

        public IEnumerable<decimal> FixedAmountList { get; set; }
    }

    public class UpdatePaySettingParams
    {
        public int SiteId { get; set; }
        public CurrencyEnum CurrencyId { get; set; }
        public int ChannelId { get; set; }

        public decimal MinAmount { get; set; }

        public decimal MaxAmount { get; set; }
        public AmountRuleEnum AmountRule { get; set; }

        public IEnumerable<decimal> FixedAmountList { get; set; }

        public int PointLevel { get; set; }

        public int Sort { get; set; }
    }

    public class PayListInfo
    {
        public int PayId { get; set; }
        public string PayName { get; set; }

        /// <summary>
        /// 全域金流狀態
        /// </summary>
        [JsonIgnore]
        public PayListStatusEnum PayStatus { get; set; }
        /// <summary>
        /// 品牌金流狀態
        /// </summary>
        [JsonIgnore]
        public PayListStatusEnum SitePayStatus { get; set; }

        public PayListStatusEnum Status => PayStatus == PayListStatusEnum.Enable
            ? SitePayStatus
            : PayStatus;

        public string LoginUrl { get; set; }
        public string LoginAccount { get; set; }
        public string LoginPassword { get; set; }
        public string TransferPin { get; set; }
        public string LoginPin { get; set; }
        public string CorporateId { get; set; }
    }

    public class UpdatePaySettingStatusParams
    {
        public int SiteId { get; set; }
        public CurrencyEnum CurrencyId { get; set; }
        public int ChannelId { get; set; }
        public PayChannelStatusEnum Status { get; set; }
    }

    public class UpdatePaySettingChargeFeeParams
    {
        public int SiteId { get; set; }
        public CurrencyEnum CurrencyId { get; set; }
        public int ChannelId { get; set; }
        public decimal Percentage { get; set; }
        public decimal PreCount { get; set; }
    }

    public class GetPaySettingListRequest : ApiRequestPageModel
    {
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public PayListMethodEnum? Method { get; set; }
        public int? PayId { get; set; }
        public PayChannelTypeEnum? Type { get; set; }
        public PayListStatusEnum? Globalstatus { get; set; }
        public PayChannelStatusEnum? Status { get; set; }
    }

    public class SetPayListLoginInfoParams
    {
        public int SiteId { get; set; }
        public int PayId { get; set; }
        public string LoginUrl { get; set; }
        public string LoginAccount { get; set; }
        public string LoginPassword { get; set; }
        public string TransferPin { get; set; }
        public string LoginPin { get; set; }
        public string Operator { get; set; }

        public string CorporateId { get; set; }
    }

    public class GetPaySettingListParams
    {
        public GetPaySettingListParams(GetPaySettingListRequest req)
        {
            SiteId = req.SiteId;
            CurrencyIds = req.CurrencyIds;
            Method = req.Method;
            PayId = req.PayId;
            Type = req.Type;
            PayStatus = req.Globalstatus;
            Status = req.Status;
        }

        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public PayListMethodEnum? Method { get; set; }
        public int? PayId { get; set; }
        public PayChannelTypeEnum? Type { get; set; }
        public PayListStatusEnum? PayStatus { get; set; }
        public PayChannelStatusEnum? Status { get; set; }
    }

    public class PaySetting
    {
        public CurrencyEnum CurrencyId { get; set; }
        public string IsoCode => CurrencyId.ToString();
        public int PayId { get; set; }
        public string PayName { get; set; }
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string GroupName { get; set; }
        public PayChannelTypeEnum Type { get; set; }
        public int Mode { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public AmountRuleEnum AmountRule { get; set; }
        [JsonIgnore]
        public string AmountShow { get; set; }
        public IEnumerable<decimal> FixedAmountList =>
            JsonConvert.DeserializeObject<IEnumerable<decimal>>(string.IsNullOrEmpty(AmountShow) ? "[]" : AmountShow);
        public int PointLevel { get; set; }
        public int Sort { get; set; }
        public PayChannelStatusEnum Status { get; set; }
        [JsonIgnore]
        public PayListStatusEnum PayStatus { get; set; }
        [JsonIgnore]
        public PayListStatusEnum SitePayStatus { get; set; }
        public PayListStatusEnum Globalstatus => SitePayStatus;
        public PayChannelMethodEnum Method { get; set; }
        public decimal ChargeFeePercentage { get; set; }
        public decimal ChargeFeePreCount { get; set; }
    }

    public class PayChannelInfo
    {
        public PayChannelInfo(PaySetting setting)
        {
            PayId = setting.PayId;
            PayName = setting.PayName;
            ChannelId = setting.ChannelId;
            ChannelName = setting.ChannelName;
            Type = setting.Type;
            Mode = setting.Mode;
            Sort = setting.Sort;
        }

        public int PayId { get; set; }
        public string PayName { get; set; }
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }
        public PayChannelTypeEnum Type { get; set; }
        public int Mode { get; set; }
        public int Sort { get; set; }
    }

    public class PayChannelInfoComparer : IEqualityComparer<PayChannelInfo>
    {
        public bool Equals(PayChannelInfo t1, PayChannelInfo t2)
        {
            return (t1.ChannelId == t2.ChannelId);
        }

        public int GetHashCode(PayChannelInfo t)
        {
            return t.ToString().GetHashCode();
        }
    }

    public class LocalPaySetting
    {
        public int CurrencyId { get; set; }

        public int SysId { get; set; }

        public int GroupId { get; set; }

        public string GroupName { get; set; }

        public DepositTypeEnum Type { get; set; }
    }

    public class PayGroup
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DepositTypeEnum Type { get; set; }
    }

    public class WithdrawalBlockSetting
    {
        public CurrencyEnum CurrencyId { get; set; }
        public WithdrawalBlockTypeEnum Type { get; set; }
        public bool Status { get; set; }
    }
    
    public class SetWithdrawalBlockSettingParams : WithdrawalBlockSetting
    {
        public int SiteId { get; set; }
        public string Operator { get; set; }
    }
}
