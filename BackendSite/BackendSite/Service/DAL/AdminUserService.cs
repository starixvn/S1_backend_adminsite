using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

namespace BackendSite.Service.DAL
{
    public class AdminUserService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public AdminUserService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public AdminUser GetAdminUserByUserName(string userName)
        {
            var para = new DynamicParameters();
            para.Add("UserName", userName);
            return _reportDbClient.QueryFirst<AdminUser>("LoginAdminUser_Get", para);
        }

        public AdminUser GetUser(int userId)
        {
            return _mainDbClient.QueryFirst<AdminUser>("Admin_User_Get", new DynamicParameters(new { UserId = userId }));
        }
    }
}
