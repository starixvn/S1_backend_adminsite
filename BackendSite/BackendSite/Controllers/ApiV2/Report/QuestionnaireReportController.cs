using BackendSite.Attributes;
using BackendSite.Controllers.ApiV2.Member.Process;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class QuestionnaireReportController : Controller
    {
        private readonly ILogger<QuestionnaireReportController> _logger;
        private readonly QuestionnaireBLL _questionnaireBLL;
        public QuestionnaireReportController(IServiceProvider provider) 
        {
            _logger = provider.GetRequiredService<ILogger<QuestionnaireReportController>>();
            _questionnaireBLL = provider.GetRequiredService<QuestionnaireBLL>();
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 154, 1 })]
        [HttpPost]
        public ApiListResult<QuestionnairList> GetList(QuestionnaireRequest req)
        {
            try
            {
                var result = _questionnaireBLL.GetList(req, out var pageInfo);
                return new ApiListResult<QuestionnairList>(ErrorCodeEnum.Success, pageInfo, result);
            }
            catch(Exception ex)
            {
                return new ApiListResult<QuestionnairList>((int)ErrorCodeEnum.SystemException, ex.Message);
            }
            
        }
    }

    
}
