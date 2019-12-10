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

        public IEnumerable<AdminUser> AccountList(int siteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<AdminUser>("AdminAccount_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public AdminUser GetAccount(int siteId, int userId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirst<AdminUser>("AdminAccountId_Get", new { SiteId = siteId, UserId = userId }, commandType: CommandType.StoredProcedure);
            }
        }

        public bool EditAccount(int siteId, AdminUser adminUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("AdminAccountId_Set", new { SiteId = siteId, UserId = adminUser.UserId, Role = adminUser.Role, Status = adminUser.Status }, commandType: CommandType.StoredProcedure)>0;
            }
        }
        
        public bool ChangePwdAccount(int siteId, AdminUser adminUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("AdminAccountChangePwd_Set", new { SiteId = siteId, UserId = adminUser.UserId, UserPWD = adminUser.Userpwd }, commandType: CommandType.StoredProcedure) > 0;
            }
        }

        public bool ChangePwd(int siteId, int userId, ChangePwd changePwd, out string Md5Userpwd)
        {
            Md5Userpwd = null;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var p = new DynamicParameters(new { SiteId = siteId, UserId = userId, Md5OldPassword = changePwd.Oldpwd, Md5NewPassword = changePwd.NewPwd });
                p.Add("Md5Userpwd", dbType: DbType.String, direction: ParameterDirection.Output);
                var result = conn.Execute("AdminAccountChangeSelfPwd_Set", p, commandType: CommandType.StoredProcedure);
                Md5Userpwd = p.Get<string>("Md5Userpwd");
                return result > 0;
            }
        }
    }
}
