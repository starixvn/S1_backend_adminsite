using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.PaymentAgent;

public class PaymentAgentInfo
{
    public int AgentId { get; set; }
    public string AgentName { get; set; }
    public int SiteId { get; set; }
    public PaymentAgentTypeEnum Type { get; set; }
    public PaymentAgentStatusEnum Status { get; set; }
}

public class GetListRequest
{
    //public int SiteId { get; set; }
}
