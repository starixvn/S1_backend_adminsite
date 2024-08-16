using System.Data;
using System.Text.RegularExpressions;

using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Dapper;

using MySql.Data.MySqlClient;

using Newtonsoft.Json;

using static BackendSite.Controllers.ApiV2.Finance.Withdrawal.WithdrawalController;

namespace BackendSite.Service.DAL
{
    public class WithdrawalService
    {
        private readonly WalletService _walletService;
        private readonly WithdrawalBankAccountingService _withdrawalBankAccountingService;
        private readonly ILogger<WithdrawalService> _logger;
        private readonly BankDetailBLL _bankDetailBll;
        private const int TypePosition = 1000;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public WithdrawalService(IServiceProvider provider)
        {
            _walletService = provider.GetRequiredService<WalletService>();
            _withdrawalBankAccountingService = provider.GetRequiredService<WithdrawalBankAccountingService>();
            _logger = provider.GetRequiredService<ILogger<WithdrawalService>>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<WithdrawalRequest> GetWithdrawalRequestList(IEnumerable<int> selectSite,
            IEnumerable<int> withdrawalType,
            DateTime startDate,
            DateTime endDate,
            WithdrawalDateTypeEnum dateType,
            string WithdrawalId,
            IEnumerable<string> userNames,
            IEnumerable<int> currencyId,
            IEnumerable<int> status,
            IEnumerable<int> levelStatus,
            int startIndex,
            int pageSize,
            decimal startAmount,
            decimal endAmount,
            WithdrawalMethodEnum withdrawalMethod,
            IEnumerable<string> selectBankCode,
            IEnumerable<int> selectCalCode,
            string agent,
            out int totalPages,
            out int rowCount,
            ref List<TotalAmount> currencyAmount)
        {
            var withdrawalId = 0;
            if (!string.IsNullOrWhiteSpace(WithdrawalId))
            {
                var m = Regex.Match(WithdrawalId, @"\d+");
                int.TryParse(m.Value, out withdrawalId);
            }

            var p = new DynamicParameters(new
            {
                _SiteIds = JsonConvert.SerializeObject(selectSite),
                _WithdrawalTypes = JsonConvert.SerializeObject(withdrawalType),
                _StartDate = startDate,
                _EndDate = endDate,
                _DateType = (int)dateType == 0 ? (int)WithdrawalDateTypeEnum.CreateTime : (int)dateType,
                _WithdrawalId = withdrawalId,
                _TransId = WithdrawalId,
                _UserNames = userNames.Any() ? JsonConvert.SerializeObject(userNames) : null,
                _CurrencyIds = currencyId.Any() ? JsonConvert.SerializeObject(currencyId) : null,
                _Statuses = JsonConvert.SerializeObject(status),
                _VipStatuses = levelStatus.Any() ? JsonConvert.SerializeObject(levelStatus) : null,
                _SelectBankCode = selectBankCode.Any() ? JsonConvert.SerializeObject(selectBankCode) : null,
                _SelectCalCode = selectCalCode.Any() ? JsonConvert.SerializeObject(selectCalCode) : null,
                _StartIndex = startIndex,
                _PageSize = pageSize,
                _StartAmount = startAmount,
                _EndAmount = endAmount,
                _Agent = string.IsNullOrEmpty(agent) ? null : agent,
                _WithdrawalMethod = withdrawalMethod
            });

            currencyAmount = _reportDbClient.RetryQuery<TotalAmount>("Admin_WithdrawalTotalAmount_Get_V7", p).ToList();

            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.RetryQuery<WithdrawalRequest>("Admin_WithdrawalList_Get_V12", p);

            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public WithdrawalRequest GetWithdrawalRequest(int withdrawalId)
        {
            return _mainDbClient.QueryFirst<WithdrawalRequest>("Admin_WithdrawalRequestTransId_Get_V5",
                new { WithdrawalId = withdrawalId });
        }

        public IEnumerable<WithdrawalComments> GetWithdrawalRequestComment(int withdrawalId, int siteId)
        {
            return _mainDbClient.Query<WithdrawalComments>("WithdrawalRequestComment_Get_V2",
                new { WithdrawalId = withdrawalId, SiteId = siteId });
        }

        public int SetWithdrawalLog(int withdrawalId, int siteId, string comment, int status, string Operator, MySqlTransaction trans = null)
        {
            const string spName = "WithdrawalLog_Ins";
            var parameter = new { WithdrawalId = withdrawalId, SiteId = siteId, Comment = comment, Action = status, Operator };

            return trans != null
                ? _mainDbClient.Execute(spName, trans, parameter)
                : _mainDbClient.Execute(spName, parameter);
        }

        public IEnumerable<WithdrawalRequest> GetWithdrawalListByCust(int custId,
            string transId,
            int? status,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount,
            out decimal totalAmount,
            out decimal totalApproveAmount,
            out int totalApproveRow)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId, dbType: DbType.Int32);
            p.Add("_TransId", transId, dbType: DbType.String);
            p.Add("_Status", status, dbType: DbType.Int32);
            p.Add("_StartDate", startDate.DateTime, dbType: DbType.DateTime);
            p.Add("_EndDate", endDate.DateTime, dbType: DbType.DateTime);
            p.Add("_PageSize", pageSize, dbType: DbType.Int32);
            p.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_TotalAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("_TotalApproveAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("_TotalApproveRow", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var depositList = _mainDbClient.Query<WithdrawalRequest>("Admin_WithdrawalListByCust_Get_V4", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            totalAmount = p.Get<decimal?>("_TotalAmount") ?? 0;
            totalApproveAmount = p.Get<decimal?>("_TotalApproveAmount") ?? 0;
            totalApproveRow = p.Get<int?>("_TotalApproveRow") ?? 0;

            return depositList;
        }

        public IEnumerable<WithdrawalInfoForExport> GetWithdrawalExportList(
            string selectSite,
            string withdrawalType,
            DateTime startDate,
            DateTime endDate,
            WithdrawalDateTypeEnum dateType,
            string WithdrawalId,
            string userName,
            string currencyId,
            string status,
            string levelStatus,
            decimal startAmount,
            decimal endAmount,
            string selectBankCode,
            string selectCalCode,
            string agent,
            WithdrawalMethodEnum withdrawalMethod)
        {
            var withdrawalId = 0;
            if (!string.IsNullOrWhiteSpace(WithdrawalId))
            {
                var m = Regex.Match(WithdrawalId, @"\d+");
                int.TryParse(m.Value, out withdrawalId);
            }

            var p = new DynamicParameters(new
            {
                _SiteIds = selectSite,
                _WithdrawalTypes = withdrawalType,
                _StartDate = startDate,
                _EndDate = endDate,
                _DateType = (int)dateType == 0 ? (int)WithdrawalDateTypeEnum.CreateTime : (int)dateType,
                _WithdrawalId = withdrawalId,
                _TransId = WithdrawalId,
                _UserNames = userName,
                _CurrencyIds = currencyId,
                _Statuses = status,
                _VipStatuses = levelStatus,
                _StartAmount = startAmount,
                _EndAmount = endAmount,
                _SelectBankCode = selectBankCode,
                _SelectCalCode = selectCalCode,
                _Agent = agent,
                _WithdrawalMethod = withdrawalMethod
            });

            var depositList = _reportDbClient.Query<WithdrawalInfoForExport>("Admin_WithdrawalListExport_Get_V10", p);
            return depositList;
        }

        public IEnumerable<WithdrawalKYC> GetWithdrawalKYC(int siteId)
        {
            return _mainDbClient.Query<WithdrawalKYC>("Admin_WithdrawalKyc_Get", new DynamicParameters(new { SiteId = siteId }));
        }

        public bool EditWithdrawalKYC(ParamsWithLog<WithdrawalKYCAmount> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new
            {
                input.SiteId,
                input.CurrencyId,
                input.After.Amount
            };

            var success = _mainDbClient.Execute("Admin_WithdrawalKyc_Set", trans, p) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.WithdrawalKycControl);
                logParam.Add("_ActionType", (int)ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
                logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
                logParam.Add("_Operator", input.Operator);

                var logSuccess = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;

                if (logSuccess)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return true;
                }

            }

