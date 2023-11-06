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
}
