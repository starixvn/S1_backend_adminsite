using System.Collections.Generic;

namespace BackendSite.Service.Model.WithdrawalBankAccounting
{
    public class GetWithdrawalInfoAndAccountingModel
    {
        public WithdrawalRequest Withdrawal { get; set; }

        public IEnumerable<GetWithdrawalBankAccountingModel> AccountingList { get; set; }

        public string IsoCode { get; set; }
    }
}
