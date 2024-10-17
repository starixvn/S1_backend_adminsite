using BackendSite.Service.Model.ApiModel;
using Newtonsoft.Json;

namespace BackendSite.Service.Model;

public class QuestionnairList
{
    public int Id { get; set; }
    public string Currency { get; set; }
    public string UserName { get; set; }
    public string ContactInfo { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreateTime { get; set; }
    public string Feedback { get; set; }
    [JsonProperty("lastUpdateUser")]
    public string Operator { get; set; }
    [JsonProperty("lastUpdateTime")]
    public DateTimeOffset UpdateTime { get; set; }
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
    public string UserName { get; set; } = "";
}

public class QuestionnaireExportRequest
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

public class FeedbackRequest
{
    public int Id { get; set; }

    public string Feedback { get; set; } = string.Empty;
}