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
    public class WinLostReportService
    {
        private IConfiguration configuration;

        public WinLostReportService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<LicWinLostReport> GetLicWinLostReport(DateTime StartDate, DateTime EndDate, IEnumerable<int> SiteList, int Currency)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<LicWinLostReport>("Report_WinLost_Get", new { StartDate, EndDate, SiteIds=JsonConvert.SerializeObject(SiteList), Currency}, commandType: CommandType.StoredProcedure);
            }
        }

        public Dictionary<int, string> GetAllGameType()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query("Admin_TpGameType_Get", commandType: CommandType.StoredProcedure).ToDictionary(row => (int)row.Type, row => (string)row.TypeName);
            }
        }
    }
}
