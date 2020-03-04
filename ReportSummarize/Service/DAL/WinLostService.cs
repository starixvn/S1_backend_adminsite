using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<WinLostService> logger;
        private readonly IHostEnvironment hostEnvironment;

        public WinLostService(IConfiguration configuration, ILogger<WinLostService> logger, IHostEnvironment hostEnvironment)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.hostEnvironment = hostEnvironment;
        }

        public void SyncDailyReport()
        {
            logger.LogInformation(hostEnvironment.EnvironmentName);
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Execute("Report_WinLost_DailyIns", commandType: CommandType.StoredProcedure);
            }
        }
    }
}
