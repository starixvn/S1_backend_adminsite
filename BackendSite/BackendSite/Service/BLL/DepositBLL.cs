using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BackendSite.Service.BLL
{
    public class DepositBLL
    {
        private readonly DepositService depositService;
        private readonly ServerInfoService serverInfoService;
        private readonly AdjustmentService adjustmentService;

        public DepositBLL(DepositService depositService, ServerInfoService serverInfoService, AdjustmentService adjustmentService)
        {
            this.depositService = depositService;
            this.serverInfoService = serverInfoService;
            this.adjustmentService = adjustmentService;
        }
        public IEnumerable<DepositRequest> GetDepositRequestList(IEnumerable<int> selectSite, int? depositType, long startDate, long endDate, string DepositId, string UserName, int? currencyId, int? status, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var serverInfo = serverInfoService.GetModel();/*Check SiteID By Permission*/
            return depositService.GetDepositRequestList(selectSite, depositType, startDate, endDate, serverInfo.SiteId/*Not require*/, DepositId, UserName, currencyId, status, pageSize, ref pageNumber, out totalPages, out rowCount);
        }

        public IEnumerable<DepositRequest> GetDepositListByCust(int CustId, string TransId, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return depositService.GetDepositListByCust(CustId, TransId, startDate, endDate, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public DepositRequest GetDepositRequest(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            return depositService.GetDepositRequest(depositId, serverInfo.SiteId/*Not require*/);/*Check SiteID By Permission*/
        }

        public IEnumerable<Comments> GetDepositRequestComment(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            var depositRequest = GetDepositRequest(transId);
            return depositService.GetDepositRequestComment(depositId, depositRequest.SiteId);
        }

        public int DepositApproved(string transId, decimal actualDeposit, string comment, string Operator)
        {
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);
            var depositRequest = GetDepositRequest(transId);

            return depositService.SetDepositInfo(transId, depositRequest.SiteId, depositRequest.Amount, comment, Operator, forStatus, DepositStatus.Approved, depositRequest);
        }

        public int DepositRejectd(string transId, decimal actualDeposit, string comment, string Operator)
        {
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);

            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Rejected, null);
        }

        public int DepositDraft(string transId, decimal actualDeposit, string comment, string Operator)
        {
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);
            forStatus.Add((int)DepositStatus.OnHold);

            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Draft, null);
        }
        public int DepositOnHold(string transId, decimal actualDeposit, string comment, string Operator)
        {
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);
            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.OnHold, null);
        }
        public int DepositPending(string transId, decimal actualDeposit, string comment, string Operator)
        {
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.OnHold);
            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Pending, null);
        }
        public DepositReceipt GetDepositReceipt(string transId)
        {
            int depositId = 0;
            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            var depositRequest = GetDepositRequest(transId);
            return depositService.GetDepositReceipt(depositId, depositRequest.SiteId);
        }
        public IEnumerable<DepositInfoForExport> GetDepositExportList(IEnumerable<int> selectSite, int? depositType, long startDate, long endDate, string DepositId, string UserName, int? currencyId, int? status)
        {
            return depositService.GetDepositExportList(selectSite, depositType, startDate, endDate, DepositId, UserName, currencyId, status);
        }
        public IEnumerable<DepositLimitCurrencyInfo> GetDepositLimitCurrencyList(int siteId, int depositType) => depositService.GetDepositLimitCurrencyList(siteId, depositType);

        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(int siteId, int depositType, int currencyId) => depositService.GetDepositLimitInfos(siteId, depositType,currencyId);

        public bool AddDepositLimit(DepositLimitInfo depositLimitInfo)
        {
            //if (depositLimitInfo.VipLevel > 1)
            //{
            //    var currentMaxLevel = depositService.GetDepositLimitInfo(depositLimitInfo.SiteId, depositLimitInfo.DepositType, depositLimitInfo.CurrencyId, depositLimitInfo.VipLevel - 1);

            //    if (depositLimitInfo.DepositMin <= currentMaxLevel.DepositMin || depositLimitInfo.DepositMax <= currentMaxLevel.DepositMax)
            //        return false;
            //}
            //else
            //{
            //    if (depositLimitInfo.DepositMin <= 0 || depositLimitInfo.DepositMax <= 0)
            //        return false;
            //}

            return depositService.AddDepositLimitInfo(depositLimitInfo);
        }

        public bool EditDepositLimit(DepositLimitInfo depositLimitInfo)
        {
            //var currentBelowLevel = depositService.GetDepositLimitInfo(depositLimitInfo.SiteId, depositLimitInfo.DepositType, depositLimitInfo.CurrencyId, depositLimitInfo.VipLevel - 1);
            //var currentUpperLevel = depositService.GetDepositLimitInfo(depositLimitInfo.SiteId, depositLimitInfo.DepositType, depositLimitInfo.CurrencyId, depositLimitInfo.VipLevel + 1);

            //if (currentBelowLevel != null)
            //{
            //    if (depositLimitInfo.DepositMin <= currentBelowLevel.DepositMin || depositLimitInfo.DepositMax <= currentBelowLevel.DepositMax)
            //        return false;
            //}
            //else
            //{
            //    if (depositLimitInfo.DepositMin <= 0 || depositLimitInfo.DepositMax <= 0)
            //        return false;
            //}

            //if (currentUpperLevel != null)
            //{
            //    if (depositLimitInfo.DepositMin >= currentUpperLevel.DepositMin || depositLimitInfo.DepositMax >= currentUpperLevel.DepositMax)
            //        return false;
            //}

            return depositService.EditDepositLimitInfo(depositLimitInfo);
        }

        public int DeleteDepositLimit(DepositLimitInfo depositLimitInfo)
        {
            var currentUpperPoint = depositService.GetDepositLimitInfo(depositLimitInfo.SiteId, depositLimitInfo.DepositType, depositLimitInfo.CurrencyId, depositLimitInfo.VipLevel + 1);

            if (currentUpperPoint != null)
                return 0;

            return depositService.DeleteDepositLimitInfo(depositLimitInfo);
        }
    }
}
