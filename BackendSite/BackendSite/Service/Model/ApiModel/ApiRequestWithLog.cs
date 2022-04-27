namespace BackendSite.Service.Model.ApiModel
{
    public class ApiRequestWithLog<T>
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string TransId { get; set; }
        public T Before { get; set; }
        public T After { get; set; }
    }
}