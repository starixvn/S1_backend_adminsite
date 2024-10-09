using BackendSite.Controllers.ApiV2.Marketing.Bonus;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Option;
using BackendSite.Service.Model.Store.GiftManagement;
using Org.BouncyCastle.Ocsp;

namespace BackendSite.Service.BLL;

public class BonusCodeBLL
{
    private readonly ILogger<RedemptionBLL> _logger;
    private readonly BonusCodeService _bonusCodeService;
    private readonly ThirdPartyService _thirdPartyService;
    private readonly string _tpServiceApiUrl;
    private readonly CourierAdapter _courierAdapter;

    public BonusCodeBLL(IServiceProvider provider)
    {
        var configBll = provider.GetRequiredService<ConfigBLL>();
        _logger = provider.GetRequiredService<ILogger<RedemptionBLL>>();
        _bonusCodeService = provider.GetRequiredService<BonusCodeService>();
        _thirdPartyService = provider.GetRequiredService<ThirdPartyService>();
        _tpServiceApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").TpService;
        _courierAdapter = provider.GetRequiredService<CourierAdapter>();
    }


    public IEnumerable<TpBonusCodeList> GetList(int SiteId,int tpId, string gameId, DateTime startTime, DateTime endTime)
    {
        return _bonusCodeService.GetList(SiteId,tpId,gameId,startTime,endTime);
    }

    public async Task<ApiResult> CreateBonusCode(int siteId, int currencyId, int tpId, string gameId, int round, string bonusCode)
    {
        var currency = await _thirdPartyService.GetTpCurrency(tpId, currencyId);
        var inputs = new Dictionary<string, string>();
        var url = _tpServiceApiUrl + "CreateBonusCode";
        inputs.Add("GameId", gameId);
        inputs.Add("BonusCode", bonusCode);
        inputs.Add("Round", round.ToString());
        inputs.Add("Currency", currency.TpCurrencyId);
        inputs.Add("ExpirationDay", "8");//freespin有效期限8天

        var request1 = new CreateBonusCodeParams()
        {
            SiteId = siteId,
            TpId = tpId,
            inputs = inputs
        };

        var apiResult = await _courierAdapter.MyPostAsync<object, CreateBonusCodeParams>(url, request1);
        _logger.LogInformation(apiResult.ErrorCode == 0
            ? $"BonusCode:{bonusCode} Create Success"
            : $"BonusCode:{bonusCode} Create Fail");

        return apiResult;
    }
}