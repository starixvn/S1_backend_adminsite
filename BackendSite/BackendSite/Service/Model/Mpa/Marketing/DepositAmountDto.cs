namespace BackendSite.Service.Model.Mpa.Marketing
{
    public interface IMpaMarketingDepositAmountBaseDto
    {
        public DateTime Date { get; set; }
        public decimal DepositAmount { get; set; }
        public int CurrencyId { get; set; }
    }

    public class MpaMarketingDepositAmountSummaryDto : IMpaMarketingDepositAmountBaseDto
    {
        public DateTime Date { get; set; }
        public decimal DepositAmount { get; set; }
        public int CurrencyId { get; set; }
    }
}