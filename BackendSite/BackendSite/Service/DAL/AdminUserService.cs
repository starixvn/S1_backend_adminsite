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
        public AdminUser GetAdminUserByUserName(string userName)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<AdminUser>("LoginAdminUser_Get", new { UserName = userName }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<AdminUser> GetUserList()
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<AdminUser>("Admin_User_GetAll", commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<AssignSites> GetAssignSiteList(int UserId)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<AssignSites>("Admin_AuthorSite_Get", new { UserId = UserId  }, commandType: CommandType.StoredProcedure);
            }
        }

        public void AssignSiteList(int UserId, IEnumerable<int> SiteIds)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    var SpName = "Admin_AuthorSite_Del";
                    conn.Execute(SpName, new { UserId }, tran, commandType: CommandType.StoredProcedure);

                    if (SiteIds != null && SiteIds.Any())
                    {
                        SpName = "Admin_AuthorSite_Ins";
                        foreach (var id in SiteIds)
                        {
                            conn.Execute(SpName, new { UserId, SiteId = id }, tran, commandType: CommandType.StoredProcedure);
                        }
                    }

                    tran.Commit();
                }
            }
        }

        public AssignGroups GetAssignGroup(int UserId)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirst<AssignGroups>("Admin_AuthorGroup_Get", new { UserId }, commandType: CommandType.StoredProcedure);
            }
        }

        public void AssignGroup(int UserId, int? GroupSEQ)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    if (GroupSEQ == null)
                    {
                        var SpName = "Admin_AuthorGroup_Del";
                        conn.Execute(SpName, new { UserId }, tran, commandType: CommandType.StoredProcedure);
                    }
                    else
                    {
                        var SpName = "Admin_AuthorGroup_Set";
                        conn.Execute(SpName, new { UserId, GroupSEQ }, tran, commandType: CommandType.StoredProcedure);
                    }

                    tran.Commit();
                }
            }
        }

        public void SetUserPassword(int UserId, string MD5Pwd)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Execute("Admin_User_SetPwd", new { UserId, UserPwd = MD5Pwd }, commandType: CommandType.StoredProcedure);
            }
        }

        public void SetUserStatus(int UserId, int Status)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Execute("Admin_User_SetStatus", new { UserId, UserStatus = Status }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<AdminUser> AddUser(string UserName, string MD5Pwd, int[] SiteList, int? GroupSEQ)
        {
            IEnumerable<AdminUser> userList = null;
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    var UserInfo = new DynamicParameters();
                    UserInfo.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    UserInfo.Add("@UserName", UserName);
                    UserInfo.Add("@Userpwd", MD5Pwd);
                    UserInfo.Add("@IsSuper", 0);
                    UserInfo.Add("@Status", 1);

                    conn.Execute("Admin_User_Ins", UserInfo, tran, commandType: CommandType.StoredProcedure);

                    var UserId = UserInfo.Get<int?>("@UserId");

                    if(UserId != null)
                    {
                        if (GroupSEQ != null)
                        {
                            var SpName = "Admin_AuthorGroup_Set";
                            conn.Execute(SpName, new { UserId, GroupSEQ }, tran, commandType: CommandType.StoredProcedure);
                        }

                        if (SiteList != null)
                        {
                            foreach (var SiteId in SiteList)
                            {
                                conn.Execute("Admin_AuthorSite_Ins", new { UserId, SiteId }, tran, commandType: CommandType.StoredProcedure);
                            }
                        }

                        userList = conn.Query<AdminUser>("Admin_User_GetAll", transaction: tran, commandType: CommandType.StoredProcedure);
                    }

                    tran.Commit();
                }
            }

            return userList;
        }

        public AdminUser GetUser(int UserId)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirst<AdminUser>("Admin_User_Get", new { UserId }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
