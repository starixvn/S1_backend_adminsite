using BackendSite.Attributes;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class SetBankCard
    {
        [LogIgnore]
        public int SysId { get; set; }
        // Log記錄用
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNo { get; set; }
        public string IFSCCode { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal DailyLimitAmount { get; set; }
        [LogPropertyName("VipLevel")]
        public int PointLevel { get; set; }
        [LogIgnore]
        public int Purpose { get; set; }
        public string PurposeDescription { get; set; }
        [LogIgnore]
        public BankDetailOffStatusEnum Status { get; set; }
        public string StatusName => Status.ToString();
        public int Sort { get; set; }
    }
    
    public class DeleteBankCard
    {
        [LogIgnore]
        public int BankId { get; set; }
        // Log記錄用
        public string BankName { get; set; }
        public string BankAccountNo { get; set; }
        
    }
}