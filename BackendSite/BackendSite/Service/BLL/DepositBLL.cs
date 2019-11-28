using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            var results = depositService.GetDepositRequestList(serverInfo.SiteId);
            return results;
        }
        public DepositRequest GetDepositRequest(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            var result = depositService.GetDepositRequest(depositId, serverInfo.SiteId);
            return result;
        }

        public IEnumerable<Comments> GetDepositRequestComment(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            var results = depositService.GetDepositRequestComment(depositId, serverInfo.SiteId);
            return results;
        }

        public int DepositApproved(string transId, decimal actualDeposit, string comment)
        {
            var serverInfo = serverInfoService.GetModel();
            return depositService.SetDepositInfo(transId, serverInfo.SiteId, actualDeposit, comment, DepositStatus.Pending, DepositStatus.Approved);
        }

        public int DepositRejectd(string transId, decimal actualDeposit, string comment)
        {
            var serverInfo = serverInfoService.GetModel();
            return depositService.SetDepositInfo(transId, serverInfo.SiteId, actualDeposit, comment, DepositStatus.Pending, DepositStatus.Rejected);
        }

        public int DepositDraft(string transId, decimal actualDeposit, string comment)
        {
            var serverInfo = serverInfoService.GetModel();
            return depositService.SetDepositInfo(transId, serverInfo.SiteId, actualDeposit, comment, DepositStatus.Draft, DepositStatus.Draft);
        }
    }
}
