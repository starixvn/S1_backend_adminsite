using BackendSite.Service.Model;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class PaymentAdjustmentReportService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        public PaymentAdjustmentReportService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<PaymentAdjustment> GetPaymentAdjustmentReport(string siteIds,
            string currencyIds,
            bool getFromAdjustmentInfo,
            string AdjustTypesForAI,
            bool getFromLogAdminAdjustWallet,
            string AdjustTypesForLAAW,
            string userName,
            DateTime startDate,
            DateTime endDate,
            string transId,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount,
            out List<string> currencyAmount)
        {
            var tempCurrencyTotalAmount = new List<string>();
            var props = new DynamicParameters();

            props.Add("_SiteIds", siteIds);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_GetFromAdjustmentInfo", getFromAdjustmentInfo);
            props.Add("_AdjustTypesForAI", AdjustTypesForAI);
            props.Add("_GetFromLogAdminAdjustWallet", getFromLogAdminAdjustWallet);
            props.Add("_AdjustTypesForLAAW", AdjustTypesForLAAW);
            props.Add("_UserName", string.IsNullOrWhiteSpace(userName) ? null : userName);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_TransId", string.IsNullOrWhiteSpace(transId) ? null : transId);

            #region GetCurrencyTotalAmount

            var currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_AdjustmentTotalAmount_Get_V5", props).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            currencyAmount = tempCurrencyTotalAmount;
            #endregion

            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<PaymentAdjustment>("Admin_AdjustmentInfo_Get_V5", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public async Task<PaymentAdjustmentReportResponseWrapper> GetPaymentAdjustmentReportV2Async(string siteIds,
         string currencyIds,
         bool getFromAdjustmentInfo,
         string AdjustTypesForAI,
         bool getFromLogAdminAdjustWallet,
         string AdjustTypesForLAAW,
         string userName,
         string sabaId,
         string depositId,
         DateTime startDate,
         DateTime endDate,
         string transId,
         int startIndex,
         int pageSize)
        {

            var result = new PaymentAdjustmentReportResponseWrapper();

            var props = new DynamicParameters();
            props.Add("_SiteIds", siteIds);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_GetFromAdjustmentInfo", getFromAdjustmentInfo);
            props.Add("_AdjustTypesForAI", AdjustTypesForAI);
            props.Add("_GetFromLogAdminAdjustWallet", getFromLogAdminAdjustWallet);
            props.Add("_AdjustTypesForLAAW", AdjustTypesForLAAW);
            props.Add("_UserName", string.IsNullOrWhiteSpace(userName) ? null : userName);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_TransId", string.IsNullOrWhiteSpace(transId) ? null : transId);
            props.Add("_TpUserName", string.IsNullOrWhiteSpace(sabaId) ? null : sabaId);
            props.Add("_DepositId", string.IsNullOrWhiteSpace(depositId) ? null : depositId);

            #region GetCurrencyTotalAmount

            var currencyTotalAmount = (await _reportDbClient.QueryAsync<TotalAmount>("Admin_AdjustmentTotalAmount_Get_V6", props)).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.Add(new TotalAmount()
                {
                    Currency = "Total",
                    Amount = Math.Round(currencyTotalAmount.Sum(r => r.Amount), 2),
                    Record = currencyTotalAmount.Sum(r => r.Record)
                });
            }

            #endregion

            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            result.QueryResult = await _reportDbClient.QueryAsync<PaymentAdjustmentV2>("Admin_AdjustmentInfo_Get_V6", props);
            result.CurrencyAmount = currencyTotalAmount;
            result.TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            result.RowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }


        public IEnumerable<PaymentAdjustment> GetPaymentAdjustmentExportReport(string siteIds,
            string currencyIds,
            bool getFromAdjustmentInfo,
            string AdjustTypesForAI,
            bool getFromLogAdminAdjustWallet,
            string AdjustTypesForLAAW,
            string userName,
            DateTime startDate,
            DateTime endDate,
            string transId)
        {
            var props = new DynamicParameters();
            props.Add("_SiteIds", siteIds);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_GetFromAdjustmentInfo", getFromAdjustmentInfo);
            props.Add("_AdjustTypesForAI", AdjustTypesForAI);
            props.Add("_GetFromLogAdminAdjustWallet", getFromLogAdminAdjustWallet);
            props.Add("_AdjustTypesForLAAW", AdjustTypesForLAAW);
            props.Add("_UserName", string.IsNullOrWhiteSpace(userName) ? null : userName);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_TransId", string.IsNullOrWhiteSpace(transId) ? null : transId);

            var result = _reportDbClient.Query<PaymentAdjustment>("Admin_AdjustmentInfoExport_Get_V5", props);

            return result;
        }


        public async Task<IEnumerable<PaymentAdjustmentV2>> GetPaymentAdjustmentExportReportV2(string siteIds,
            string currencyIds,
            bool getFromAdjustmentInfo,
            string AdjustTypesForAI,
            bool getFromLogAdminAdjustWallet,
            string AdjustTypesForLAAW,
            string userName,
            string sabaId,
            string depositId,
            DateTime startDate,
            DateTime endDate,
            string transId)
        {
            var props = new DynamicParameters();
            props.Add("_SiteIds", siteIds);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_GetFromAdjustmentInfo", getFromAdjustmentInfo);
            props.Add("_AdjustTypesForAI", AdjustTypesForAI);
            props.Add("_GetFromLogAdminAdjustWallet", getFromLogAdminAdjustWallet);
            props.Add("_AdjustTypesForLAAW", AdjustTypesForLAAW);
            props.Add("_UserName", string.IsNullOrWhiteSpace(userName) ? null : userName);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_TransId", string.IsNullOrWhiteSpace(transId) ? null : transId);
            props.Add("_TpUserName", string.IsNullOrWhiteSpace(sabaId) ? null : sabaId);
            props.Add("_DepositId", string.IsNullOrWhiteSpace(depositId) ? null : depositId);

            var result = await _reportDbClient.QueryAsync<PaymentAdjustmentV2>("Admin_AdjustmentInfoExport_Get_V6", props);
            return result;
        }


        public IEnumerable<PaymentAdjustment> GetAdjustmentInfoByCust(int custId,
            DateTime? startDate,
            DateTime? endDate,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount,
            ref List<string> CurrencyAmount)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var tempCurrencyTotalAmount = new List<string>();
            var p = new DynamicParameters();

            p.Add("_CustId", custId, dbType: DbType.Int32);
            p.Add("_StartDate", startDate, dbType: DbType.DateTime);
            p.Add("_EndDate", endDate, dbType: DbType.DateTime);

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_AdjustmentTotalAmountByCust_Get_V3", p, commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            CurrencyAmount = tempCurrencyTotalAmount;
            #endregion

            p.Add("_PageSize", pageSize, dbType: DbType.Int32);
            p.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);


            var depositList = conn.Query<PaymentAdjustment>("Admin_AdjustmentInfoByCust_Get_V3", p, commandType: CommandType.StoredProcedure);

            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return depositList;
        }

        public async Task<PaymentAdjustmentWrapper> GetAdjustmentInfoByCustV2Async(int siteId,
            int custId,
            int adjustType,
            DateTime? startDate,
            DateTime? endDate,
            int pageSize,
            int startIndex)
        {
            var result = new PaymentAdjustmentWrapper();

            await using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));

            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_CustId", custId, dbType: DbType.Int32);
            p.Add("_AdjustType", adjustType, dbType: DbType.Int32);
            p.Add("_StartDate", startDate, dbType: DbType.DateTime);
            p.Add("_EndDate", endDate, dbType: DbType.DateTime);
            
            #region GetCurrencyTotalAmount

            var currencyTotalAmount = (await conn.QueryAsync<TotalAmount>("Admin_AdjustmentTotalAmountByCust_Get_V4", p, commandType: CommandType.StoredProcedure)).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.Add(new TotalAmount()
                {
                    Currency = "Total",
                    Amount = Math.Round(currencyTotalAmount.Sum(r => r.Amount), 2),
                    Record = currencyTotalAmount.Sum(r => r.Record)
                });
            }

            #endregion

            p.Add("_PageSize", pageSize, dbType: DbType.Int32);
            p.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            result.Data = await conn.QueryAsync<PaymentAdjustmentV2>("Admin_AdjustmentInfoByCust_Get_V4", p, commandType: CommandType.StoredProcedure);
            result.CurrencyAmount = currencyTotalAmount;
            result.TotalPages = p.Get<int?>("_TotalPages") ?? 0;
            result.RowCount = p.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public bool SetLogAdminAdjustWallet(ParamsWithLog<SetAdjustment> input, int transferId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters();
            p.Add("_TransferId", transferId);
            p.Add("_Category", input.After.Category);
            p.Add("_Memo", input.After.Memo ?? string.Empty);

            var success = conn
                .Execute("Admin_LogAdminAdjustWallet_Set", p, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.ThirdPartyFundsAdjustment);
                logParam.Add("_ActionType", (int)ActionType.Update);
                logParam.Add("_RefTransId", input.TransId);
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }

            }

            trans.Rollback();

            return false;
        }

        public bool SetAdjustmentInfo(ParamsWithLog<SetAdjustment> input, int adjustId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters();
            p.Add("_AdjustId", adjustId);
            p.Add("_Category", input.After.Category);
            p.Add("_Memo", input.After.Memo);

            var success = conn
                .Execute("Admin_AdjustmentInfo_Set", p, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.BalanceAdjustment);
                logParam.Add("_ActionType", (int)ActionType.Update);
                logParam.Add("_RefTransId", input.TransId);
                logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
                logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
                logParam.Add("_Operator", input.Operator);

                var logSuccess = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }

            }

            trans.Rollback();

            return false;
        }
    }
}
