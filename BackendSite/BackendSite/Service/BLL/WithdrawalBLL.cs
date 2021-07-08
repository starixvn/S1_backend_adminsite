using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BackendSite.Service.Model.WithdrawalBankAccounting;

namespace BackendSite.Service.BLL
{
    public class WithdrawalBLL
    {
        #region Fields

        private readonly WithdrawalService _withdrawalService;
        private readonly ServerInfoService _serverInfoService;
        private readonly WithdrawalLimitService _withdrawalLimitService;
        private readonly ReportService _reportService;
        private readonly WithdrawalBankAccountingService _withdrawalBankAccountingService;

        #endregion

        #region Constructors

        public WithdrawalBLL(WithdrawalService withdrawalService,
            ServerInfoService serverInfoService,
            WithdrawalLimitService withdrawalLimitService,
            ReportService reportService,
            WithdrawalBankAccountingService withdrawalBankAccountingService)
        {
            _withdrawalService = withdrawalService;
            _serverInfoService = serverInfoService;
            _withdrawalLimitService = withdrawalLimitService;
            _reportService = reportService;
            _withdrawalBankAccountingService = withdrawalBankAccountingService;
        }

        #endregion

        #region Public Methods

        public IEnumerable<WithdrawalRequest> GetWithdrawalRequestList(IEnumerable<int> selectSite,
            IEnumerable<int> withdrawalType, long startDate, long endDate, string withdrawalId, string userName,
            IEnumerable<int> currencyId, IEnumerable<int> status, IEnumerable<int> levelStatus, int pageSize,
            ref int pageNumber, out int totalPages, out int rowCount, ref List<string> currencyAmount)
        {
            var serverInfo = _serverInfoService.GetModel();
            return _withdrawalService.GetWithdrawalRequestList(selectSite, withdrawalType, startDate, endDate,
                serverInfo.SiteId /*Not require*/, withdrawalId, userName, currencyId, status, levelStatus, pageSize,
                ref pageNumber, out totalPages, out rowCount, ref currencyAmount);
        }

