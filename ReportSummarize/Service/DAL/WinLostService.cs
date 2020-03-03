using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ReportSummarize.Service.DAL
{
    public class WinLostService
    {
        private IConfiguration configuration;

        public WinLostService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void SyncDailyReport()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Execute("Report_WinLost_DailyIns", commandType: CommandType.StoredProcedure);
            }
        }
    }
}
