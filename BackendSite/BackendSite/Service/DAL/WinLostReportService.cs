using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using BackendSite.Service.Model;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class WinLostReportService
    {
        private IConfiguration configuration;

        public WinLostReportService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<WinLostReport> GetWinLostReport(DateTime StartDate, DateTime EndDate, IEnumerable<int> SiteList, int Currency)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<WinLostReport>("Report_WinLostV2_Get", new { StartDate, EndDate, SiteIds=JsonConvert.SerializeObject(SiteList), Currency}, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<GameType> GetAllGameType()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<GameType>("Admin_TpGameType_Get", commandType: CommandType.StoredProcedure);
        }
    }
}
