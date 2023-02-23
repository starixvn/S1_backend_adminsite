using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.WithdrawalBankAccounting;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text.RegularExpressions;
using BackendSite.Service.Library;
using BackendSite.Service.Model.ApiModel;
using static BackendSite.Controllers.ApiV2.Finance.Withdrawal.WithdrawalController;

namespace BackendSite.Service.DAL
{
    public class WithdrawalService
    {
        #region Fields

        private readonly IConfiguration _configuration;
        private readonly WalletService _walletService;
        private readonly WithdrawalBankAccountingService _withdrawalBankAccountingService;
        private readonly ILogger<WithdrawalService> _logger;
        private const int TypePosition = 1000;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        #endregion

        #region Constructors

        public WithdrawalService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _walletService = provider.GetRequiredService<WalletService>();
            _withdrawalBankAccountingService = provider.GetRequiredService<WithdrawalBankAccountingService>();
            _logger = provider.GetRequiredService<ILogger<WithdrawalService>>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        #endregion

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
            
            var tempWithdrawalMethod = (int)withdrawalMethod;
            if(withdrawalType.Count() > 1)
            {
                tempWithdrawalMethod = (int)WithdrawalMethodEnum.All;
            }
            else
            {
                if (!withdrawalType.ToList().Exists(x => x == (int)WithdrawalTypeEnum.LocalBank))
                {
                    tempWithdrawalMethod = (int)WithdrawalMethodEnum.All;
                }
            }
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            
            var p = new DynamicParameters(new
            {
                _SiteIds = JsonConvert.SerializeObject(selectSite),
                _WithdrawalTypes = JsonConvert.SerializeObject(withdrawalType),
                _StartDate = startDate,
                _EndDate = endDate,
                _DateType = (int)dateType == 0 ? (int)WithdrawalDateTypeEnum.CreateTime : (int)dateType,
                _WithdrawalId = withdrawalId,
                _TransId = WithdrawalId,
                _UserNames = userNames.Any() ? JsonConvert.SerializeObject(userNames): null,
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
                _WithdrawalMethod = tempWithdrawalMethod
            });

            #region GetCurrencyTotalAmount

            currencyAmount = conn.Query<TotalAmount>("Admin_WithdrawalTotalAmount_Get_V6", p, commandType: CommandType.StoredProcedure).ToList();
            
            #endregion

            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = conn.Query<WithdrawalRequest>("Admin_WithdrawalList_Get_V10", p, commandType: CommandType.StoredProcedure);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public WithdrawalRequest GetWithdrawalRequest(int withdrawalId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<WithdrawalRequest>("Admin_WithdrawalRequestTransId_Get_V5",
                new { WithdrawalId = withdrawalId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<WithdrawalComments> GetWithdrawalRequestComment(int withdrawalId, int siteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Query<WithdrawalComments>("WithdrawalRequestComment_Get_V2",
                new { WithdrawalId = withdrawalId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public int SetWithdrawalLog(int withdrawalId, int siteId, string comment, int status, string Operator,
            MySqlConnection conn = null, MySqlTransaction trans = null)
        {
            if (conn != null)
            {
                return conn.Execute("WithdrawalLog_Ins",
                    new { WithdrawalId = withdrawalId, SiteId = siteId, Comment = comment, Action = status, Operator },
                    trans, commandType: CommandType.StoredProcedure);
            }
            else
            {
                using var connection =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
                return connection.Execute("WithdrawalLog_Ins",
                    new { WithdrawalId = withdrawalId, SiteId = siteId, Comment = comment, Action = status, Operator },
                    commandType: CommandType.StoredProcedure);
            }
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
            ref List<string> currencyAmount)
        {
            var tempCurrencyTotalAmount = new List<string>();
            var p = new DynamicParameters();
            p.Add("_CustId", custId, dbType: DbType.Int32);
            p.Add("_TransId", transId, dbType: DbType.String);
            p.Add("_Status", status, dbType: DbType.Int32);
            p.Add("_StartDate", startDate.DateTime, dbType: DbType.DateTime);
            p.Add("_EndDate", endDate.DateTime, dbType: DbType.DateTime);

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));

            #region GetCurrencyTotalAmount

            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_WithdrawalTotalAmountByCust_Get_V3", p,
                commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }

            currencyAmount = tempCurrencyTotalAmount;

            #endregion
            p.Add("_PageSize", pageSize, dbType: DbType.Int32);
            p.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);


            var depositList = conn.Query<WithdrawalRequest>("Admin_WithdrawalListByCust_Get_V3", p,
                commandType: CommandType.StoredProcedure);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
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
           
            var tempWithdrawalMethod = (int)withdrawalMethod;
            if (withdrawalType.Count() > 1)
            {
                tempWithdrawalMethod = (int)WithdrawalMethodEnum.All;
            }
            else
            {
                if (!withdrawalType.ToList().Exists(x => x == (int)WithdrawalTypeEnum.LocalBank))
                {
                    tempWithdrawalMethod = (int)WithdrawalMethodEnum.All;
                }
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
                _WithdrawalMethod = tempWithdrawalMethod
            });

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var depositList = conn.Query<WithdrawalInfoForExport>("Admin_WithdrawalListExport_Get_V9", p,
                commandType: CommandType.StoredProcedure);
            return depositList;
        }

        public IEnumerable<WithdrawalKYC> GetWithdrawalKYC(int siteId)
        {
            return _mainDbClient.Query<WithdrawalKYC>("Admin_WithdrawalKyc_Get", new DynamicParameters(new { SiteId = siteId }));
        }

        public bool EditWithdrawalKYC(ParamsWithLog<WithdrawalKYCAmount> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new
            {
                input.SiteId,
                input.CurrencyId,
                input.After.Amount
            };

            var success = conn
                .Execute("Admin_WithdrawalKyc_Set", p, commandType: CommandType.StoredProcedure) > 0;

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

                var logSuccess = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }

            }

