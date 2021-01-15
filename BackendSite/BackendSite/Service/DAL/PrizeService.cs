using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class PrizeService
    {
        private readonly IConfiguration configuration;

        public PrizeService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public PrizeSearchResult PrizeSearch(PrizeSelect prizeSelect)
        {
            PrizeSearchResult SearchResult = new PrizeSearchResult();
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("PageNumber", prizeSelect.PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", prizeSelect.PageSize);
            props.Add("StartDate", prizeSelect.StartDate);
            props.Add("EndDate", prizeSelect.EndDate);
            props.Add("SelectSite", prizeSelect.SelectSite != null ? prizeSelect.SelectSite.Any()? JsonConvert.SerializeObject(prizeSelect.SelectSite) :null: null);
            props.Add("CurrencyId", prizeSelect.CurrencyId);

            var result = conn.Query<PrizeData>("Admin_Prize_Get", props, commandType: CommandType.StoredProcedure);
            SearchResult.PrizeData = result.ToList();
            SearchResult.TotalPages = props.Get<int?>("PageNumber") ?? 0;
            SearchResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchResult.RowCount = props.Get<int?>("RowCount") ?? 0;

            return SearchResult;
        }

        public PrizeLogResult GetPrizeLog(PrizeLogPara prizeLogPara)
        {
            PrizeLogResult LogResult = new PrizeLogResult();
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("PageNumber", prizeLogPara.PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", prizeLogPara.PageSize);
            props.Add("StartDate", prizeLogPara.StartDate);
            props.Add("EndDate", prizeLogPara.EndDate);
            props.Add("EventId", prizeLogPara.EventId);
            props.Add("PrizeId", prizeLogPara.PrizeId);

            var result = conn.Query<PrizeLogData>("Admin_Prize_ListOfWinner_Get", props, commandType: CommandType.StoredProcedure);
            LogResult.PrizeLogData = result.ToList();
            LogResult.TotalPages = props.Get<int?>("PageNumber") ?? 0;
            LogResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            LogResult.RowCount = props.Get<int?>("RowCount") ?? 0;

            return LogResult;
        }
    }
}
