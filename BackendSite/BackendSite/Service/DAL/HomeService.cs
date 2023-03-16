using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class HomeService
    {
        private IConfiguration configuration;
        private readonly MySqlClient _mainDbClient;

        public HomeService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.FirstOrDefault(p => p.DataBase == DataBase.Main);
        }
        public int GetRegisteredCustomers(int SiteId, long StartDate, long EndDate)
        {
            return _mainDbClient.QuerySingle<int>("Admin_RegisteredCustomer_Get", new { SiteId, StartDate, EndDate }, commandType: CommandType.StoredProcedure);
        }
        public int GetDepositCustomers(int SiteId, long StartDate, long EndDate)
        {
            return _mainDbClient.QuerySingle<int>("Admin_DepositCustomer_Get", new { SiteId, StartDate, EndDate }, commandType: CommandType.StoredProcedure);
        }
    }
}
