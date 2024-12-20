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
    [JsonIgnore]
    public string JsonCurrencyIds { get; set; }
    public IEnumerable<int> CurrencyIds => JsonConvert.DeserializeObject<IEnumerable<int>>(JsonCurrencyIds);
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
    public IEnumerable<int> CurrencyIds { get; set; }
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
        CurrencyIds = JsonConvert.SerializeObject(req.CurrencyIds);
    }
    
    public int AgentId { get; set; }
    public string AgentName { get; set; }
    public string SiteIds { get; set; }
    public string CurrencyIds { get; set; }
    public PaymentAgentTypeEnum Type { get; set; }
    public PaymentAgentStatusEnum Status { get; set; }
}

public class SetUserRequest
{
    public int AgentId { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public IEnumerable<int> SiteIds { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public IEnumerable<int> GroupIds { get; set; }
    public int UserId { get; set; }
    
    public PaymentAgentStatusEnum Status { get; set; }
}

public class GetUserListRequest
{
    public int AgentId { get; set; }
}

public class GetUserRequest
{
    public int AgentId { get; set; }
    public int UserId { get; set; }
}