using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class RiskBLL
    {
        private readonly RiskService _riskService;

        public RiskBLL(RiskService riskService)
        {
            _riskService = riskService;
        }

        public async Task<int> DisableUserForGame(TpDisableUser tpDisableUser)
        {
            return await _riskService.TpDisableUserSet(tpDisableUser);
        }
    }
}