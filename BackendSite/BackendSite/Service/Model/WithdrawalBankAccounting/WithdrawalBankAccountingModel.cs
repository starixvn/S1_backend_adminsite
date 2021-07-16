using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public DateTime CreateTime
        {
            get;
            set;
        }

        public int Operator
        {
            get;
            set;
        }

        public DateTime OperateTime
        {
            get;
            set;
        }
    }
}
