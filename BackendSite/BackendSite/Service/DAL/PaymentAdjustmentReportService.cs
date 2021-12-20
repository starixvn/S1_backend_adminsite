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

        public IEnumerable<PaymentAdjustment> GetPaymentAdjustmentReport(string transId, int currencyId, string userName, int adjustType, IEnumerable<int> siteList, DateTime startDate, DateTime endDate, int pageSize, ref int pageNumber, ref int totalPages, ref int rowCount, ref List<string> currencyAmount)
        {
            var tempCurrencyTotalAmount = new List<string>();
            var props = new DynamicParameters();
            props.Add("@PageSize", Math.Abs(pageSize));
            props.Add("@TransID", transId);
            props.Add("@Currency", currencyId);
            props.Add("@UserName", userName);
            props.Add("@AdjustType", adjustType);
            props.Add("@StartDate", startDate);
            props.Add("@EndDate", endDate);
            props.Add("@SiteIds", JsonConvert.SerializeObject(siteList));

            #region GetCurrencyTotalAmount
            var currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_AdjustmentTotalAmount_Get_V4", props).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            currencyAmount = tempCurrencyTotalAmount;
            #endregion

            props.Add("@StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),DbType.Int32, ParameterDirection.Input);
            props.Add("@PageNumber", Math.Abs(pageNumber), DbType.Int32,ParameterDirection.InputOutput);
            props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            

            var result = _reportDbClient.Query<PaymentAdjustment>("Admin_AdjustmentInfo_Get_V4", props);
            pageNumber = props.Get<int?>("@PageNumber") ?? 0;
            totalPages = props.Get<int?>("@TotalPages") ?? 0;
            rowCount = props.Get<int?>("@RowCount") ?? 0;

            return result;
        }

        public IEnumerable<PaymentAdjustment> GetPaymentAdjustmentExportReport(string transId, int currencyId, string userName, int adjustType, IEnumerable<int> siteList, DateTime startDate, DateTime endDate)
        {
            var props = new DynamicParameters();
            props.Add("@TransID", transId);
            props.Add("@Currency", currencyId);
            props.Add("@UserName", userName);
            props.Add("@AdjustType", adjustType);
            props.Add("@StartDate", startDate);
            props.Add("@EndDate", endDate);
            props.Add("@SiteIds", JsonConvert.SerializeObject(siteList));

            var result = _reportDbClient.Query<PaymentAdjustment>("Admin_AdjustmentInfoExport_Get_V4", props);

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
    }
}
