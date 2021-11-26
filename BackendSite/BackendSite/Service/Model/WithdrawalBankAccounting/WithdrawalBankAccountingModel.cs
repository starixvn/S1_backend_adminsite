using System;

namespace BackendSite.Service.Model.WithdrawalBankAccounting
{
    public class WithdrawalBankAccountingModel
    {
        private DateTimeOffset _createTime { get; set; }
        private DateTimeOffset _operateTime { get; set; }

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

        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        public int Operator
        {
            get;
            set;
        }

        public DateTimeOffset OperateTime
        {
            get => _operateTime;
            set => _operateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
}
