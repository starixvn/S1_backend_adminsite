using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model.PaymentAgent;

public class PaymentAgentInfo
{
    public int AgentId { get; set; }
    public string AgentName { get; set; }
    
    [JsonIgnore]
    public string JsonSiteIds { get; set; }
    public IEnumerable<int> SiteIds => JsonConvert.DeserializeObject<IEnumerable<int>>(JsonSiteIds);
    public PaymentAgentTypeEnum Type { get; set; }
    public PaymentAgentStatusEnum Status { get; set; }
}

public class GetListRequest
{
    public IEnumerable<PaymentAgentTypeEnum> Types { get; set; }
    public IEnumerable<PaymentAgentStatusEnum> Status { get; set; }
}
