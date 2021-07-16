using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model.WithdrawalBankAccounting
{
    public class GetWithdrawalInfoAndAccountingModel
    {
        public WithdrawalRequest Withdrawal { get; set; }

        public IEnumerable<GetWithdrawalBankAccountingModel> AccountingList { get; set; }

        public string IsoCode { get; set; }
    }
}
