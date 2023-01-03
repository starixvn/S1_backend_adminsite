namespace BackendSite.Service.Model;

public class ModifyPaymentChargeFeeLog
{
   public int Id { get; set; }
   public int CurrencyId { get; set; }
   public string PayName { get; set; }
   public string ChannelName { get; set; }
   public decimal ChargeFeePercentage { get; set; }
   public decimal ChargeFeePreCount { get; set; }
   public DateTimeOffset StartTime { get; set; }
   public DateTimeOffset EndTime { get; set; }
   public DateTimeOffset CreateTime { get; set; }
   public string Operator { get; set; }
}

public class ModifyPaymentChargeFeeParams
{
   public int SiteId { get; set; }
   public int CurrencyId { get; set; }
   public int ChannelId { get; set; }
   public decimal ChargeFeePercentage { get; set; }
   public decimal ChargeFeePreCount { get; set; }
   public DateTimeOffset StartTime { get; set; }
   public DateTimeOffset EndTime { get; set; }
   public string Operator { get; set; }
}