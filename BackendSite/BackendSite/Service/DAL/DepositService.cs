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

        public IEnumerable<DepositRequest> GetDepositRequestList(int siteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<DepositRequest>("DepositList_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public DepositRequest GetDepositRequest(int depositId, int siteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<DepositRequest>("DepositRequestTransID_Get", new { DepositId = depositId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<Comments> GetDepositRequestComment(int depositId, int siteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<Comments>("DepositRequestComment_Get", new { DepositId = depositId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public int SetDepositInfo(string transId, int siteId, decimal actualDeposit, string comment, string Operator, DepositStatus forStatus, DepositStatus status)
        {
            int depositId = 0;
            int type = 1;
            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);

            int result = 0;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        var p = new DynamicParameters(new { DepositId = depositId, SiteId = siteId, ActualDeposit = actualDeposit, Comment = comment, ForStatus = (int)forStatus, Status = (int)status, ThirdId = (string)null });
                        p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
                        result = conn.Execute("DepositInfo_Set", p, trans, commandType: CommandType.StoredProcedure);

                        if (result > 0)
                        {
                            SetDepositLog(depositId, siteId, comment, (int)status, actualDeposit, Operator);

                            if (status == DepositStatus.Approved)
                            {
                                var depositRequest = conn.QueryFirstOrDefault<DepositRequest>("DepositRequestTransID_Get", new { DepositId = depositId, SiteId = siteId }, trans, commandType: CommandType.StoredProcedure);
                                if (depositRequest.DepositType != 3)
                                {
                                    var pay = (IPay)depositRequest.depositInfo;
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
                                        var depositInfo3 = (DepositInfo3_1)depositRequest.depositInfo;
                                        result = walletService.SetBankOffDummy(depositRequest.SiteId, depositInfo3.SysId, depositInfo3.AccountNo, depositInfo3.CurrencyId, actualDeposit, conn, trans);
                                    }
                                    else if (depositRequest.depositInfo is IPay)
                                    {
                                        var pay = (IPay)depositRequest.depositInfo;
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
