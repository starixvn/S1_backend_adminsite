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

        public IEnumerable<WithdrawalRequest> GetWithdrawalRequestList()
        {
            var serverInfo = serverInfoService.GetModel();
            return withdrawalService.GetWithdrawalRequestList(serverInfo.SiteId);
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

        public int WithdrawalApproved(string transId, string comment)
        {
            var serverInfo = serverInfoService.GetModel();
            return withdrawalService.SetWithdrawalInfo(transId, serverInfo.SiteId, comment, WithdrawalStatus.Pending, WithdrawalStatus.Approved);
        }

        public int WithdrawalRejectd(string transId, string comment)
        {
            var serverInfo = serverInfoService.GetModel();
            return withdrawalService.SetWithdrawalInfo(transId, serverInfo.SiteId, comment, WithdrawalStatus.Pending, WithdrawalStatus.Rejected);
        }

        public int WithdrawalVerify(string transId, string comment)
        {
            var serverInfo = serverInfoService.GetModel();
            return withdrawalService.SetWithdrawalInfo(transId, serverInfo.SiteId, comment, WithdrawalStatus.Pending, WithdrawalStatus.Pending);
        }
    }
}
