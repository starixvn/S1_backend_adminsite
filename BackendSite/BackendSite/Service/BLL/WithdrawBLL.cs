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

            //foreach (var item in results)
            //{
            //    switch (item.DispositType)
            //    {
            //        case (int)DispositType.OnlineBanking:
            //            item.DispositTypeString = DispositType.OnlineBanking.ToString();
            //            break;
            //        case (int)DispositType.MobilePayment:
            //            item.DispositTypeString = DispositType.MobilePayment.ToString();
            //            break;
            //        case (int)DispositType.WireTransfer:
            //            item.DispositTypeString = DispositType.WireTransfer.ToString();
            //            break;
            //    }

            //    switch (item.Status)
            //    {
            //        case (int)DepositStatus.Draft:
            //            item.StatusString = DepositStatus.Draft.ToString();
            //            break;
            //        case (int)DepositStatus.Pending:
            //            item.StatusString = DepositStatus.Pending.ToString();
            //            break;
            //        case (int)DepositStatus.Paid:
            //            item.StatusString = DepositStatus.Paid.ToString();
            //            break;
            //        case (int)DepositStatus.Approved:
            //            item.StatusString = DepositStatus.Approved.ToString();
            //            break;
            //        case (int)DepositStatus.Rejected:
            //            item.StatusString = DepositStatus.Rejected.ToString();
            //            break;
            //        case (int)DepositStatus.Canceled:
            //            item.StatusString = DepositStatus.Canceled.ToString();
            //            break;
            //        case (int)DepositStatus.Expired:
            //            item.StatusString = DepositStatus.Expired.ToString();
            //            break;
            //        case (int)DepositStatus.Error:
            //            item.StatusString = DepositStatus.Error.ToString();
            //            break;
            //    }

            //}

            return results;
        }
    }
}
