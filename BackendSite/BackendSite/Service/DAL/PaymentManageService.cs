using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data;
using BackendSite.Extensions;
using BackendSite.Service.Model.enums;

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

        [Obsolete("請改用GetPayList(int siteId)")]
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
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", input.After.ToLog());
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
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", input.After.ToLog());
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
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", input.After.ToLog());
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
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", input.After.ToLog());
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

        #region PaymentGateway Setting Renew
        
        public IEnumerable<PayListInfo> GetPayList(int siteId)
        {
            return _reportDbClient
                .Query<PayListInfo>("Admin_SitePayList_Get", new DynamicParameters(new { _SiteId = siteId }));
        }
        
        public bool SetPayListStatus(int siteId, int payId, PayListStatusEnum status)
        {
            return _mainDbClient .Execute("Admin_SitePayList_Status_Set",
                new DynamicParameters(new
                {
                    _SiteId = siteId,
                    _PayId = payId,
                    _Status = (int)status
                })) > 0;
        }
        
        public IEnumerable<PaySetting> GetPaySettingList(GetPaySettingListParams input, int pageSize, int startIndex, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new
            {
                _SiteId = input.SiteId,
                _CurrencyIds = input.CurrencyIds == null ? null : JsonConvert.SerializeObject(input.CurrencyIds),
                _Method = input.Method,
                _Status = input.Status,
                _PayStatus = input.PayStatus,
                _Type = input.Type,
                _PayId = input.PayId,
                _PageSize = pageSize,
                _StartIndex = startIndex
            });

            p.Add("_TotalPages", dbType:DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType:DbType.Int32, direction: ParameterDirection.Output);
            
            var result = _reportDbClient.Query<PaySetting>("Admin_PaySetting_GetList", p);
            
            totalPages = p.Get<int>("_TotalPages");
            rowCount = p.Get<int>("_RowCount");

            return result;
        }
        
        public IEnumerable<PaySetting> GetPaySetting(int siteId, int? channelId = null)
        {
            var p = new DynamicParameters(new
            {
                _SiteId = siteId,
                _ChannelId = channelId
            });
            
            return _reportDbClient.Query<PaySetting>("Admin_PaySetting_Get", p);
        }
        
        public bool UpdatePaySetting(UpdatePaySettingParams input, string @operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var paySetting = GetPaySetting(input.SiteId, input.ChannelId)
                .First(x => x.CurrencyId == input.CurrencyId);

            var jsonAmountShow = JsonConvert.SerializeObject(input.FixedAmountList);
            #region props
            var props = new DynamicParameters();
            props.Add("_SiteId", input.SiteId);
            props.Add("_CurrencyId", (int)input.CurrencyId);
            props.Add("_ChannelId", input.ChannelId);
            props.Add("_MinAmount", input.MinAmount);
            props.Add("_MaxAmount", input.MaxAmount);
            props.Add("_AmountRule", (int)input.AmountRule);
            props.Add("_AmountShow", jsonAmountShow);
            props.Add("_Sort", input.Sort);
            props.Add("_PointLevel", input.PointLevel);
            #endregion

            #region propsLog
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int) ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(new
            {
                paySetting.ChannelName,
                paySetting.PointLevel,
                paySetting.MinAmount,
                paySetting.MaxAmount,
                AmountRule = paySetting.AmountRule.ToString(),
                paySetting.AmountShow,
                paySetting.Sort
            }));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                paySetting.ChannelName,
                input.PointLevel,
                input.MinAmount,
                input.MaxAmount,
                AmountRule = input.AmountRule.ToString(),
                AmountShow = jsonAmountShow,
                input.Sort
            }));
            logParam.Add("_Operator", @operator);
            #endregion

            if (_mainDbClient.Execute("Admin_PaySetting_Set", props, conn, trans) > 0)
            {
                if (_mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, conn, trans) > 0)
                {
                    trans.Commit();
                    return true;
                }
            }
            
            trans.Rollback();
            return false;
        }
        
        public bool UpdatePaySettingStatus(UpdatePaySettingStatusParams input, string @operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var paySetting = GetPaySetting(input.SiteId, input.ChannelId)
                .First(x => x.CurrencyId == input.CurrencyId);

            #region props
            var props = new DynamicParameters();
            props.Add("_SiteId", input.SiteId);
            props.Add("_CurrencyId", (int)input.CurrencyId);
            props.Add("_ChannelId", input.ChannelId);
            props.Add("_Status", (int)input.Status);
            #endregion

            #region propsLog
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int) ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(new
            {
                paySetting.ChannelName,
                Status = paySetting.Status.ToString()
            }));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                paySetting.ChannelName,
                Status = input.Status.ToString()
            }));
            logParam.Add("_Operator", @operator);
            #endregion

            if (_mainDbClient.Execute("Admin_PaySetting_Status_Set", props, conn, trans) > 0)
            {
                if (_mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, conn, trans) > 0)
                {
                    trans.Commit();
                    return true;
                }
            }
            
            trans.Rollback();
            return false;
        }
        
        public IEnumerable<PayListBankInfoSys> GetBankSysInfoList(int siteId, int payId)
        {
            return _reportDbClient.Query<PayListBankInfoSys>("Admin_BankInfoSys_GetV2", new
            {
                _PayId = payId,
                _SiteId = siteId
            });
        }
        
        public IEnumerable<PayListBankInfoSys> GetBankInfoPayForSetting(int siteId, int currencyId, int channelId)
        {
            return _reportDbClient.Query<PayListBankInfoSys>("Admin_BankInfoPayForSetting_Get", new
            {
                _SiteId = siteId,
                _CurrencyId = currencyId,
                _ChannelId = channelId
            });
        }
        
        public bool UpdateBankInfo(int siteId, int payId, int sysId, BankInfoPayStatusEnum status, string @operator)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var bankSysInfo = GetBankSysInfoList(siteId, payId)
                .First(x => x.Id == sysId);

            #region props
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_PayId", payId);
            props.Add("_SysId", sysId);
            props.Add("_Status", status);
            #endregion

            #region propsLog
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", siteId);
            logParam.Add("_CurrencyId", bankSysInfo.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int) ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(new
            {
                bankSysInfo.BankName,
                Status = bankSysInfo.Status.ToString()
            }));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                bankSysInfo.BankName,
                Status = status.ToString()
            }));
            logParam.Add("_Operator", @operator);
            #endregion

            var setResult = _mainDbClient.Execute("Admin_BankInfoPay_Set_V2", props, conn, trans);
            if (setResult > 0)
            {
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, conn, trans);
                if (logResult > 0)
                {
                    trans.Commit();
                    return true;
                }
            }

            trans.Rollback();
                
            return false;
        }

        #endregion
    }
}