            _mainDbClient.RollbackAndDisposeConnection(conn, trans);

            return false;
        }

        public IEnumerable<WithdrawalMinInfo> GetWithdrawalLimitData(int SiteId)
        {
            var WithdrawalMinInfo = _mainDbClient.Query<WithdrawalMinInfo>("Admin_WithdrawalMinInfo_Get", new { SiteId });
            return WithdrawalMinInfo;
        }

        public bool InsertWithdrawalLimit(int SiteId, int CurrencyId, int WithdrawalMin, string Operator)
        {
            return _mainDbClient.Execute("Admin_WithdrawalMinInfo_Ins", new { SiteId, CurrencyId, Operator, WithdrawalMin }) > 0;
        }

        public bool UpdateWithdrawalLimit(int SiteId, int CurrencyId, int WithdrawalMin, string Operator)
        {
            return _mainDbClient.Execute("Admin_WithdrawalMinInfo_Upd", new { SiteId, CurrencyId, Operator, WithdrawalMin, }) > 0;
        }

        public bool DeleteWithdrawalLimit(int SiteId, int CurrencyId)
        {
            return _mainDbClient.Execute("Admin_WithdrawalMinInfo_Del", new { SiteId, CurrencyId }) > 0;
        }

        public bool UpdateWithdrawalPicCount(string transId, int count)
        {
            return _mainDbClient.Execute("Admin_BankSlipPic_Ins_V2", new
            {
                TransId = transId,
                Count = count
            }) > 0;
        }

