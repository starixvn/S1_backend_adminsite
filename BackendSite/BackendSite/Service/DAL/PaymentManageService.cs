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
            var SpName = "Admin_PayList_Get";
            return _mainDbClient.Query<PayList>(SpName);
        }

        public IEnumerable<PayChannel> GetPayChannelList()
        {
            var SpName = "Admin_PayChannel_Get";
            return _mainDbClient.Query<PayChannel>(SpName, new DynamicParameters(new { PayId = 0 }));
        }

        public IEnumerable<PayCurrency> GetPayCurrencyList()
        {
            var SpName = "Admin_PayCurrency_Get";
            return _mainDbClient.Query<PayCurrency>(SpName, new DynamicParameters(new { ChannelId = 0 }));
        }

        public void UpdatePayList(int payId, int status,string @operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("PayId", payId);
            props.Add("Status", status);
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
            propsLog.Add("Operator", @operator);
            #endregion

            _mainDbClient.Execute("Admin_PayList_Set", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, conn, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
        }

        public void UpdatePayChannel(int channelId, int sort, int status, int pointLevel, string @operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("ChannelId", channelId);
            props.Add("Sort", sort);
            props.Add("Status", status);
            props.Add("PointLevel", pointLevel);
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
            propsLog.Add("Operator", @operator);
            #endregion

            _mainDbClient.Execute("Admin_PayChannel_Set", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, conn, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
        }

        public void UpdatePayCurrency(int channelId, int currencyId, decimal minAmount, decimal maxAmount, int amountRule, string amountShow, string @operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var SpName = "Admin_PayCurrency_Set";
            conn.Execute(SpName, new {ChannelId = channelId, CurrencyId = currencyId, MinAmount = minAmount, MaxAmount = maxAmount,
                AmountRule = amountRule,
                AmountShow = amountShow }, tran, commandType: CommandType.StoredProcedure);

            var list = new Dictionary<string, string>();

            var props = new DynamicParameters();
            props.Add("ChannelId", channelId);
            props.Add("CurrencyId", currencyId);
            props.Add("MinAmount", minAmount);
            props.Add("MaxAmount", maxAmount);
            props.Add("AmountRule", amountRule);
            props.Add("AmountShow", amountShow);
            foreach (var paraName in props.ParameterNames)
            {
                var value = Convert.ToString(props.Get<dynamic>(paraName));
                list.Add(paraName, value);
            }

            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu= "PaymentManagement", ActionType = 1, DetailInfo= JsonConvert.SerializeObject(list), Operator = @operator }, tran, commandType: CommandType.StoredProcedure);

            tran.Commit();
        }

        public void UpdatePayCurrencyAmount(int channelId, int currencyId, decimal minAmount, decimal maxAmount, string @operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("ChannelId", channelId);
            props.Add("CurrencyId", currencyId);
            props.Add("MinAmount", minAmount);
            props.Add("MaxAmount", maxAmount);
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
            propsLog.Add("Operator", @operator);
            #endregion

            _mainDbClient.Execute("Admin_PayCurrency_SetAmount", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, conn, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
        }

        public IEnumerable<PayListBankInfoSys> GetBankSysInfoList(int payId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var SpName = "Admin_BankInfoSys_Get";
            return conn.Query<PayListBankInfoSys>(SpName, new {PayId = payId }, commandType: CommandType.StoredProcedure);
        }

        public void UpdateBankInfo(int payId, int sysId, int status, string @operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("PayId", payId);
            props.Add("SysId", sysId);
            props.Add("Status", status);
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
            propsLog.Add("Operator", @operator);
            #endregion

            _mainDbClient.Execute("Admin_BankInfoPay_Set", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", propsLog, conn, trans);
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
