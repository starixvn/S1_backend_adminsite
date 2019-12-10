using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class AdminUserService
    {
        private IConfiguration configuration;

        public AdminUserService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public AdminUser GetAdminUserByUserName(string userName, int siteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<AdminUser>("LoginAdminUser_Get", new { UserName = userName, SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
