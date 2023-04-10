using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class AdminGroupService
    {
        private readonly IConfiguration configuration;
        private readonly MySqlClient _reportDbClient;
        private readonly MySqlClient _mainDbClient;

        public AdminGroupService(IConfiguration configuration, IServiceProvider provider)
        {
            this.configuration = configuration;
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        }

        public IEnumerable<GroupPermisison> GetGroupPermissions(int SEQ)
        {
            var para = new DynamicParameters();
            para.Add("SEQ", SEQ);
            return _reportDbClient.Query<GroupPermisison>("Admin_GroupFunc_Get", para);
        }
    }
}
