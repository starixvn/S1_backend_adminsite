using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using NPOI.POIFS.Crypt.Dsig;
using System.Net;
using BackendSite.Extensions;

namespace BackendSite.Service.BLL;

public class QuestionnaireBLL
{
    private readonly QuestionnaireService _questionnaireService;

    public QuestionnaireBLL(IServiceProvider provider)
    {
        _questionnaireService = provider.GetService<QuestionnaireService>();
    }

    public IEnumerable<QuestionnairList> GetList(QuestionnaireRequest req, out IPageInfo pageInfo)
    {
        pageInfo = new PageInfo();
        var data = _questionnaireService.GetList(req.SiteId, req.CurrencyId, req.UserName, req.StartDate, req.EndDate)
            .ToList();

        var (index, count) = pageInfo.ListPagination(data.Count, req.PageNumber, req.PageSize);
        return data.GetRange(index, count);
    }

    public int Set(FeedbackRequest req, string @operator)
    {
        return _questionnaireService.Set(req.Id, req.Feedback, @operator);
    }

    public IEnumerable<QuestionnairList> ExportList(QuestionnaireExportRequest req)
    {
        return _questionnaireService.GetList(req.SiteId, req.CurrencyId, req.UserName, req.StartDate, req.EndDate);
    }
}