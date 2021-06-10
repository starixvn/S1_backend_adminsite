using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration configuration;
        private readonly WalletService walletService;
        private int typePosition = 1000;

        public WithdrawalService(IConfiguration configuration, WalletService walletService)
        {
            this.configuration = configuration;
            this.walletService = walletService;
        }

        public IEnumerable<WithdrawalRequest> GetWithdrawalRequestList(IEnumerable<int> selectSite, IEnumerable<int> withdrawalType, long startDate, long endDate, int siteId, string WithdrawalId, string UserName, IEnumerable<int> currencyId, IEnumerable<int> status, IEnumerable<int> levelstatus,int pageSize, ref int pageNumber, out int totalPages, out int rowCount,ref List<string> CurrencyAmount)
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
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var p = new DynamicParameters(new { SelectSite, WithdrawalType, StartDate = startDate, EndDate = endDate, SiteId = siteId, WithdrawalId = withdrawalId, TransId = WithdrawalId, UserName, CurrencyId, Status, VipStatus= VipStatus, PageSize = pageSize });

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_WithdrawalTotalAmount_Get", p, commandType: CommandType.StoredProcedure).ToList();
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

            var result = conn.Query<WithdrawalRequest>("Admin_WithdrawalListV3_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }

        public WithdrawalRequest GetWithdrawalRequest(int withdrawalId, int siteId)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<WithdrawalRequest>("Admin_WithdrawalRequestTransIDV3_Get", new { WithdrawalId = withdrawalId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Comments> GetWithdrawalRequestComment(int withdrawalId, int siteId)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<Comments>("WithdrawalRequestComment_Get", new { WithdrawalId = withdrawalId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public int SetWithdrawalInfo(string transId, int siteId, string comment, string Operator, IEnumerable<int> forStatus, WithdrawalStatus status, string WithdrawalInfo, int? withdrawalType, out int custId, out decimal amount)
        {
            int withdrawalId = 0;
            int type = 1 + typePosition;
            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);
            custId = 0;
            amount = 0m;
            int result = 0;
            try
            {
                using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
                conn.Open();
                using var trans = conn.BeginTransaction();
                string refWithdrawalInfo = WithdrawalInfo;
                /*Approved don't to Set WithdrawalInfo*/
                if (status == WithdrawalStatus.Approved)
                {
                    refWithdrawalInfo = null;
                }

                string sForStatus = Newtonsoft.Json.JsonConvert.SerializeObject(forStatus);
                var p = new DynamicParameters(new { WithdrawalId = withdrawalId, SiteId = siteId, Comment = comment, ForStatus = sForStatus, Status = (int)status, WithdrawalInfo = refWithdrawalInfo, WithdrawalType = withdrawalType, RefId = (string)null, Operator = Operator });
                p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
                p.Add("Amount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                result = conn.Execute("Admin_WithdrawalInfo_Set", p, trans, commandType: CommandType.StoredProcedure);
                custId = p.Get<int?>("CustId") ?? 0;

                if (result > 0)
                {
                    string UserName = p.Get<string>("UserName");
                    amount = p.Get<decimal>("Amount");

                    SetWithdrawalLog(withdrawalId, siteId, comment, (int)status, Operator);

                    if (status == WithdrawalStatus.Rejected)
                    {
                        type = 2 + typePosition; /*Rejected  Withdrawal 1002*/
                        byte is1stDepoist = 0;
                        decimal afterAmount = 0;

                        result = walletService.SetBalance(custId, siteId, UserName, amount, type, $"Withdrawal Rejected: {amount}", transId, out afterAmount, out is1stDepoist, conn, trans);
                    }
                    else if (status == WithdrawalStatus.Approved)
                    {
                        var withdrawalInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WithdrawalInfo>(WithdrawalInfo);
                        if (withdrawalType == (int)WithdrawalType.OnlinePayment)
                        {
                            result = walletService.SetPayDummy(siteId, withdrawalInfo.PayId ?? 0, withdrawalInfo.CurrencyId ?? 0, -1 * amount, conn, trans);
                        }
                        else if (withdrawalType == (int)WithdrawalType.LocalBank)
                        {
                            result = walletService.SetBankOffDummy(siteId, withdrawalInfo.FromSysId ?? 0, withdrawalInfo.FromAccountNo, withdrawalInfo.CurrencyId ?? 0, -1 * amount, conn, trans);
                        }
                    }

                    if (result > 0)
                    {
                        trans.Commit();
                        result = p.Get<int?>("CustId") ?? result;
                    }
                }
            }
            catch (Exception ex)
            {
                SetWithdrawalLog(withdrawalId, siteId, ex.Message, (int)WithdrawalStatus.Error, Operator);
            }

            return result;
        }
        private int SetWithdrawalLog(int withdrawalId, int siteId, string comment, int status, string Operator)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("WithdrawalLog_Ins", new { WithdrawalId = withdrawalId, SiteId = siteId, Comment = comment, Action = status, Operator }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<WithdrawalRequest> GetWithdrawalListByCust(int CustId, string transId, int? Status, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount, ref List<string> CurrencyAmount)
        {
            List<string> tempCurrencyTotalAmount = new List<string>();
            var p = new DynamicParameters(new { CustId, TransId = transId, Status, StartDate = startDate, EndDate = endDate, PageSize = pageSize });
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_WithdrawalTotalAmountByCust_Get", p, commandType: CommandType.StoredProcedure).ToList();
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

            
            var depositList = conn.Query<WithdrawalRequest>("Admin_WithdrawalListByCust_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }

        public IEnumerable<WithdrawalInfoForExport> GetWithdrawalExportList(string selectSite, string withdrawalType, long startDate, long endDate, string WithdrawalId, string UserName, string currencyId, string status, string LevelStatus)
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
            var p = new DynamicParameters(new { SelectSite, WithdrawalType, StartDate = startDate, EndDate = endDate, WithdrawalId = withdrawalId, TransId = WithdrawalId, UserName, CurrencyId, Status, VipStatus= VipStatus });

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var depositList = conn.Query<WithdrawalInfoForExport>("Admin_WithdrawalListExportV4_Get", p, commandType: CommandType.StoredProcedure);
            return depositList;
        }

        public IEnumerable<WithdrawalKYC> GetWithdrawalKYC(int SiteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var withdrawalKYCList = conn.Query<WithdrawalKYC>("Admin_WithdrawalKyc_Get", new { SiteId }, commandType: CommandType.StoredProcedure);
            return withdrawalKYCList;
        }

        public bool EditWithdrawalKYC(WithdrawalKYC withdrawalKYC)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_WithdrawalKyc_Set", new { withdrawalKYC.SiteId, withdrawalKYC.CurrencyId, withdrawalKYC.Amount }, commandType: CommandType.StoredProcedure) > 0;
        }

        public IEnumerable<WithdrawalMinInfo> GetWithdrawalLimitData(int SiteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var WithdrawalMinInfo = conn.Query<WithdrawalMinInfo>("Admin_WithdrawalMinInfo_Get", new { SiteId }, commandType: CommandType.StoredProcedure);
            return WithdrawalMinInfo;
        }
        public bool InsertWithdrawalLimit(int SiteId, int CurrencyId, int WithdrawalMin, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_WithdrawalMinInfo_Ins", new { SiteId, CurrencyId, Operator, WithdrawalMin }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool UpdateWithdrawalLimit(int SiteId, int CurrencyId,int WithdrawalMin,string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_WithdrawalMinInfo_Upd", new { SiteId, CurrencyId, Operator, WithdrawalMin,  }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool DeleteWithdrawalLimit(int SiteId, int CurrencyId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_WithdrawalMinInfo_Del", new { SiteId, CurrencyId }, commandType: CommandType.StoredProcedure) > 0;
        }

        public bool UpdateWithdrawalPicCount(string transId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_BankSlipPic_Ins", new { TransId=transId }, commandType: CommandType.StoredProcedure) > 0;
        }
        public BankSlipPic GetWithdrawalPicCount(string transId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var WithdrawalMinInfo = conn.Query<BankSlipPic>("Admin_BankSlipPic_Get", new { TransId = transId }, commandType: CommandType.StoredProcedure).FirstOrDefault();
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
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
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
            var result = conn.Query<WithdrawalTimeList>("Admin_WithdrawalStatusTime_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.WithdrawalTimeList = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return SearchData;
        }

        public List<WithdrawalTimeList> GetExportData(IEnumerable<int> SelectSite, string WithdrawalID, long StartDate, long EndDate, int Status, int CurrencyId)
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

            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("StartDate", StartDate);
            props.Add("EndDate", EndDate);
            props.Add("SelectSite", selectSite);
            props.Add("CurrencyId", CurrencyId);
            props.Add("WithdrawalId", tempWithdrawalId);
            props.Add("SelectStatus", SelectStatus);
            return conn.Query<WithdrawalTimeList>("Admin_WithdrawalStatusTimeExport_Get", props, commandType: CommandType.StoredProcedure).ToList();

        }
    }
}
