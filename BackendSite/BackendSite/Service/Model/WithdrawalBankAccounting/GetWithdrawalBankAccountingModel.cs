namespace BackendSite.Service.Model.WithdrawalBankAccounting
{
    public class GetWithdrawalBankAccountingModel : WithdrawalBankAccountingModel
    {
        public string BankCode
        {
            get; set;
        }

        public string BankAccountName
        {
            get; set;
        }

        public string CreatorName
        {
            get; set;
        }

        public string OperatorName
        {
            get; set;
        }
    }
}
