using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            List<string> tempCurrencyTotalAmount = new List<string>();
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("@PageSize", PageSize);
            props.Add("@TransID", TransId);
            props.Add("@Currency", CurrencyId);
            props.Add("@UserName", UserName);
            props.Add("@AdjustType", AdjustType);
            props.Add("@StartDate", StartDate);
            props.Add("@EndDate", EndDate);
            props.Add("@SiteIds", JsonConvert.SerializeObject(SiteList));

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_AdjustmentTotalAmountV2_Get", props, commandType: CommandType.StoredProcedure).ToList();
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

            result = conn.Query<PaymentAdjustment>("Admin_AdjustmentInfoV2_Get", props, commandType: CommandType.StoredProcedure);
            PageNumber = props.Get<int?>("@PageNumber") ?? 0;
            TotalPages = props.Get<int?>("@TotalPages") ?? 0;
            RowCount = props.Get<int?>("@RowCount") ?? 0;

            return result;
        }

        public IEnumerable<PaymentAdjustment> GetPaymentAdjustmentExportReport(string TransId, int CurrencyId, string UserName, int AdjustType, IEnumerable<int> SiteList, long StartDate, long EndDate)
        {
            IEnumerable<PaymentAdjustment> result = null;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_AdjustmentInfoExportV2_Get";
                DynamicParameters props = new DynamicParameters();
                props.Add("@TransID", TransId);
                props.Add("@Currency", CurrencyId);
                props.Add("@UserName", UserName);
                props.Add("@AdjustType", AdjustType);
                props.Add("@StartDate", StartDate);
                props.Add("@EndDate", EndDate);
                props.Add("@SiteIds", JsonConvert.SerializeObject(SiteList));

                result = conn.Query<PaymentAdjustment>(SpName, props, commandType: CommandType.StoredProcedure);
            }

            return result;
        }

        public IEnumerable<PaymentAdjustment> GetAdjustmentInfoByCust(int CustId, long? startDate, long? endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount, ref List<string> CurrencyAmount)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            List<string> tempCurrencyTotalAmount = new List<string>();
            var p = new DynamicParameters(new { CustId = CustId, StartDate = startDate, EndDate = endDate, PageSize = pageSize });

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_AdjustmentTotalAmountByCustV2_Get", p, commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            CurrencyAmount = tempCurrencyTotalAmount;
            #endregion

            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            
            var depositList = conn.Query<PaymentAdjustment>("Admin_AdjustmentInfoByCustV2_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }
    }
}
