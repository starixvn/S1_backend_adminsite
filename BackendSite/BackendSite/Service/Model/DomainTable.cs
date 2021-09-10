namespace BackendSite.Service.Model
{
    public class DomainTable
    {
        public int DId { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public string Domain { get; set; }
        public int Sort { get; set; }
        public int Status { get; set; }
        public string LastUpdateUser { get; set; }
    }
}
