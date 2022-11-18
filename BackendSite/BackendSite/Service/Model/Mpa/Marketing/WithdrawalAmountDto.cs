namespace BackendSite.Service.Model.Mpa.Marketing
{
    public class MpaMarketingWithdrawalAmountSummaryDto
    {
        public DateTime Date { get; set; }

        public int CustId { get; set; }

        public int CurrencyId { get; set; }

        public decimal WithdrawalAmount { get; set; }
    }
}
