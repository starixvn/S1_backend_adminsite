using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace BackendSite.Service.DAL
{
    public class DepositService
    {
        private readonly IConfiguration configuration;
        private readonly WalletService walletService;
        private readonly Common common;
        private readonly WithdrawalLimitService withdrawalLimitService;
        private int payTypePosition = 100;

        public DepositService(IConfiguration configuration, WalletService walletService, Common common, WithdrawalLimitService withdrawalLimitService)
        {
            this.configuration = configuration;
            this.walletService = walletService;
            this.common = common;
            this.withdrawalLimitService = withdrawalLimitService;
        }

        public IEnumerable<DepositRequest> GetDepositRequestList(IEnumerable<int> selectSite, IEnumerable<int> depositType, long startDate, long endDate, int siteId, string DepositId, string UserName, IEnumerable<int> currencyId, IEnumerable<int> status, IEnumerable<int> levelstatus,int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var SelectSite = JsonConvert.SerializeObject(selectSite);
            var DepositType = JsonConvert.SerializeObject(depositType);
            var CurrencyId = JsonConvert.SerializeObject(currencyId);
            var Status = JsonConvert.SerializeObject(status);
            var VipStatus = JsonConvert.SerializeObject(levelstatus);
            int depositId = 0;
            if (!string.IsNullOrWhiteSpace(DepositId))
            {
                Match m = Regex.Match(DepositId, @"\d+");
                int.TryParse(m.Value, out depositId);
            }
            var p = new DynamicParameters(new { SelectSite, DepositType, StartDate = startDate, EndDate = endDate, SiteId = siteId, DepositId = depositId, TransId = DepositId, UserName, CurrencyId, Status, VipStatus= VipStatus, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var depositList = conn.Query<DepositRequest>("Admin_DepositListV5_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }

        public IEnumerable<DepositRequest> GetDepositListByCust(int CustId, string TransId, int? Status, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new { CustId, TransId, Status, StartDate = startDate, EndDate = endDate, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var depositList = conn.Query<DepositRequest>("Admin_DepositInfoListByCust_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }

        public DepositRequest GetDepositRequest(int depositId, int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<DepositRequest>("Admin_DepositRequestTransID_Get", new { DepositId = depositId/*, SiteId = siteId*/ }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Comments> GetDepositRequestComment(int depositId, int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<Comments>("DepositRequestComment_Get", new { DepositId = depositId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public string InsertDepositRequest(DepositDetailInfo deposit)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using MySqlTransaction trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                deposit.CustId,
                deposit.UserName,
                deposit.CurrencyId,
                deposit.SiteId,
                deposit.DepositInfo,
                deposit.Amount,
                deposit.Comment,
                deposit.RefId,
                deposit.DepositTime,
                deposit.Operator,
                Prefix = $"D{common.GetEnv()}"
            });

            p.Add("vTransId", dbType: DbType.String, direction: ParameterDirection.Output);

            var result = conn.Execute("Admin_DepositInfo_Ins", p, transaction: trans, commandType: CommandType.StoredProcedure);
            trans.Commit();

            var transId = p.Get<string>("vTransId");

            return transId;
        }

        public int SetDepositReceiptPic(int custId, int depositId, string picFileName, byte[] depositPic)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_DepositReceiptPic_Set", new { CustId = custId, DepositId = depositId, PicFileName = picFileName, DepositPic = depositPic }, commandType: CommandType.StoredProcedure);
        }

        public int SetDepositInfo(string transId, int siteId, decimal actualDeposit, string comment, string Operator, IEnumerable<int> forStatus, DepositStatus status, DepositRequest depositRequest)
        {
            int depositId = 0;
            int type = 1;
            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);

            int result = 0;
            try
            {
                using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
                conn.Open();
                using var trans = conn.BeginTransaction();
                string sForStatus = Newtonsoft.Json.JsonConvert.SerializeObject(forStatus);
                var p = new DynamicParameters(new { DepositId = depositId, SiteId = siteId, ActualDeposit = actualDeposit, Comment = comment, ForStatus = sForStatus, Status = (int)status, ThirdId = (string)null, Operator = Operator });
                p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
                result = conn.Execute("Admin_DepositInfo_Set", p, trans, commandType: CommandType.StoredProcedure);

                if (result > 0)
                {
                    SetDepositLog(depositId, siteId, comment, (int)status, actualDeposit, Operator);

                    if (status == DepositStatus.Approved)
                    {
                        if (depositRequest.DepositType != 3)
                        {
                            var pay = (IPay)depositRequest.oDepositInfo;
                            type = payTypePosition + pay.PayId;
                        }

                        byte is1stDepoist = 0;
                        decimal afterAmount = 0;

                        int custId = p.Get<int?>("CustId") ?? 0;
                        string UserName = p.Get<string>("UserName");
                        result = walletService.SetBalance(custId, siteId, UserName, actualDeposit, type, $"Deposit Approved: {actualDeposit}", transId, out afterAmount, out is1stDepoist, conn, trans);

                        if (result > 0)
                        {
                            decimal withdrawalLimit = 0;
                            result = withdrawalLimitService.SetWithdrawalLimit(custId, UserName, siteId, actualDeposit, $"WithdrawalLimit: {actualDeposit}", transId, out withdrawalLimit, conn, trans);
                        }

                        if (is1stDepoist == 1 && result > 0)
                        {
                            result = SetDepositInfoFirstDeposit(depositId, siteId, conn, trans);
                        }

                        if (result > 0)
                        {
                            if (depositRequest.DepositType == 3)
                            {
                                var depositInfo3 = (DepositInfo3_1)depositRequest.oDepositInfo;
                                result = walletService.SetBankOffDummy(depositRequest.SiteId, depositInfo3.SysId, depositInfo3.AccountNo, depositInfo3.CurrencyId, actualDeposit, conn, trans);
                            }
                            else if (depositRequest.oDepositInfo is IPay)
                            {
                                var pay = (IPay)depositRequest.oDepositInfo;
                                result = walletService.SetPayDummy(depositRequest.SiteId, pay.PayId, pay.CurrencyId, actualDeposit, conn, trans);
                            }
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
                SetDepositLog(depositId, siteId, ex.Message, (int)DepositStatus.Error, actualDeposit, Operator);
            }

            return result;
        }

        private int SetDepositInfoFirstDeposit(int depositId, int siteId, MySqlConnection conn, MySqlTransaction trans)
        {
            return conn.Execute("DepositInfoFirstDeposit_Set", new { DepositId = depositId, SiteId = siteId }, trans, commandType: CommandType.StoredProcedure);
        }

        private int SetDepositLog(int depositId, int siteId, string comment, int status, decimal actualDeposit, string Operator)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("DepositLog_Ins", new { DepositId = depositId, SiteId = siteId, Comment = comment, Action = status, ActualDeposit = actualDeposit, Operator = Operator }, commandType: CommandType.StoredProcedure);
        }

        public DepositReceipt GetDepositReceipt(int depositId, int siteId)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<DepositReceipt>("DepositReceipt_Get", new { DepositId = depositId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<DepositInfoForExport> GetDepositExportList(string selectSite, string depositType, long startDate, long endDate, string DepositId, string UserName, string currencyId, string status,string LevelStatus)
        {
            //var SelectSite = JsonConvert.SerializeObject(selectSite);

            var SelectSite = $"[{selectSite}]";
            var DepositType = $"[{depositType}]";
            var CurrencyId = $"[{currencyId}]";
            var Status = $"[{status}]";
            var VipStatus = $"[{LevelStatus}]";

            int depositId = 0;
            if (!string.IsNullOrWhiteSpace(DepositId))
            {
                Match m = Regex.Match(DepositId, @"\d+");
                int.TryParse(m.Value, out depositId);
            }
            var p = new DynamicParameters(new { SelectSite, DepositType, StartDate = startDate, EndDate = endDate, DepositId = depositId, TransId = DepositId, UserName, CurrencyId, Status, VipStatus = VipStatus });
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var depositList = conn.Query<DepositInfoForExport>("Admin_DepositListForExportV5_Get", p, commandType: CommandType.StoredProcedure);
            return depositList;
        }
        public IEnumerable<DepositLimitCurrencyInfo> GetDepositLimitCurrencyList(int siteId, int depositType)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<DepositLimitCurrencyInfo>("Admin_DepositCurrencyList_Get", new { SiteId = siteId, DepositType = depositType }, commandType: CommandType.StoredProcedure);
            }
        }
        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(int siteId, int depositType, int currencyId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<DepositLimitInfo>("Admin_DepositLimitInfo_Get", new { SiteId = siteId, DepositType = depositType, CurrencyId = currencyId }, commandType: CommandType.StoredProcedure);
            }
        }

        public bool AddDepositLimitInfo(DepositLimitInfo depositLimitInfo)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var param = new
                {
                    depositLimitInfo.SiteId,
                    depositLimitInfo.DepositType,
                    depositLimitInfo.CurrencyId,
                    depositLimitInfo.VipLevel,
                    depositLimitInfo.DepositMin,
                    depositLimitInfo.DepositMax
                };
                return conn.Execute("Admin_DepositLimitInfo_Ins", param, commandType: CommandType.StoredProcedure) > 0;
            }
        }

        public bool EditDepositLimitInfo(DepositLimitInfo depositLimitInfo)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var param = new
                {
                    depositLimitInfo.SiteId,
                    depositLimitInfo.DepositType,
                    depositLimitInfo.CurrencyId,
                    depositLimitInfo.VipLevel,
                    depositLimitInfo.DepositMin,
                    depositLimitInfo.DepositMax,
                    depositLimitInfo.DepositStatus
                };
                return conn.Execute("Admin_DepositLimitInfo_Set", param, commandType: CommandType.StoredProcedure) > 0;
            }
        }

        public DepositLimitInfo GetDepositLimitInfo(int siteId, int depositType, int currencyId, int viplevel)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<DepositLimitInfo>("Admin_DepositLimitInfoDetail_Get", new { SiteId = siteId, DepositType = depositType, CurrencyId = currencyId, VipLevel = viplevel }, commandType: CommandType.StoredProcedure);
            }
        }

        public int DeleteDepositLimitInfo(DepositLimitInfo depositLimitInfo)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("Admin_DepositLimitInfo_Del", new { SiteId = depositLimitInfo.SiteId, CurrencyId = depositLimitInfo.CurrencyId, VipLevel = depositLimitInfo.VipLevel }, commandType: CommandType.StoredProcedure);
            }
        }

    }
}