        public WithdrawalRequest GetWithdrawalRequest(string transId)
        {
            var serverInfo = _serverInfoService.GetModel();

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out int withdrawalId);
            return _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId /*Not require*/);
        }

        public IEnumerable<Comments> GetWithdrawalRequestComment(string transId)
        {
            var withdrawalRequest = GetWithdrawalRequest(transId);
            var results =
                _withdrawalService.GetWithdrawalRequestComment(withdrawalRequest.WithdrawalId,
                    withdrawalRequest.SiteId);
            return results;
        }

        public int WithdrawalApproved(string transId, string comment, string updater, int userId)
        {
            var serverInfo = _serverInfoService.GetModel();
            var forStatus = new List<int> {(int) WithdrawalStatus.Process};

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out int withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            var withdrawalInfo =
                JsonConvert.DeserializeObject<WithdrawalInfo>(withdrawalRequest.WithdrawalInfo);
            withdrawalInfo.CurrencyId = withdrawalRequest.CurrencyId;
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatus.Approved, JsonConvert.SerializeObject(withdrawalInfo),
                withdrawalRequest.WithdrawalType, out int custId, out decimal amount, userId);
        }

        public int WithdrawalRejected(string transId, string comment, string updater)
        {
            var serverInfo = _serverInfoService.GetModel();
            var forStatus = new List<int>
            {
                (int) WithdrawalStatus.OnHold,
                (int) WithdrawalStatus.Verify,
                (int) WithdrawalStatus.ToKyc,
                (int) WithdrawalStatus.Pending
            };

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out int withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatus.Rejected, null, null, out int custId, out decimal amount);
        }

        public int WithdrawalVerify(string transId, string comment, string updater)
        {
            var serverInfo = _serverInfoService.GetModel();
            var forStatus = new List<int>
            {
                (int) WithdrawalStatus.Pending,
                (int) WithdrawalStatus.OnHold,
                (int) WithdrawalStatus.Process
                //(int)WithdrawalStatus.ToKyc
            };

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out int withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatus.Verify, null, null, out int custId, out decimal amount);
        }

        public int WithdrawalProcess(string transId, string comment, string updater, string withdrawalInfo,
            int? withdrawalType, out int custId, out decimal amount)
        {
            var serverInfo = _serverInfoService.GetModel();
            var forStatus = new List<int>
            {
                (int) WithdrawalStatus.Verify
            };

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out int withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatus.Process, withdrawalInfo, withdrawalType, out custId, out amount);
        }

        public int WithdrawalOnHold(string transId, string comment, string updater)
        {
            var serverInfo = _serverInfoService.GetModel();
            var forStatus = new List<int>
            {
                (int) WithdrawalStatus.Pending
                //(int)WithdrawalStatus.Verify
                //(int)WithdrawalStatus.ToKyc
            };

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out int withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatus.OnHold, null, null, out int custId, out decimal amount);
        }

        public int WithdrawalDraft(string transId, string comment, string updater)
        {
            var serverInfo = _serverInfoService.GetModel();
            var forStatus = new List<int>
            {
                (int) WithdrawalStatus.Pending,
                (int) WithdrawalStatus.OnHold,
                (int) WithdrawalStatus.Verify,
                (int) WithdrawalStatus.Process,
                (int) WithdrawalStatus.ToKyc
            };

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out int withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus),
                updater,
                WithdrawalStatus.Draft, null, null, out int custId, out decimal amount);
        }

        public int WithdrawalPending(string transId, string comment, string updater)
        {
            var serverInfo = _serverInfoService.GetModel();
            var forStatus = new List<int>
            {
                (int) WithdrawalStatus.Verify,
                (int) WithdrawalStatus.ToKyc
                //(int)WithdrawalStatus.OnHold
            };

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out int withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus),
                updater,
                WithdrawalStatus.Pending, null,null, out int custId, out decimal amount);
        }

        public int WithdrawalToKyc(string transId, string comment, string updater)
        {
            var serverInfo = _serverInfoService.GetModel();
            var forStatus = new List<int>
            {
                (int) WithdrawalStatus.Pending
            };

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out int withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus),
                updater,
                WithdrawalStatus.ToKyc, null, null, out int custId, out decimal amount);
        }

        public IEnumerable<WithdrawalRequest> GetWithdrawalListByCust(int custId, string transId, int? status,
            long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount,
            ref List<string> currencyAmount)
        {
            return _withdrawalService.GetWithdrawalListByCust(custId, transId, status, startDate, endDate, pageSize,
                ref pageNumber, out totalPages, out rowCount, ref currencyAmount);
        }

        public IEnumerable<WithdrawalInfoForExport> GetWithdrawalExportList(string selectSite, string withdrawalType,
            long startDate, long endDate, string withdrawalId, string userName, string currencyId, string status,
            string levelStatus)
        {
            return _withdrawalService.GetWithdrawalExportList(selectSite, withdrawalType, startDate, endDate,
                withdrawalId, userName, currencyId, status, levelStatus);
        }

        public IEnumerable<WithdrawalLimitInfoManage> GetWithdrawalLimitInfos(int siteId, int currencyId) =>
            _withdrawalLimitService.GetWithdrawalLimitInfos(siteId, currencyId);

        public bool AddWithdrawalLimit(WithdrawalLimitInfoManage withdrawalLimitInfoManage, string updater)
        {
            if (withdrawalLimitInfoManage.WithdrawalMax < 0 || withdrawalLimitInfoManage.WithdrawalMin < 0)
            {
                return false;
            }

            return _withdrawalLimitService.AddWithdrawalLimitInfo(withdrawalLimitInfoManage, updater);
        }

        public bool EditWithdrawalLimit(WithdrawalLimitInfoManage withdrawalLimitInfoManage, string updater)
        {
            if (withdrawalLimitInfoManage.WithdrawalMax < 0 || withdrawalLimitInfoManage.WithdrawalMin < 0)
            {
                return false;
            }

            return _withdrawalLimitService.EditWithdrawalLimitInfo(withdrawalLimitInfoManage, updater);
        }

        public int DeleteWithdrawalLimit(WithdrawalLimitInfoManage withdrawalLimitInfoManage)
        {
            var currentUpperPoint = _withdrawalLimitService.GetWithdrawalLimitInfo(withdrawalLimitInfoManage.SiteId,
                withdrawalLimitInfoManage.CurrencyId, withdrawalLimitInfoManage.VipLevel + 1);

            if (currentUpperPoint != null)
                return 0;

            return _withdrawalLimitService.DeleteWithdrawalLimitInfo(withdrawalLimitInfoManage);
        }

        public IEnumerable<WithdrawalKYC> GetWithdrawalKYCList(int siteId)
        {
            return _withdrawalService.GetWithdrawalKYC(siteId);
        }

        public bool EditWithdrawalKYCList(WithdrawalKYC withdrawalKYC)
        {
            return _withdrawalService.EditWithdrawalKYC(withdrawalKYC);
        }

        public List<WithdrawalData> GetWithdrawalLimitData(int siteId)
        {
            //IEnumerable<CurrencyDataV2> CurrencyData = null;
            IEnumerable<ServerInfo> siteDataList = null;
            List<WithdrawalData> result = new List<WithdrawalData>();
            siteDataList = _serverInfoService.GetSiteList();
            List<int> currencyData =
                siteDataList.Where(x => x.SiteId == siteId)?.FirstOrDefault().ICurrencyList.ToList();
            Dictionary<int, string> currencyList =
                _reportService.GetCurrencyList().ToDictionary(x => x.CurrencyId, x => x.IsoCode);
            var WithdrawalMinInfo = _withdrawalService.GetWithdrawalLimitData(siteId).ToList();
            currencyData.ForEach(x =>
            {
                var target = WithdrawalMinInfo.Find(y => y.CurrencyId == x);
                result.Add(new WithdrawalData()
                {
                    HasDetail = target != null,
                    CurrencyId = x,
                    CurrencyName = currencyList[x],
                    SiteId = siteId,
                    WithdrawalMin = target != null ? target.WithdrawalMin : 0
                });
            });
            return result;
        }

        public bool InsertWithdrawalLimit(int siteId, int currencyId, int withdrawalMin, string updater)
        {
            var withdrawalMinInfo = _withdrawalService.GetWithdrawalLimitData(siteId).ToList();
            if (withdrawalMinInfo.Exists(x => x.CurrencyId == currencyId))
            {
                return true;
            }

            return _withdrawalService.InsertWithdrawalLimit(siteId, currencyId, withdrawalMin, updater);
        }

        public bool UpdateWithdrawalLimit(int siteId, int currencyId, int withdrawalMin, string updater)
        {
            return _withdrawalService.UpdateWithdrawalLimit(siteId, currencyId, withdrawalMin, updater);
        }

        public bool DeleteWithdrawalLimit(int siteId, int currencyId)
        {
            return _withdrawalService.DeleteWithdrawalLimit(siteId, currencyId);
        }

        public bool UpdateWithdrawalPicCount(string transId)
        {
            return _withdrawalService.UpdateWithdrawalPicCount(transId);
        }

        public BankSlipPic GetWithdrawalPicCount(string transId)
        {
            return _withdrawalService.GetWithdrawalPicCount(transId);
        }

        public WithdrawalTimeData WithdrawalTimeReportGet(WithdrawalTimeReportPara para)
        {
            var result = _withdrawalService.WithdrawalTimeReportGet(para);

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

        public IEnumerable<WithdrawalTimeExportData> WithdrawalTimeExportGet(string selectSite, string withdrawalId,
            long startDate, long endDate, int status, int currencyId)
        {
            var exportDataToExcel = new List<WithdrawalTimeExportData>();
            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(selectSite);

            var result =
                _withdrawalService.GetExportData(siteList, withdrawalId, startDate, endDate, status, currencyId);

            if (result != null)
            {
                result.ForEach(x =>
                {
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

                    exportDataToExcel.Add(new WithdrawalTimeExportData
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

            return exportDataToExcel;
        }

        #region WithdrawalBankAccounting

        #endregion

        #endregion

        #region Private Methods

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

        private int InsertWithdrawalBankAccounting(WithdrawalBankAccountingModel model)
        {
            return _withdrawalBankAccountingService.Insert(model);
        }

        private int InsertWithdrawalBankAccountingLog(WithdrawalBankAccountingModel model)
        {
            return _withdrawalBankAccountingService.InsertLog(model);
        }

        private int UpdateWithdrawalBankAccounting(WithdrawalBankAccountingModel model)
        {
            return _withdrawalBankAccountingService.Update(model);
        }

        private int DeleteWithdrawalBankAccounting(int id, string transId)
        {
            return _withdrawalBankAccountingService.Delete(id, transId);
        }

        #endregion
    }
}