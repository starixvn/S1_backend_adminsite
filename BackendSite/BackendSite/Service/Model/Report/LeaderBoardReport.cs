using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model.Report;

public class GetLeaderboardReportRequest : IApiRequest
{
    private DateTimeOffset _startDate;
    private DateTimeOffset _endDate;
    public int SiteId { get; set; }
    public int CurrencyId { get; set; }
    public int CategoryId { get; set; }
    public DateTimeOffset StartDate
    {
        get => _startDate;
        set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
    }
    public DateTimeOffset EndDate
    {
        get => _endDate;
        set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
    }
    public IEnumerable<int> SiteIds { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }
}

public class LeaderboardReportModel
{
    public DateTime Date { get; set; }
    public int SiteId { get; set; }
    public string UserName { get; set; }
    public int CurrencyId { get; set; }
    public string IsoCode { get; set; }
    public int CategoryId { get; set; }
    public int CustId { get; set; }
    public decimal TurnOver { get; set; }
    public decimal Points { get; set; }
    public string SiteName { get; set; }
}
public class LeaderboardReportResponse
{
    public List<GetLeaderboardReportResponse> detail { get; set; }
    public int SiteId { get; set; }
}
public class GetLeaderboardReportResponse
{
    public string Ranking { get; set; }
    public int SiteId { get; set; }
    public string SiteName { get; set; }
    public int CustId { get; set; }
    public string UserName { get; set; }
    public int CurrencyId { get; set; }
    public string IsoCode { get; set; }
    public decimal TurnOver { get; set; }
    public decimal Points { get; set; }
    
}

public class LeaderboardExtraPointReq
{
    public int SiteId { get; set; }
    public IEnumerable<int> CategoryIds { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class SetLeaderboardExtraPointReq
{
    public DateTime ActiveDate { get; set; }
    public int SiteId { get; set; }
    public int CategoryId { get; set; }
    public string UserName { get; set; }
    public decimal ExtraPoint { get; set; }
}

public class SetBatchLeaderboardExtraPointReq
{
    public int SiteId { get; set; }
    public IFormFile excelUpload { get; set; }
}

public class SetLeaderboardExtraPointModel
{
    public int _SiteId { get; set; }
    public DateTime _ActiveDate { get; set; }
    public int _CategoryId { get; set; }
    public string _UserName { get; set; }
    public decimal _ExtraPoint { get; set; }
    public string _LastUpdateUser { get; set; }
    public int RTN { get; set; }
}

public class LeaderboardExtraPointList
{
    private DateTimeOffset _createTime;
    private DateTimeOffset _updateTime;
    public DateTime ActiveDate { get; set; }
    public int CustId { get; set; }
    public string UserName { get; set; }
    public string Currency { get; set; }
    public string CategoryName { get; set; }
    public decimal ExtraPoint { get; set; }
    public DateTimeOffset CreateTime
    {
        get => _createTime;
        set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
    }
    public DateTimeOffset UpdateTime
    {
        get => _updateTime;
        set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
    }
    public string LastUpdateUser { get; set; }
}

public class ExcelData
{
    public DateTime ActiveDate { get; set; }
    public int CategoryId { get; set; }
    public string UserName { get; set; }
    public decimal ExtraPoint { get; set; }
}