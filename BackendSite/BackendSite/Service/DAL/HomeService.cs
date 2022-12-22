using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Policy;

namespace BackendSite.Service.DAL
{
    public class HomeService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public HomeService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public int GetRegisteredCustomers(int SiteId, long StartDate, long EndDate)
        {
            var props = new DynamicParameters(new
            {
                SiteId = SiteId,
                StartDate = StartDate,
                EndDate = EndDate,
            });
            return _mainDbClient.QueryFirst<int>("Admin_RegisteredCustomer_Get", props);
        }
        public int GetDepositCustomers(int SiteId, long StartDate, long EndDate)
        {
            var props = new DynamicParameters(new
            {
                SiteId = SiteId,
                StartDate = StartDate,
                EndDate = EndDate,
            });
            return _mainDbClient.QueryFirst<int>("Admin_DepositCustomer_Get", props);
        }
    }
}
