using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public DashBoardData GetDashBoardData(int SiteId, int CurrencyId, int BaseCurrencyId, List<int> CanSearchSite)
        {
            List<int> SiteList = new List<int>();
            SiteList.Add(SiteId);
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("CurrencyId", CurrencyId);
            props.Add("BaseCurrencyId", BaseCurrencyId);
            props.Add("SelectSite", SiteId != 0 ? JsonConvert.SerializeObject(SiteList) : JsonConvert.SerializeObject(CanSearchSite));

            return conn.Query<DashBoardData>("Admin_DashBoard_Get", props, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }

        public DashBoardDetailData GetDashBoardTodayData(int SiteId, int CurrencyId, int BaseCurrencyId, List<int> CanSearchSite)
        {
            List<int> SiteList = new List<int>();
            SiteList.Add(SiteId);
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("CurrencyId", CurrencyId);
            props.Add("BaseCurrencyId", BaseCurrencyId);
            props.Add("SelectSite", SiteId != 0 ? JsonConvert.SerializeObject(SiteList) : JsonConvert.SerializeObject(CanSearchSite));

            return conn.Query<DashBoardDetailData>("Admin_DashBoard_Today_Get", props, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }
        public DashBoardDetailData GetDashBoardTodayExtraData(int SiteId, int CurrencyId, int BaseCurrencyId, List<int> CanSearchSite,int TempTableType)
        {
            List<int> SiteList = new List<int>();
            SiteList.Add(SiteId);
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("CurrencyId", CurrencyId);
            props.Add("BaseCurrencyId", BaseCurrencyId);
            props.Add("SelectSite", SiteId != 0 ? JsonConvert.SerializeObject(SiteList) : JsonConvert.SerializeObject(CanSearchSite));
            props.Add("TempTableType", TempTableType);

            return conn.Query<DashBoardDetailData>("Admin_DashBoard_TodayExtra_Get", props, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }
    }
}
