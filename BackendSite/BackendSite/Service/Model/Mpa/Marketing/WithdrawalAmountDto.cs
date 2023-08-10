namespace BackendSite.Service.Model.Mpa.Marketing
{
    public interface IMpaMarketingWithdrawalAmountBaseDto
    {
        public DateTime Date { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public int CurrencyId { get; set; }
    }

    public class MpaMarketingWithdrawalAmountSummaryDto : IMpaMarketingWithdrawalAmountBaseDto
    {
        public DateTime Date { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
    }


    public class MpaMarketingWithdrawalAmountDetailDto : IMpaMarketingWithdrawalAmountBaseDto
    {
        public DateTime Date { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public DateTime RegisteredDate { get; set; }
        public int WithdrawalCount { get; set; }
    }
}
