using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class DepositService
    {
        private readonly IConfiguration configuration;
        private readonly WalletService walletService;
        private int payTypePosition = 100;

        public DepositService(IConfiguration configuration, WalletService walletService)
        {
            this.configuration = configuration;
            this.walletService = walletService;
        }

        public IEnumerable<DepositRequest> GetDepositRequestList(int? depositType, long startDate, long endDate, int siteId, string DepositId, string UserName, int? currencyId, int? status, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            int depositId = 0;
            if (!string.IsNullOrWhiteSpace(DepositId))
            {
                Match m = Regex.Match(DepositId, @"\d+");
                int.TryParse(m.Value, out depositId);
            }
            var p = new DynamicParameters(new { DepositType = depositType, StartDate = startDate, EndDate = endDate, SiteId = siteId, DepositId = depositId, TransId = DepositId, UserName, CurrencyId = currencyId, Status = status, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var depositList = conn.Query<DepositRequest>("DepositList_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }

        public DepositRequest GetDepositRequest(int depositId, int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<DepositRequest>("DepositRequestTransID_Get", new { DepositId = depositId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Comments> GetDepositRequestComment(int depositId, int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<Comments>("DepositRequestComment_Get", new { DepositId = depositId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
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
                using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        string sForStatus = Newtonsoft.Json.JsonConvert.SerializeObject(forStatus);
                        var p = new DynamicParameters(new { DepositId = depositId, SiteId = siteId, ActualDeposit = actualDeposit, Comment = comment, ForStatus = sForStatus, Status = (int)status, ThirdId = (string)null });
                        p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
                        result = conn.Execute("DepositInfo_Set", p, trans, commandType: CommandType.StoredProcedure);

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
    }
}
