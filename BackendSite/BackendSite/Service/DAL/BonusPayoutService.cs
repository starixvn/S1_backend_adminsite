using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Dapper;
using static BackendSite.Controllers.ApiV2.Marketing.Bonus.BonusPayoutController;

namespace BackendSite.Service.DAL
{
    public class BonusPayoutService
    {
        private readonly MySqlClient _mainDbClient;

        public BonusPayoutService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        }

        public CheckTierRedepositBonusRep CheckTierRedepositBonus(CheckTierRedepositBonusReq request)
        {
            return _mainDbClient.QueryFirst<CheckTierRedepositBonusRep>("Admin_CheckTierRedepositBonus_Get", new DynamicParameters(request));
        }
    }
}
