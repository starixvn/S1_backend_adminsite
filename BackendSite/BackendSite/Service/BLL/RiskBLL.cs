using System;
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
        
        public IEnumerable<TpDisableUser> GetTpDisableUser(IEnumerable<int> siteIds, string userName, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            return _riskService.TpDisableUserGet(siteIds, userName, startDate, endDate, pageSize, pageNumber, out totalPages, out rowCount);
        }

        public async Task<TpDisableUserDetail> GetTpDisableUserBuCustId(int custId)
        {
            return await _riskService.TpDisableUserGetByCustId(custId);
        }

        public async Task<bool> UpdateStatus(ParamsWithLog<DisableDisableUser> input)
        {
            return await _riskService.UpdateStatus(input);
        }

        public async Task<bool> DeleteTpDisableUser(ParamsWithLog<DeleteDisableUser> input)
        {
            return await _riskService.DeleteTpDisableUser(input);
        }
    }
}