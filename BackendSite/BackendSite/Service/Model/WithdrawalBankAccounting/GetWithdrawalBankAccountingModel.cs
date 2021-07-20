using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
