using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class WithdrawalBLL
    {
        private readonly WithdrawalService withdrawalService;
        private readonly ServerInfoService serverInfoService;

        public WithdrawalBLL(WithdrawalService withdrawalService, ServerInfoService serverInfoService)
        {
            this.withdrawalService = withdrawalService;
            this.serverInfoService = serverInfoService;
        }

        public IEnumerable<WithdrawalRequest> GetWithdrawalRequestList(int? withdrawalType, long startDate, long endDate, string WithdrawalId, string UserName, int? currencyId, int? status, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var serverInfo = serverInfoService.GetModel();
            return withdrawalService.GetWithdrawalRequestList(withdrawalType, startDate, endDate, serverInfo.SiteId, WithdrawalId, UserName, currencyId, status, pageSize, ref pageNumber, out totalPages, out rowCount);
        }

        public WithdrawalRequest GetWithdrawalRequest(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int withdrawalId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            return withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
        }

        public IEnumerable<Comments> GetWithdrawalRequestComment(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int withdrawalId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            var results = withdrawalService.GetWithdrawalRequestComment(withdrawalId, serverInfo.SiteId);
            return results;
        }

        public int WithdrawalApproved(string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.Process);

            return withdrawalService.SetWithdrawalInfo(transId, serverInfo.SiteId, comment, Operator, forStatus, WithdrawalStatus.Approved);
        }

        public int WithdrawalRejectd(string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.OnHold);
            forStatus.Add((int)WithdrawalStatus.Verify);

            return withdrawalService.SetWithdrawalInfo(transId, serverInfo.SiteId, comment, Operator, forStatus, WithdrawalStatus.Rejected);
        }

        public int WithdrawalVerify(string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.Pending);
            forStatus.Add((int)WithdrawalStatus.OnHold);
            forStatus.Add((int)WithdrawalStatus.Process);
            return withdrawalService.SetWithdrawalInfo(transId, serverInfo.SiteId, comment, Operator, forStatus, WithdrawalStatus.Verify);
        }
        public int WithdrawalOnHold (string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.Pending);
            return withdrawalService.SetWithdrawalInfo(transId, serverInfo.SiteId, comment, Operator, forStatus, WithdrawalStatus.OnHold);
        }
        public int WithdrawalDraft(string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.Pending);
            forStatus.Add((int)WithdrawalStatus.OnHold);
            forStatus.Add((int)WithdrawalStatus.Verify);
            forStatus.Add((int)WithdrawalStatus.Process);
            return withdrawalService.SetWithdrawalInfo(transId, serverInfo.SiteId, comment, Operator, forStatus, WithdrawalStatus.Draft);
        }
    }
}
