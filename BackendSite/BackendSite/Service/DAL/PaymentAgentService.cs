using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.PaymentAgent;

namespace BackendSite.Service.DAL;

public class PaymentAgentService
{
    private readonly MySqlClient _mainDbClient;
    private readonly MySqlClient _reportDbClient;
    private readonly ILogger<PaymentAgentService> _logger;

    public PaymentAgentService(IServiceProvider provider)
    {
        _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        _logger = provider.GetRequiredService<ILogger<PaymentAgentService>>();
    }
    
    public IEnumerable<PaymentAgentInfo> GetPaymentAgentList(int agentId = 0)
    {
        return _reportDbClient.Query<PaymentAgentInfo>("Admin_PaymentAgentList_Get"
            , new { _AgentId = agentId });
    }
    
    public bool SetPaymentAgentStatus(int agentId, PaymentAgentStatusEnum status)
    {
        return _mainDbClient
            .Execute("Admin_PaymentAgentList_SetStatus", new
            {
                _AgentId = agentId,
                _Status = (int)status
            }) > 0;
    }
    
    public bool SetPaymentAgent(SetParams input)
    {
        return _mainDbClient
            .Execute("Admin_PaymentAgentList_SetStatus", new
            {
                _AgentId = input.AgentId,
                _AgentName = input.AgentName,
                _SiteIds = input.SiteIds,
                _Status = (int)input.Status,
                _Type = (int)input.Type,
            }) > 0;
    }
    
    
}