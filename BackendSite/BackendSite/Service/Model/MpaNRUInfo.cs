namespace BackendSite.Service.Model
{
    public class MpaDomainConfig
    {
        public Dictionary<string, GroupUrl> GroupUrl { get; set; }
    }
    public class GroupUrl
    {
        public IEnumerable<string> Domain { get; set; }
    }
    
    public class NRUReportData
    {
        public SummaryData SummaryData { get; set; }
        public List<SignUpDataSum> SignUpDataSum { get; set; }
        public SignUpData TotalSignUpData { get; set; }
    }
    public class SignUpDataSum
    {
        public List<SignUpData> Childs { get; set; }
        public string SignUpSource { get; set; }
        public int NewlyRegisterUsers{ get; set; }
        public int DepositUsers { get; set; }
        public decimal FirstDepositAmount { get; set; }
        public decimal TotalDepositAmount { get; set; }
    }
    
    public class SignUpData
    {
        public string SignUpSource { get; set; }
        public int NewlyRegisterUsers{ get; set; }
        public int DepositUsers { get; set; }
        public decimal FirstDepositAmount { get; set; }
        public decimal TotalDepositAmount { get; set; }
    }
    
    public class MpaMarketingNRUInfo
    {
        public string CustId { get; set; }
        public int AgentId{ get; set; }
        public string SignupUrl { get; set; }
        public string AffCode { get; set; }
        public decimal FirstDeposit { get; set; }
        public decimal TotalDeposit { get; set; }
    }

    public class SummaryData
    {
        public int NewlyRegisterUsers{ get; set; }
        public int CompareNewlyRegisterUsers{ get; set; }
        public decimal ChangeRate { get; set; }
    }

    public class MpaMarketingNRUBonus
    {
        public string BonusCode { get; set; }
        public string BonusName { get; set; }
        public int NRUCount{ get; set; }
        public int DepositCount{ get; set; }
        public decimal FirstDeposit { get; set; }
        public decimal TotalDeposit { get; set; }
    }
    
    public class MpaMarketingNRUBonusDetail
    {
        public string BonusCode { get; set; }
        public string UserName { get; set; }
        public string Currency { get; set; }
        public DateTime RegTime { get; set; }
        public string SignupUrl{ get; set; }
        public int AffCode{ get; set; }
        public bool IsDeposit{ get; set; }
        public decimal FirstDeposit { get; set; }
        public decimal TotalDeposit { get; set; }
    }
    
}