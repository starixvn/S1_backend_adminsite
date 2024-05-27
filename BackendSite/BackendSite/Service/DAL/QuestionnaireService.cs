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

    public IEnumerable<QuestionnairList> GetList(int siteId, int currencyId, string userName, DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        var p = new DynamicParameters();
        p.Add("_SiteId", siteId);
        p.Add("_CurrencyId", currencyId);
        p.Add("_UserName", userName);
        p.Add("_StartDate", startDate.DateTime);
        p.Add("_EndDate", endDate.DateTime);

        return _reportDbClient.Query<QuestionnairList>("Admin_Questionnair_Get", p);
    }

    public int Set(int id, string feedback, string @operator)
    {
        var p = new DynamicParameters();
        p.Add("_Id", id);
        p.Add("_Feedback", feedback);
        p.Add("_Operator", @operator);

        return _mainDbClient.Execute("Admin_Questionnair_Set", p);
    }
}