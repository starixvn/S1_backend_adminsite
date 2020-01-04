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
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<Comments>("WithdrawalRequestComment_Get", new { WithdrawalId = withdrawalId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public int SetWithdrawalInfo(string transId, int siteId, string comment, WithdrawalStatus forStatus, WithdrawalStatus status)
        {
            int withdrawalId = 0;
            int type = 1 + typePosition;
            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out withdrawalId);

            int result = 0;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        var p = new DynamicParameters(new { WithdrawalId = withdrawalId, SiteId = siteId, Comment = comment, ForStatus = (int)forStatus, Status = (int)status });
                        p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
                        p.Add("Amount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                        result = conn.Execute("WithdrawalInfo_Set", p, trans, commandType: CommandType.StoredProcedure);

                        if (result > 0)
                        {
                            SetWithdrawalLog(withdrawalId, siteId, comment, (int)status);

                            if (status == WithdrawalStatus.Rejected)
                            {
                                byte is1stDepoist = 0;
                                decimal afterAmount = 0;

                                int custId = p.Get<int?>("CustId") ?? 0;
                                string UserName = p.Get<string>("UserName");
                                decimal amount = p.Get<decimal>("Amount");
                                result = walletService.SetBalance(custId, siteId, UserName, amount, type, $"Withdrawal Rejected: {amount}", transId, out afterAmount, out is1stDepoist, conn, trans);
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
                SetWithdrawalLog(withdrawalId, siteId, ex.Message, (int)WithdrawalStatus.Error);
            }

            return result;
        }


        private int SetWithdrawalLog(int withdrawalId, int siteId, string comment, int status)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("WithdrawalLog_Ins", new { WithdrawalId = withdrawalId, SiteId = siteId, Comment = comment, Action = status }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
