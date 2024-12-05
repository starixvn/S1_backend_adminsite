using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;

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
    
    public IEnumerable<PaymentAgent> GetPaymentAgentList(IEnumerable<int> siteIds, IEnumerable<int> types)
    {
        var p = new DynamicParameters(
            new
            {
                _SiteIds = JsonConvert.SerializeObject(siteIds),
                _Types = JsonConvert.SerializeObject(types)
            });

        return _reportDbClient.Query<PaymentAgent>("Admin_PaymentAgentList_Get", p);
    }
}