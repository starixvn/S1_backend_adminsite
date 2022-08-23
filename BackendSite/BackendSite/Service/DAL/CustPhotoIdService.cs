using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Dapper;

namespace BackendSite.Service.DAL;

public class CustPhotoIdService
{
    private readonly MySqlClient _mainDbClient;

    public CustPhotoIdService(IEnumerable<MySqlClient> dbClients)
    {
        _mainDbClient = dbClients.FirstOrDefault(a=>a.DataBase == DataBase.Main);
    }

    public async Task<long> Add(int custId, int siteId)
    {
        var p = new DynamicParameters();
        p.Add("_CustId", custId);
        p.Add("_SiteId", siteId);
        p.Add("SQ", dbType: DbType.Int64, direction: ParameterDirection.Output);

        var result = await _mainDbClient.ExecuteAsync("Admin_CustPhotoId_Ins_V2", p);
        if (result > 0)
        {
            var sq = p.Get<long>("SQ");
            if (sq > 0)
                return sq;
        }

        return 0;
    }
}