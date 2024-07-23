using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Dapper;
using Org.BouncyCastle.Ocsp;

namespace BackendSite.Service.DAL;

public class TaskSettingService
{
    private readonly MySqlClient _mainDbClient;
    private readonly MySqlClient _reportDbClient;

    public TaskSettingService(IServiceProvider serviceProvider)
    {
        _mainDbClient = serviceProvider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        _reportDbClient = serviceProvider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
    }
    public int Set(int custId, TaskSetting firstDepositTime,DateTimeOffset taskTime)
    {
        var p = new DynamicParameters(new
        {
            _CustId = custId,
            _TaskId = (int)firstDepositTime,
            _TaskTime = taskTime
        });

        return _mainDbClient.Execute("Service_CustTaskTime_Set", p);
    }
}