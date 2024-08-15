using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model;

public class IovationReportGetListRequest : ApiRequestPageModel
{
    public int CurrencyId { get; set; }
    public IovationDeviceType DeviceType { get; set; }
    public string UserName { get; set; }
    public string DeviceId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}

public class IovationReportPageResult<T>
{
    public IEnumerable<T> IovationReport { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int RowCount { get; set; }
}

public class IovationReportData
{
    public DateTime UpdateTime { get; set; }
    public string IsoCode { get; set; }
    public string UserName { get; set; }
    public string DeviceId { get; set; }
    public string Iovation { get; set; }
    public string DeviceType { get; set; }
    public int Score { get; set; }
    public int Frequency { get; set; }
    public int AssociateCount { get; set; }
}

public class IovationReport
{
    public DateTimeOffset LastUsedTime { get; set; }
    public string IsoCode { get; set; }
    public string UserName { get; set; }
    public string DeviceId { get; set; }
    public string Allow { get; set; }
    public string DeviceType { get; set; }
    public int Score { get; set; }
    public int Frequency { get; set; }
    public int AssociateCount { get; set; }
}

public class IovationReportExportRequest
{
    public int CurrencyId { get; set; }
    public IovationDeviceType DeviceType { get; set; }
    public string UserName { get; set; }
    public string DeviceId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}

public class GetAssociateAccountRequest
{
    public string DeviceId { get; set; }
}

public class GetAssociateDevicesRequest
{
    public int CustId { get; set; }
}

