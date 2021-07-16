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
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class WithdrawalService
    {
        #region Fields

        private readonly IConfiguration _configuration;
        private readonly WalletService _walletService;
        private readonly WithdrawalBankAccountingService _withdrawalBankAccountingService;
        private readonly ILogger<WithdrawalService> _logger;

        private int _typePosition = 1000;

        #endregion

        #region Constructors

        public WithdrawalService(IConfiguration configuration,
            WalletService walletService,
            WithdrawalBankAccountingService withdrawalBankAccountingService,
            ILogger<WithdrawalService> logger)
        {
            _configuration = configuration;
            _walletService = walletService;
            _withdrawalBankAccountingService = withdrawalBankAccountingService;
            _logger = logger;
        }

        #endregion

        public IEnumerable<WithdrawalRequest> GetWithdrawalRequestList(IEnumerable<int> selectSite,
            IEnumerable<int> withdrawalType, long startDate, long endDate, int siteId, string WithdrawalId,
            string UserName, IEnumerable<int> currencyId, IEnumerable<int> status, IEnumerable<int> levelstatus,
            int pageSize, ref int pageNumber, out int totalPages, out int rowCount, ref List<string> CurrencyAmount)
        {
            var SelectSite = JsonConvert.SerializeObject(selectSite);
            var WithdrawalType = JsonConvert.SerializeObject(withdrawalType);
            var CurrencyId = JsonConvert.SerializeObject(currencyId);
            var Status = JsonConvert.SerializeObject(status);
            var VipStatus = JsonConvert.SerializeObject(levelstatus);
            int withdrawalId = 0;
            List<string> tempCurrencyTotalAmount = new List<string>();
            if (!string.IsNullOrWhiteSpace(WithdrawalId))
            {
                Match m = Regex.Match(WithdrawalId, @"\d+");
                int.TryParse(m.Value, out withdrawalId);
            }

            using MySqlConnection conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var p = new DynamicParameters(new
            {
                SelectSite,
                WithdrawalType,
                StartDate = startDate,
                EndDate = endDate,
                SiteId = siteId,
                WithdrawalId = withdrawalId,
                TransId = WithdrawalId,
                UserName,
                CurrencyId,
                Status,
                VipStatus = VipStatus,
                PageSize = pageSize
            });

            #region GetCurrencyTotalAmount

            var CurrencyTotalAmount =
                conn.Query<TotalAmount>("Admin_WithdrawalTotalAmount_Get", p, commandType: CommandType.StoredProcedure)
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

            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = conn.Query<WithdrawalRequest>("Admin_WithdrawalListV3_Get", p,
                commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }

        public WithdrawalRequest GetWithdrawalRequest(int withdrawalId, int siteId)
        {
            using MySqlConnection conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<WithdrawalRequest>("Admin_WithdrawalRequestTransIDV3_Get",
                new {WithdrawalId = withdrawalId, SiteId = siteId}, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Comments> GetWithdrawalRequestComment(int withdrawalId, int siteId)
        {
            using MySqlConnection conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Query<Comments>("WithdrawalRequestComment_Get",
                new {WithdrawalId = withdrawalId, SiteId = siteId}, commandType: CommandType.StoredProcedure);
        }

        private int SetWithdrawalLog(int withdrawalId, int siteId, string comment, int status, string Operator,
            MySqlConnection conn = null, MySqlTransaction trans = null)
        {
            if (conn != null)
            {
                return conn.Execute("WithdrawalLog_Ins",
                    new {WithdrawalId = withdrawalId, SiteId = siteId, Comment = comment, Action = status, Operator},
                    trans, commandType: CommandType.StoredProcedure);
            }
            else
            {
                using MySqlConnection connection =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
                return connection.Execute("WithdrawalLog_Ins",
                    new {WithdrawalId = withdrawalId, SiteId = siteId, Comment = comment, Action = status, Operator},
                    commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<WithdrawalRequest> GetWithdrawalListByCust(int CustId, string transId, int? Status,
            long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount,
            ref List<string> CurrencyAmount)
        {
            List<string> tempCurrencyTotalAmount = new List<string>();
            var p = new DynamicParameters(new
                {CustId, TransId = transId, Status, StartDate = startDate, EndDate = endDate, PageSize = pageSize});
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));

            #region GetCurrencyTotalAmount

            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_WithdrawalTotalAmountByCust_Get", p,
                commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }

            CurrencyAmount = tempCurrencyTotalAmount;

            #endregion


            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);


            var depositList = conn.Query<WithdrawalRequest>("Admin_WithdrawalListByCust_Get", p,
                commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }

        public IEnumerable<WithdrawalInfoForExport> GetWithdrawalExportList(string selectSite, string withdrawalType,
            long startDate, long endDate, string WithdrawalId, string UserName, string currencyId, string status,
            string LevelStatus)
        {
            var SelectSite = "[" + selectSite + "]";
            var WithdrawalType = "[" + withdrawalType + "]";
            var CurrencyId = "[" + currencyId + "]";
            var Status = "[" + status + "]";
            var VipStatus = $"[{LevelStatus}]";
            int withdrawalId = 0;
            if (!string.IsNullOrWhiteSpace(WithdrawalId))
            {
                Match m = Regex.Match(WithdrawalId, @"\d+");
                int.TryParse(m.Value, out withdrawalId);
            }

            var p = new DynamicParameters(new
            {
                SelectSite,
                WithdrawalType,
                StartDate = startDate,
                EndDate = endDate,
                WithdrawalId = withdrawalId,
                TransId = WithdrawalId,
                UserName,
                CurrencyId,
                Status,
                VipStatus = VipStatus
            });

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var depositList = conn.Query<WithdrawalInfoForExport>("Admin_WithdrawalListExportV4_Get", p,
                commandType: CommandType.StoredProcedure);
            return depositList;
        }

        public IEnumerable<WithdrawalKYC> GetWithdrawalKYC(int SiteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var withdrawalKYCList = conn.Query<WithdrawalKYC>("Admin_WithdrawalKyc_Get", new {SiteId},
                commandType: CommandType.StoredProcedure);
            return withdrawalKYCList;
        }

        public bool EditWithdrawalKYC(WithdrawalKYC withdrawalKYC)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_WithdrawalKyc_Set",
                new {withdrawalKYC.SiteId, withdrawalKYC.CurrencyId, withdrawalKYC.Amount},
                commandType: CommandType.StoredProcedure) > 0;
        }

        public IEnumerable<WithdrawalMinInfo> GetWithdrawalLimitData(int SiteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var WithdrawalMinInfo = conn.Query<WithdrawalMinInfo>("Admin_WithdrawalMinInfo_Get", new {SiteId},
                commandType: CommandType.StoredProcedure);
            return WithdrawalMinInfo;
        }

        public bool InsertWithdrawalLimit(int SiteId, int CurrencyId, int WithdrawalMin, string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_WithdrawalMinInfo_Ins", new {SiteId, CurrencyId, Operator, WithdrawalMin},
                commandType: CommandType.StoredProcedure) > 0;
        }

        public bool UpdateWithdrawalLimit(int SiteId, int CurrencyId, int WithdrawalMin, string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_WithdrawalMinInfo_Upd", new {SiteId, CurrencyId, Operator, WithdrawalMin,},
                commandType: CommandType.StoredProcedure) > 0;
        }

        public bool DeleteWithdrawalLimit(int SiteId, int CurrencyId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_WithdrawalMinInfo_Del", new {SiteId, CurrencyId},
                commandType: CommandType.StoredProcedure) > 0;
        }

        public bool UpdateWithdrawalPicCount(string transId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_BankSlipPic_Ins", new {TransId = transId},
                commandType: CommandType.StoredProcedure) > 0;
        }

        public BankSlipPic GetWithdrawalPicCount(string transId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var WithdrawalMinInfo = conn.Query<BankSlipPic>("Admin_BankSlipPic_Get", new {TransId = transId},
                commandType: CommandType.StoredProcedure).FirstOrDefault();
            return WithdrawalMinInfo;
        }

        public WithdrawalTimeData WithdrawalTimeReportGet(WithdrawalTimeReportPara para)
        {
            var SelectSite = JsonConvert.SerializeObject(para.SelectSite);
            List<int> selectstatus = new List<int>();
            if (para.Status == 0)
            {
                selectstatus.Add(3);
                selectstatus.Add(4);
                selectstatus.Add(5);
                selectstatus.Add(6);
                selectstatus.Add(7);
            }
            else
            {
                selectstatus.Add(para.Status);
            }

            var SelectStatus = JsonConvert.SerializeObject(selectstatus);

            WithdrawalTimeData SearchData = new WithdrawalTimeData();
            DynamicParameters props = new DynamicParameters();
            using MySqlConnection conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            int tempWithdrawalID;
            Int32.TryParse(para.WithdrawalID, out tempWithdrawalID);

            props.Add("SelectSite", SelectSite);
            props.Add("WithdrawalId", tempWithdrawalID);
            props.Add("StartDate", para.StartDate);
            props.Add("EndDate", para.EndDate);
            props.Add("SelectStatus", SelectStatus);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("PageNumber", para.PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", para.PageSize);
            var result = conn.Query<WithdrawalTimeList>("Admin_WithdrawalStatusTime_Get", props,
                commandType: CommandType.StoredProcedure).ToList();

            SearchData.WithdrawalTimeList = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return SearchData;
        }

        public List<WithdrawalTimeList> GetExportData(IEnumerable<int> SelectSite, string WithdrawalID, long StartDate,
            long EndDate, int Status, int CurrencyId)
        {
            var selectSite = JsonConvert.SerializeObject(SelectSite);
            List<int> selectstatus = new List<int>();
            if (Status == 0)
            {
                selectstatus.Add(3);
                selectstatus.Add(4);
                selectstatus.Add(5);
                selectstatus.Add(6);
                selectstatus.Add(7);
            }
            else
            {
                selectstatus.Add(Status);
            }

            var SelectStatus = JsonConvert.SerializeObject(selectstatus);
            int tempWithdrawalId;
            Int32.TryParse(WithdrawalID, out tempWithdrawalId);

            using MySqlConnection conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("StartDate", StartDate);
            props.Add("EndDate", EndDate);
            props.Add("SelectSite", selectSite);
            props.Add("CurrencyId", CurrencyId);
            props.Add("WithdrawalId", tempWithdrawalId);
            props.Add("SelectStatus", SelectStatus);
            return conn.Query<WithdrawalTimeList>("Admin_WithdrawalStatusTimeExport_Get", props,
                commandType: CommandType.StoredProcedure).ToList();
        }

        #region SetWithdrawalInfo Refactory

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
            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out int withdrawalId);

            custId = 0;
            amount = 0m;
            int result = 0;
            Dictionary<string, string> paraLog = new Dictionary<string, string>();
            try
            {
                using MySqlConnection conn =
                    new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
                conn.Open();
                using var trans = conn.BeginTransaction();

                //step1:Set WithdrawalInfo
                result = SetWithdrawalInfo(withdrawalId, siteId, comment,
                    forStatus, updater, status,
                    /*Approved don't to Set WithdrawalInfo*/
                    (status == WithdrawalStatus.Approved ? null : withdrawalInfo),
                    withdrawalType,
                    out custId, out string userName, out amount, out paraLog,
                    conn, trans);

                if (result > 0)
                {
                    //step2: Set WithdrawalLog
                    int logResult = SetWithdrawalLog(withdrawalId, siteId, comment, (int) status, updater, conn, trans);
                    int specialResult = 0;

                    if (logResult > 0)
                    {
                        //step3: special Flow by Status.
                        switch (status)
                        {
                            case WithdrawalStatus.Rejected:
                                int balanceResult = 0;
                                int type = 2 + _typePosition; /*Rejected  Withdrawal 1002*/

                                balanceResult = _walletService.SetBalance(custId, siteId, userName, amount, type,
                                    $"Withdrawal Rejected: {amount}", transId, out var afterAmount,
                                    out var isFirstDeposit, conn, trans);

                                specialResult = balanceResult > 0 ? 1 : 0;

                                break;
                            case WithdrawalStatus.Approved:
                                int dummyResult = 0;
                                int accountingResult = 0;
                                var withdrawalInfoData = JsonConvert.DeserializeObject<WithdrawalInfo>(withdrawalInfo);

                                switch (withdrawalType)
                                {
                                    case (int) WithdrawalType.OnlinePayment:
                                        dummyResult = _walletService.SetPayDummy(siteId,
                                            withdrawalInfoData.PayId ?? 0,
                                            withdrawalInfoData.CurrencyId ?? 0,
                                            -1 * amount,
                                            conn,
                                            trans);
                                        break;
                                    case (int) WithdrawalType.LocalBank:
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
                                    if (withdrawalType == (int) WithdrawalType.LocalBank)
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
                                            && _withdrawalBankAccountingService.InsertLog(model, conn, trans) > 0)
                                        {
                                            accountingResult = 1; //InsertAccounting Success.
                                        }
                                        else
                                        {
                                            accountingResult = 0; //InsertAccounting fail.
                                        }
                                    }
                                }

                                specialResult = (dummyResult == 0 || accountingResult == 0) ? 0 : 1;

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
                SetWithdrawalLog(withdrawalId, siteId, ex.Message, (int) WithdrawalStatus.Error, updater);
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
            int result = 0;

            var p = new DynamicParameters(new
            {
                WithdrawalId = withdrawalId,
                SiteId = siteId,
                Comment = comment,
                ForStatus = forStatus,
                Status = (int) status,
                WithdrawalInfo = withdrawalInfo,
                WithdrawalType = withdrawalType,
                RefId = (string) null,
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

        #endregion

        /// <summary>
        /// 取得單筆提款單資訊
        /// </summary>
        /// <param name="withdrawalId">提款單ID</param>
        /// <param name="transId">提款單編號</param>
        /// <returns></returns>
        public WithdrawalRequest GetWithdrawalInfo(int withdrawalId,
            string transId)
        {
            using MySqlConnection conn =
                new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.QueryFirstOrDefault<WithdrawalRequest>("Admin_WithdrawalInfo_Get",
                new {_WithdrawalId = withdrawalId, _TransId = transId}, commandType: CommandType.StoredProcedure);
        }
    }
}