using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.Model;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class PaymentAdjustmentReportService
    {
        private readonly IConfiguration configuration;
        public PaymentAdjustmentReportService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<PaymentAdjustment> GetPaymentAdjustmentReport(string TransId, int CurrencyId, string UserName, int AdjustType, IEnumerable<int> SiteList, long StartDate, long EndDate, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount, ref List<string> CurrencyAmount)
        {
            IEnumerable<PaymentAdjustment> result = null;
            var tempCurrencyTotalAmount = new List<string>();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("@PageSize", PageSize);
            props.Add("@TransID", TransId);
            props.Add("@Currency", CurrencyId);
            props.Add("@UserName", UserName);
            props.Add("@AdjustType", AdjustType);
            props.Add("@StartDate", StartDate);
            props.Add("@EndDate", EndDate);
            props.Add("@SiteIds", JsonConvert.SerializeObject(SiteList));

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_AdjustmentTotalAmountV3_Get", props, commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            CurrencyAmount = tempCurrencyTotalAmount;
            #endregion

            props.Add("@PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            result = conn.Query<PaymentAdjustment>("Admin_AdjustmentInfoV3_Get", props, commandType: CommandType.StoredProcedure);
            PageNumber = props.Get<int?>("@PageNumber") ?? 0;
            TotalPages = props.Get<int?>("@TotalPages") ?? 0;
            RowCount = props.Get<int?>("@RowCount") ?? 0;

            return result;
        }

        public IEnumerable<PaymentAdjustment> GetPaymentAdjustmentExportReport(string TransId, int CurrencyId, string UserName, int AdjustType, IEnumerable<int> SiteList, long StartDate, long EndDate)
        {
            IEnumerable<PaymentAdjustment> result = null;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var SpName = "Admin_AdjustmentInfoExportV3_Get";
            var props = new DynamicParameters();
            props.Add("@TransID", TransId);
            props.Add("@Currency", CurrencyId);
            props.Add("@UserName", UserName);
            props.Add("@AdjustType", AdjustType);
            props.Add("@StartDate", StartDate);
            props.Add("@EndDate", EndDate);
            props.Add("@SiteIds", JsonConvert.SerializeObject(SiteList));

            result = conn.Query<PaymentAdjustment>(SpName, props, commandType: CommandType.StoredProcedure);

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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
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
    }
}
