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
        public IEnumerable<DepositRequest> GetDepositRequestList(int? depositType, long startDate, long endDate, string DepositId, string UserName, int? currencyId, int? status, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var serverInfo = serverInfoService.GetModel();
            return depositService.GetDepositRequestList(depositType, startDate, endDate, serverInfo.SiteId, DepositId, UserName, currencyId, status, pageSize, ref pageNumber, out totalPages, out rowCount);
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
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);

            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);
            var depositRequest = depositService.GetDepositRequest(depositId, serverInfo.SiteId);

            return depositService.SetDepositInfo(transId, serverInfo.SiteId, depositRequest.Amount, comment, Operator, forStatus, DepositStatus.Approved, depositRequest);
        }

        public int DepositRejectd(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);
            return depositService.SetDepositInfo(transId, serverInfo.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Rejected, null);
        }

        public int DepositDraft(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);
            forStatus.Add((int)DepositStatus.OnHold);
            return depositService.SetDepositInfo(transId, serverInfo.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Draft, null);
        }
        public int DepositOnHold(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);
            return depositService.SetDepositInfo(transId, serverInfo.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.OnHold, null);
        }
        public int DepositPending(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.OnHold);
            return depositService.SetDepositInfo(transId, serverInfo.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Pending, null);
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
