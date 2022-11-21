using BackendSite.Extensions;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model;

public class GetCustTpAdjustmentLogRequest : ApiRequestPageModel
{
    public int CustId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}


public class GetCustTpAdjustmentLogParams
{
    public int SiteId { get; set; }
    public int TpId { get; set; }
    public int CustId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}

public class TpAdjustmentLog: TpTransferInfo
{
    public string SiteName { set; get; }
    public string UserName { set; get; }
    public string Operator { set; get; }
    public string Remarks { set; get; }
    public int Category { set; get; }
    public string CategoryName { set; get; }
}