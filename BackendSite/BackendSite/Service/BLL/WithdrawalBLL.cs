using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
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
        private readonly ReportService reportService;

        public WithdrawalBLL(WithdrawalService withdrawalService, ServerInfoService serverInfoService, WithdrawalLimitService withdrawalLimitService, ReportService reportService)
        {
            this.withdrawalService = withdrawalService;
            this.serverInfoService = serverInfoService;
            this.withdrawalLimitService = withdrawalLimitService;
            this.reportService = reportService;
        }

        public IEnumerable<WithdrawalRequest> GetWithdrawalRequestList(IEnumerable<int> selectSite, IEnumerable<int> withdrawalType, long startDate, long endDate, string WithdrawalId, string UserName, IEnumerable<int> currencyId, IEnumerable<int> status, IEnumerable<int> levelstatus,int pageSize, ref int pageNumber, out int totalPages, out int rowCount, ref List<string> CurrencyAmount)
        {
            var serverInfo = serverInfoService.GetModel();
            return withdrawalService.GetWithdrawalRequestList(selectSite, withdrawalType, startDate, endDate, serverInfo.SiteId/*Not require*/, WithdrawalId, UserName, currencyId, status, levelstatus,pageSize, ref pageNumber, out totalPages, out rowCount, ref CurrencyAmount);
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
            forStatus.Add((int)WithdrawalStatus.Pending);
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
            //forStatus.Add((int)WithdrawalStatus.ToKyc);
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
            //forStatus.Add((int)WithdrawalStatus.Verify);
            forStatus.Add((int)WithdrawalStatus.Pending);
            //forStatus.Add((int)WithdrawalStatus.ToKyc);
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
        public int WithdrawalPending(string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            //forStatus.Add((int)WithdrawalStatus.OnHold);
            forStatus.Add((int)WithdrawalStatus.Verify);
            forStatus.Add((int)WithdrawalStatus.ToKyc);
            int custId = 0;
            decimal amount = 0m;
            int withdrawalId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            var withdrawalRequest = withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return withdrawalService.SetWithdrawalInfo(transId, withdrawalRequest.SiteId, comment, Operator, forStatus, WithdrawalStatus.Pending, null, (int?)null, out custId, out amount);
        }
        public int WithdrawalToKyc(string transId, string comment, string Operator)
        {
            var serverInfo = serverInfoService.GetModel();
            List<int> forStatus = new List<int>();
            forStatus.Add((int)WithdrawalStatus.Pending);
            int custId = 0;
            decimal amount = 0m;
            int withdrawalId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            var withdrawalRequest = withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return withdrawalService.SetWithdrawalInfo(transId, withdrawalRequest.SiteId, comment, Operator, forStatus, WithdrawalStatus.ToKyc, null, (int?)null, out custId, out amount);
        }
        
        public IEnumerable<WithdrawalRequest> GetWithdrawalListByCust(int CustId, string TransId, int? Status, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount, ref List<string> CurrencyAmount)
        {
            return withdrawalService.GetWithdrawalListByCust(CustId, TransId, Status, startDate, endDate, pageSize, ref pageNumber, out totalPages, out rowCount, ref CurrencyAmount);
        }

        public IEnumerable<WithdrawalInfoForExport> GetWithdrawalExportList(string selectSite, string withdrawalType, long startDate, long endDate, string WithdrawalId, string UserName, string currencyId, string status,string LevelStatus)
        {
            return withdrawalService.GetWithdrawalExportList(selectSite, withdrawalType, startDate, endDate, WithdrawalId, UserName, currencyId, status, LevelStatus);
        }
        public IEnumerable<WithdrawalLimitInfoManage> GetWithdrawalLimitInfos(int siteId, int currencyId) => withdrawalLimitService.GetWithdrawalLimitInfos(siteId, currencyId);

        public bool AddWithdrawalLimit(WithdrawalLimitInfoManage withdrawalLimitInfoManage, string UpdateOperator)
        {
            if (withdrawalLimitInfoManage.WithdrawalMax < 0 || withdrawalLimitInfoManage.WithdrawalMin < 0)
            {
                return false;
            }
            return withdrawalLimitService.AddWithdrawalLimitInfo(withdrawalLimitInfoManage, UpdateOperator);
        }

        public bool EditWithdrawalLimit(WithdrawalLimitInfoManage withdrawalLimitInfoManage,string UpdateOperator)
        {
            if(withdrawalLimitInfoManage.WithdrawalMax <0 || withdrawalLimitInfoManage.WithdrawalMin < 0)
            {
                return false;
            }
            return withdrawalLimitService.EditWithdrawalLimitInfo(withdrawalLimitInfoManage, UpdateOperator);
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
        public List<WithdrawalData> GetWithdrawalLimitData(int SiteId)
        {
            //IEnumerable<CurrencyDataV2> CurrencyData = null;
            IEnumerable<ServerInfo> SiteDataList = null;
            List<WithdrawalData> result = new List<WithdrawalData>();
            SiteDataList = serverInfoService.GetSiteList();
            List<int> CurrencyData = SiteDataList.Where(x => x.SiteId == SiteId)?.FirstOrDefault().ICurrencyList.ToList();
            Dictionary<int,string> currencyList = reportService.GetCurrencyList().ToDictionary(x=> x.CurrencyId,x=> x.IsoCode) ;
            var WithdrawalMinInfo = withdrawalService.GetWithdrawalLimitData(SiteId).ToList();
            CurrencyData.ForEach(x =>
            {
                var target = WithdrawalMinInfo.Find(y => y.CurrencyId == x);
                result.Add(new WithdrawalData()
                {
                    HasDetail = target != null,
                    CurrencyId = x,
                    CurrencyName = currencyList[x],
                    SiteId = SiteId,
                    WithdrawalMin = target != null ? target.WithdrawalMin : 0
                });
            });
            return result;
        }
        public bool InsertWithdrawalLimit(int SiteId,int CurrencyId, int WithdrawalMin,string Operator)
        {
            var WithdrawalMinInfo = withdrawalService.GetWithdrawalLimitData(SiteId).ToList();
            if(WithdrawalMinInfo.Exists(x=> x.CurrencyId == CurrencyId))
            {
                return true;
            }
            return withdrawalService.InsertWithdrawalLimit(SiteId, CurrencyId, WithdrawalMin, Operator);
        }
        public bool UpdateWithdrawalLimit(int SiteId, int CurrencyId, int WithdrawalMin,string Operator)
        {
            return withdrawalService.UpdateWithdrawalLimit(SiteId, CurrencyId, WithdrawalMin, Operator);         
        }
        public bool DeleteWithdrawalLimit(int SiteId, int CurrencyId)
        {
            return withdrawalService.DeleteWithdrawalLimit(SiteId, CurrencyId);
        }
        public bool UpdateWithdrawalPicCount(string transId)
        {
            return withdrawalService.UpdateWithdrawalPicCount(transId);
        }
        public BankSlipPic GetWithdrawalPicCount(string transId)
        {
            return withdrawalService.GetWithdrawalPicCount(transId);
        }
        public WithdrawalTimeData WithdrawalTimeReportGet(WithdrawalTimeReportPara para)
        {
            var result = withdrawalService.WithdrawalTimeReportGet(para);

            if (result != null)
            {
                result.WithdrawalTimeList.ForEach(x =>
                {
                    TimeSpan ts1 = new TimeSpan(x.CreateTime.Ticks);
                    TimeSpan ts2 = new TimeSpan(x.UpdateTime.Ticks);
                    TimeSpan ts = ts1.Subtract(ts2).Duration();
                    x.CompleteTime = GetTimeString(ts);
                    if (x.VerifyTime > DateTime.Now.AddYears(-1000))
                    {
                        TimeSpan ts3 = new TimeSpan(x.VerifyTime.Ticks);
                        TimeSpan tsv = ts1.Subtract(ts3).Duration();
                        if (x.Status == 6)
                        {
                            TimeSpan tsa = ts3.Subtract(ts2).Duration();
                            x.ToApprovedTime = GetTimeString(tsa);
                        }                    
                        x.ToVerifyTime = GetTimeString(tsv);                    
                    }
                });
            }

            return result;
        }
        public IEnumerable<WithdrawalTimeExportData> WithdrawalTimeExportGet(string SelectSite, string WithdrawalID, long StartDate, long EndDate, int Status, int CurrencyId)
        {
            List<WithdrawalTimeExportData> ExportDataToExcel = new List<WithdrawalTimeExportData>();
            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(SelectSite);

            var result = withdrawalService.GetExportData(siteList, WithdrawalID, StartDate, EndDate, Status, CurrencyId);

            if (result != null)
            {
                result.ForEach(x => {
                    TimeSpan ts1 = new TimeSpan(x.CreateTime.Ticks);
                    TimeSpan ts2 = new TimeSpan(x.UpdateTime.Ticks);
                    TimeSpan ts = ts1.Subtract(ts2).Duration();
                    string tempToVerifyTime = "";
                    string tempToApprovedTime = "";
                    if (x.VerifyTime > DateTime.Now.AddYears(-1000))
                    {
                        TimeSpan ts3 = new TimeSpan(x.VerifyTime.Ticks);
                        TimeSpan tsv = ts1.Subtract(ts3).Duration();
                        if (x.Status == 6)
                        {
                            TimeSpan tsa = ts3.Subtract(ts2).Duration();
                            tempToApprovedTime = GetTimeString(tsa);
                        }
                        tempToVerifyTime = GetTimeString(tsv);
                    }
                    ExportDataToExcel.Add(new WithdrawalTimeExportData
                    {
                        SiteName = x.SiteName,
                        WithdrawalId = x.WithdrawalId,
                        WithdrawalType = x.WithdrawalTypeName,
                        UserName = x.UserName,
                        Currency = x.IsoCode,
                        Status = x.StatusName,
                        CreateTime = x.CreateTime,
                        UpdateTime = x.UpdateTime,
                        ToVerifyTime = tempToVerifyTime,
                        ToApprovedTime = tempToApprovedTime,
                        CompleteTime = GetTimeString(ts)
                    });
                });
            }
            return ExportDataToExcel;
        }
        private string GetTimeString(TimeSpan ts)
        {
            string tempDays = "";
            string tempHours = "";
            string tempMinutes = "";
            string tempSeconds = "";
            string result = "";
            if (ts.Days != 0)
            {
                tempDays = string.Format("{0}Days", ts.Days.ToString());
            }
            if (ts.Hours >= 10)
            {
                tempHours = string.Format("{0}", ts.Hours.ToString());
            }
            else
            {
                tempHours = string.Format("0{0}", ts.Hours.ToString());
            }
            if (ts.Minutes >= 10)
            {
                tempMinutes = string.Format("{0}", ts.Minutes.ToString());
            }
            else
            {
                tempMinutes = string.Format("0{0}", ts.Minutes.ToString());
            }
            if (ts.Seconds >= 10)
            {
                tempSeconds = string.Format("{0}", ts.Seconds.ToString());
            }
            else
            {
                tempSeconds = string.Format("0{0}", ts.Seconds.ToString());
            }
            result = string.Format("{0}{1}:{2}:{3}", tempDays, tempHours, tempMinutes, tempSeconds);
            return result;
        }
    }
}

