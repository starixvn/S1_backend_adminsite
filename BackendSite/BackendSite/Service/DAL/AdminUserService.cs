using System;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<AdminUser>("LoginAdminUser_Get", new { UserName = userName }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<AdminUser> GetUserList(int startIndex, int pageSize, out int totalPages, out int rowCount)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var para = new DynamicParameters();
            para.Add("_StartIndex", startIndex);
            para.Add("_PageSize", pageSize);
            para.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            para.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = conn.Query<AdminUser>("Admin_User_GetAll_V2", para, commandType: CommandType.StoredProcedure);

            totalPages = para.Get<int?>("_TotalPages") ?? 0;
            rowCount = para.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public IEnumerable<AssignSites> GetAssignSiteList(int userId)
        {
            return _mainDbClient.Query<AssignSites>("Admin_AuthorSite_Get", new DynamicParameters(new { UserId = userId }));
        }

        public void AssignSiteList(int UserId, IEnumerable<int> SiteIds)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
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

        public AssignGroups GetAssignGroup(int UserId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirst<AssignGroups>("Admin_AuthorGroup_Get", new { UserId }, commandType: CommandType.StoredProcedure);
        }

        public void AssignGroup(int UserId, int? GroupSEQ)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
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

        public void SetUserPassword(int UserId, string MD5Pwd)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Execute("Admin_User_SetPwd", new { UserId, UserPwd = MD5Pwd }, commandType: CommandType.StoredProcedure);
        }

        public void SetUserStatus(int UserId, int Status)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Execute("Admin_User_SetStatus", new { UserId, UserStatus = Status }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<AdminUser> AddUser(string UserName, string MD5Pwd, IEnumerable<int> SiteList, int? GroupSEQ)
        {
            IEnumerable<AdminUser> userList = null;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var UserInfo = new DynamicParameters();
            UserInfo.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            UserInfo.Add("@UserName", UserName);
            UserInfo.Add("@Userpwd", MD5Pwd);
            UserInfo.Add("@IsSuper", 0);
            UserInfo.Add("@Status", 1);

            conn.Execute("Admin_User_Ins", UserInfo, tran, commandType: CommandType.StoredProcedure);

            var UserId = UserInfo.Get<int?>("@UserId");

            if (UserId != null)
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

            return userList;
        }

        public AdminUser GetUser(int userId)
        {
            return _mainDbClient.QueryFirst<AdminUser>("Admin_User_Get", new DynamicParameters(new { UserId = userId }));
        }
    }
}
