using System.Data;

using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankCardManagement;
using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class BankDetailService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly MySqlClient _reportbackupDbClient;

        public BankDetailService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
            _reportbackupDbClient = sqlClients.FirstOrDefault(p => p.DataBase == DataBase.ReportBackupDB);
        }

        public async Task<(IEnumerable<OfflineBankingSetting> Datas, int TotalPages, int RowCount)> GetOfflineBankingSettingListAsync
            (GetOfflineBankingSettingParams input)
        {
            var para = new DynamicParameters();
            para.Add("_SiteId", input.SiteId);
            para.Add("_CurrencyId", input.CurrencyId);
            para.Add("_Purposes", input.Purposes.Any() ? JsonConvert.SerializeObject(input.Purposes) : "[]", DbType.String);
            para.Add("_Statuses", JsonConvert.SerializeObject(input.Statuses));
            para.Add("_SysIds", input.SysIds.Any() ? JsonConvert.SerializeObject(input.SysIds) : "[]", DbType.String);
            para.Add("_VipLevels", input.VipLevels.Any() ? JsonConvert.SerializeObject(input.VipLevels) : "[]", DbType.String);
            para.Add("_PointLevels", input.PointLevels.Any() ? JsonConvert.SerializeObject(input.PointLevels) : "[]", DbType.String);
            para.Add("_AccountNo", input.AccountNo ?? string.Empty);
            para.Add("_Agent", input.Agent ?? string.Empty);
            para.Add("_Channels", input.SupportChannels.Any() ? JsonConvert.SerializeObject(input.SupportChannels) : "[]", DbType.String);
            para.Add("_StartIndex", input.StartIndex);
            para.Add("_PageSize", input.PageSize);
            para.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            para.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportbackupDbClient != null
                ? await _reportbackupDbClient.RetryQueryAsync<OfflineBankingSetting>("Admin_OfflineBankingSetting_Get_V3", para)
                : await _reportDbClient.QueryAsync<OfflineBankingSetting>("Admin_OfflineBankingSetting_Get_V3", para);

            var totalPages = para.Get<int?>("_TotalPages") ?? 0;
            var rowCount = para.Get<int?>("_RowCount") ?? 0;

            return (result, totalPages, rowCount);
        }

        public async Task<OfflineBankingSetting> GetOfflineBankingSettingByIdAsync(int id)
        {
            var para = new DynamicParameters();
            para.Add("_FinancialAccountId", id);

            var result = await _reportDbClient
                .QueryAsync<OfflineBankingSetting>("Admin_OfflineBankingSetting_GetById_V2", para);

            return result.FirstOrDefault();
        }

        public Task<int> SetOfflineBankingSettingAsync(SetOfflineBankingSettingParams input)
        {
            var para = new DynamicParameters();
            para.Add("_FinancialAccountId", input.FinancialAccountId);
            para.Add("_MinAmount", input.MinAmount);
            para.Add("_MaxAmount", input.MaxAmount);
            para.Add("_DailyLimitAmount", input.DailyLimitAmount);
            para.Add("_VipLevels", JsonConvert.SerializeObject(input.VipLevels));
            para.Add("_PointLevels", JsonConvert.SerializeObject(input.PointLevels));
            para.Add("_Memo", input.Memo);
            para.Add("_Status", (int)input.Status);
            para.Add("_Sort", input.Sort);
            para.Add("_SupportChannel", JsonConvert.SerializeObject(input.SupportChannels));
            para.Add("_GroupIds", JsonConvert.SerializeObject(input.GroupIds));
            para.Add("_QRCodeUrl", input.QrCodeUrl);
            para.Add("_Operator", input.Operator);


            return _mainDbClient.ExecuteAsync("Admin_OfflineBankingSetting_Set_V2", para);
        }

        public async Task<IEnumerable<LocalPaySetting>> GetLocalPaySettingAsync()
        {
            return await _mainDbClient.QueryAsync<LocalPaySetting>("Admin_LocalPaySetting_Get");
        }

        public int SetBankCard(ParamsWithLog<SetBankCard> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("SiteId", input.SiteId);
            props.Add("SysId", input.After.SysId);
            props.Add("BankAccountName", input.After.BankAccountName);
            props.Add("BankAccountNo", input.After.BankAccountNo);
            props.Add("MinAmount", input.After.MinAmount);
            props.Add("MaxAmount", input.After.MaxAmount);
            props.Add("DailyLimitAmount", input.After.DailyLimitAmount);
            props.Add("PointLevel", input.After.PointLevel);
            props.Add("Purpose", input.After.Purpose);
            props.Add("Status", input.After.Status);
            props.Add("Sort", input.After.Sort);
            #endregion

            #region propsLog
            var isCreate = input.Before.SysId == 0;
            propsLog.Add("_SiteId", input.SiteId);
            propsLog.Add("_CurrencyId", input.CurrencyId);
            propsLog.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.BankCardManagement);
            propsLog.Add("_ActionType", (int)(isCreate ? ActionType.Create : ActionType.Update));
            propsLog.Add("_RefTransId", string.Empty);
            propsLog.Add("_BeforeDetailInfo", isCreate ? null : input.Before.ToLog());
            propsLog.Add("_AfterDetailInfo", input.After.ToLog());
            propsLog.Add("_Operator", input.Operator);
            #endregion

            var r = _mainDbClient.Execute("BankCard_Set", trans, props);
            var result = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, propsLog);
            if (result > 0)
            {
                _mainDbClient.CommitAndDisposeConnection(conn, trans);
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }
            return result;
        }

        public int SetBankCardwithIFSC(ParamsWithLog<SetBankCard> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("SiteId", input.SiteId);
            props.Add("SysId", input.After.SysId);
            props.Add("BankAccountName", input.After.BankAccountName);
            props.Add("BankAccountNo", input.After.BankAccountNo);
            props.Add("IFSCCode", input.After.IFSCCode ?? string.Empty);
            props.Add("MinAmount", input.After.MinAmount);
            props.Add("MaxAmount", input.After.MaxAmount);
            props.Add("DailyLimitAmount", input.After.DailyLimitAmount);
            props.Add("PointLevel", input.After.PointLevel);
            props.Add("Purpose", input.After.Purpose);
            props.Add("Status", input.After.Status);
            props.Add("Sort", input.After.Sort);
            #endregion

            #region propsLog
            var isCreate = input.Before == null || input.Before.SysId == 0;
            propsLog.Add("_SiteId", input.SiteId);
            propsLog.Add("_CurrencyId", input.CurrencyId);
            propsLog.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.BankCardManagement);
            propsLog.Add("_ActionType", (int)(isCreate ? ActionType.Create : ActionType.Update));
            propsLog.Add("_RefTransId", string.Empty);
            propsLog.Add("_BeforeDetailInfo", isCreate ? null : input.Before.ToLog(), DbType.String);
            propsLog.Add("_AfterDetailInfo", input.After.ToLog());
            propsLog.Add("_Operator", input.Operator);
            #endregion

            var r = _mainDbClient.Execute("BankCardwithIFSC_Set", trans, props);
            var result = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, propsLog);
            if (result > 0)
            {
                _mainDbClient.CommitAndDisposeConnection(conn, trans);
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }
            return result;
        }
        public int DelBankCard(ParamsWithLog<DeleteBankCard> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("SiteId", input.SiteId);
            props.Add("BankId", input.Before.BankId);
            #endregion

            #region propsLog
            propsLog.Add("_SiteId", input.SiteId);
            propsLog.Add("_CurrencyId", input.CurrencyId);
            propsLog.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.BankCardManagement);
            propsLog.Add("_ActionType", (int)ActionType.Delete);
            propsLog.Add("_RefTransId", string.Empty);
            propsLog.Add("_BeforeDetailInfo", input.Before.ToLog());
            propsLog.Add("_AfterDetailInfo", null, DbType.String);
            propsLog.Add("_Operator", input.Operator);
            #endregion

            var r = _mainDbClient.Execute("BankCard_Del", trans, props);
            var result = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, propsLog);
            if (result > 0)
            {
                _mainDbClient.CommitAndDisposeConnection(conn, trans);
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }
            return result;
        }

        public IEnumerable<BankInfoSys> GetBankSysInfoList()
            => _reportDbClient.Query<BankInfoSys>("Admin_BankInfoSys_GetAll");

        public bool SetBankSysInfo(SetBankInfoSysModel input, BankInfoSys before, string @operator)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            
            #region props
            props.Add("_SysId", input.SysId);
            props.Add("_BankCode", input.BankCode);
            props.Add("_BankName", input.BankName);
            props.Add("_Status", input.Status);
            #endregion

            var result = _mainDbClient.Execute("Admin_BankInfoSys_Set", trans, props);
            
            if (result > 0)
            {
                #region propsLog
                props.Add("_SiteId", 0);
                props.Add("_CurrencyId", before.CurrencyId);
                props.Add("_Menu", AdminAdjustmentLogMenuEnum.EditSystemBankInfo);
                props.Add("_ActionType", ActionType.Update);
                props.Add("_RefTransId", string.Empty);
                props.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(new
                {
                    before.SysId,
                    before.BankCode,
                    before.BankName,
                    Status = before.Status.ToString()
                }));
                props.Add("_AfterDetailInfo", JsonConvert.SerializeObject(new
                {
                    input.SysId,
                    input.BankCode,
                    input.BankName,
                    Status = input.Status.ToString()
                }));
                props.Add("_Operator", @operator);
                #endregion
                
                result = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, props);
                if (result > 0)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);

                    return true;
                }
            }
            
            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            
            return false;
        }
    }
}
