using BackendSite.Service.Model.ApiModel;

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

    public int Direction { get; set; }
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
    public int Category { set; get; }
    public string CategoryName { set; get; }
}

public class TpAdjustmentLogV2 : TpTransferInfo
{
    public string SiteName { set; get; }
    public string UserName { set; get; }
    public string Operator { set; get; }
    public string Remarks { set; get; }
    public int Category { set; get; }
    public string CategoryName { set; get; }

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

    /// <summary>
    /// 0:All
    /// 1: ToTp (�T����]�[��)
    /// 2:ToMain(�T����]����)
    /// </summary>
    public int Direction { get; set; }

    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}
