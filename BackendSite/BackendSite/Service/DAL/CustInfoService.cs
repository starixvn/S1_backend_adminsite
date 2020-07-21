using BackendSite.Service.Model;
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
    public class CustInfoService
    {
        private readonly IConfiguration configuration;

        public CustInfoService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Customer GetCustInfo(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<Customer>("Customer_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<CustInfoDetail> GetCustInfoList(int? birDate, int? birMonth, int? birYear, string userName, string AccountName, string LoginIp, string transId, string TpUserName, string Email, string PhoneNo, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            totalPages = 0;
            var p = new DynamicParameters(new { BirDate = birDate, BirMonth = birMonth, BirYear = birYear, UserName = userName, AccountName = AccountName, LoginIp = LoginIp, TransId = transId, TpUserName = TpUserName, Email = Email, PhoneNo = PhoneNo, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.Query<CustInfoDetail>("Admin_CustInfoSearch_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }

        public IEnumerable<CustInfoDetailForExport> GetCustInfoExportList(int? birDate, int? birMonth, int? birYear, string userName, string AccountName, string LoginIp, string transId, string TpUserName, string Email, string PhoneNo)
        {
            var p = new DynamicParameters(new { BirDate = birDate, BirMonth = birMonth, BirYear = birYear, UserName = userName, AccountName = AccountName, LoginIp = LoginIp, TpUserName = TpUserName, TransId = transId, Email = Email, PhoneNo = PhoneNo });

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var custInfoDetailList = conn.Query<CustInfoDetailForExport>("Admin_CustInfoSearchForExport_Get", p, commandType: CommandType.StoredProcedure);
            return custInfoDetailList;
        }

        public int GetVipLevel(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.ExecuteScalar<int>("VipLevel_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public CustInfoDetail GetCustInfoDetail(int custId, string userName)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.QueryFirstOrDefault<CustInfoDetail>("Admin_CustInfoDetailSearch_Get", new { CustId = custId, UserName = userName }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public CustInfoDetail GetCustInfos(int? custId, string userName, int? siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.QueryFirstOrDefault<CustInfoDetail>("Admin_CustInfo_Get", new { CustId = custId, UserName = userName, SiteId = siteId }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public int SetCustStatusSwitch(int custId, int Op)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_CustStatusSwitch_Set", new { CustId = custId, Op }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public int SetCustStatusLevel(int custId, int Level, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_CustStatusLevel_Set", new { CustId = custId, Level, Operator }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public CustPhotoId GetCustPhotoId(int CustId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<CustPhotoId>("Admin_CustPhotoId_Get", new { CustId = CustId }, commandType: CommandType.StoredProcedure);
        }
        public bool DelCustPhotoId(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustPhotoId_Del", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
        }

        public bool AddCustNote(int custId, string Note, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustNotes_Ins", new { CustId = custId, Note = Note, Operator = Operator }, commandType: CommandType.StoredProcedure) > 0;
        }

        public IEnumerable<CustNote> GetCustNotes(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<CustNote>("Admin_CustNotes_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<LoginLog> GetLoginLogByCust(int CustId, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new { CustId = CustId, StartDate = startDate, EndDate = endDate, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var depositList = conn.Query<LoginLog>("Admin_LoginLogByCust_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }
        public bool SetResetPwd(int custId, string md5NewPassword)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_ResetPwd_Set", new { CustId = custId, Md5NewPassword = md5NewPassword }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool ResetProfile(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_ResetProfile_Set", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool ResetCustBankInfo(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_ResetCustBankInfo_Set", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool SetCustStatus(int custId, int status)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustStatus_Set", new { CustId = custId, Status = status }, commandType: CommandType.StoredProcedure) > 0;
        }
        public decimal GetWithdrawalAvailableBalance(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted);
            var result = GetWithdrawalAvailableBalance(custId, conn, trans);
            trans.Rollback();
            return result;
        }
        public decimal GetWithdrawalAvailableBalance(int custId, MySqlConnection conn, MySqlTransaction trans)
        {
            return conn.ExecuteScalar<decimal>("Common_WithdrawalAvailableBalance_Get", new { CustId = custId }, transaction: trans, commandType: CommandType.StoredProcedure);
        }
    }
}