        public BankSlipPic GetWithdrawalPicCount(string transId)
        {
            var WithdrawalMinInfo = _mainDbClient.Query<BankSlipPic>("Admin_BankSlipPic_Get", new { TransId = transId }).FirstOrDefault();
            return WithdrawalMinInfo;
        }

        public WithdrawalTimeData WithdrawalTimeReportGet(string selectSite,
            string withdrawalId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            string selectStatus,
            string currencyIds,
            int custId,
            int pageNumber,
            int pageSize,
            out List<TotalAmount> currencyTotalAmount)

        {

            var searchData = new WithdrawalTimeData();
            var props = new DynamicParameters();
            int.TryParse(withdrawalId, out var tempWithdrawalId);

            props.Add("SelectSite", selectSite);
            props.Add("WithdrawalId", tempWithdrawalId);
            props.Add("StartDate", startDate.DateTime);
            props.Add("EndDate", endDate.DateTime);
            props.Add("SelectStatus", selectStatus);
            props.Add("CurrencyIds", currencyIds);
            props.Add("CustId", custId);

            currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_WithdrawalStatusTimeTotal_Get", props).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.Add(new TotalAmount()
                {
                    Currency = "Total",
                    Record = currencyTotalAmount.Sum(r => r.Record)
                });
            }

            props.Add("StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
            props.Add("PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", Math.Abs(pageSize));
            var result = _reportDbClient.Query<WithdrawalTimeList>("Admin_WithdrawalStatusTime_Get_V4", props).ToList();

            searchData.WithdrawalTimeList = result;
            searchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return searchData;
        }

        public List<WithdrawalTimeList> GetExportData(string selectSite, string withdrawalId, DateTime startDate,
            DateTime endDate, string selectStatus, string currencyIds, int custId)
        {
            int.TryParse(withdrawalId, out var tempWithdrawalId);

            var props = new DynamicParameters();
            props.Add("StartDate", startDate);
            props.Add("EndDate", endDate);
            props.Add("SelectSite", selectSite);
            props.Add("CurrencyIds", currencyIds);
            props.Add("WithdrawalId", tempWithdrawalId);
            props.Add("SelectStatus", selectStatus);
            props.Add("CustId", custId);
            return _reportDbClient.Query<WithdrawalTimeList>("Admin_WithdrawalStatusTimeExport_Get_V4", props).ToList();
        }

        public int  SetWithdrawalInfoFlow(string transId,
            int siteId,
            string comment,
            string forStatus,
            string updater,
            WithdrawalStatusEnum status,
            string withdrawalInfo,
            int? withdrawalType,
            IEnumerable<WithdrawalAccountingModel> accountingModels,
            out int custId,
            out decimal amount,
            int userId = 0,
            bool hasPic = false,
            decimal chargeFee = 0)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);

            custId = 0;
            amount = 0m;
            var result = 0;
            var paraLog = new Dictionary<string, string>();
            try
            {
                using var conn = _mainDbClient.GetMySqlConnection();
                conn.Open();
                using var trans = conn.BeginTransaction();

                //step1:Set WithdrawalInfo
                result = SetWithdrawalInfo(withdrawalId, siteId,
                    forStatus, updater, status,
                    /*Approved don't to Set WithdrawalInfo*/
                    (status == WithdrawalStatusEnum.Approved && withdrawalType != (int)WithdrawalTypeEnum.CryptoWallet
                        ? null : withdrawalInfo),
                    withdrawalType, chargeFee,
                    out custId, out var userName, out amount, out paraLog, trans);

                var accountingModelString = SetAccountingString(accountingModels);
                var beforeAccountingModels = _withdrawalBankAccountingService.GetByTransId(transId);

                if (result > 0)
                {
                    //step2: Set WithdrawalLog
                    if (!string.IsNullOrEmpty(accountingModelString))
                    {
                        if (!string.IsNullOrEmpty(comment))
                        {
                            comment = $"Comment : {comment}, ";
                        }

                        var compare = accountingModels.Except(beforeAccountingModels, new WithdrawalAccountingModelComparer());

                        if (compare.Any())
                        {
                            comment += $"AccountingModel :{accountingModelString}";
                        }
                    }

                    var logResult = SetWithdrawalLog(withdrawalId, siteId, comment, (int)status, updater, trans);
                    var specialResult = 0;

                    if (logResult > 0)
                    {
                        int updateFlowEndResult; //For Reject AAnd Approved Use.
                        //step3: special Flow by Status.
                        switch (status)
                        {
                            case WithdrawalStatusEnum.Rejected:
                                var type = 2 + TypePosition; /*Rejected  Withdrawal 1002*/

                                var balanceResult = _walletService.SetBalance(custId, siteId, userName, amount, type,
                                    $"Withdrawal Rejected: {amount}", transId, out _, out _, trans);

                                specialResult = balanceResult == 0 ? 0 : 1;
                                break;

                            case WithdrawalStatusEnum.Draft:
                            case WithdrawalStatusEnum.Process:

                                if (!string.IsNullOrEmpty(accountingModelString))
                                {
                                    var delResult = 1;
                                    if (beforeAccountingModels.Any())
                                    {
                                        delResult = DelWithdrawalAccounting(transId, trans) >= 0 ? 1 : 0;
                                    }

                                    if (delResult == 1)
                                    {
                                        specialResult = SetWithdrawalAccounting(transId, accountingModelString, userId, trans) > 0 ? 1 : 0;
                                    }
                                }
                                else
                                {
                                    specialResult = 1;
                                }

                                break;

                            default:
                                specialResult = 1; //No special Flow status,Result is always success.
                                break;
                        }
                    }

                    if (logResult > 0 && specialResult > 0)
                    {
                        _mainDbClient.CommitAndDisposeConnection(conn, trans);
                        result = custId == 0 ? result : custId;
                    }
                    else
                    {
                        _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    }
                }
            }
            catch (Exception ex)
            {
                SetWithdrawalLog(withdrawalId, siteId, ex.Message, (int)WithdrawalStatusEnum.Error, updater);
                _logger.LogError(ex,
                    $"TransactionFlow:SetWithdrawalInfo,Para:{JsonConvert.SerializeObject(paraLog)}");
            }

            return result;
        }

        private int SetWithdrawalInfo(int withdrawalId,
            int siteId,
            string forStatus,
            string updater,
            WithdrawalStatusEnum status,
            string withdrawalInfo,
            int? withdrawalType,
            decimal chargeFee,
            out int custId,
            out string userName,
            out decimal amount,
            out Dictionary<string, string> paraLog,
            MySqlTransaction trans = null)
        {
            custId = 0;
            userName = string.Empty;
            amount = 0m;
            paraLog = new Dictionary<string, string>();
            var result = 0;

            var p = new DynamicParameters(new
            {
                _WithdrawalId = withdrawalId,
                _SiteId = siteId,
                _ForStatus = forStatus,
                _Status = (int)status,
                _WithdrawalInfo = withdrawalInfo,
                _WithdrawalType = withdrawalType,
                _ChargeFee = chargeFee,
                _RefId = (string)null,
                _Operator = updater
            });
            p.Add("_CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_UserName", dbType: DbType.String, direction: ParameterDirection.Output);
            p.Add("_Amount", dbType: DbType.Decimal, direction: ParameterDirection.Output);

            const string spName = "Service_WithdrawalInfo_Set";
            result = trans != null
                ? _mainDbClient.Execute(spName, trans, p)
                : _mainDbClient.Execute(spName, p);

            custId = p.Get<int?>("_CustId") ?? 0;
            userName = p.Get<string>("_UserName");
            amount = p.Get<decimal>("_Amount");

            foreach (var paraName in p.ParameterNames)
            {
                var value = Convert.ToString(p.Get<object>(paraName));
                paraLog.Add(paraName, value);
            }

            return result;
        }

        /// <summary>
        /// 取得單筆提款單資訊
        /// </summary>
        /// <param name="withdrawalId">提款單ID</param>
        /// <param name="transId">提款單編號</param>
        /// <returns></returns>
        public WithdrawalRequest GetWithdrawalInfo(int withdrawalId, string transId)
        {
            return _mainDbClient.QueryFirst<WithdrawalRequest>("Admin_WithdrawalInfo_Get_V2",
                new { _WithdrawalId = withdrawalId, _TransId = transId });
        }

        private int SetWithdrawalAccounting(string transId, string accountingModels, int userId, MySqlTransaction trans = null)
        {
            var p = new DynamicParameters(new
            {
                _Infos = accountingModels,
                _TransId = transId,
                _Creator = userId
            });
            const string spName = "Admin_WithdrawalAccountingForProcess_Ins";
            return trans != null
                ? _mainDbClient.Execute(spName, trans, p)
                : _mainDbClient.Execute(spName, p);
        }

        private int DelWithdrawalAccounting(string transId, MySqlTransaction trans = null)
        {
            var p = new DynamicParameters();
            p.Add("_TransId", transId);
            const string spName = "Admin_WithdrawalBankAccountingByTransId_Del";
            return trans != null
                ? _mainDbClient.Execute(spName, trans, p)
                : _mainDbClient.Execute(spName, p);
        }

        public bool IsExistRunningWithdrawalInfo(int custId)
        {
            return _mainDbClient.QueryFirst<bool>("Member_WithdrawalInfoCheck_Get",
                new DynamicParameters(new { CustId = custId }));
        }

        public (decimal Amount, int Count) GetDailyWithdrawalSummary(int custId)
        {
            var result = _mainDbClient.Query<dynamic>("Member_DailyCustomerWithdrawalInfo_Get",
                new DynamicParameters(new { CustId = custId })).Select(x => (IDictionary<string, object>)x).First();
            return (Convert.ToDecimal(result["Amount"]), Convert.ToInt32(result["Count"]));
        }

        public bool CreateWithdrawalInfo(int custId, string userName, int currencyId,
            int siteId, decimal amount, string memo, string updater,
            string withdrawalInfo, string prefix, MySqlTransaction trans,
            out decimal afterAmount, out string transId)
        {
            transId = "";
            afterAmount = 0;
            int toKyc = 0;

            var p = new DynamicParameters(new { CustId = custId, UserName = userName, CurrencyId = currencyId, SiteId = siteId, Amount = amount, Memo = "WithdrawalSubmission", WithdrawalInfo = withdrawalInfo, Prefix = $"W{prefix}", Operator = updater });
            p.Add("vTransId", dbType: DbType.String, direction: ParameterDirection.Output);
            p.Add("ToKyc", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            _mainDbClient.Execute("Admin_WithdrawalInfo_Ins", trans, p);
            transId = p.Get<string>("vTransId");
            toKyc = p.Get<byte?>("ToKyc") ?? 0;

            if (GetWithdrawalAvailableBalance(custId) < amount)
                return false;

            var result = _walletService.SetBalance(custId, siteId, userName, amount * -1, 1001, null, transId, out afterAmount, out _, trans);
            if (result > 0)
            {
                var m = Regex.Match(transId, @"\d+");
                int.TryParse(m.Value, out var withdrawalId);
                var status = toKyc == 1 ? WithdrawalStatusEnum.ToKyc : WithdrawalStatusEnum.Pending;
                var logResult = SetWithdrawalLog(withdrawalId, siteId, memo, (int)status, updater, trans);

                if (logResult > 0)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public decimal GetWithdrawalAvailableBalance(int custId)
        {
            return _mainDbClient.QueryFirst<decimal>("Common_WithdrawalAvailableBalance_Get", new DynamicParameters(new { CustId = custId }));
        }

        public bool SetWithdrawalMessage(ApiRequestWithLog<WithdrawalMessageRequest> request)
        {
            var p = new DynamicParameters(new { _WithdrawalId = request.After.WithdrawalId, _Status = request.After.Status });
            return _mainDbClient.Execute("Admin_WithdrawalMessage_Set", p) > 0;
        }

        public IEnumerable<WithdrawalMessage> GetWithdrawalMessage(IEnumerable<int> withdrawalIds)
        {
            var props = new DynamicParameters();
            props.Add("_WithdrawalIds", JsonConvert.SerializeObject(withdrawalIds));

            return _mainDbClient.Query<WithdrawalMessage>("Admin_WithdrawalMessage_Get", props);
        }

        public async Task<bool> SetWithdrawalInfoOnUser(string transId, string onUser)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            var p = new DynamicParameters();
            p.Add("_WithdrawalId", withdrawalId);
            p.Add("_OnUser", onUser);
            var result = await _mainDbClient.ExecuteAsync("Admin_WithdrawalInfoOnUser_Set", p) > 0;

            return result;
        }

        public async Task<bool> SetWithdrawalAccountCounter(int custId, string accountNo, WithdrawalAccountTypeEnum accountType, int counter)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            p.Add("_AccountNo", accountNo);
            p.Add("_AccountType", (int)accountType);
            p.Add("_Counter", counter);
            return await _mainDbClient.ExecuteAsync("Admin_WithdrawalAccountCounter_Set", p) > 0;
        }

        public async Task<bool> SetWithdrawalProcessWay(int withdrawalId, WithdrawalInfoProcessWayEnum processWay)
        {
            var p = new DynamicParameters();
            p.Add("_WithdrawalId", withdrawalId);
            p.Add("_ProcessWay", (int)processWay);
            return await _mainDbClient.ExecuteAsync("Admin_WithdrawalInfoProcessWay_Set", p) > 0;
        }

        #region private

        private string SetAccountingString(IEnumerable<WithdrawalAccountingModel> accountingModels)
        {
            string accountingModelString = null;
            if (accountingModels != null && accountingModels.Any())
            {
                var bankAccountList = _bankDetailBll.GetOfflineBankAccount();
                var seq = 1;
                foreach (var model in accountingModels)
                {
                    model.Seq = seq;
                    model.DisplayName = bankAccountList.FirstOrDefault(x => x.BankId == model.BankId)?.DisplayName;
                    model.Amount = Math.Round(model.Amount, 4);
                    model.ChargeFee = Math.Round(model.ChargeFee, 4);
                    seq += 1;
                }
                accountingModelString = JsonConvert.SerializeObject(accountingModels);
            }
            return accountingModelString;
        }

        private class WithdrawalAccountingModelComparer : IEqualityComparer<WithdrawalAccountingModel>
        {
            public bool Equals(WithdrawalAccountingModel t1, WithdrawalAccountingModel t2)
            {
                var isSame = t1.Seq == t2.Seq &&
                             t1.BankId == t2.BankId &&
                             t1.Amount == t2.Amount &&
                             t1.ChargeFee == t2.ChargeFee;
                return isSame;
            }

            public int GetHashCode(WithdrawalAccountingModel t)
            {
                return t.ToString().GetHashCode();
            }
        }

        #endregion

    }
}
