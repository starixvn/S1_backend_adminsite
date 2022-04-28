using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class SetBankCard
    {
        public int BankId { get; set; }
        // Log記錄用
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNo { get; set; }
        public string IFSCCode { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal DailyLimitAmount { get; set; }
        public int PointLevel { get; set; }
        public int Purpose { get; set; }
        public BankDetailOffStatusEnum Status { get; set; }
        public int Sort { get; set; }
    }
    
    public class DeleteBankCard
    {
        public int BankId { get; set; }
        // Log記錄用
        public string BankName { get; set; }
        public string BankAccountNo { get; set; }
        
    }
}