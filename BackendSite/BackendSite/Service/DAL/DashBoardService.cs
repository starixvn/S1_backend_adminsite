using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Newtonsoft.Json;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class DashBoardService
    {
        private readonly MySqlClient _reportDbClient;

        public DashBoardService(IServiceProvider provider)
        {
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public DashBoardData GetDashBoardData(int siteId,
            int currencyId,
            int baseCurrencyId,
            DateTime startDate,
            DateTime endDate,
            List<int> canSearchSite,
            int timeZone)
        {
            var siteList = new List<int> { siteId };
            var props = new DynamicParameters();
            props.Add("_CurrencyId", currencyId);
            //props.Add("_BaseCurrencyId", baseCurrencyId); //根本就沒有用到阿...傳進來好看的嗎
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            //props.Add("_TimeZone", timeZone);
            props.Add("_SiteIds", siteId != 0 ? JsonConvert.SerializeObject(siteList) : JsonConvert.SerializeObject(canSearchSite));

            return _reportDbClient.Query<DashBoardData>("Admin_DashBoard_Get_V2", props).FirstOrDefault();
        }

        public DashBoardDetailData GetDashBoardTodayData(int siteId,
            int currencyId,
            int baseCurrencyId,
            DateTime startDate,
            DateTime endDate,
            List<int> canSearchSite,
            int timeZone)
        {
            var siteList = new List<int> { siteId };
            var props = new DynamicParameters();
            props.Add("_CurrencyId", currencyId);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            //props.Add("_TimeZone", timeZone);
            props.Add("_SiteIds", siteId != 0 ? JsonConvert.SerializeObject(siteList) : JsonConvert.SerializeObject(canSearchSite));

            return _reportDbClient.Query<DashBoardDetailData>("Admin_DashBoard_Today_Get_V2", props).FirstOrDefault();
        }
        public DashBoardDetailData GetDashBoardTodayExtraData(int siteId,
            int currencyId,
            int baseCurrencyId,
            DateTime startDate,
            DateTime endDate,
            List<int> canSearchSite,
            int tempTableType,
            int timeZone)
        {
            var siteList = new List<int> { siteId };
            var props = new DynamicParameters();
            props.Add("_CurrencyId", currencyId);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            //props.Add("_TimeZone", timeZone);
            props.Add("_SiteIds", siteId != 0 ? JsonConvert.SerializeObject(siteList) : JsonConvert.SerializeObject(canSearchSite));
            props.Add("_Type", tempTableType);

            return _reportDbClient.Query<DashBoardDetailData>("Admin_DashBoard_TodayExtra_Get_V2", props).FirstOrDefault();
        }
    }
}
