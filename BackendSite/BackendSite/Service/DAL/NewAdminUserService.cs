using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using Dapper;
using System.Collections.Generic;

namespace BackendSite.Service.DAL
{
    public class NewAdminUserService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public NewAdminUserService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public NewAdminUser GetAdminUserByUserName(string userName)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<NewAdminUser>("LoginNewAdminUser_Get", new { UserName = userName }, commandType: CommandType.StoredProcedure);
        }

        public int SetLoginTime(int userId)
        {
            var props = new DynamicParameters();
            props.Add("UserId", userId);
            return _mainDbClient.Execute("Admin_LoginTime_Set", props);
        }

        public List<AssignBrand> GetAssignSiteListV2(int userId)
        {
            return _reportDbClient.Query<AssignBrand>("Admin_AuthorBrand_Get", new DynamicParameters(new { UserId = userId })).ToList();
        }
    }
}
