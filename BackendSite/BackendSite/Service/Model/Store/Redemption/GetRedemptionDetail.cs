using Newtonsoft.Json;

namespace BackendSite.Service.Model.Store.Redemption;

public class GetRedemptionDetail
{
    public int SiteId { get; set; }
    public int CurrencyId { get; set; }
    public string UserName { get; set; }
    public decimal PointBalance { get; set; }
    public int RedemptionId { get; set; }
    public string TransId { get; set; }
    public string ItemId { get; set; }
    public string ItemName { get; set; }
    public int RedeemPoint { get; set; }
    public decimal PriceWorth { get; set; }
    public decimal ActualCost { get; set; }
    public int Status { get; set; }
    [JsonIgnore]
    private string ReceiptUrlString { get; set; }
    public IEnumerable<string> ReceiptUrls { get; set; }
    public string AreaCode { get; set; }
    public string PhoneNo { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string PostCode { get; set; }
    public string LineFirst { get; set; }
    public string LineSecond { get; set; }
    public string AccountName { get; set; }
}
