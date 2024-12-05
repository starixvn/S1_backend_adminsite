using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model;

public class PaymentAgent
{
    public int AgentId { get; set; }
    public string AgentName { get; set; }
    public int SiteId { get; set; }
    public string SiteName { get; set; }
    public PaymentAgentEnum Type { get; set; }
}