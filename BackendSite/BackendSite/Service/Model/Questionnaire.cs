using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model;

public class QuestionnairList
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public int CustId { get; set; }

    public string Currency { get; set; }
    public string Contact { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreateTime { get; set; }
}
public class QuestionnaireRequest : ApiRequestPageModel
{
    private DateTimeOffset _startDate { get; set; }
    private DateTimeOffset _endDate { get; set; }
    public DateTimeOffset StartDate
    {
        get => _startDate;
        set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
    }
    public DateTimeOffset EndDate
    {
        get => _endDate;
        set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
    }
    public int SiteId { get; set; }
    public int CurrencyId { get; set; }
    public string UserName { get; set; }

}