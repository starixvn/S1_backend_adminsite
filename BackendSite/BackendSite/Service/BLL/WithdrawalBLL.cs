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

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
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

        public int WithdrawalApproved(string transId, string comment, string updater, int userId, bool hasPic)
        {
            var serverInfo = _serverInfoService.GetModel();
            var forStatus = new List<int> { (int)WithdrawalStatus.Process };

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            var withdrawalInfo =
                JsonConvert.DeserializeObject<WithdrawalInfo>(withdrawalRequest.WithdrawalInfo);
            withdrawalInfo.CurrencyId = withdrawalRequest.CurrencyId;
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatus.Approved, JsonConvert.SerializeObject(withdrawalInfo),
                withdrawalRequest.WithdrawalType, out _, out _, userId, hasPic);
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

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatus.Rejected, null, null, out _, out _);
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

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatus.Verify, null, null, out _, out _);
        }

        public int WithdrawalProcess(string transId, string comment, string updater, string withdrawalInfo,
            int? withdrawalType, out int custId, out decimal amount)
        {
            var serverInfo = _serverInfoService.GetModel();
            var forStatus = new List<int>
            {
                (int) WithdrawalStatus.Verify
            };

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
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

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus), updater,
                WithdrawalStatus.OnHold, null, null, out _, out _);
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
                (int) WithdrawalStatus.ToKyc,
                (int) WithdrawalStatus.Cancelled,
                (int) WithdrawalStatus.Rejected,
                (int) WithdrawalStatus.Approved
            };

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus),
                updater,
                WithdrawalStatus.Draft, null, null, out _, out _);
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

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus),
                updater,
                WithdrawalStatus.Pending, null, null, out _, out _);
        }

        public int WithdrawalToKyc(string transId, string comment, string updater)
        {
            var serverInfo = _serverInfoService.GetModel();
            var forStatus = new List<int>
            {
                (int) WithdrawalStatus.Pending
            };

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            var withdrawalRequest = _withdrawalService.GetWithdrawalRequest(withdrawalId, serverInfo.SiteId);
            return _withdrawalService.SetWithdrawalInfoFlow(transId, withdrawalRequest.SiteId, comment,
                JsonConvert.SerializeObject(forStatus),
                updater,
                WithdrawalStatus.ToKyc, null, null, out _, out _);
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

        public int DeleteWithdrawalLimit(WithdrawalLimitInfoManage withdrawalLimitInfoManage, string Operator)
        {
            var currentUpperPoint = _withdrawalLimitService.GetWithdrawalLimitInfo(withdrawalLimitInfoManage.SiteId,
                withdrawalLimitInfoManage.CurrencyId, withdrawalLimitInfoManage.VipLevel + 1);

            if (currentUpperPoint != null)
                return 0;

            return _withdrawalLimitService.DeleteWithdrawalLimitInfo(withdrawalLimitInfoManage, Operator);
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
            var result = new List<WithdrawalData>();
            siteDataList = _serverInfoService.GetSiteList();
            var currencyData =
                siteDataList.Where(x => x.SiteId == siteId)?.FirstOrDefault()?.ICurrencyList.ToList();
            var currencyList =
                _reportService.GetCurrencyList().ToDictionary(x => x.CurrencyId, x => x.IsoCode);
            var withdrawalMinInfo = _withdrawalService.GetWithdrawalLimitData(siteId).ToList();
            currencyData.ForEach(x =>
            {
                var target = withdrawalMinInfo.Find(y => y.CurrencyId == x);
                result.Add(new WithdrawalData()
                {
                    HasDetail = target != null,
                    CurrencyId = x,
                    CurrencyName = currencyList[x],
                    SiteId = siteId,
                    WithdrawalMin = target?.WithdrawalMin ?? 0
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
                    var ts1 = new TimeSpan(x.CreateTime.Ticks);
                    var ts2 = new TimeSpan(x.UpdateTime.Ticks);
                    var timeSpanOfFlowEndTime = new TimeSpan(x.FlowEndTime.Ticks);
                    var ts = ts1.Subtract(timeSpanOfFlowEndTime).Duration();
                    x.CompleteTime = GetTimeString(ts);
                    if (x.VerifyTime > DateTime.Now.AddYears(-1000))
                    {
                        var ts3 = new TimeSpan(x.VerifyTime.Ticks);
                        var tsv = ts1.Subtract(ts3).Duration();
                        if (x.Status == 6)
                        {
                            var tsa = ts3.Subtract(timeSpanOfFlowEndTime).Duration();
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
                    var ts1 = new TimeSpan(x.CreateTime.Ticks);
                    var ts2 = new TimeSpan(x.UpdateTime.Ticks);
                    var timeSpanOfFlowEndTime = new TimeSpan(x.FlowEndTime.Ticks);
                    var ts = ts1.Subtract(timeSpanOfFlowEndTime).Duration();
                    var tempToVerifyTime = "";
                    var tempToApprovedTime = "";
                    if (x.VerifyTime > DateTime.Now.AddYears(-1000))
                    {
                        var ts3 = new TimeSpan(x.VerifyTime.Ticks);
                        var tsv = ts1.Subtract(ts3).Duration();
                        if (x.Status == 6)
                        {
                            var tsa = ts3.Subtract(timeSpanOfFlowEndTime).Duration();
                            tempToApprovedTime = GetTimeString(tsa);
                        }

                        tempToVerifyTime = GetTimeString(tsv);
                    }

                    DateTime? flowEndTime = null;
                    //Not Flow End.
                    if (x.Status == (int)WithdrawalStatus.Cancelled
                    || x.Status == (int)WithdrawalStatus.Approved
                    || x.Status == (int)WithdrawalStatus.Rejected)
                    {
                        flowEndTime = x.FlowEndTime;
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
                        FlowEndTime = flowEndTime,
                        ToVerifyTime = tempToVerifyTime,
                        ToApprovedTime = tempToApprovedTime,
                        CompleteTime = GetTimeString(ts)
                    });
                });
            }

            return exportDataToExcel;
        }

        #region WithdrawalBankAccounting

        public GetWithdrawalInfoAndAccountingListModel GetWithdrawalInfoAndAccountingList(
            IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            string startDate,
            string endDate,
            string transId,
            string userName,
            int pageNumber,
            int pageSize)
        {
            var data = GetWithdrawalInfoAndAccountingData(siteIds,
                currencyIds,
                startDate,
                endDate,
                transId,
                userName,
                pageNumber,
                pageSize,
                out var totalPages,
                out var rowCount);

            return new GetWithdrawalInfoAndAccountingListModel()
            {
                Datas = data.ToList(),
                PageNumber = Math.Abs(pageNumber),
                RowCount = rowCount,
                TotalPages = totalPages
            };
        }

        public IEnumerable<WithdrawalInfoAndAccountingListForExportModel> GetWithdrawalInfoAndAccountingListForExport(
            string siteIds,
            string currencyIds,
            string startDate,
            string endDate,
            string transId,
            string userName)
        {
            return GetWithdrawalInfoAndAccountingDataForExport(siteIds,
                currencyIds,
                startDate,
                endDate,
                transId,
                userName);
        }

        public bool GetAccountingListByTransId(string transId, out GetWithdrawalInfoAndAccountingModel data)
        {
            data = new GetWithdrawalInfoAndAccountingModel();
            var withdrawalInfo = _withdrawalService.GetWithdrawalInfo(0, transId);

            if (withdrawalInfo == null)
            {
                return false;
            }
            else
            {
                if (withdrawalInfo.WithdrawalType == (int)WithdrawalType.LocalBank
                    && withdrawalInfo.Status == (int)WithdrawalStatus.Approved)
                {
                    var accountingData = GetAccountingList(transId, withdrawalInfo.SiteId, withdrawalInfo.CurrencyId);
                    var isoCode = _reportService.GetCurrencyList().Where(x => x.CurrencyId == withdrawalInfo.CurrencyId)
                        .Select(x => x.IsoCode).FirstOrDefault();

                    data.Withdrawal = withdrawalInfo;
                    data.AccountingList = accountingData;
                    data.IsoCode = isoCode;

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool GetWithdrawalBankAccountingById(int id, out GetWithdrawalBankAccountingModel data)
        {
            data = GetAccountingById(id);
            return true;
        }

        public bool InsertWithdrawalAccounting(WithdrawalBankAccountingModel data)
        {
            try
            {
                return InsertWithdrawalBankAccounting(data);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateWithdrawalAccounting(WithdrawalBankAccountingModel data)
        {
            try
            {
                return UpdateWithdrawalBankAccounting(data);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateWithdrawalAccountingForUploadPicture(int id, out int newUploadPicTimes)
        {
            newUploadPicTimes = 0;
            try
            {
                var data = GetAccountingById(id);
                data.UploadPicTimes += 1;
                newUploadPicTimes = data.UploadPicTimes;
                return UpdateWithdrawalBankAccounting(data);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteWithdrawalAccounting(int id, out string transId, out int seq, out int uploadPicTimes)
        {
            transId = string.Empty;
            seq = 0;
            uploadPicTimes = 0;

            try
            {
                var item = GetAccountingById(id);

                if (DeleteWithdrawalBankAccounting(item))
                {
                    transId = item.TransId;
                    seq = item.Seq;
                    uploadPicTimes = item.UploadPicTimes;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private string GetTimeString(TimeSpan ts)
        {
            var tempDays = "";
            var tempHours = "";
            var tempMinutes = "";
            var tempSeconds = "";
            var result = "";
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

        private IEnumerable<WithdrawalInfoAndAccountingListModel> GetWithdrawalInfoAndAccountingData(
            IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            string startDate,
            string endDate,
            string transId,
            string userName,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            return _withdrawalBankAccountingService.GetWithdrawalInfoAndAccountingList(
                JsonConvert.SerializeObject(siteIds),
                JsonConvert.SerializeObject(currencyIds),
                startDate,
                endDate,
                string.IsNullOrEmpty(transId) ? null : transId,
                string.IsNullOrEmpty(userName) ? null : userName,
                Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }

        private IEnumerable<WithdrawalInfoAndAccountingListForExportModel> GetWithdrawalInfoAndAccountingDataForExport(
            string siteIds,
            string currencyIds,
            string startDate,
            string endDate,
            string transId,
            string userName)
        {
            return _withdrawalBankAccountingService.GetWithdrawalInfoAndAccountingListForExport(
                siteIds,
                currencyIds,
                startDate,
                endDate,
                string.IsNullOrEmpty(transId) ? null : transId,
                string.IsNullOrEmpty(userName) ? null : userName);
        }

        private IEnumerable<GetWithdrawalBankAccountingModel> GetAccountingList(string transId, int siteId, int currencyId)
        {
            return _withdrawalBankAccountingService.GetAccountingListByTransId(transId, siteId, currencyId);
        }

        private GetWithdrawalBankAccountingModel GetAccountingById(int id)
        {
            return _withdrawalBankAccountingService.GetById(id);
        }

        private bool InsertWithdrawalBankAccounting(WithdrawalBankAccountingModel model)
        {
            return _withdrawalBankAccountingService.SetFlow(ActionType.Create, model);
        }

        private bool UpdateWithdrawalBankAccounting(WithdrawalBankAccountingModel model)
        {
            return _withdrawalBankAccountingService.SetFlow(ActionType.Update, model);
        }

        private bool DeleteWithdrawalBankAccounting(WithdrawalBankAccountingModel model)
        {
            return _withdrawalBankAccountingService.SetFlow(ActionType.Delete, model);
        }

        #endregion
    }
}