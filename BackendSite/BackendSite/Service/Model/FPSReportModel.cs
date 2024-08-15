using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class FPSReportGetListRequest : ApiRequestPageModel
    {
        public int CurrencyId { get; set; }
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
    }
    
    
    public class FPSReport
    {
        public DateTimeOffset LastUsedTime { get; set; }
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
    }
    
    public class FPSReportExportRequest
    {
        public int CurrencyId { get; set; }
        public IovationDeviceType DeviceType { get; set; }
        public string UserName { get; set; }
        public string DeviceId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}