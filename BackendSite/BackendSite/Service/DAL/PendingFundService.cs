using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class PendingFundService
    {
        private readonly MySqlClient _reportDbClient;

        public PendingFundService(IServiceProvider provider)
        {
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<PendingFund> PendingFundList(int siteId)
        {
            return _reportDbClient.Query<PendingFund>("TmpPendingFund_Get", new { SiteId = siteId });
        }
    }

}
