using BackendSite.Service.DAL;

namespace BackendSite.Service.BLL;

public class PaymentAgentBLL
{
    private readonly PaymentAgentService _paymentAdjustmentService;
    private readonly ILogger<PaymentAgentBLL> _logger;

    public PaymentAgentBLL(IServiceProvider provider)
    {
        _paymentAdjustmentService = provider.GetRequiredService<PaymentAgentService>();
    }
    
    
}