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

public class SetStatusRequest
{
    public int AgentId { get; set; }
    public PaymentAgentStatusEnum Status { get; set; }
}

public class SetRequest
{
    public int AgentId { get; set; }
    public string AgentName { get; set; }
    public IEnumerable<int> SiteIds { get; set; }
    public PaymentAgentTypeEnum Type { get; set; }
    public PaymentAgentStatusEnum Status { get; set; }
}

public class SetParams
{
    public SetParams(SetRequest req)
    {
        AgentId = req.AgentId;
        AgentName = req.AgentName;
        Type = req.Type;
        Status = req.Status;
        SiteIds = JsonConvert.SerializeObject(req.SiteIds);
    }
    
    public int AgentId { get; set; }
    public string AgentName { get; set; }
    public string SiteIds { get; set; }
    public PaymentAgentTypeEnum Type { get; set; }
    public PaymentAgentStatusEnum Status { get; set; }
}

public class SetUserRequest
{
    public int AgentId { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public PaymentAgentStatusEnum Status { get; set; }
}