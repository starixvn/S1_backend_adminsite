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


public class GetCustTpAdjustmentLogParamsV2
{
    public int SiteId { get; set; }
    public int TpId { get; set; }
    public int CustId { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public int StartIndex { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}


public class TpAdjustmentLog: TpTransferInfo
{
    public string SiteName { set; get; }
    public string UserName { set; get; }
    public string Operator { set; get; }
    public string Remarks { set; get; }
    public AdjustmentCategoryEnum Category { set; get; }
    public string CategoryName => Category.GetDescription();
}

public class TpAdjustmentLogV2 : TpTransferInfo
{
    public string SiteName { set; get; }
    public string UserName { set; get; }
    public string Operator { set; get; }
    public string Remarks { set; get; }
    public AdjustmentCategoryEnum Category { set; get; }
    public string CategoryName => Category.GetDescription();

    public string DepositId { get; set; }
}



public class TpAdjustmentLogWrapper 
{
    public IEnumerable<TpAdjustmentLogV2> Data { get; set; }
    public int TotalPages { get; set;}
    public int RowCount { get; set; }
}

public class GetCustTpAdjustmentLogRequestV2 : ApiRequestPageModel
{
    public int CustId { get; set; }
    public int TpId { get; set; }
    public int SiteId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}