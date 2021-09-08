using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public AdminService(IConfiguration configuration, ILogger<MySqlClient> logger)
        {
            this.configuration = configuration;
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
        }

        public bool AddAccount(int siteId, AdminUser adminUser)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var p = new DynamicParameters(new { 
                    SiteId = siteId, 
                    adminUser.UserName, 
                    adminUser.Userpwd
                });
                p.Add("UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                return conn.Execute("AdminAccount_Ins", p, commandType: CommandType.StoredProcedure) >0;
            }
        }

        public IEnumerable<AdminUser> AccountList(int siteId)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<AdminUser>("AdminAccount_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
            }
        }

        public AdminUser GetAccount(int siteId, int userId)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirst<AdminUser>("AdminAccountId_Get", new { SiteId = siteId, UserId = userId }, commandType: CommandType.StoredProcedure);
            }
        }

        public bool EditAccount(int siteId, AdminUser adminUser)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("AdminAccountId_Set", new { 
                    SiteId = siteId, 
                    UserId = adminUser.UserId,
                    Status = adminUser.Status 
                }, commandType: CommandType.StoredProcedure)>0;
            }
        }
        
        public bool ChangePwdAccount(int siteId, AdminUser adminUser)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("AdminAccountChangePwd_Set", new { SiteId = siteId, UserId = adminUser.UserId, UserPWD = adminUser.Userpwd }, commandType: CommandType.StoredProcedure) > 0;
            }
        }

        public bool ChangePwd(int siteId, int userId, ChangePwd changePwd, out string Md5Userpwd)
        {
            Md5Userpwd = null;
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var p = new DynamicParameters(new { SiteId = siteId, UserId = userId, Md5OldPassword = changePwd.OldPwd, Md5NewPassword = changePwd.NewPwd });
                p.Add("Md5Userpwd", dbType: DbType.String, direction: ParameterDirection.Output);
                var result = conn.Execute("AdminAccountChangeSelfPwd_Set", p, commandType: CommandType.StoredProcedure);
                Md5Userpwd = p.Get<string>("Md5Userpwd");
                return result > 0;
            }
        }

        public SpecialPasswordManagement GetSpecialPasswordData(int sitId)
        {
            return _reportDbClient.Query<SpecialPasswordManagement>("Admin_SpecialPassword_Get", new DynamicParameters(new { SiteId = sitId })).FirstOrDefault();
        }

        public int SetSpecialPasswordData(int SiteId, string Password, string Operator)
        {
            var props = new DynamicParameters();
            props.Add("SiteId", SiteId);
            props.Add("Password", Password);
            props.Add("Operator", Operator);

            return _mainDbClient.Execute("Admin_SpecialPassword_Set", props);
        }
    }
}
