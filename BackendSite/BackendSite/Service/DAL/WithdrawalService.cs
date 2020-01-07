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

        public IEnumerable<WithdrawalRequest> GetWithdrawalRequestList(int? withdrawalType, long startDate, long endDate, int siteId, string WithdrawalId, string UserName, int? currencyId, int? status, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            int withdrawalId = 0;
            if (!string.IsNullOrWhiteSpace(WithdrawalId))
            {
                Match m = Regex.Match(WithdrawalId, @"\d+");
                int.TryParse(m.Value, out withdrawalId);
            }
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var p = new DynamicParameters(new { WithdrawalType = withdrawalType, StartDate = startDate, EndDate = endDate, SiteId = siteId, WithdrawalId = withdrawalId, TransId = WithdrawalId, UserName, CurrencyId = currencyId, Status = status, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = conn.Query<WithdrawalRequest>("WithdrawalList_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }

        public WithdrawalRequest GetWithdrawalRequest(int withdrawalId, int siteId)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<WithdrawalRequest>("WithdrawalRequestTransID_Get", new { WithdrawalId = withdrawalId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
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
                using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        string refWithdrawalInfo = WithdrawalInfo;
                        /*Approved don't to Set WithdrawalInfo*/
                        if (status == WithdrawalStatus.Approved)
                        {
                            refWithdrawalInfo = null;
                        }
                        
                        string sForStatus = Newtonsoft.Json.JsonConvert.SerializeObject(forStatus);
                        var p = new DynamicParameters(new { WithdrawalId = withdrawalId, SiteId = siteId, Comment = comment, ForStatus = sForStatus, Status = (int)status, WithdrawalInfo = refWithdrawalInfo, WithdrawalType = withdrawalType, RefId = (string)null });
                        p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
                        p.Add("Amount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                        result = conn.Execute("WithdrawalInfo_Set", p, trans, commandType: CommandType.StoredProcedure);
                        custId = p.Get<int?>("CustId") ?? 0;
                        string UserName = p.Get<string>("UserName");
                        amount = p.Get<decimal>("Amount");
                        if (result > 0)
                        {
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
                                if (withdrawalType == 1)
                                {
                                    result = walletService.SetPayDummy(siteId, withdrawalInfo.PayId ?? 0, withdrawalInfo.CurrencyId ?? 0, -1 * amount, conn, trans);
                                }
                                else if (withdrawalType == 2)
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
    }
}
