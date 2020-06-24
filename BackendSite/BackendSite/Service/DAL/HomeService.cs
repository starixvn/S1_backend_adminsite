using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class HomeService
    {
        private IConfiguration configuration;

        public HomeService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public int GetRegisteredCustomers(int SiteId, long StartDate, long EndDate)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QuerySingle<int>("Admin_RegisteredCustomer_Get", new { SiteId, StartDate, EndDate }, commandType: CommandType.StoredProcedure);
        }
        public int GetDepositCustomers(int SiteId, long StartDate, long EndDate)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QuerySingle<int>("Admin_DepositCustomer_Get", new { SiteId, StartDate, EndDate }, commandType: CommandType.StoredProcedure);
        }
    }
}
