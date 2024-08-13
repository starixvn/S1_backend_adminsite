namespace BackendSite.Service.Model
{

    public class CustIovationData
    {
        public string DeviceId { get; set; }
        public string Iovation { get; set; }
        public int Frequency { get; set; }
        public string DeviceType { get; set; }
        public int AssociateCount { get; set; }
        public int Score { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
    public class CustShieldRiskInfoData
    {
        public string DeviceId { get; set; }
        public int Frequency { get; set; }
        public string Platform { get; set; }
        public int AssociateCount { get; set; }
        public int DeviceScore { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
    
    public class CustFPSData
    {
        public string DeviceId { get; set; }
        public string RiskLevel { get; set; }
        public int Frequency { get; set; }
        public string DeviceType { get; set; }
        public int AssociateCount { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}