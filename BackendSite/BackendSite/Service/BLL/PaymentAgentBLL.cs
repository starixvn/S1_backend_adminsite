using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.PaymentAgent;

namespace BackendSite.Service.BLL;

public class PaymentAgentBLL
{
    private readonly PaymentAgentService _paymentAgentService;
    private readonly ILogger<PaymentAgentBLL> _logger;

    public PaymentAgentBLL(IServiceProvider provider)
    {
        _paymentAgentService = provider.GetRequiredService<PaymentAgentService>();
    }

    public IEnumerable<PaymentAgentInfo> GetPaymentAgents()//IEnumerable<int> siteIds, IEnumerable<PaymentAgentTypeEnum> types)
    {
        var result = _paymentAgentService.GetPaymentAgentList();
            //.Where(x => siteIds.Contains(x.SiteId) && types.Contains(x.Type));

        return result;
    }
}