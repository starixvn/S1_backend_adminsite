namespace BackendSite.Service.Model;

public class BettingMonitorModel
{
    private DateTimeOffset _lastUpdateTime { get; set; }
    public int SiteId { get; set; }
    public int CurrencyId { get; set; }
    public string Currency { get; set; }
    public int TpId { get; set; }
    public string TpName { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public int GameType { get; set; }
    public string GameTypeName { get; set; }
    public int Frequency { get; set; }
    public int Status { get; set; }
    public string LastUpdateUser { get; set; }
    public DateTimeOffset LastUpdateTime
    {
        get => _lastUpdateTime;
        set => _lastUpdateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
    }
}