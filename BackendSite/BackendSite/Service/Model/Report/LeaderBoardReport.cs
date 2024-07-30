using BackendSite.Service.Model.ApiModel;
using Newtonsoft.Json;

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
        set => _startDate = new DateTimeOffset(value.DateTime, new TimeSpan(8, 0, 0));
    }
    public DateTimeOffset EndDate
    {
        get => _endDate;
        set => _endDate = new DateTimeOffset(value.DateTime, new TimeSpan(8, 0, 0));
    }
    public IEnumerable<int> SiteIds { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }
}

public class Leaderboard
{
    public int SiteId { get; set; }
    public string SiteName { get; set; }
    public int CurrencyId { get; set; }
    public string IsoCode { get; set; }
    public int CustId { get; set; }
    public string UserName { get; set; }
    public decimal WTurnover { get; set; }
    public int Status { get; set; }
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
    public string SiteName { get; set; }
    public decimal SportTurnOver { get; set; }
    public decimal SlotTurnOver { get; set; }
    public decimal LiveCasinoTurnOver { get; set; }
    public int Status { get; set; }
    public int Points { get; set; }
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
    public decimal Reward { get; set; }
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

public class LeaderboardRankingBonusReq
{
    public IEnumerable<int> SiteIds { get; set; }
}

public class LeaderboardRankingBonusData
{
    public int SiteId { get; set; }
    public string SiteName { get; set; }
    public string Currency { get; set; }
    public int CurrencyId { get; set; }
    public string Ranking { get; set; }
    public decimal MinPoint { get; set; }
    public decimal Bonus { get; set; }
    public int UserCount { get; set; }
}

public class LeaderboardRankingBonusList
{
    public int SiteId { get; set; }
    public string SiteName { get; set; }
    public string Currency { get; set; }
    public int CurrencyId { get; set; }
    public IEnumerable<LeaderboardRankingBonusDetail> Data { get; set; }
}

public class LeaderboardRankingBonusDetail
{
    public string Ranking { get; set; }
    public string RankingName { get; set; }
    public decimal MinPoint { get; set; }
    public decimal Bonus { get; set; }
}

public class SetLeaderboardRankingBonusReq
{
    public string Ranking { get; set; }
    public decimal MinPoint { get; set; }
    public decimal Bonus { get; set; }
}

public class SetLeaderboardPointCalculationReq
{
    public decimal SportTurnOver { get; set; }
    public decimal SlotTurnOver { get; set; }
    public decimal LiveCasinoTurnOver { get; set; }
    public int Status { get; set; }
}

public class LeaderboardPointCalculation
{
    public int SiteId { get; set; }
    public string SiteName { get; set; }
    public string Currency { get; set; }
    public int CurrencyId { get; set; }
    public decimal SportTurnOver { get; set; }
    public decimal SlotTurnOver { get; set; }
    public decimal LiveCasinoTurnOver { get; set; }
    public int Status { get; set; }
    public string Updater { get; set; }
    public DateTimeOffset UpdateTime { get; set; }
}

public class LeaderboardTagExcludeList
{
    public int SiteId { get; set; }
    public string TagExcludeList { get; set; }
    public IEnumerable<int> TagsExclude
    {
        get
        {
            if (TagExcludeList?.Length > 0)
            {
                return JsonConvert.DeserializeObject<IEnumerable<int>>(TagExcludeList);
            }
            return null;
        }
    }
    public string Updater { get; set; }
    public DateTimeOffset UpdateTime { get; set; }
}

public class LeaderboardTagExcludeReq
{
    public int SiteId { get; set; }
}

public class SetLeaderboardTagExcludeReq
{
    public int SiteId { get; set; }
    public List<int> TagExcludeList { get; set; }
}