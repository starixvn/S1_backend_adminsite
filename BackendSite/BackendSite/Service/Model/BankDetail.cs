using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    
    
    public class BankInfoSys
    {
        public int SysId { get; set; }
        public CurrencyEnum CurrencyId { get; set; }
        public string IsoCode => CurrencyId.ToString();
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public GeneralStatusEnum Status { get; set; }
    }
    
    public class SetBankInfoSysModel
    {
        public int SysId { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public GeneralStatusEnum Status { get; set; }
    }

    public class BankDetail
    {
        public int BankId { set; get; }
        public string AccountName { set; get; }
        public string AccountNo { set; get; }
        public string IFSCCode { set; get; }
        public decimal MinAmount { set; get; }
        public decimal MaxAmount { set; get; }
        public decimal DailyLimitAmount { set; get; }
        public int Purpose { set; get; }
        public decimal Dummy { set; get; }
        public string Memo { set; get; }
        public int BSort { set; get; }
        public int CurrencyId { set; get; }
        public string BankCode { set; get; }
        public string BankName { set; get; }
        public string BankLogo { set; get; }
        public string BankSite { set; get; }
        public int Sort { set; get; }
        public int Status { set; get; }
        public int PointLevel { set; get; }
        public virtual int SysId { set; get; }
    }
    public class BankListPara
    {
        public string SabaId { get; set; }
        public string UserName { get; set; }
        public int SiteId { get; set; }
    }

}
