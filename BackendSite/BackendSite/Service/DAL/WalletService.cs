using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using BackendSite.Service.Model;

namespace BackendSite.Service.DAL
{
    public class WalletService
    {
        private readonly IConfiguration configuration;

        public WalletService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public int SetBalance(int custId, int siteId, string userName, decimal Amount, int type, string Memo, string RefTransId, out decimal afterAmount, out byte is1stDepoist, MySqlConnection conn, MySqlTransaction trans)
        {
            var p = new DynamicParameters(new { CustId = custId, SiteId = siteId, UserName = userName, Amount = Amount, Type = type, Memo = Memo, RefTransId = RefTransId });
            p.Add("Is1stDepoist", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            p.Add("AfterAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            conn.Execute("Wallet_Set", p, transaction: trans, commandType: CommandType.StoredProcedure);
            is1stDepoist = p.Get<byte?>("Is1stDepoist") ?? 0;
            afterAmount = p.Get<decimal?>("AfterAmount") ?? 0;
            return afterAmount >= 0 ? 1 : 0;
        }
        public decimal GetBalance(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.ExecuteScalar<decimal>("Balance_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }
        public int SetPayDummy(int siteId, int payId, int currencyId, decimal amount, MySqlConnection conn, MySqlTransaction trans)
        {
            return conn.Execute("PayDummy_Set", new { SiteId = siteId, PayId = payId, CurrencyId = currencyId, Amount = amount }, trans, commandType: CommandType.StoredProcedure);
        }
        public int SetBankOffDummy(int siteId, int sysId, string accountNo, int currencyId, decimal amount, MySqlConnection conn, MySqlTransaction trans)
        {
            return conn.Execute("BankOffDummy_Set", new { SiteId = siteId, SysId = sysId, AccountNo = accountNo, CurrencyId = currencyId, Amount = amount }, trans, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<FinancialLog> GetFinancialLog(int CustId, long? startDate, long? endDate, string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new { CustId = CustId, StartDate = startDate, EndDate = endDate, RefTransId = refTransId, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var depositList = conn.Query<FinancialLog>("Admin_FinancialLogList_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }
    }
}
