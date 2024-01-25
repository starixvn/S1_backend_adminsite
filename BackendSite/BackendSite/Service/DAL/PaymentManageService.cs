using System.Data;

using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using System.Security.Policy;

using Dapper;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class PaymentManageService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly MySqlClient _reportbackupDbClient;

        public PaymentManageService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
            _reportbackupDbClient = sqlClients.FirstOrDefault(p => p.DataBase == DataBase.ReportBackupDB);
        }

        [Obsolete("請改用GetPayList(int siteId)")]
        public IEnumerable<PayList> GetPayList()
        {
            var SpName = "Admin_PayList_Get";
            return _mainDbClient.Query<PayList>(SpName);
        }

        #region PaymentGateway Setting Renew
        
        public IEnumerable<PayListInfo> GetPayList(IEnumerable<int> siteIds)
        {
            var _siteIds = JsonConvert.SerializeObject(siteIds);
            return _reportDbClient
                .Query<PayListInfo>("Admin_SitePayList_Get_V2", new DynamicParameters(new { _SiteId = _siteIds }));
        }

        public bool SetPayListStatus(int siteId, int payId, PayListStatusEnum status, string @operator)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var siteIds = Enumerable.Repeat(siteId, 1);
            var pay = GetPayList(siteIds)
                .First(x => x.PayId == payId);

            #region props

            var props = new DynamicParameters(new
            {
                _SiteId = siteId,
                _PayId = payId,
                _Status = (int)status
            });

            #endregion

            #region propsLog
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", siteId);
            logParam.Add("_CurrencyId", 0);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int)ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(new
            {
                pay.PayName,
                Status = pay.Status.ToString()
            }));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                pay.PayName,
                Status = status.ToString()
            }));
            logParam.Add("_Operator", @operator);
            #endregion

            if (_mainDbClient.Execute("Admin_SitePayList_Status_Set", trans, props) > 0)
            {
                if (_mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            return false;
        }

        public bool SetPayListLoginInfo(SetPayListLoginInfoParams input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var siteIds = Enumerable.Repeat(input.SiteId, 1);
            var pay = GetPayList(siteIds)
                .First(x => x.PayId == input.PayId);

            #region props

            var props = new DynamicParameters(new
            {
                _SiteId = input.SiteId,
                _PayId = input.PayId,
                _LoginUrl = input.LoginUrl,
                _LoginAccount = input.LoginAccount,
                _LoginPassword = input.LoginPassword,
                _LoginPin = input.LoginPin,
                _TransferPin = input.TransferPin,
                _CorporateId = input.CorporateId
            });

            #endregion

            #region propsLog
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", 0);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int)ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(new
            {
                pay.PayName,
                pay.LoginUrl,
                pay.LoginAccount,
                pay.LoginPassword,
                pay.LoginPin,
                pay.TransferPin,
                pay.CorporateId
            }));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                pay.PayName,
                input.LoginUrl,
                input.LoginAccount,
                input.LoginPassword,
                input.LoginPin,
                input.TransferPin,
                input.CorporateId
            }));
            logParam.Add("_Operator", input.Operator);
            #endregion

            if (_mainDbClient.Execute("Admin_SitePayList_LoginInfo_Set_V2", trans, props) > 0)
            {
                if (_mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            return false;
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

            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportbackupDbClient != null
                ? _reportbackupDbClient.RetryQuery<PaySetting>("Admin_PaySetting_GetList", p)
                : _reportDbClient.Query<PaySetting>("Admin_PaySetting_GetList", p);

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
            using var conn = _mainDbClient.GetMySqlConnection();
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
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int)ActionType.Update);
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

            if (_mainDbClient.Execute("Admin_PaySetting_Set", trans, props) > 0)
            {
                if (_mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            return false;
        }

        public bool UpdatePaySettingStatus(UpdatePaySettingStatusParams input, string @operator)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
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
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int)ActionType.Update);
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

            if (_mainDbClient.Execute("Admin_PaySetting_Status_Set", trans, props) > 0)
            {
                if (_mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            return false;
        }

        public bool UpdatePaySettingChargeFee(UpdatePaySettingChargeFeeParams input, string @operator)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var paySetting = GetPaySetting(input.SiteId, input.ChannelId)
                .First(x => x.CurrencyId == input.CurrencyId);

            #region props
            var props = new DynamicParameters();
            props.Add("_SiteId", input.SiteId);
            props.Add("_CurrencyId", (int)input.CurrencyId);
            props.Add("_ChannelId", input.ChannelId);
            props.Add("_ChargeFeePercentage", input.Percentage);
            props.Add("_ChargeFeePreCount", input.PreCount);
            #endregion

            #region propsLog
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int)ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(new
            {
                paySetting.ChannelName,
                Status = paySetting.Status.ToString()
            }));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                paySetting.ChannelName,
                input.Percentage,
                input.PreCount
            }));
            logParam.Add("_Operator", @operator);
            #endregion

            if (_mainDbClient.Execute("Admin_PaySetting_ChargeFee_Set", trans, props) > 0)
            {
                if (_mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            return false;
        }

        public IEnumerable<BankInfoPayForSetting> GetBankInfoPayList(int siteId, int payId)
        {
            return _reportDbClient.Query<BankInfoPayForSetting>("Admin_BankInfoPay_Get_V3", new
            {
                _PayId = payId,
                _SiteId = siteId
            });
        }

        public IEnumerable<BankInfoPayForSetting> GetBankInfoPayForSetting(int siteId, int currencyId, int channelId)
        {
            return _reportDbClient.Query<BankInfoPayForSetting>("Admin_BankInfoPayForSetting_Get_V3", new
            {
                _SiteId = siteId,
                _CurrencyId = currencyId,
                _ChannelId = channelId
            });
        }

        public bool UpdateBankInfo(int siteId, int payId, int sysId,
            BankInfoPayStatusEnum status, int channelId, string @operator)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var bankSysInfo = GetBankInfoPayList(siteId, payId)
                .First(x => x.Id == sysId && x.ChannelId == channelId);

            #region props
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_PayId", payId);
            props.Add("_SysId", sysId);
            props.Add("_Status", status);
            props.Add("_ChannelId", channelId);
            #endregion

            #region propsLog
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", siteId);
            logParam.Add("_CurrencyId", bankSysInfo.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int)ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(new
            {
                bankSysInfo.BankName,
                Method = bankSysInfo.ChannelName,
                Status = bankSysInfo.Status.ToString()
            }));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                bankSysInfo.BankName,
                Method = bankSysInfo.ChannelName,
                Status = status.ToString()
            }));
            logParam.Add("_Operator", @operator);
            #endregion

            var setResult = _mainDbClient.Execute("Admin_BankInfoPay_Set_V4", trans, props);
            if (setResult > 0)
            {
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam);
                if (logResult > 0)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, trans);

            return false;
        }

        public IEnumerable<CryptoInfo> GetCryptoInfoList(int siteId)
            => _reportDbClient.Query<CryptoInfo>("Admin_CryptoInfo_Get_V2", new { _SiteId = siteId });

        public bool SetCryptoStatus(int siteId, CryptoCurrencyEnum currencyId, string network, GeneralStatusEnum status, string @operator)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            #region props
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyId", currencyId);
            props.Add("_Network", network);
            props.Add("_Status", status);
            #endregion

            #region propsLog
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", siteId);
            logParam.Add("_CurrencyId", currencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.PaymentGateway);
            logParam.Add("_ActionType", (int)ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(new
            {
                CryptoCurrency = currencyId.ToString(),
                Network = network,
                Status = (status == GeneralStatusEnum.Enable ? GeneralStatusEnum.Disable : GeneralStatusEnum.Enable).ToString()
            }));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                CryptoCurrency = currencyId.ToString(),
                Network = network,
                Status = status.ToString()
            }));
            logParam.Add("_Operator", @operator);
            #endregion

            var setResult = _mainDbClient.Execute("Admin_CryptoNetwork_Set", trans, props);
            if (setResult > 0)
            {
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam);
                if (logResult > 0)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, trans);

            return false;
        }

        public async Task<IEnumerable<PayGroup>> GetPayGroup()
        {
            return await _reportDbClient.QueryAsync<PayGroup>("Service_PayGroup_Get");
        }
        
        public IEnumerable<WithdrawalBlockSetting> GetWithdrawalBlockSettingList(int siteId)
        {
            var p = new DynamicParameters(new
            {
                _SiteId = siteId
            });
            
            return _reportDbClient.Query<WithdrawalBlockSetting>("Service_WithdrawalBlockSetting_Get", p);
        }
        
        public bool SetWithdrawalBlockSetting(SetWithdrawalBlockSettingParams input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var setting = GetWithdrawalBlockSettingList(input.SiteId)
                .First(x => x.CurrencyId == input.CurrencyId
                    && x.Type == input.Type);

            #region props
            var props = new DynamicParameters();
            props.Add("_SiteId", input.SiteId);
            props.Add("_CurrencyId", input.CurrencyId);
            props.Add("_Type", input.Type);
            props.Add("_Status", input.Status);
            #endregion

            #region propsLog
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.WithdrawalBlockSetting);
            logParam.Add("_ActionType", (int) ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(new
            {
                Type = input.Type.ToString(),
                Status = setting.Status.ToString()
            }));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
            {
                Type = input.Type.ToString(),
                Status = input.Status.ToString()
            }));
            logParam.Add("_Operator", input.Operator);
            #endregion

            if (_mainDbClient.Execute("Admin_WithdrawalBlockSetting_Set", trans, props) > 0)
            {
                if (_mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return true;
                }
            }
            
            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            return false;
        }

        #endregion
    }
}
