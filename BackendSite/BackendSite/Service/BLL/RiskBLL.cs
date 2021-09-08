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

        public async Task<int> DisableUserForGame(int custId, int siteId, List<DisableTpGame> gameIndo, string Operator)
        {
            return await _riskService.TpDisableUserSet(custId, siteId, gameIndo, Operator);
        }
        
        public IEnumerable<TpDisableUser> GetTpDisableUser(IEnumerable<int> siteIds, string userName, long startDate, long endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            return _riskService.TpDisableUserGet(siteIds, userName, startDate, endDate, pageSize, pageNumber, out totalPages, out rowCount);
        }

        public async Task<TpDisableUserDetail> GetTpDisableUserBuCustId(int custId)
        {
            return await _riskService.TpDisableUserGetByCustId(custId);
        }

        public async Task<int> UpdateStatus(int custId, int status, string Operator)
        {
            return await _riskService.UpdateStatus(custId, status, Operator);
        }

        public async Task<int> DeleteTpDisableUser(int custId)
        {
            return await _riskService.DeleteTpDisableUser(custId);
        }
    }
}