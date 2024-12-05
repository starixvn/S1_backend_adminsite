using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class BankInfoOff
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int BankId { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public int AccountStatus { get; set; }
        public int SysId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int PointLevel { get; set; }
        public int Purpose { get; set; }
        public string Memo { get; set; }
        public int BSort { get; set; }
        public int CurrencyId { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string BankStatus { get; set; }
        public string BankLogo { get; set; }
        public string BankSite { get; set; }
        public int Sort { get; set; }
        public int Ver { get; set; }
    }

    public class OfflineBankAccount
    {
        public int BankId { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public TransferChannelEnum Channel { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public string AccountCode { get; set; }
        public string DisplayName { get; set; }
        public TransferAccountStatusEnum AccountStatus { get; set; }
        public BankCardManagementStatusEnum SettingStatus { get; set; }

        public string Purpose { get; set; }

        public IEnumerable<TransferAccountPurposeEnum> Purposes =>
            JsonConvert.DeserializeObject<IEnumerable<TransferAccountPurposeEnum>>(Purpose ?? "[]");

        public string JsonPointLevels { set; get; }
        public IEnumerable<int> PointLevels =>
            JsonConvert.DeserializeObject<IEnumerable<int>>(JsonPointLevels ?? "[]");
        public string JsonVipLevels { set; get; }
        public IEnumerable<int> VipLevels =>
            JsonConvert.DeserializeObject<IEnumerable<int>>(JsonVipLevels ?? "[]");

        public decimal MinAmount { set; get; }
        public decimal MaxAmount { set; get; }

        public int SysId { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string BankLoginUrl { get; set; }
        public string JsonBankLoginInfo { get; set; }
        public IEnumerable<BankLoginInfo> BankLoginInfos =>
            JsonConvert.DeserializeObject<IEnumerable<BankLoginInfo>>(JsonBankLoginInfo ?? "[]");
        //todo: remove
        public string BankLoginAccount { get; set; }
        public string BankLoginPassword { get; set; }
        public string TransferPin { get; set; }
        public string LoginPin { get; set; }
        public string MobilePin { get; set; }
        public string CorporateID { get; set; }
    }

    public class BankLoginInfo
    {
        
        public string Account { get; set; }
        public string Password { get; set; }
    }

    public class BankCodeModel
    {
        public int SysId { get; set; }
        public int CurrencyId { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
    }

    public class CalCodeModel
    {
        public int BankId { set; get; }
        public string CalCodeName { set; get; }
        public int CurrencyId { set; get; }
        public int SysId { set; get; }
    }

    public class BankCodeModelComparer : IEqualityComparer<BankCodeModel>
    {
        public bool Equals(BankCodeModel t1, BankCodeModel t2)
        {
            return (t1.SysId == t2.SysId);
        }

        public int GetHashCode(BankCodeModel t)
        {
            return t.ToString().GetHashCode();
        }
    }
}
