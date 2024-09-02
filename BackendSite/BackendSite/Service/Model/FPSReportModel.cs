using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class FPSReportGetListRequest : ApiRequestPageModel
    {
        public IEnumerable<int> SiteIds { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public FpsDeviceType DeviceType { get; set; }
        public string UserName { get; set; }
        public string DeviceId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
    
    public class FPSReportPageResult<T>
    {
        public IEnumerable<T> FPSDataReport { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    
    public class FPSReportData
    {
        public DateTime UpdateTime { get; set; }
        public int SiteId { get; set; }
        public int CustId { get; set; }
        public string IsoCode { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string DeviceId { get; set; }
        public string RiskLevel { get; set; }
        public string Browser { get; set; }
        public string DeviceType { get; set; }
        public string IPAddress { get; set; }
        public string RobotTracking { get; set; }
        public int AssociateDevices { get; set; }
        public int AssociateCount { get; set; }
        public List<AssociatedDevice> AssociatedDevice { get; set; }
        public List<DeviceUser> DeviceUser { get; set; }
    }
    
    
    public class FPSReport
    {
        public DateTimeOffset LastUsedTime { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string IsoCode { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string DeviceId { get; set; }
        public string RiskLevel { get; set; }
        public string Browser { get; set; }
        public string DeviceType { get; set; }
        public string IPAddress { get; set; }
        public string RobotTracking { get; set; }
        public int AssociateDevices { get; set; }
        public int AssociateCount { get; set; }
        public List<DeviceUser> DeviceUser { get; set; }
        public List<AssociatedDevice> AssociatedDevice { get; set; }
    }
    
    public class FPSReportExportRequest
    {
        public IEnumerable<int> SiteIds { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public FpsDeviceType DeviceType { get; set; }
        public string UserName { get; set; }
        public string DeviceId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
    
    #region API
    public class FPSServicesConfig
    {
        public string ResourceURL { get; set; }
        public string Subscriber { get; set; }
        public string Token { get; set; }
        public string ApiUrl { get; set; }

    }

    public class FPSGetAssociationByAccountResponse
    {
        public int statusCode { get; set; }
        public string errorMessage { get; set; }
        public int associatedDevices { get; set; }
        public List<FPSAssociatedDeviceDetail> associatedDeviceDetails { get; set; }
        public int associatedAccounts { get; set; }
        public List<FPSAssociatedAccountDetail> associatedAccountDetails { get; set; }
    }
    
    public class FPSAssociatedAccountDetail
    {
        [JsonProperty("account")]
        public string UserName { get; set; }
        public DateTime firstAssociationDate { get; set; }
    }

    public class FPSAssociatedDeviceDetail
    {
        [JsonProperty("device")]
        public string DeviceId { get; set; }
        public DateTime firstSeenDate { get; set; }
    }
    #endregion
}