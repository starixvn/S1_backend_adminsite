using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.WithdrawalBankAccounting;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using BackendSite.Service.Library;
using Microsoft.Extensions.DependencyInjection;

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
            string WithdrawalId,
            string UserName,
            IEnumerable<int> currencyId,
            IEnumerable<int> status,
            IEnumerable<int> levelstatus,
            int startIndex,
            int pageSize,
            int amounttype,
            string amount,
            out int totalPages,
            out int rowCount,
            ref List<string> CurrencyAmount)
        {
            var SelectSite = JsonConvert.SerializeObject(selectSite);
            var WithdrawalType = JsonConvert.SerializeObject(withdrawalType);
            var CurrencyId = JsonConvert.SerializeObject(currencyId);
            var Status = JsonConvert.SerializeObject(status);
            var VipStatus = JsonConvert.SerializeObject(levelstatus);
            var withdrawalId = 0;
            var tempCurrencyTotalAmount = new List<string>();
            if (!string.IsNullOrWhiteSpace(WithdrawalId))
            {
                var m = Regex.Match(WithdrawalId, @"\d+");
                int.TryParse(m.Value, out withdrawalId);
            }
            decimal tempAmount = 0;
            if (!string.IsNullOrWhiteSpace(amount))
            {
                tempAmount = Convert.ToDecimal(amount);
            }
            using var conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var p = new DynamicParameters(new
            {
                _SiteIds = SelectSite,
                _WithdrawalTypes = WithdrawalType,
                _StartDate = startDate,
                _EndDate = endDate,
                _WithdrawalId = withdrawalId,
                _TransId = WithdrawalId,
                _UserName = UserName,
                _CurrencyIds = CurrencyId,
                _Statuses = Status,
                _VipStatuses = VipStatus,
                _StartIndex = startIndex,
                _PageSize = pageSize,
                _AmountType = amounttype,
                _Amount = tempAmount
            });

            #region GetCurrencyTotalAmount

            var CurrencyTotalAmount =
                conn.Query<TotalAmount>("Admin_WithdrawalTotalAmount_Get_V3", p, commandType: CommandType.StoredProcedure)
                    .ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }

            CurrencyAmount = tempCurrencyTotalAmount;

            #endregion

            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = conn.Query<WithdrawalRequest>("Admin_WithdrawalList_Get_V6", p,
                commandType: CommandType.StoredProcedure);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public WithdrawalRequest GetWithdrawalRequest(int withdrawalId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<WithdrawalRequest>("Admin_WithdrawalRequestTransId_Get_V4",
                new { WithdrawalId = withdrawalId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Comments> GetWithdrawalRequestComment(int withdrawalId, int siteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Query<Comments>("WithdrawalRequestComment_Get_V2",
                new { WithdrawalId = withdrawalId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        private int SetWithdrawalLog(int withdrawalId, int siteId, string comment, int status, string Operator,
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

        public IEnumerable<WithdrawalInfoForExport> GetWithdrawalExportList(string selectSite, string withdrawalType,
            DateTime startDate, DateTime endDate, string WithdrawalId, string UserName, string currencyId, string status,
            string LevelStatus, int AmountType, string Amount)
        {
            var SelectSite = "[" + selectSite + "]";
            var WithdrawalType = "[" + withdrawalType + "]";
            var CurrencyId = "[" + currencyId + "]";
            var Status = "[" + status + "]";
            var VipStatus = $"[{LevelStatus}]";
            var withdrawalId = 0;
            if (!string.IsNullOrWhiteSpace(WithdrawalId))
            {
                var m = Regex.Match(WithdrawalId, @"\d+");
                int.TryParse(m.Value, out withdrawalId);
            }
            decimal tempAmount = 0;
            if (!string.IsNullOrWhiteSpace(Amount))
            {
                tempAmount = Convert.ToDecimal(Amount);
            }
            var p = new DynamicParameters(new
            {
                 _SiteIds = SelectSite,
                _WithdrawalTypes = WithdrawalType,
                _StartDate = startDate,
                _EndDate = endDate,
                _WithdrawalId = withdrawalId,
                _TransId = WithdrawalId,
                _UserName = UserName,
                _CurrencyIds = CurrencyId,
                _Statuses = Status,
                _VipStatuses = VipStatus,
                _AmountType = AmountType,
                _Amount = tempAmount
            });

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var depositList = conn.Query<WithdrawalInfoForExport>("Admin_WithdrawalListExport_Get_V7", p,
                commandType: CommandType.StoredProcedure);
            return depositList;
        }

        public IEnumerable<WithdrawalKYC> GetWithdrawalKYC(int siteId)
        {
            return _mainDbClient.Query<WithdrawalKYC>("Admin_WithdrawalKyc_Get", new DynamicParameters(new { SiteId = siteId }));
        }

        public bool EditWithdrawalKYC(WithdrawalKYC withdrawalKYC)
        {
            return _mainDbClient.Execute("Admin_WithdrawalKyc_Set", new DynamicParameters(new { withdrawalKYC.SiteId, withdrawalKYC.CurrencyId, withdrawalKYC.Amount })) > 0;
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

        public bool UpdateWithdrawalPicCount(string transId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_BankSlipPic_Ins", new { TransId = transId },
                commandType: CommandType.StoredProcedure) > 0;
        }

        public BankSlipPic GetWithdrawalPicCount(string transId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var WithdrawalMinInfo = conn.Query<BankSlipPic>("Admin_BankSlipPic_Get", new { TransId = transId },
                commandType: CommandType.StoredProcedure).FirstOrDefault();
            return WithdrawalMinInfo;
        }

        public WithdrawalTimeData WithdrawalTimeReportGet(WithdrawalTimeReportPara para)
        {
            var selectSite = JsonConvert.SerializeObject(para.SiteIds);
            var selectStatus = new List<int>();
            if (para.Status == 0)
            {
                selectStatus.Add(3);
                selectStatus.Add(4);
                selectStatus.Add(5);
                selectStatus.Add(6);
                selectStatus.Add(7);
            }
            else
            {
                selectStatus.Add(para.Status);
            }

            var searchData = new WithdrawalTimeData();
            var props = new DynamicParameters();
            int.TryParse(para.WithdrawalId, out var tempWithdrawalId);

            props.Add("SelectSite", selectSite);
            props.Add("WithdrawalId", tempWithdrawalId);
            props.Add("StartDate", para.StartDate.DateTime);
            props.Add("EndDate", para.EndDate.DateTime);
            props.Add("SelectStatus", JsonConvert.SerializeObject(selectStatus));
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("StartIndex", (Math.Abs(para.PageNumber) - 1) * Math.Abs(para.PageSize), DbType.Int32, ParameterDirection.Input);
            props.Add("PageNumber", Math.Abs(para.PageNumber), DbType.Int32, ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", Math.Abs(para.PageSize));
            var result = _reportDbClient.Query<WithdrawalTimeList>("Admin_WithdrawalStatusTime_Get_V3", props).ToList();

            searchData.WithdrawalTimeList = result;
            searchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return searchData;
        }

        public List<WithdrawalTimeList> GetExportData(IEnumerable<int> selectSite, string withdrawalId, DateTime startDate,
            DateTime endDate, int status, int currencyId)
        {
            var selectSiteString = JsonConvert.SerializeObject(selectSite);
            var selectStatus = new List<int>();
            if (status == 0)
            {
                selectStatus.Add(3);
                selectStatus.Add(4);
                selectStatus.Add(5);
                selectStatus.Add(6);
                selectStatus.Add(7);
            }
            else
            {
                selectStatus.Add(status);
            }

            var selectStatusString = JsonConvert.SerializeObject(selectStatus);
            int.TryParse(withdrawalId, out var tempWithdrawalId);

            using var conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("StartDate", startDate);
            props.Add("EndDate", endDate);
            props.Add("SelectSite", selectSiteString);
            props.Add("CurrencyId", currencyId);
            props.Add("WithdrawalId", tempWithdrawalId);
            props.Add("SelectStatus", selectStatusString);
            return conn.Query<WithdrawalTimeList>("Admin_WithdrawalStatusTimeExport_Get_V3", props,
                commandType: CommandType.StoredProcedure).ToList();
        }

        public int SetWithdrawalInfoFlow(string transId,
            int siteId,
            string comment,
            string forStatus,
            string updater,
            WithdrawalStatus status,
            string withdrawalInfo,
            int? withdrawalType,
            out int custId,
            out decimal amount,
            int userId = 0,
            bool hasPic = false)
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
                    (status == WithdrawalStatus.Approved ? null : withdrawalInfo),
                    withdrawalType,
                    out custId, out var userName, out amount, out paraLog,
                    conn, trans);

                if (result > 0)
                {
                    //step2: Set WithdrawalLog
                    var logResult = SetWithdrawalLog(withdrawalId, siteId, comment, (int)status, updater, conn, trans);
                    var specialResult = 0;

                    if (logResult > 0)
                    {
                        var updateFlowEndResult = 0; //For Reject AAnd Approved Use.
                        //step3: special Flow by Status.
                        switch (status)
                        {
                            case WithdrawalStatus.Rejected:
                                var balanceResult = 0;
                                var type = 2 + TypePosition; /*Rejected  Withdrawal 1002*/

                                updateFlowEndResult = SetWithdrawalInfoFlowEndTime(withdrawalId, conn, trans);

                                balanceResult = _walletService.SetBalance(custId, siteId, userName, amount, type,
                                $"Withdrawal Rejected: {amount}", transId, out var afterAmount,
                                out var isFirstDeposit, conn, trans);

                                specialResult = (updateFlowEndResult == 0 || balanceResult == 0) ? 0 : 1;


                                break;
                            case WithdrawalStatus.Approved:
                                var dummyResult = 0;
                                var accountingResult = 0;
                                var withdrawalInfoData = JsonConvert.DeserializeObject<WithdrawalInfo>(withdrawalInfo);

                                updateFlowEndResult = SetWithdrawalInfoFlowEndTime(withdrawalId, conn, trans);

                                switch (withdrawalType)
                                {
                                    case (int)WithdrawalType.OnlinePayment:
                                        dummyResult = _walletService.SetPayDummy(siteId,
                                            withdrawalInfoData.PayId ?? 0,
                                            withdrawalInfoData.CurrencyId ?? 0,
                                            -1 * amount,
                                            conn,
                                            trans);
                                        break;
                                    case (int)WithdrawalType.LocalBank:
                                        dummyResult = _walletService.SetBankOffDummy(siteId,
                                            withdrawalInfoData.FromSysId ?? 0,
                                            withdrawalInfoData.FromAccountNo,
                                            withdrawalInfoData.CurrencyId ?? 0,
                                            -1 * amount,
                                            conn,
                                            trans);
                                        break;
                                    default:
                                        break;
                                }

                                if (dummyResult > 0)
                                {
                                    //Insert Withdrawal Accounting
                                    var withdrawalInfoItem = GetWithdrawalInfo(0, transId);
                                    if (withdrawalType == (int)WithdrawalType.LocalBank)
                                    {
                                        var model = new WithdrawalBankAccountingModel()
                                        {
                                            TransId = withdrawalInfoItem.TransId,
                                            Seq = 1,
                                            BankAccountNo = withdrawalInfoItem.FromAccountNo,
                                            Amount = withdrawalInfoItem.Amount,
                                            ChargeFee = withdrawalInfoItem.ChargeFee,
                                            UploadPicTimes = hasPic ? 1 : 0,
                                            Creator = userId
                                        };
                                        if (_withdrawalBankAccountingService.Insert(model, conn, trans) > 0
                                            && _withdrawalBankAccountingService.InsertLog((int)ActionType.Create, model, conn, trans) > 0)
                                        {
                                            accountingResult = 1; //InsertAccounting Success.
                                        }
                                        else
                                        {
                                            accountingResult = 0; //InsertAccounting fail.
                                        }
                                    }
                                }

                                specialResult = (updateFlowEndResult == 0 || dummyResult == 0 || accountingResult == 0) ? 0 : 1;

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
                SetWithdrawalLog(withdrawalId, siteId, ex.Message, (int)WithdrawalStatus.Error, updater);
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
            WithdrawalStatus status,
            string withdrawalInfo,
            int? withdrawalType,
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
                WithdrawalId = withdrawalId,
                SiteId = siteId,
                Comment = comment,
                ForStatus = forStatus,
                Status = (int)status,
                WithdrawalInfo = withdrawalInfo,
                WithdrawalType = withdrawalType,
                RefId = (string)null,
                Operator = updater
            });
            p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
            p.Add("Amount", dbType: DbType.Decimal, direction: ParameterDirection.Output);

            if (conn == null)
            {
                //No execute in transaction
                using var connection = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
                result = connection.Execute("Admin_WithdrawalInfo_Set", p, commandType: CommandType.StoredProcedure);
            }
            else
            {
                //Execute in transaction
                result = conn.Execute("Admin_WithdrawalInfo_Set", p, trans, commandType: CommandType.StoredProcedure);
            }

            custId = p.Get<int?>("CustId") ?? 0;
            userName = p.Get<string>("UserName");
            amount = p.Get<decimal>("Amount");

            foreach (var paraName in p.ParameterNames)
            {
                var value = Convert.ToString(p.Get<dynamic>(paraName));
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
            return conn.QueryFirstOrDefault<WithdrawalRequest>("Admin_WithdrawalInfo_Get",
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
    }
}