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

            var results = depositService.GetDepositRequestList(serverInfo.SiteId);

            foreach (var item in results)
            {
                switch (item.DispositType)
                {
                    case (int)DispositType.OnlineBanking:
                        item.DispositTypeString = DispositType.OnlineBanking.ToString();
                        break;
                    case (int)DispositType.MobilePayment:
                        item.DispositTypeString = DispositType.MobilePayment.ToString();
                        break;
                    case (int)DispositType.WireTransfer:
                        item.DispositTypeString = DispositType.WireTransfer.ToString();
                        break;
                }

                switch (item.Status)
                {
                    case (int)DepositStatus.Draft:
                        item.StatusString = DepositStatus.Draft.ToString();
                        break;
                    case (int)DepositStatus.Pending:
                        item.StatusString = DepositStatus.Pending.ToString();
                        break;
                    case (int)DepositStatus.Paid:
                        item.StatusString = DepositStatus.Paid.ToString();
                        break;
                    case (int)DepositStatus.Approved:
                        item.StatusString = DepositStatus.Approved.ToString();
                        break;
                    case (int)DepositStatus.Rejected:
                        item.StatusString = DepositStatus.Rejected.ToString();
                        break;
                    case (int)DepositStatus.Canceled:
                        item.StatusString = DepositStatus.Canceled.ToString();
                        break;
                    case (int)DepositStatus.Expired:
                        item.StatusString = DepositStatus.Expired.ToString();
                        break;
                    case (int)DepositStatus.Error:
                        item.StatusString = DepositStatus.Error.ToString();
                        break;
                }

            }

            return results;
        }
        public DepositRequest GetDepositRequest(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            var result = depositService.GetDepositRequest(depositId, serverInfo.SiteId);

            switch (result.DispositType)
            {
                case (int)DispositType.OnlineBanking:
                    result.DispositTypeString = DispositType.OnlineBanking.ToString();
                    break;
                case (int)DispositType.MobilePayment:
                    result.DispositTypeString = DispositType.MobilePayment.ToString();
                    break;
                case (int)DispositType.WireTransfer:
                    result.DispositTypeString = DispositType.WireTransfer.ToString();
                    break;
            }

            switch (result.Status)
            {
                case (int)DepositStatus.Draft:
                    result.StatusString = DepositStatus.Draft.ToString();
                    break;
                case (int)DepositStatus.Pending:
                    result.StatusString = DepositStatus.Pending.ToString();
                    break;
                case (int)DepositStatus.Paid:
                    result.StatusString = DepositStatus.Paid.ToString();
                    break;
                case (int)DepositStatus.Approved:
                    result.StatusString = DepositStatus.Approved.ToString();
                    break;
                case (int)DepositStatus.Rejected:
                    result.StatusString = DepositStatus.Rejected.ToString();
                    break;
                case (int)DepositStatus.Canceled:
                    result.StatusString = DepositStatus.Canceled.ToString();
                    break;
                case (int)DepositStatus.Expired:
                    result.StatusString = DepositStatus.Expired.ToString();
                    break;
                case (int)DepositStatus.Error:
                    result.StatusString = DepositStatus.Error.ToString();
                    break;
            }

            return result;
        }

        public IEnumerable<Comments> GetDepositRequestComment(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
           var results = depositService.GetDepositRequestComment(depositId, serverInfo.SiteId);

            foreach (var item in results)
                switch (item.Action)
                {
                    case (int)DepositStatus.Draft:
                        item.ActionString = DepositStatus.Draft.ToString();
                        break;
                    case (int)DepositStatus.Pending:
                        item.ActionString = DepositStatus.Pending.ToString();
                        break;
                    case (int)DepositStatus.Paid:
                        item.ActionString = DepositStatus.Paid.ToString();
                        break;
                    case (int)DepositStatus.Approved:
                        item.ActionString = DepositStatus.Approved.ToString();
                        break;
                    case (int)DepositStatus.Rejected:
                        item.ActionString = DepositStatus.Rejected.ToString();
                        break;
                    case (int)DepositStatus.Canceled:
                        item.ActionString = DepositStatus.Canceled.ToString();
                        break;
                    case (int)DepositStatus.Expired:
                        item.ActionString = DepositStatus.Expired.ToString();
                        break;
                    case (int)DepositStatus.Error:
                        item.ActionString = DepositStatus.Error.ToString();
                        break;
                }

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
