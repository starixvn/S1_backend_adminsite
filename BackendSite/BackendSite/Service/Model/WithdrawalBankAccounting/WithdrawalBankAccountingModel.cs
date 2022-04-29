namespace BackendSite.Service.Model.WithdrawalBankAccounting
{
    public class WithdrawalBankAccountingModel
    {
        public int Id
        {
            get;
            set;
        }

        public string TransId
        {
            get;
            set;
        }

        public int Seq
        {
            get;
            set;
        }

        public string BankAccountNo
        {
            get;
            set;
        }

        public decimal Amount
        {
            get;
            set;
        }

        public decimal ChargeFee
        {
            get;
            set;
        }

        public int UploadPicTimes
        {
            get;
            set;
        }
        public string Memo
        {
            get;
            set;
        }

        public int Creator
        {
            get;
            set;
        }
    }
    
    public class SetWithdrawalAccounting
    {
        public int Id { get; set; }

        public string TransId { get; set; }

        public int Seq { get; set; }

        public string BankAccountNo { get; set; }

        public decimal Amount { get; set; }

        public decimal ChargeFee { get; set; }

        public string Memo { get; set; }


        public int UploadPicTimes { get; set; }
    }
    
    public class DeleteWithdrawalAccounting
    {
        public int Id { get; set; }
    }
}
