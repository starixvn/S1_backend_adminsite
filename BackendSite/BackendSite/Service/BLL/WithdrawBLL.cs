using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class WithdrawBLL
    {
        private readonly WithdrawService withdrawService;
        private readonly ServerInfoService serverInfoService;

        public WithdrawBLL(WithdrawService withdrawService, ServerInfoService serverInfoService)
        {
            this.withdrawService = withdrawService;
            this.serverInfoService = serverInfoService;
        }

        public IEnumerable<WithdrawRequest> GetWithdrawRequestList()
        {
            var serverInfo = serverInfoService.GetModel();
            var results = withdrawService.GetWithdrawRequestList(serverInfo.SiteId);

            return results;
        }
    }
}