            trans.Rollback();

            return false;
        }

        public IEnumerable<WithdrawalMinInfo> GetWithdrawalLimitData(int SiteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var WithdrawalMinInfo = conn.Query<WithdrawalMinInfo>("Admin_WithdrawalMinInfo_Get", new { SiteId },
                commandType: CommandType.StoredProcedure);
            return WithdrawalMinInfo;
        }

        public bool InsertWithdrawalLimit(int SiteId, int CurrencyId, int WithdrawalMin, string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_WithdrawalMinInfo_Ins", new { SiteId, CurrencyId, Operator, WithdrawalMin },
                commandType: CommandType.StoredProcedure) > 0;
        }

        public bool UpdateWithdrawalLimit(int SiteId, int CurrencyId, int WithdrawalMin, string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_WithdrawalMinInfo_Upd", new { SiteId, CurrencyId, Operator, WithdrawalMin, },
                commandType: CommandType.StoredProcedure) > 0;
        }

        public bool DeleteWithdrawalLimit(int SiteId, int CurrencyId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_WithdrawalMinInfo_Del", new { SiteId, CurrencyId },
                commandType: CommandType.StoredProcedure) > 0;
        }

        public bool UpdateWithdrawalPicCount(string transId , int count)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_BankSlipPic_Ins_V2", new
                {
                    TransId = transId,
                    Count = count
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        public BankSlipPic GetWithdrawalPicCount(string transId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var WithdrawalMinInfo = conn.Query<BankSlipPic>("Admin_BankSlipPic_Get", new { TransId = transId },
                commandType: CommandType.StoredProcedure).FirstOrDefault();
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

            #region totalSummary

            currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_WithdrawalStatusTimeTotal_Get", props).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.Add(new TotalAmount()
                {
                    Currency = "Total",
                    Record = currencyTotalAmount.Sum(r => r.Record)
                });
            }

            #endregion

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

            using var conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("StartDate", startDate);
            props.Add("EndDate", endDate);
            props.Add("SelectSite", selectSite);
            props.Add("CurrencyIds", currencyIds);
            props.Add("WithdrawalId", tempWithdrawalId);
            props.Add("SelectStatus", selectStatus);
            props.Add("CustId", custId);
            return conn.Query<WithdrawalTimeList>("Admin_WithdrawalStatusTimeExport_Get_V4", props,
                commandType: CommandType.StoredProcedure).ToList();
        }

        public int SetWithdrawalInfoFlow(string transId,
            int siteId,
            string comment,
            string forStatus,
            string updater,
            WithdrawalStatusEnum status,
            string withdrawalInfo,
            int? withdrawalType,
            string accountingModelString,
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
                using var conn =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
                conn.Open();
                using var trans = conn.BeginTransaction();

                //step1:Set WithdrawalInfo
                result = SetWithdrawalInfo(withdrawalId, siteId, comment,
                    forStatus, updater, status,
                    /*Approved don't to Set WithdrawalInfo*/
                    (status == WithdrawalStatusEnum.Approved && withdrawalType != (int)WithdrawalTypeEnum.CryptoWallet 
                        ? null : withdrawalInfo),
                    withdrawalType, chargeFee,
                    out custId, out var userName, out amount, out paraLog,
                    conn, trans);

                if (result > 0)
                {
                    //step2: Set WithdrawalLog
                    if (!string.IsNullOrEmpty(accountingModelString))
                    {
                        comment = $"Comment :{comment} , AccountingModel :{accountingModelString}";
                    }
                    var logResult = SetWithdrawalLog(withdrawalId, siteId, comment, (int)status, updater, conn, trans);
                    var specialResult = 0;

                    if (logResult > 0)
                    {
                        int updateFlowEndResult; //For Reject AAnd Approved Use.
                        //step3: special Flow by Status.
                        switch (status)
                        {
                            case WithdrawalStatusEnum.Rejected:
                                var type = 2 + TypePosition; /*Rejected  Withdrawal 1002*/

                                updateFlowEndResult = SetWithdrawalInfoFlowEndTime(withdrawalId, conn, trans);

                                var balanceResult = _walletService.SetBalance(custId, siteId, userName, amount, type,
                                    $"Withdrawal Rejected: {amount}", transId, out _, out _, conn, trans);

                                specialResult = (updateFlowEndResult == 0 || balanceResult == 0) ? 0 : 1;
                                break;
                            
                            case WithdrawalStatusEnum.Draft:
                            case WithdrawalStatusEnum.Process:
                                
                                if (!string.IsNullOrEmpty(accountingModelString))
                                {
                                    var delResult = 1;
                                    var accountingList = _withdrawalBankAccountingService.GetByTransId(transId);
                                    if (accountingList.Any())
                                    {
                                        delResult = DelWithdrawalAccounting(transId,conn,trans) >= 0 ? 1 : 0;
                                    }

                                    if (delResult == 1)
                                    {
                                        specialResult = SetWithdrawalAccounting(transId,accountingModelString,userId,conn,trans) > 0 ? 1 : 0;
                                    }
                                }
                                else
                                {
                                    specialResult = 1;
                                }
                                
                                break;
                            
                            case WithdrawalStatusEnum.Approved:
                                var dummyResult = 0;
                                // var accountingResult = 0;
                                var withdrawalInfoData = JsonConvert.DeserializeObject<WithdrawalInfo>(withdrawalInfo);

                                updateFlowEndResult = SetWithdrawalInfoFlowEndTime(withdrawalId, conn, trans);

                                switch (withdrawalType)
                                {
                                    case (int)WithdrawalTypeEnum.OnlinePayment:
                                        dummyResult = _walletService.SetPayDummy(siteId,
                                            withdrawalInfoData.PayId ?? 0,
                                            withdrawalInfoData.CurrencyId ?? 0,
                                            -1 * amount,
                                            conn,
                                            trans);
                                        break;
                                    case (int)WithdrawalTypeEnum.LocalBank:
                                        dummyResult = _walletService.SetBankOffDummy(siteId,
                                            withdrawalInfoData.FromSysId ?? 0,
                                            withdrawalInfoData.FromAccountNo,
                                            withdrawalInfoData.CurrencyId ?? 0,
                                            -1 * amount,
                                            conn,
                                            trans);
                                        break;
                                    case (int)WithdrawalTypeEnum.CryptoWallet:
                                        dummyResult = 1;
                                        break;
                                }
                                
                                specialResult = updateFlowEndResult == 0 || dummyResult == 0 ? 0 : 1;

                                break;
                            
                            default:
                                specialResult = 1; //No special Flow status,Result is always success.
                                break;
                        }
                    }

                    if (logResult > 0 && specialResult > 0)
                    {
                        trans.Commit();
                        result = custId == 0 ? result : custId;
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
            string comment,
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
            MySqlConnection conn = null,
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
                _Comment = comment,
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

            if (conn == null)
            {
                //No execute in transaction
                result = _mainDbClient.Execute("Admin_WithdrawalInfo_Set_V2", p);
            }
            else
            {
                //Execute in transaction
                result = _mainDbClient.Execute("Admin_WithdrawalInfo_Set_V2", p, conn, trans);
            }

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
        public WithdrawalRequest GetWithdrawalInfo(int withdrawalId,
            string transId)
        {
            using var conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.QueryFirstOrDefault<WithdrawalRequest>("Admin_WithdrawalInfo_Get_V2",
                new { _WithdrawalId = withdrawalId, _TransId = transId }, commandType: CommandType.StoredProcedure);
        }

        private int SetWithdrawalInfoFlowEndTime(int withdrawalId,
            MySqlConnection conn = null,
            MySqlTransaction trans = null)
        {
            var p = new DynamicParameters(new
            {
                _WithdrawalId = withdrawalId
            });

            if (conn == null)
            {
                //No execute in transaction
                using var connection = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
                return connection.Execute("Admin_WithdrawalInfo_FlowEndTime_Set", p, commandType: CommandType.StoredProcedure);
            }
            else
            {
                //Execute in transaction
                return conn.Execute("Admin_WithdrawalInfo_FlowEndTime_Set", p, trans, commandType: CommandType.StoredProcedure);
            }
        }

        private int SetWithdrawalAccounting(string transId , string accountingModels, int userId , MySqlConnection conn = null, MySqlTransaction trans = null)
        {
            var p = new DynamicParameters(new
            {
                _Infos = accountingModels,
                _TransId = transId,
                _Creator = userId
            });

            if (conn == null)
            {
                //No execute in transaction
                using var connection = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
                return connection.Execute("Admin_WithdrawalAccountingForProcess_Ins", p, commandType: CommandType.StoredProcedure);
            }
            else
            {
                //Execute in transaction
                return conn.Execute("Admin_WithdrawalAccountingForProcess_Ins", p, trans, commandType: CommandType.StoredProcedure);
            }
        }
        
        private int DelWithdrawalAccounting(string transId , MySqlConnection conn = null, MySqlTransaction trans = null)
        {
            var p = new DynamicParameters();
            p.Add("_TransId", transId);
           
            if (conn == null)
            {
                //No execute in transaction
                using var connection = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
                return connection.Execute("Admin_WithdrawalBankAccountingByTransId_Del", p, commandType: CommandType.StoredProcedure);
            }
            else
            {
                //Execute in transaction
                return conn.Execute("Admin_WithdrawalBankAccountingByTransId_Del", p, trans, commandType: CommandType.StoredProcedure);
            }
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
            string withdrawalInfo, string prefix, MySqlConnection conn, MySqlTransaction trans,
            out decimal afterAmount, out string transId)
        {
            transId = "";
            afterAmount = 0;
            int toKyc = 0;

            var p = new DynamicParameters(new { CustId = custId, UserName = userName, CurrencyId = currencyId, SiteId = siteId, Amount = amount, Memo = "WithdrawalSubmission", WithdrawalInfo = withdrawalInfo, Prefix = $"W{prefix}", Operator = updater });
            p.Add("vTransId", dbType: DbType.String, direction: ParameterDirection.Output);
            p.Add("ToKyc", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            conn.Execute("Admin_WithdrawalInfo_Ins", p, transaction: trans, commandType: CommandType.StoredProcedure);
            transId = p.Get<string>("vTransId");
            toKyc = p.Get<byte?>("ToKyc") ?? 0;

            if (GetWithdrawalAvailableBalance(custId, conn, trans) < amount)
                return false;

            var result = _walletService.SetBalance(custId, siteId, userName, amount * -1, 1001, null, transId, out afterAmount, out _, conn, trans);
            if (result > 0)
            {
                var m = Regex.Match(transId, @"\d+");
                int.TryParse(m.Value, out var withdrawalId);
                var status = toKyc == 1 ? WithdrawalStatusEnum.ToKyc : WithdrawalStatusEnum.Pending;
                var logResult = SetWithdrawalLog(withdrawalId, siteId, memo, (int)status, updater, conn, trans);

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

        public decimal GetWithdrawalAvailableBalance(int custId, MySqlConnection conn = null, MySqlTransaction trans = null)
        {
            if (conn == null)
            {
                return _mainDbClient.QueryFirst<decimal>("Common_WithdrawalAvailableBalance_Get", new DynamicParameters(new { CustId = custId }));
            }
            else
            {
                return conn.ExecuteScalar<decimal>("Common_WithdrawalAvailableBalance_Get", new { CustId = custId }, transaction: trans, commandType: CommandType.StoredProcedure);
            }
        }

        public bool SetWithdrawalMessage(ApiRequestWithLog<WithdrawalMessageRequest> request)
        {
            var p = new DynamicParameters(new { _WithdrawalId = request.After.WithdrawalId, _Status = request.After.Status });
            return _mainDbClient.Execute("Admin_WithdrawalMessage_Set", p) > 0;
        }
    }
}
