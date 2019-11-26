using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class DepositBLL
    {
        private readonly DepositService depositService;
        private readonly ServerInfoService serverInfoService;

        public DepositBLL(DepositService depositService, ServerInfoService serverInfoService)
        {
            this.depositService = depositService;
            this.serverInfoService = serverInfoService;
        }
        public IEnumerable<DepositRequest> GetDepositRequestList()
        {
            var serverInfo = serverInfoService.GetModel();

            return depositService.GetDepositRequestList(serverInfo.SiteId);
        }
        public DepositRequest GetDepositRequest(string transId)
        {
            var serverInfo = serverInfoService.GetModel();

            return depositService.GetDepositRequest(transId, serverInfo.SiteId);
        }
    }
}
