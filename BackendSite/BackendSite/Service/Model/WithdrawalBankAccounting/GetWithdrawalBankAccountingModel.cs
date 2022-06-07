using System;
using BackendSite.Attributes;

namespace BackendSite.Service.Model.WithdrawalBankAccounting
{
    public class GetWithdrawalBankAccountingModel : WithdrawalBankAccountingModel
    {
        private DateTimeOffset _createTime { get; set; }
        private DateTimeOffset _operateTime { get; set; }
        
        [LogIgnore]
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
        
        [LogIgnore]
        public int Operator
        {
            get;
            set;
        }
        
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        public DateTimeOffset OperateTime
        {
            get => _operateTime;
            set => _operateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
}
