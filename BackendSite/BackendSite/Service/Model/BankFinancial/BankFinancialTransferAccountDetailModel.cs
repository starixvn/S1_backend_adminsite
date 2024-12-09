using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.Model.BankFinancial
{
    public class BankFinancialTransferAccountDetailModel
    {
        private DateTimeOffset _initTime { set; get; }
        /// <summary>
        /// 讀寫 識別值ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 讀寫 網站ID
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// 讀寫 網站名稱
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// 讀寫 幣別Id
        /// </summary>
        public int CurrencyId { get; set; }

        /// <summary>
        /// 讀寫 幣別代號
        /// </summary>
        public string IsoCode { get; set; }

        /// <summary>
        /// 讀寫 銀行ID
        /// </summary>
        public int SysId { get; set; }

        /// <summary>
        /// 讀寫 銀行代號
        /// </summary>
        public string BankCode { get; set; }

        /// <summary>
        /// 讀寫 目的
        /// 1=Deposit,2=Withdrawal,3=Transfer
        /// </summary>
        public string JsonPurpose { get; set; }

        public IEnumerable<TransferAccountPurposeEnum> Purposes =>
            JsonConvert.DeserializeObject<IEnumerable<TransferAccountPurposeEnum>>(JsonPurpose ?? "[]");

        /// <summary>
        /// 讀寫 付款方式
        /// </summary>
        public TransferChannelEnum Channel { get; set; }

        /// <summary>
        /// 讀寫 代理人
        /// </summary>
        public string Agent { get; set; }

        /// <summary>
        /// 讀寫 第三方支付ID
        /// </summary>
        public int PayId { get; set; }

        /// <summary>
        /// 讀寫 第三方支付名稱
        /// </summary>
        public string PayName { get; set; }

        /// <summary>
        /// 讀寫 帳戶帳號
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// 讀寫 帳戶名稱
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 目前只 for INR IFSC. 使用
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// 讀寫 帳戶顯示名稱
        /// </summary>
        public string DisplayName { get; set; }

        public string FrontendDisplayName { get; set; }

        /// <summary>
        /// 讀寫 初始金額
        /// </summary>
        public decimal InitialAmount { get; set; }
        public DateTimeOffset InitialTime
        {
            get => _initTime;
            set => _initTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        /// <summary>
        /// 讀寫 超過餘額警示
        /// </summary>
        public decimal MaxBalanceAlertAmount { get; set; }

        /// <summary>
        /// 讀寫 原因
        /// </summary>
        public int Reason { get; set; }

        /// <summary>
        /// 讀寫 備註
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 讀寫 資料狀態
        /// </summary>
        public TransferAccountStatusEnum Status { get; set; }

        /// <summary>
        /// 讀寫 資料建立時間
        /// </summary>
        public DateTimeOffset CreateTime { get; set; }

        /// <summary>
        /// 讀寫 資料建立者
        /// </summary>
        public int Creator { get; set; }

        /// <summary>
        /// 讀寫 資料建立者名稱
        /// </summary>
        public string CreatorName { get; set; }

        /// <summary>
        /// 讀寫 資料最近更新時間
        /// </summary>
        public DateTimeOffset UpdateTime { get; set; }

        /// <summary>
        /// 讀寫 資料更新者
        /// </summary>
        public int Updator { get; set; }

        /// <summary>
        /// 讀寫 資料更新者名稱
        /// </summary>
        public string UpdatorName { get; set; }

        public string BankLoginUrl { get; set; }
        public string BankLoginAccount { get; set; }
        public string BankLoginPassword { get; set; }
        public string TransferPin { get; set; }
        public string LoginPin { get; set; }
        public string MobilePin { get; set; }
        public string CorporateID { get; set; }
        public CryptoCurrencyEnum? CryptoCurrencyId { get; set; }
        public string Network { get; set; }
    }

    public class BankFinancialTransferAccountDisplayModel
    {
        public BankFinancialTransferAccountDisplayModel(BankFinancialTransferAccountDetailModel data)
        {
            Id = data.Id;
            SiteId = data.SiteId;
            SiteName = data.SiteName;
            CurrencyId = data.CurrencyId;
            IsoCode = data.IsoCode;
            SysId = data.SysId;
            BankCode = data.BankCode;
            Purposes = data.Purposes;
            Channel = data.Channel;
            Agent = data.Agent;
            PayId = data.PayId;
            PayName = data.PayName;
            AccountNo = data.AccountNo;
            AccountName = data.AccountName;
            DisplayName = data.DisplayName;
            FrontendDisplayName = data.FrontendDisplayName;
            InitialAmount = data.InitialAmount;
            InitialTime = data.InitialTime;
            Status = data.Status;
            CreateTime = data.CreateTime;
            CreatorName = data.CreatorName;
            UpdateTime = data.UpdateTime;
            UpdatorName = data.UpdatorName;
            CryptoCurrency = data.CryptoCurrencyId?.ToString();
            CryptoNetwork = data.Network;
        }

        public int Id { get; set; }

        public int SiteId { get; set; }

        public string SiteName { get; set; }

        public int CurrencyId { get; set; }

        public string IsoCode { get; set; }

        public int SysId { get; set; }

        public string BankCode { get; set; }

        public IEnumerable<TransferAccountPurposeEnum> Purposes { get; set; }

        public TransferChannelEnum Channel { get; set; }

        public string Agent { get; set; }

        public int PayId { get; set; }

        public string CryptoCurrency { get; set; }

        public string CryptoNetwork { get; set; }

        public string PayName { get; set; }

        public string AccountNo { get; set; }

        public string AccountName { get; set; }

        public string DisplayName { get; set; }

        public string FrontendDisplayName { get; set; }

        public decimal InitialAmount { get; set; }
        public DateTimeOffset InitialTime { get; set; }

        public TransferAccountStatusEnum Status { get; set; }

        public DateTimeOffset CreateTime { get; set; }

        public string CreatorName { get; set; }

        public DateTimeOffset UpdateTime { get; set; }

        public string UpdatorName { get; set; }
    }

    public class BankFinancialTransferAccountExportModel
    {
        public int Id { get; set; }
        public string SiteName { get; set; }
        public string Currency { get; set; }
        public string BankCode { get; set; }
        public string Purpose { get; set; }
        public string Channel { get; set; }
        public string Agent { get; set; }
        public string PayName { get; set; }
        public string CryptoCurrency { get; set; }
        public string CryptoNetwork { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string DisplayName { get; set; }
        public string FrontendDisplayName { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal MaxBalanceAlertAmount { get; set; }
        public string Remark { get; set; }
        public string BankLoginUrl { get; set; }
        public string BankLoginAccount { get; set; }
        public string BankLoginPassword { get; set; }
        public string TransferPin { get; set; }
        public string LoginPin { get; set; }
        public string MobilePin { get; set; }
        public string CorporateID { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public string Creator { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string Updator { get; set; }
    }

    public class BankFinancialTransferAccount
    {
        public int FinancialId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public int SysId { get; set; }
        public string BankCode { get; set; }
        public TransferChannelEnum Channel { get; set; }
        public string Agent { get; set; }
        public int PayId { get; set; }
        public string PayName { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string DisplayName { get; set; }
        public TransferAccountStatusEnum Status { get; set; }
        public string JsonPurpose { get; set; }
        [JsonIgnore]
        public IEnumerable<TransferAccountPurposeEnum> Purposes =>
            JsonConvert.DeserializeObject<IEnumerable<TransferAccountPurposeEnum>>(JsonPurpose ?? "[]");
        
    }
}
