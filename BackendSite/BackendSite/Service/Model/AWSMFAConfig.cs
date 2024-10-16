namespace BackendSite.Service.Model
{
    public class AWSMFAConfig
    {
        public string SecretKey { get; set; }
        public string SerialNumber { get; set; }
        public int DurationSeconds { get; set; }
    }
}
