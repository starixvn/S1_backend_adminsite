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
    public class PaymentManageService
    {
        private readonly IConfiguration configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly string _connectionString;
        public PaymentManageService(IConfiguration configuration, ILogger<MySqlClient> logger)
        {
            this.configuration = configuration;
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
            _connectionString = configuration.GetConnectionString("ConnectionString");
        }

        public IEnumerable<PayList> GetPayList()
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var SpName = "Admin_PayList_Get";
                return conn.Query<PayList>(SpName, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<PayChannel> GetPayChannelList()
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var SpName = "Admin_PayChannel_Get";
                return conn.Query<PayChannel>(SpName, new { PayId = 0 }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<PayCurrency> GetPayCurrencyList()
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var SpName = "Admin_PayCurrency_Get";
                return conn.Query<PayCurrency>(SpName, new { ChannelId = 0 }, commandType: CommandType.StoredProcedure);
            }
        }

        public void UpdatePayList(int PayId, int Status,string Operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("PayId", PayId);
            props.Add("Status", Status);
            #endregion

            #region propsLog
            var list = new Dictionary<string, string>();
            foreach (var paraName in props.ParameterNames)
            {
                var value = Convert.ToString(props.Get<dynamic>(paraName));
                list.Add(paraName, value);
            }
            propsLog.Add("Menu", "PaymentManagement");
            propsLog.Add("ActionType", 1);
            propsLog.Add("DetailInfo", JsonConvert.SerializeObject(list));
            propsLog.Add("Operator", Operator);
            #endregion

            _mainDbClient.Execute("Admin_PayList_Set", props, trans);
            var result = _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
        }

        public void UpdatePayChannel(int ChannelId, int Sort, int Status, int PointLevel, string Operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("ChannelId", ChannelId);
            props.Add("Sort", Sort);
            props.Add("Status", Status);
            props.Add("PointLevel", PointLevel);
            #endregion

            #region propsLog
            var list = new Dictionary<string, string>();
            foreach (var paraName in props.ParameterNames)
            {
                var value = Convert.ToString(props.Get<dynamic>(paraName));
                list.Add(paraName, value);
            }
            propsLog.Add("Menu", "PaymentManagement");
            propsLog.Add("ActionType", 1);
            propsLog.Add("DetailInfo", JsonConvert.SerializeObject(list));
            propsLog.Add("Operator", Operator);
            #endregion

            _mainDbClient.Execute("Admin_PayChannel_Set", props, trans);
            var result = _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
        }

        public void UpdatePayCurrency(int ChannelId, int CurrencyId, decimal MinAmount, decimal MaxAmount, int AmountRule, string AmountShow, string Operator)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    var SpName = "Admin_PayCurrency_Set";
                    conn.Execute(SpName, new { ChannelId, CurrencyId, MinAmount, MaxAmount, AmountRule, AmountShow }, tran, commandType: CommandType.StoredProcedure);

                    var list = new Dictionary<string, string>();

                    var props = new DynamicParameters();
                    props.Add("ChannelId", ChannelId);
                    props.Add("CurrencyId", CurrencyId);
                    props.Add("MinAmount", MinAmount);
                    props.Add("MaxAmount", MaxAmount);
                    props.Add("AmountRule", AmountRule);
                    props.Add("AmountShow", AmountShow);
                    foreach (var paraName in props.ParameterNames)
                    {
                        var value = Convert.ToString(props.Get<dynamic>(paraName));
                        list.Add(paraName, value);
                    }

                    conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu= "PaymentManagement", ActionType = 1, DetailInfo= JsonConvert.SerializeObject(list), Operator }, tran, commandType: CommandType.StoredProcedure);

                    tran.Commit();
                }
                    
            }
        }

        public void UpdatePayCurrencyAmount(int ChannelId, int CurrencyId, decimal MinAmount, decimal MaxAmount, string Operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("ChannelId", ChannelId);
            props.Add("CurrencyId", CurrencyId);
            props.Add("MinAmount", MinAmount);
            props.Add("MaxAmount", MaxAmount);
            #endregion

            #region propsLog
            var list = new Dictionary<string, string>();
            foreach (var paraName in props.ParameterNames)
            {
                var value = Convert.ToString(props.Get<dynamic>(paraName));
                list.Add(paraName, value);
            }
            propsLog.Add("Menu", "PaymentManagement");
            propsLog.Add("ActionType", 1);
            propsLog.Add("DetailInfo", JsonConvert.SerializeObject(list));
            propsLog.Add("Operator", Operator);
            #endregion

            _mainDbClient.Execute("Admin_PayCurrency_SetAmount", props, trans);
            var result = _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
        }

        public IEnumerable<PayListBankInfoSys> GetBankSysInfoList(int PayId)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var SpName = "Admin_BankInfoSys_Get";
                return conn.Query<PayListBankInfoSys>(SpName, new { PayId }, commandType: CommandType.StoredProcedure);
            }
        }

        public void UpdateBankInfo(int PayId, int SysId, int Status, string Operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("PayId", PayId);
            props.Add("SysId", SysId);
            props.Add("Status", Status);
            #endregion

            #region propsLog
            var list = new Dictionary<string, string>();
            foreach (var paraName in props.ParameterNames)
            {
                var value = Convert.ToString(props.Get<dynamic>(paraName));
                list.Add(paraName, value);
            }
            propsLog.Add("Menu", "PaymentManagement");
            propsLog.Add("ActionType", 1);
            propsLog.Add("DetailInfo", JsonConvert.SerializeObject(list));
            propsLog.Add("Operator", Operator);
            #endregion

            _mainDbClient.Execute("Admin_BankInfoPay_Set", props, trans);
            var result = _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
        }
    }
}
