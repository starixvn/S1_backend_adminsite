namespace BackendSite.Service.Model.Mpa.Marketing
{
    public class WithdrawalAmountSummaryResponse
    {
        public decimal WithdrawalAmount { get; set; }

        public decimal CompareWithdrawalAmount { get; set; }

        public int WithdrawalUsers { get; set; }

        public int CompareWithdrawalUsers { get; set; }

        public string WithdrawalAmountChangeRate { get; set; }

        public string WithdrawalUsersChangeRate { get; set; }
    }

    public class WithdrawalAmountDetailData
    {
         public string UserName { get; set; }

        public string CurrencyName { get; set; }

        public string RegisteredDate { get; set; }

        public int WithdrawalCount { get; set; }

        public decimal WithdrawalAmount { get; set; }
    }
}
