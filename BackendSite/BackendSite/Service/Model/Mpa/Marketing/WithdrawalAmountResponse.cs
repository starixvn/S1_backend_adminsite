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
}
