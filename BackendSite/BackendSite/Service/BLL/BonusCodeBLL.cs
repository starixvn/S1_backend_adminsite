using BackendSite.Controllers.ApiV2.Marketing.Bonus;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Option;

namespace BackendSite.Service.BLL;

public class BonusCodeBLL
{
    private readonly ILogger<RedemptionBLL> _logger;
    private readonly BonusCodeService _bonusCodeService;
    
    public BonusCodeBLL(IServiceProvider provider)
    {
        _logger = provider.GetRequiredService<ILogger<RedemptionBLL>>();
        _bonusCodeService = provider.GetRequiredService<BonusCodeService>();
    }


    public IEnumerable<TpBonusCodeList> GetList(int SiteId,int tpId, string gameId, DateTime startTime, DateTime endTime)
    {
        return _bonusCodeService.GetList(SiteId,tpId,gameId,startTime,endTime);
    }
}