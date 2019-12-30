using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
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
            return depositService.GetDepositRequestList(serverInfo.SiteId);
        }
        public DepositRequest GetDepositRequest(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            return depositService.GetDepositRequest(depositId, serverInfo.SiteId);
        }

        public IEnumerable<Comments> GetDepositRequestComment(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            return depositService.GetDepositRequestComment(depositId, serverInfo.SiteId);
        }

        public int DepositApproved(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            return depositService.SetDepositInfo(transId, serverInfo.SiteId, actualDeposit, comment, Operator, DepositStatus.Pending, DepositStatus.Approved);
        }

        public int DepositRejectd(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            return depositService.SetDepositInfo(transId, serverInfo.SiteId, actualDeposit, comment, Operator, DepositStatus.Pending, DepositStatus.Rejected);
        }

        public int DepositDraft(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            return depositService.SetDepositInfo(transId, serverInfo.SiteId, actualDeposit, comment, Operator, DepositStatus.Draft, DepositStatus.Draft);
        }

        public DepositReceipt GetDepositReceipt(string transId)
        {
            int depositId = 0;
            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            var serverInfo = serverInfoService.GetModel();
            return depositService.GetDepositReceipt(depositId, serverInfo.SiteId);
        }
    }
}
