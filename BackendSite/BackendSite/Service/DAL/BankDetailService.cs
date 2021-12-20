using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class BankDetailService
    {
        private readonly IConfiguration configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly string _connectionString;

        public BankDetailService(IConfiguration configuration, ILogger<MySqlClient> logger)
        {
            this.configuration = configuration;
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
            _connectionString = configuration.GetConnectionString("ConnectionString");
        }

        public IEnumerable<BankDetail> GetLoaclBankList(int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<BankDetail>("LoaclBankList_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public int SetBankCard(int siteId, int SysId, string BankAccountName, string BankAccountNo, decimal MinAmount, decimal MaxAmount, decimal DailyLimitAmount, int PointLevel, int Purpose, int Status, int Sort,string Operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("SiteId", siteId);
            props.Add("SysId", SysId);
            props.Add("BankAccountName", BankAccountName);
            props.Add("BankAccountNo", BankAccountNo);
            props.Add("MinAmount", MinAmount);
            props.Add("MaxAmount", MaxAmount);
            props.Add("DailyLimitAmount", DailyLimitAmount);
            props.Add("PointLevel", PointLevel);
            props.Add("Purpose", Purpose);
            props.Add("Status", Status);
            props.Add("Sort", Sort);
            #endregion

            #region propsLog
            var list = new Dictionary<string, string>();
            foreach (var paraName in props.ParameterNames)
            {
                var value = Convert.ToString(props.Get<dynamic>(paraName));
                list.Add(paraName, value);
            }
            propsLog.Add("Menu", "BankCardManagement");
            propsLog.Add("ActionType",1);
            propsLog.Add("DetailInfo", JsonConvert.SerializeObject(list));
            propsLog.Add("Operator", Operator);
            #endregion

            _mainDbClient.Execute("BankCard_Set", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, conn, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
            return result;
        }

        public int SetBankCardwithIFSC(int siteId, int SysId, string BankAccountName, string BankAccountNo, string IFSC, decimal MinAmount, decimal MaxAmount, decimal DailyLimitAmount, int PointLevel, int Purpose, int Status, int Sort, string Operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("SiteId", siteId);
            props.Add("SysId", SysId);
            props.Add("BankAccountName", BankAccountName);
            props.Add("BankAccountNo", BankAccountNo);
            props.Add("IFSCCode", IFSC);
            props.Add("MinAmount", MinAmount);
            props.Add("MaxAmount", MaxAmount);
            props.Add("DailyLimitAmount", DailyLimitAmount);
            props.Add("PointLevel", PointLevel);
            props.Add("Purpose", Purpose);
            props.Add("Status", Status);
            props.Add("Sort", Sort);
            #endregion

            #region propsLog
            var list = new Dictionary<string, string>();
            foreach (var paraName in props.ParameterNames)
            {
                var value = Convert.ToString(props.Get<dynamic>(paraName));
                list.Add(paraName, value);
            }
            propsLog.Add("Menu", "BankCardManagement");
            propsLog.Add("ActionType", 1);
            propsLog.Add("DetailInfo", JsonConvert.SerializeObject(list));
            propsLog.Add("Operator", Operator);
            #endregion

            _mainDbClient.Execute("BankCardwithIFSC_Set", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, conn, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
            return result;
        }
        public int DelBankCard(int siteId, int bankId, string Operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("SiteId", siteId);
            props.Add("BankId", bankId);
            #endregion

            #region propsLog
            var list = new Dictionary<string, string>();
            foreach (var paraName in props.ParameterNames)
            {
                var value = Convert.ToString(props.Get<dynamic>(paraName));
                list.Add(paraName, value);
            }
            propsLog.Add("Menu", "BankCardManagement");
            propsLog.Add("ActionType", 2);
            propsLog.Add("DetailInfo", JsonConvert.SerializeObject(list));
            propsLog.Add("Operator", Operator);
            #endregion

            _mainDbClient.Execute("BankCard_Del", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, conn, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
            return result;
        }
        public bool AddBankDetail(int siteId, BankDetail bankDetail)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("BankDetail_Ins", new { SiteId = siteId, SysId = bankDetail.SysId, BankAccountName = bankDetail.AccountName, BankAccountNo = bankDetail.AccountNo, MinAmount = bankDetail.MinAmount, MaxAmount = bankDetail.MaxAmount, PointLevel = bankDetail.PointLevel }, commandType: CommandType.StoredProcedure) > 0;
        }

        public bool EditBankDetail(int siteId, BankDetail bankDetail)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("BankDetail_Set", new { SiteId = siteId, SysId = bankDetail.BankId, BankAccountName = bankDetail.AccountName, BankAccountNo = bankDetail.AccountNo, MinAmount = bankDetail.MinAmount, MaxAmount = bankDetail.MaxAmount, PointLevel = bankDetail.PointLevel }, commandType: CommandType.StoredProcedure) > 0;
        }

        public IEnumerable<BankInfoSys> GetSysBankList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<BankInfoSys>("BankInfoSysList_Get", new { SiteId = (int?)null }, commandType: CommandType.StoredProcedure);
        }
    }
}