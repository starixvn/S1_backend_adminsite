namespace BackendSite.Service.Model.Mpa.Marketing
{
    public class WithdrawalAmountSummaryResponse
    {
        public decimal WithdrawalAmount { get; set; }

        public decimal CompareWithdrawalAmount { get; set; }

        public int WithdrawalUsers { get; set; }

        public int CompareWithdrawalUsers { get; set; }

        public int WithdrawalAmountChangeRate { get; set; }

        public int WithdrawalUsersChangeRate { get; set; }
    }
}
