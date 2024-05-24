using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;

namespace BackendSite.Service.DAL;

public class QuestionnaireService
{
    private readonly MySqlClient _mainDbClient;
    private readonly MySqlClient _reportDbClient;

    public QuestionnaireService(IEnumerable<MySqlClient> dbClient)
    {
        _mainDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Main);
        _reportDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Report);
    }

    public IEnumerable<QuestionnairList> GetList(QuestionnaireRequest req)
    {
        var p = new DynamicParameters();
        p.Add("_SiteId", req.SiteId);
        p.Add("_CurrencyId", req.CurrencyId);
        p.Add("_UserName", req.UserName);
        p.Add("_StartDate", req.StartDate.DateTime);
        p.Add("_EndDate", req.EndDate.DateTime);
        
        return _reportDbClient.Query<QuestionnairList>("Admin_Questionnair_Get",p);
    }
}