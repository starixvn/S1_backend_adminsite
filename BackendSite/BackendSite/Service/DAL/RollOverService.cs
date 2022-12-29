using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Policy;

namespace BackendSite.Service.DAL
{
    public class RollOverService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        public RollOverService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public CustRollOver GetCustRollOver(int custId, string bonusCode)
        {
            var props = new DynamicParameters(new
            {
                CustId = custId,
                BonusCode = bonusCode
            });
            var result = _mainDbClient.QueryFirst<CustRollOver>("Common_RollOver_Get", props);
            return result;
        }
    }
}
