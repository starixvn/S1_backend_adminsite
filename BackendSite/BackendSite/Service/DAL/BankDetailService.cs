using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using BackendSite.Extensions;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.BankCardManagement;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

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

        public async Task<(IEnumerable<OfflineBankingSetting> Datas, int TotalPages, int RowCount)> GetOfflineBankingSettingListAsync
            (GetOfflineBankingSettingParams input)
        {
            var para = new DynamicParameters();
            para.Add("_SiteId", input.SiteId);
            para.Add("_CurrencyId", input.CurrencyId);
            para.Add("_Purposes", input.Purposes.Any() ? JsonConvert.SerializeObject(input.Purposes) : null, DbType.String);
            para.Add("_Statuses", JsonConvert.SerializeObject(input.Statuses));
            para.Add("_SysIds", JsonConvert.SerializeObject(input.SysIds));
            para.Add("_VipLevels", input.VipLevels.Any() ? JsonConvert.SerializeObject(input.VipLevels) : null, DbType.String);
            para.Add("_PointLevels", input.PointLevels.Any() ? JsonConvert.SerializeObject(input.PointLevels) : null, DbType.String);
            para.Add("_AccountNo", input.AccountNo ?? string.Empty);
            para.Add("_Agent", input.Agent ?? string.Empty);
            para.Add("_Channels", input.SupportChannels.Any() ? JsonConvert.SerializeObject(input.SupportChannels) : null, DbType.String);
            para.Add("_StartIndex", input.StartIndex);
            para.Add("_PageSize", input.PageSize);
            para.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            para.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _reportDbClient.QueryAsync<OfflineBankingSetting>("Admin_OfflineBankingSetting_Get", para);

            var totalPages = para.Get<int?>("_TotalPages") ?? 0;
            var rowCount = para.Get<int?>("_RowCount") ?? 0;

            return (result, totalPages, rowCount);
        }
        
        public async Task<OfflineBankingSetting> GetOfflineBankingSettingByIdAsync(int id)
        {
            var para = new DynamicParameters();
            para.Add("_FinancialAccountId", id);

            var result = await _reportDbClient
                .QueryAsync<OfflineBankingSetting>("Admin_OfflineBankingSetting_GetById", para);

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
            para.Add("_QRCodeUrl", input.QrCodeUrl);
            para.Add("_Operator", input.Operator);

            
            return _mainDbClient.ExecuteAsync("Admin_OfflineBankingSetting_Set", para);
        }
        
        public IEnumerable<BankDetail> GetLoaclBankList(string siteIds,
            string currencyIdList,
            string statusList, 
            int startIndex,int pageSize,
            out int totalPages, out int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteIds);
            props.Add("_CurrencyId", currencyIdList);
            props.Add("_Status", statusList);
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<BankDetail>("LoaclBankList_Get_V2", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public int SetBankCard(ParamsWithLog<SetBankCard> input)
        {
            using var conn = new MySqlConnection(_connectionString);
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
            propsLog.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BankCardManagement);
            propsLog.Add("_ActionType", (int) (isCreate ? ActionType.Create : ActionType.Update));
            propsLog.Add("_RefTransId", string.Empty);
            propsLog.Add("_BeforeDetailInfo", isCreate ? null :input.Before.ToLog());
            propsLog.Add("_AfterDetailInfo", input.After.ToLog());
            propsLog.Add("_Operator", input.Operator);
            #endregion

            var r = _mainDbClient.Execute("BankCard_Set", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", propsLog, conn, trans);
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

        public int SetBankCardwithIFSC(ParamsWithLog<SetBankCard> input)
        {
            using var conn = new MySqlConnection(_connectionString);
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
            propsLog.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BankCardManagement);
            propsLog.Add("_ActionType", (int) (isCreate ? ActionType.Create : ActionType.Update));
            propsLog.Add("_RefTransId", string.Empty);
            propsLog.Add("_BeforeDetailInfo", isCreate ? null : input.Before.ToLog(), DbType.String);
            propsLog.Add("_AfterDetailInfo", input.After.ToLog());
            propsLog.Add("_Operator", input.Operator);
            #endregion

            var r = _mainDbClient.Execute("BankCardwithIFSC_Set", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", propsLog, conn, trans);
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
        public int DelBankCard(ParamsWithLog<DeleteBankCard> input)
        {
            using var conn = new MySqlConnection(_connectionString);
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
            propsLog.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BankCardManagement);
            propsLog.Add("_ActionType", (int) ActionType.Delete);
            propsLog.Add("_RefTransId", string.Empty);
            propsLog.Add("_BeforeDetailInfo", input.Before.ToLog());
            propsLog.Add("_AfterDetailInfo", null, DbType.String);
            propsLog.Add("_Operator", input.Operator);
            #endregion

            var r =  _mainDbClient.Execute("BankCard_Del", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", propsLog, conn, trans);
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

        public IEnumerable<BankInfoSys> GetSysBankList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<BankInfoSys>("BankInfoSysList_Get", new { SiteId = (int?)null }, commandType: CommandType.StoredProcedure);
        }
    }
}
