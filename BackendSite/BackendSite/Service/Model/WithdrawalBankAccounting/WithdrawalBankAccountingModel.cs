using BackendSite.Attributes;

namespace BackendSite.Service.Model.WithdrawalBankAccounting
{
    public class WithdrawalBankAccountingModel
    {
        [LogIgnore]
        public int Id
        {
            get;
            set;
        }

        [LogIgnore]
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

        [LogIgnore]
        public int Creator
        {
            get;
            set;
        }
    }
}
