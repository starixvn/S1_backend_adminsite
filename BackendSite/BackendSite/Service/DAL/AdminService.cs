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
    public class AdminService
    {
        private IConfiguration configuration;

        public AdminService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public bool AddAccount(int siteId, AdminUser adminUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var p = new DynamicParameters(new { SiteId = siteId, adminUser.UserName, adminUser.Userpwd, adminUser.Role });
                p.Add("UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                return conn.Execute("AdminAccount_Ins", p, commandType: CommandType.StoredProcedure) >0;
            }
        }
    }
}
