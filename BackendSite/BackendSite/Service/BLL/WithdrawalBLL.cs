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
        private readonly WithdrawalLimitService withdrawalLimitService;

        public WithdrawalBLL(WithdrawalService withdrawalService, ServerInfoService serverInfoService, WithdrawalLimitService withdrawalLimitService)
        {
            this.withdrawalService = withdrawalService;
            this.serverInfoService = serverInfoService;
            this.withdrawalLimitService = withdrawalLimitService;
        }

        public IEnumerable<WithdrawalRequest> GetWithdrawalRequestList(IEnumerable<int> selectSite, IEnumerable<int> withdrawalType, long startDate, long endDate, string WithdrawalId, string UserName, IEnumerable<int> currencyId, IEnumerable<int> status, IEnumerable<int> levelstatus,int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var serverInfo = serverInfoService.GetModel();
            return withdrawalService.GetWithdrawalRequestList(selectSite, withdrawalType, startDate, endDate, serverInfo.SiteId/*Not require*/, WithdrawalId, UserName, currencyId, status, levelstatus,pageSize, ref pageNumber, out totalPages, out rowCount);
        }

        public WithdrawalRequest GetWithdrawalRequest(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int withdrawalId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            return withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId/*Not require*/);
        }

        public IEnumerable<Comments> GetWithdrawalRequestComment(string transId)
        {
            var withdrawalRequest = GetWithdrawalRequest(transId);
            var results = withdrawalService.GetWithdrawalRequestComment(withdrawalRequest.WithdrawalId, withdrawalRequest.SiteId);
            return results;
        }

        public int WithdrawalApproved(string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.Process);
            int custId = 0;
            decimal amount = 0m;
            int withdrawalId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            var withdrawalRequest = withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            var withdrawalInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WithdrawalInfo>(withdrawalRequest.WithdrawalInfo);
            withdrawalInfo.CurrencyId = withdrawalRequest.CurrencyId;
            return withdrawalService.SetWithdrawalInfo(transId, withdrawalRequest.SiteId, comment, Operator, forStatus, WithdrawalStatus.Approved, Newtonsoft.Json.JsonConvert.SerializeObject(withdrawalInfo), withdrawalRequest.WithdrawalType, out custId, out amount);
        }

        public int WithdrawalRejectd(string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.OnHold);
            forStatus.Add((int)WithdrawalStatus.Verify);
            forStatus.Add((int)WithdrawalStatus.ToKyc);
            int custId = 0;
            decimal amount = 0m;
            int withdrawalId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            var withdrawalRequest = withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return withdrawalService.SetWithdrawalInfo(transId, withdrawalRequest.SiteId, comment, Operator, forStatus, WithdrawalStatus.Rejected, null, (int?)null, out custId, out amount);
        }

        public int WithdrawalVerify(string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.Pending);
            forStatus.Add((int)WithdrawalStatus.OnHold);
            forStatus.Add((int)WithdrawalStatus.Process);
            forStatus.Add((int)WithdrawalStatus.ToKyc);
            int custId = 0;
            decimal amount = 0m;
            int withdrawalId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            var withdrawalRequest = withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return withdrawalService.SetWithdrawalInfo(transId, withdrawalRequest.SiteId, comment, Operator, forStatus, WithdrawalStatus.Verify, null, (int?)null, out custId, out amount);
        }
        public int WithdrawalProcess(string transId, string comment, string Operator, string WithdrawalInfo, int? withdrawalType, out int custId, out decimal amount)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.Verify);

            int withdrawalId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            var withdrawalRequest = withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return withdrawalService.SetWithdrawalInfo(transId, withdrawalRequest.SiteId, comment, Operator, forStatus, WithdrawalStatus.Process, WithdrawalInfo, withdrawalType, out custId, out amount);
        }
        public int WithdrawalOnHold(string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.Verify);
            forStatus.Add((int)WithdrawalStatus.Pending);
            forStatus.Add((int)WithdrawalStatus.ToKyc);
            int custId = 0;
            decimal amount = 0m;
            int withdrawalId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            var withdrawalRequest = withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return withdrawalService.SetWithdrawalInfo(transId, withdrawalRequest.SiteId, comment, Operator, forStatus, WithdrawalStatus.OnHold, null, (int?)null, out custId, out amount);
        }
        public int WithdrawalDraft(string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.Pending);
            forStatus.Add((int)WithdrawalStatus.OnHold);
            forStatus.Add((int)WithdrawalStatus.Verify);
            forStatus.Add((int)WithdrawalStatus.Process);
            forStatus.Add((int)WithdrawalStatus.ToKyc);
            int custId = 0;
            decimal amount = 0m;
            int withdrawalId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            var withdrawalRequest = withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return withdrawalService.SetWithdrawalInfo(transId, withdrawalRequest.SiteId, comment, Operator, forStatus, WithdrawalStatus.Draft, null, (int?)null, out custId, out amount);
        }
        public IEnumerable<WithdrawalRequest> GetWithdrawalListByCust(int CustId, string TransId, int? Status, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return withdrawalService.GetWithdrawalListByCust(CustId, TransId, Status, startDate, endDate, pageSize, ref pageNumber, out totalPages, out rowCount);
        }

        public IEnumerable<WithdrawalInfoForExport> GetWithdrawalExportList(string selectSite, string withdrawalType, long startDate, long endDate, string WithdrawalId, string UserName, string currencyId, string status,string LevelStatus)
        {
            return withdrawalService.GetWithdrawalExportList(selectSite, withdrawalType, startDate, endDate, WithdrawalId, UserName, currencyId, status, LevelStatus);
        }
        public IEnumerable<WithdrawalLimitInfoManage> GetWithdrawalLimitInfos(int siteId, int currencyId) => withdrawalLimitService.GetWithdrawalLimitInfos(siteId, currencyId);

        public bool AddWithdrawalLimit(WithdrawalLimitInfoManage withdrawalLimitInfoManage)
        {
            if (withdrawalLimitInfoManage.VipLevel > 1)
            {
                var currentMaxLevel = withdrawalLimitService.GetWithdrawalLimitInfo(withdrawalLimitInfoManage.SiteId, withdrawalLimitInfoManage.CurrencyId, withdrawalLimitInfoManage.VipLevel - 1);

                if (withdrawalLimitInfoManage.WithdrawalMin <= currentMaxLevel.WithdrawalMin || withdrawalLimitInfoManage.WithdrawalMax <= currentMaxLevel.WithdrawalMax)
                    return false;
            }
            else
            {
                if (withdrawalLimitInfoManage.WithdrawalMin <= 0 || withdrawalLimitInfoManage.WithdrawalMax <= 0)
                    return false;
            }

            return withdrawalLimitService.AddWithdrawalLimitInfo(withdrawalLimitInfoManage);
        }

        public bool EditWithdrawalLimit(WithdrawalLimitInfoManage withdrawalLimitInfoManage)
        {
            var currentBelowLevel = withdrawalLimitService.GetWithdrawalLimitInfo(withdrawalLimitInfoManage.SiteId, withdrawalLimitInfoManage.CurrencyId, withdrawalLimitInfoManage.VipLevel - 1);
            var currentUpperLevel = withdrawalLimitService.GetWithdrawalLimitInfo(withdrawalLimitInfoManage.SiteId, withdrawalLimitInfoManage.CurrencyId, withdrawalLimitInfoManage.VipLevel + 1);

            if (currentBelowLevel != null)
            {
                if (withdrawalLimitInfoManage.WithdrawalMin <= currentBelowLevel.WithdrawalMin || withdrawalLimitInfoManage.WithdrawalMax <= currentBelowLevel.WithdrawalMax)
                    return false;
            }
            else
            {
                if (withdrawalLimitInfoManage.WithdrawalMin <= 0 || withdrawalLimitInfoManage.WithdrawalMax <= 0)
                    return false;
            }

            if (currentUpperLevel != null)
            {
                if (withdrawalLimitInfoManage.WithdrawalMin >= currentUpperLevel.WithdrawalMin || withdrawalLimitInfoManage.WithdrawalMax >= currentUpperLevel.WithdrawalMax)
                    return false;
            }

            return withdrawalLimitService.EditWithdrawalLimitInfo(withdrawalLimitInfoManage);
        }

        public int DeleteWithdrawalLimit(WithdrawalLimitInfoManage withdrawalLimitInfoManage)
        {
            var currentUpperPoint = withdrawalLimitService.GetWithdrawalLimitInfo(withdrawalLimitInfoManage.SiteId, withdrawalLimitInfoManage.CurrencyId, withdrawalLimitInfoManage.VipLevel + 1);

            if (currentUpperPoint != null)
                return 0;

            return withdrawalLimitService.DeleteWithdrawalLimitInfo(withdrawalLimitInfoManage);
        }
        public IEnumerable<WithdrawalKYC> GetWithdrawalKYCList(int SiteId)
        {
            return withdrawalService.GetWithdrawalKYC(SiteId);
        }

        public bool EditWithdrawalKYCList(WithdrawalKYC withdrawalKYC)
        {
            return withdrawalService.EditWithdrawalKYC(withdrawalKYC);
        }

    }
}
