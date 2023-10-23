using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.Model.BankCardManagement
{
    public class OfflineBankingSetting
    {
        private DateTimeOffset _lastUpdateTime;
        public int FinancialAccountId { set; get; }
        public string AccountName { set; get; }
        public string AccountNo { set; get; }

        public string Agent { set; get; }
        /// <summary>
        /// 目前只有 INR IFSCCode 使用
        /// </summary>
        public string AccountCode { set; get; }

        public string FrontendDisplayName { set; get; }
        public decimal MinAmount { set; get; }
        public decimal MaxAmount { set; get; }
        public decimal DailyLimitAmount { set; get; }
        [JsonIgnore]
        public string FinancialAccountPurposes { set; get; }
        private IEnumerable<TransferAccountPurposeEnum> AccountPurposes =>
            JsonConvert.DeserializeObject<IEnumerable<TransferAccountPurposeEnum>>(FinancialAccountPurposes);

        [JsonIgnore]
        public TransferChannelEnum FinancialAccountChannel { set; get; }

        public string AccountChannel => FinancialAccountChannel.ToString();

        public IEnumerable<string> Purposes
            => AccountPurposes
                .Where(x => x != TransferAccountPurposeEnum.Transfer)
                .Select(x => x.ToString());

        public int CurrencyId { set; get; }
        public int SysId { set; get; }
        public string BankCode { set; get; }
        public string BankName { set; get; }
        public BankCardManagementStatusEnum Status { set; get; }
        [JsonIgnore]
        public string JsonPointLevels { set; get; }
        public IEnumerable<int> PointLevels =>
            JsonConvert.DeserializeObject<IEnumerable<int>>(JsonPointLevels ?? "[]");
        [JsonIgnore]
        public string JsonVipLevels { set; get; }
        public IEnumerable<int> VipLevels =>
            JsonConvert.DeserializeObject<IEnumerable<int>>(JsonVipLevels ?? "[]");
        public IEnumerable<string> VipLevelNames { set; get; }
        // 沒用
        public decimal Dummy { set; get; }
        public string Memo { set; get; }
        public int Sort { set; get; }
        [JsonIgnore]
        public string JsonSupportChannels { set; get; }
        public IEnumerable<BankCardManagementSupportChannelEnum> SupportChannels
            => JsonConvert.DeserializeObject<IEnumerable<BankCardManagementSupportChannelEnum>>(JsonSupportChannels ?? "[]");
        public IEnumerable<string> SupportChannelNames =>
            SupportChannels.Select(x => x.ToString());
        [JsonIgnore]
        public string JsonGroupIds { set; get; }
        public IEnumerable<int> GroupIds =>
            JsonConvert.DeserializeObject<IEnumerable<int>>(JsonGroupIds ?? "[]");
        public string QrCodeUrl { set; get; }
        public DateTimeOffset LastUpdateTime
        {
            get => _lastUpdateTime;
            set => _lastUpdateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string LastUpdater { set; get; }
    }
}