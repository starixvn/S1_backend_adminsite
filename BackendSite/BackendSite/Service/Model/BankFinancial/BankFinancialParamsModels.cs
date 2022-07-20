using System.Collections.Generic;
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
            StrPurposes = JsonConvert.SerializeObject(req.Purposes);
            StrSysIds = JsonConvert.SerializeObject(req.SysIds);
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
}