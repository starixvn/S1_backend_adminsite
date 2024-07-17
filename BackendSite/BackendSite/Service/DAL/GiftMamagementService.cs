using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Store.GiftManagement;
using Dapper;

namespace BackendSite.Service.DAL;

public class GiftMamagementService
{
    private readonly MySqlClient _mainDbClient;
    private readonly MySqlClient _reportDbClient;

    public GiftMamagementService(IServiceProvider serviceProvider)
    {
        _mainDbClient = serviceProvider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        _reportDbClient = serviceProvider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
    }



    public int SetWorkerPeriod(CreateGiftRequest req)
    {
        var p = new DynamicParameters(new
        {
            _SiteId = req.SiteId,
            _CurrencyId = req.CurrencyId,
            _WorkerName = "CreatePPBonusCode"
        });
        return _mainDbClient.Execute("Service_FreeSpinWorkerPeriod_Set", p);
    }

    public IEnumerable<ProvideGameListDto> GetGameName()
    {
        return _reportDbClient.Query<ProvideGameListDto>("Admin_FreeSpinProviderList_Get", null);
    }
}