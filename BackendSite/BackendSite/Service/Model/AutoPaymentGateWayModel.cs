namespace BackendSite.Service.Model
{
    public class BaseRequest
    {
        public string Token { get; set; }
    }
    
    public class AutoPaymentRequest : BaseRequest
    {
        public string TransId { get; set; }
        public string Comment { get; set; }
    }
}

