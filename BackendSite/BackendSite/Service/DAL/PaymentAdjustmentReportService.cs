using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.Model;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
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
            props.Add("_UserName", userName);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_TransId", transId);

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
            props.Add("_UserName", userName);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_TransId", transId);

            var result = _reportDbClient.Query<PaymentAdjustment>("Admin_AdjustmentInfoExport_Get_V5", props);

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

        public bool SetLogAdminAdjustWallet(int id, int category, string memo, string logDetail, string userName)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters();
            p.Add("_TransferId", id);
            p.Add("_Category", category);
            p.Add("_Memo", memo ?? string.Empty);

            var success = conn
                .Execute("Admin_LogAdminAdjustWallet_Set", p, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                p = new DynamicParameters();
                p.Add("Menu", "EditAdjustmentInfo");
                p.Add("ActionType", 1);
                p.Add("DetailInfo", logDetail);
                p.Add("Operator", userName);

                var logSuccess = conn
                    .Execute("Admin_AdjustmentInfoLog_Set", p, commandType: CommandType.StoredProcedure) > 0;
                
                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }
                
            }

            trans.Rollback();

            return false;
        }

        public bool SetAdjustmentInfo(int id, int category, string memo, string logDetail, string userName)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters();
            p.Add("_AdjustId", id);
            p.Add("_Category", category);
            p.Add("_Memo", memo);

            var success = conn
                .Execute("Admin_AdjustmentInfo_Set", p, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                p = new DynamicParameters();
                p.Add("Menu", "EditAdjustmentInfo");
                p.Add("ActionType", 1);
                p.Add("DetailInfo", logDetail);
                p.Add("Operator", userName);

                var logSuccess = conn
                    .Execute("Admin_AdjustmentInfoLog_Set", p, commandType: CommandType.StoredProcedure) > 0;

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
