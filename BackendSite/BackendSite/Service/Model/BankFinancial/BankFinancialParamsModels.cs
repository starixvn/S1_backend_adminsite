using Newtonsoft.Json;

namespace BackendSite.Service.Model.BankFinancial
{
    public class GetTransferAccountInfoListParams
    {
        public GetTransferAccountInfoListParams(int id)
        {
            IsSetting = true;
            StrCurrencyIds = "[]";
            StrChannels = "[]";
            StrStatus = "[]";
            StrPurposes = "[]";
            StrSysIds = "[]";
            Agent = string.Empty;
            AccountNo = string.Empty;
            Id = id;
        }
        public GetTransferAccountInfoListParams(GetTransferAccountInfoListRequest req)
        {
            IsSetting = false;
            SiteId = req.SiteId;
            StrCurrencyIds = JsonConvert.SerializeObject(req.CurrencyIds);
            StrChannels = JsonConvert.SerializeObject(req.Channels);
            StrStatus = JsonConvert.SerializeObject(req.Status);
            StrPurposes = req.Purposes.Any() ? JsonConvert.SerializeObject(req.Purposes) : "[]";
            StrSysIds = req.SysIds.Any() ? JsonConvert.SerializeObject(req.SysIds) : "[]";
            Agent = req.Agent ?? string.Empty;
            AccountNo = req.AccountNo ?? string.Empty;
        }

        public bool IsSetting { get; set; }
        public int SiteId { get; set; }
        public string StrCurrencyIds { get; set; }
        public string StrChannels { get; set; }
        public string Agent { get; set; }
        public string StrStatus { get; set; }
        public string StrPurposes { get; set; }
        public string StrSysIds { get; set; }
        public string AccountNo { get; set; }
        public int Id { get; set; }
    }

    public class GetTransferInfoListParams
    {
        public GetTransferInfoListParams(GetTransferInfoListRequest req)
        {
            SiteId = req.SiteId;
            StartTime = req.StartTime;
            EndTime = req.EndTime;
            FromCurrencyId = req.FromCurrencyId;
            ToCurrencyId = req.ToCurrencyId;
            StrTransferTypes = JsonConvert.SerializeObject(req.TransferTypes);
            FromAccountNo = string.IsNullOrWhiteSpace(req.FromAccountNo) ? null : req.FromAccountNo;
            ToAccountNo = string.IsNullOrWhiteSpace(req.ToAccountNo) ? null : req.ToAccountNo;
            FromChannel = req.FromChannel;
            ToChannel = req.ToChannel;
            FromAgent = string.IsNullOrWhiteSpace(req.FromAgent) ? null : req.FromAgent;
            ToAgent = string.IsNullOrWhiteSpace(req.ToAgent) ? null : req.ToAgent;
            Reason = req.Reason;
        }

        public GetTransferInfoListParams(ExportTransferInfoListRequest req)
        {
            IsExport = true;
            SiteId = req.SiteId;
            StartTime = req.StartTime;
            EndTime = req.EndTime;
            FromCurrencyId = req.FromCurrencyId;
            ToCurrencyId = req.ToCurrencyId;
            StrTransferTypes = JsonConvert.SerializeObject(req.TransferTypes);
            FromAccountNo = string.IsNullOrWhiteSpace(req.FromAccountNo) ? null : req.FromAccountNo;
            ToAccountNo = string.IsNullOrWhiteSpace(req.ToAccountNo) ? null : req.ToAccountNo;
            FromChannel = req.FromChannel;
            ToChannel = req.ToChannel;
            FromAgent = string.IsNullOrWhiteSpace(req.FromAgent) ? null : req.FromAgent;
            ToAgent = string.IsNullOrWhiteSpace(req.ToAgent) ? null : req.ToAgent;
            Reason = req.Reason;
        }

        public bool IsExport { get; set; }
        public int SiteId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int? FromCurrencyId { get; set; }
        public int? ToCurrencyId { get; set; }
        public string StrTransferTypes { get; set; }
        public string FromAccountNo { get; set; }
        public string ToAccountNo { get; set; }
        public int? FromChannel { get; set; }
        public int? ToChannel { get; set; }
        public int? Reason { get; set; }
        public string FromAgent { get; set; }
        public string ToAgent { get; set; }
        public string TransferId { get; set; }
    }
}
