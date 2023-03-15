using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using MySql.Data.MySqlClient;

namespace BackendSite.Service.DAL;

public class PaymentChargeFeeService
{
    private readonly IConfiguration _configuration;
    private readonly MySqlClient _mainDbClient;
    private readonly MySqlClient _reportDbClient;

    public PaymentChargeFeeService(IServiceProvider provider)
    {
        _configuration = provider.GetRequiredService<IConfiguration>();
        _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
    }

    public IEnumerable<ModifyPaymentChargeFeeLog> GetModifyPaymentChargeFeeLogList(int siteId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int startIndex,
        int pageSize,
        out int totalPages,
        out int rowCount)
    {
        var parameters = new DynamicParameters(
            new
            {
                _SiteId = siteId,
                _StartTime = startTime.UtcDateTime,
                _EndTime = endTime.UtcDateTime,
                _PageSize = pageSize,
                _StartIndex = startIndex
            });
        
        parameters.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parameters.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var result = _reportDbClient.Query<ModifyPaymentChargeFeeLog>("Admin_ModifyPaymentChargeFeeLog_Get", parameters);
        
        totalPages = parameters.Get<int?>("_TotalPages") ?? 0;
        rowCount = parameters.Get<int?>("_RowCount") ?? 0;

        return result;
    }
    
    public int CreateModifyPaymentChargeFeeLog(ModifyPaymentChargeFeeParams input)
    {
        using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
        conn.Open();
        using var trans = conn.BeginTransaction();
        var parameters = new DynamicParameters(
            new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _ChannelId = input.ChannelId,
                _ChargeFeePercentage = input.ChargeFeePercentage,
                _ChargeFeePreCount = input.ChargeFeePreCount,
                _StartTime = input.StartTime.UtcDateTime,
                _EndTime = input.EndTime.UtcDateTime,
                _Operator = input.Operator
            });
        
        parameters.Add("_Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
        
        _ = _mainDbClient.Execute("Admin_ModifyPaymentChargeFeeLog_Ins", trans, parameters);

        var result = parameters.Get<int>("_Result");

        if (result == (int)ErrorCodeEnum.Success)
        {
            trans.Commit();
        }
        else
        {
            trans.Rollback();
        }

        return result;
    }
}