using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class DashBoardService
    {
        private readonly IConfiguration configuration;

        public DashBoardService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public DashBoardData GetDashBoardData(int siteId, int currencyId, int baseCurrencyId, List<int> canSearchSite , int timeZone)
        {
            var siteList = new List<int> {siteId};
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("CurrencyId", currencyId);
            props.Add("BaseCurrencyId", baseCurrencyId);
            props.Add("TimeZone", timeZone);
            props.Add("SelectSite", siteId != 0 ? JsonConvert.SerializeObject(siteList) : JsonConvert.SerializeObject(canSearchSite));

            return conn.Query<DashBoardData>("Admin_DashBoard_Get_V2", props, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }

        public DashBoardDetailData GetDashBoardTodayData(int siteId, int currencyId, int baseCurrencyId, List<int> canSearchSite , int timeZone)
        {
            var siteList = new List<int> {siteId};
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("CurrencyId", currencyId);
            props.Add("BaseCurrencyId", baseCurrencyId);
            props.Add("TimeZone", timeZone);
            props.Add("SelectSite", siteId != 0 ? JsonConvert.SerializeObject(siteList) : JsonConvert.SerializeObject(canSearchSite));

            return conn.Query<DashBoardDetailData>("Admin_DashBoard_Today_Get_V2", props, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }
        public DashBoardDetailData GetDashBoardTodayExtraData(int siteId, int currencyId, int baseCurrencyId, List<int> canSearchSite,int tempTableType, int timeZone)
        {
            var siteList = new List<int> {siteId};
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("CurrencyId", currencyId);
            props.Add("BaseCurrencyId", baseCurrencyId);
            props.Add("TimeZone", timeZone);
            props.Add("SelectSite", siteId != 0 ? JsonConvert.SerializeObject(siteList) : JsonConvert.SerializeObject(canSearchSite));
            props.Add("TempTableType", tempTableType);

            return conn.Query<DashBoardDetailData>("Admin_DashBoard_TodayExtra_Get_V2", props, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }
    }
}
