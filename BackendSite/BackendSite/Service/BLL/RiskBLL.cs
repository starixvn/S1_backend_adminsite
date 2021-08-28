using System.Collections.Generic;
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

        public async Task<int> DisableUserForGame(int custId, int siteId, List<DisableTpGame> gameIndo, string updateOperator)
        {
            return await _riskService.TpDisableUserSet(custId, siteId, gameIndo, updateOperator);
        }
    }
}