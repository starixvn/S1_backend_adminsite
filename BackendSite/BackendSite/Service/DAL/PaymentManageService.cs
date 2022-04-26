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
using BackendSite.Service.Model.enums;
using Org.BouncyCastle.Utilities.Zlib;

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

        public bool UpdatePayList(ParamsWithLog<UpdatePayInfo> input)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();

            #region props
            var props = new DynamicParameters();
            props.Add("PayId", input.After.PayId);
            props.Add("Status", (int)input.After.Status);
            #endregion

            #region propsLog
            
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int) ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
            logParam.Add("_Operator", input.Operator);

            #endregion

            var result = _mainDbClient.Execute("Admin_PayList_Set", props, conn, trans) > 0;
            if (result)
            {
                var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;
                if (logResult)
                {
                    trans.Commit();
                    return true;
                }
            }
            
            trans.Rollback();
            return false; 
        }

        public bool UpdatePayChannel(ParamsWithLog<UpdatePayChannel> input)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();

            #region props
            var props = new DynamicParameters();
            props.Add("ChannelId", input.After.ChannelId);
            props.Add("Sort", input.After.Sort);
            props.Add("Status", (int)input.After.Status);
            props.Add("PointLevel", input.After.PointLevel);
            #endregion

            #region propsLog
            
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int) ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
            logParam.Add("_Operator", input.Operator);

            #endregion

            var result = _mainDbClient.Execute("Admin_PayChannel_Set", props, conn, trans) > 0;
            if (result)
            {
                var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;
                if (logResult)
                {
                    trans.Commit();
                    return true;
                }
            }
            
            trans.Rollback();
            return false;
        }

        public bool UpdatePayCurrency(ParamsWithLog<UpdatePayCurrency> input, string amountShow)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            
            #region props
            var props = new DynamicParameters();
            props.Add("ChannelId", input.After.ChannelId);
            props.Add("CurrencyId", input.CurrencyId);
            props.Add("MinAmount", input.After.MinAmount);
            props.Add("MaxAmount", input.After.MaxAmount);
            props.Add("AmountRule", (int)input.After.AmountRule);
            props.Add("AmountShow", amountShow, DbType.String);
            #endregion

            #region propsLog
            
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int) ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
            logParam.Add("_Operator", input.Operator);

            #endregion

            var result = _mainDbClient.Execute("Admin_PayCurrency_Set", props, conn, trans) > 0;
            if (result)
            {
                var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;
                if (logResult)
                {
                    trans.Commit();
                    return true;
                }
            }
            
            trans.Rollback();
            return false;
        }

        public bool UpdatePayCurrencyAmount(ParamsWithLog<UpdatePayCurrency> input)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();

            #region props
            var props = new DynamicParameters();
            props.Add("ChannelId", input.After.ChannelId);
            props.Add("CurrencyId", input.CurrencyId);
            props.Add("MinAmount", input.After.MinAmount);
            props.Add("MaxAmount", input.After.MaxAmount);
            #endregion

            #region propsLog
            
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int) ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
            logParam.Add("_Operator", input.Operator);

            #endregion

            var result = _mainDbClient.Execute("Admin_PayCurrency_SetAmount", props, conn, trans) > 0;
            if (result)
            {
                var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;
                if (logResult)
                {
                    trans.Commit();
                    return true;
                }
            }
            
            trans.Rollback();
            return false;
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
