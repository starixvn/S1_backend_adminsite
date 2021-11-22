using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static BackendSite.Controllers.Api.TpController;

namespace BackendSite.Service.DAL
{
    public class ThirdPartyService
    {
        private readonly IConfiguration configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public ThirdPartyService(IConfiguration configuration, ILogger<MySqlClient> logger)
        {
            this.configuration = configuration;
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
        }
        public IEnumerable<TpWallet> GetTpWallet(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<TpWallet>("TpWallet_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<TpTransferInfo> GetTpTransferInfoByCust(int CustId,
            string TransId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", CustId, dbType: DbType.Int32);
            p.Add("_TransId", TransId, dbType: DbType.String);
            p.Add("_StartDate", startDate.DateTime, dbType: DbType.DateTime);
            p.Add("_EndDate", endDate.DateTime, dbType: DbType.DateTime);
            p.Add("_PageSize", pageSize, dbType: DbType.Int32);
            p.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var tpTransferInfoList = conn.Query<TpTransferInfo>("Admin_TpTransferInfoByCust_Get_V4", p, commandType: CommandType.StoredProcedure);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return tpTransferInfoList;
        }
        public IEnumerable<SiteTpListIfo> GetSiteTpList(int SiteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<SiteTpListIfo>("Admin_SiteTpList_Get", new { SiteId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<AdminTpTransferInfo> GetAdjustmentTpLog(int SiteId, int TpId, int CustId, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            IEnumerable<AdminTpTransferInfo> result = null;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var SpName = "Admin_LogAdminAdjustWallet_Get";
            var props = new DynamicParameters();
            props.Add("@SiteId", SiteId);
            props.Add("@TpId", TpId);
            props.Add("@CustId", CustId);
            props.Add("@PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@PageSize", PageSize);

            result = conn.Query<AdminTpTransferInfo>(SpName, props, commandType: CommandType.StoredProcedure);
            PageNumber = props.Get<int?>("@PageNumber") ?? 0;
            TotalPages = props.Get<int?>("@TotalPages") ?? 0;
            RowCount = props.Get<int?>("@RowCount") ?? 0;

            return result;
        }

        public IEnumerable<SiteTpListIfo> GetTpBySiteAndCurrency(TpDetailPara TpDetailPara)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<SiteTpListIfo>("Admin_TpList_GetByCurrency", new { CurrencyId = TpDetailPara.Currency, SiteId = TpDetailPara.Site }, commandType: CommandType.StoredProcedure);
        }

        public void SetAdjustmentTpCustInfoLog(string Menu, int ActionType, string DetailInfo, string Operator)
        {
            var props = new DynamicParameters();
            props.Add("Menu", Menu);
            props.Add("ActionType", ActionType);
            props.Add("DetailInfo", DetailInfo);
            props.Add("Operator", Operator);

            _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", props);
        }

        public async Task<TpCurrency> GetTpCurrency(int tpId, int currencyId)
        {
            return await _reportDbClient.QueryFirstAsync<TpCurrency>("TpCurrency_Get", new DynamicParameters(new { TpId = tpId, CurrencyId = currencyId }) );
        }
    }
}
