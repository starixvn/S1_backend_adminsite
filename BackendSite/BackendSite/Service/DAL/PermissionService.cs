using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Data;
using static BackendSite.Controllers.ApiV2.Admin.Permission.PermissionController;

namespace BackendSite.Service.DAL
{
    public class PermissionService
    {
        private IConfiguration configuration;
        private readonly IWebHostEnvironment env;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly string _connectionString;
        private readonly Tools tools;

        public PermissionService(IWebHostEnvironment env, IConfiguration configuration, IServiceProvider provider, Tools tools)
        {
            this.env = env;
            this.configuration = configuration;
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _connectionString = configuration.GetConnectionString("ConnectionString");
            this.tools = tools;
        }

        public string GetPermissionJSONData()
        {
            var contentRootPath = env.ContentRootPath;
            var initPermissionData = File.ReadAllText($"{contentRootPath}/App_Data/FunctionPermission.json");

            return initPermissionData;
        }

        public IEnumerable<RolePermission> GetRolePermission(int role)
        {
            return _reportDbClient.Query<RolePermission>("Admin_RolePermission_Get", new DynamicParameters(new { Role = role }));
        }
        public IEnumerable<UserPermissionData> GetUserPermissionData(int userId,int siteId)
        {
            return _reportDbClient.Query<UserPermissionData>("Admin_UserPermission_Get", new DynamicParameters(new { UserId = userId, SiteId = siteId }));
        }

        public IEnumerable<AdminRole> GetAdminRole()
        {
            return _reportDbClient.Query<AdminRole>("Admin_AdminRole_Get");
        }
        public int AddAdminRole(int siteId, string roleName, string remark, IEnumerable<int> currencyList, List<SetPermission> permission,int userId)
        {
            var result = 1;
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();

            props.Add("RoleName", roleName);
            props.Add("Remark", remark);
            props.Add("CurrencyList", JsonConvert.SerializeObject(currencyList));
            props.Add("UserId", userId);
            props.Add("SiteId", siteId);
            props.Add("SEQ", dbType: DbType.Int32, direction: ParameterDirection.Output);

            result = _mainDbClient.Execute("Admin_AdminRole_Ins", props, conn, trans);

            if(result <= 0)
            {
                trans.Rollback();
                return result;
            }
            var SEQ = props.Get<int?>("@SEQ");

            if (SEQ == null)
            {
                trans.Rollback();
                result = 0;
                return result;
            }
            foreach (var data in permission)
            {
                _mainDbClient.Execute("Admin_RolePermission_Ins", new DynamicParameters(new { Role= SEQ, MenuId = data.DisPlayKey, Permission = JsonConvert.SerializeObject(data.PermissionIdList)}), conn, trans);
            }
            trans.Commit();
            return result;
        }

        public int UpdateAdminRole(int role, string roleName, string remark, IEnumerable<int> currencyList, List<SetPermission> permission)
        {
            var result = 1;
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            props.Add("Id", role);
            props.Add("RoleName", roleName);
            props.Add("Remark", remark);
            props.Add("CurrencyList", JsonConvert.SerializeObject(currencyList));

            result = _mainDbClient.Execute("Admin_AdminRole_Set", props, conn, trans);

            if (result <= 0)
            {
                trans.Rollback();
                return result;
            }

            _mainDbClient.Execute("Admin_RolePermission_Del", new DynamicParameters(new { Role = role }), conn, trans);

            foreach (var data in permission)
            {
                _mainDbClient.Execute("Admin_RolePermission_Ins", new DynamicParameters(new { Role = role, MenuId = data.DisPlayKey, Permission = JsonConvert.SerializeObject(data.PermissionIdList) }), conn, trans);
            }
            trans.Commit();
            return result;
        }

        public void DeleteAdminRole(int role)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            _mainDbClient.Execute("Admin_AdminRole_Del", new DynamicParameters(new { Id = role }), conn, trans);
            _mainDbClient.Execute("Admin_RolePermission_Del", new DynamicParameters(new { Role = role }), conn, trans);
            trans.Commit();
        }

        public IEnumerable<RoleList> GetRoleList(string roleName, int siteId)
        {
            return _reportDbClient.Query<RoleList>("Admin_RoleList_Get", 
                new DynamicParameters(new { RoleName = string.IsNullOrWhiteSpace(roleName) ? null: roleName, SiteId = siteId }));
        }
        public IEnumerable<UserList> GetAdminUserList(string userName, int siteId)
        {
            return _reportDbClient.Query<UserList>("Admin_UserList_Get",
                new DynamicParameters(new { UserName = string.IsNullOrWhiteSpace(userName) ? null : userName, SiteId = siteId }));
        }

        public int AddAdminUser(int siteId,int role, string userName, string password, IEnumerable<int> currencyList, List<SetPermission> permission)
        {
            var result = 1;
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            List<int> roleList = new List<int>();
            roleList.Add(role);
            props.Add("UserName", userName);
            props.Add("Userpwd", tools.MD5Encrypt(password));
            props.Add("SEQ", dbType: DbType.Int32, direction: ParameterDirection.Output);

            result = _mainDbClient.Execute("Admin_AdminUser_Ins", props, conn, trans);

            if (result <= 0)
            {
                trans.Rollback();
                return result;
            }
            var SEQ = props.Get<int?>("@SEQ");

            if (SEQ == null)
            {
                trans.Rollback();
                result = 0;
                return result;
            }

            result = _mainDbClient.Execute("Admin_AdminBrand_Ins", new DynamicParameters
                (new { UserId = SEQ, SiteId = siteId, CurrencyList = JsonConvert.SerializeObject(currencyList), Role = JsonConvert.SerializeObject(roleList) }), conn, trans);

            if (result <= 0)
            {
                trans.Rollback();
                return result;
            }

            foreach (var data in permission)
            {
                _mainDbClient.Execute("Admin_UserPermission_Ins", new DynamicParameters(new { UserId = SEQ, SiteId = siteId, MenuId = data.DisPlayKey, Permission = JsonConvert.SerializeObject(data.PermissionIdList) }), conn, trans);
            }
            trans.Commit();
            return result;
        }

        public int UpdateAdminUser(int siteId, int userId,int role, IEnumerable<int> currencyList, List<SetPermission> permission)
        {
            var result = 1;
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            List<int> roleList = new List<int>();
            roleList.Add(role);

            result = _mainDbClient.Execute("Admin_AdminBrandRole_Set", new DynamicParameters
                (new { UserId = userId, SiteId = siteId, CurrencyList = JsonConvert.SerializeObject(currencyList), Role = JsonConvert.SerializeObject(roleList) }), conn, trans);

            if (result <= 0)
            {
                trans.Rollback();
                return result;
            }

            _mainDbClient.Execute("Admin_UserPermission_Del", new DynamicParameters(new { UserId = userId, SiteId = siteId}), conn, trans);

            foreach (var data in permission)
            {
                _mainDbClient.Execute("Admin_UserPermission_Ins", new DynamicParameters(new { UserId = userId, SiteId = siteId, MenuId = data.DisPlayKey, Permission = JsonConvert.SerializeObject(data.PermissionIdList) }), conn, trans);
            }
            trans.Commit();
            return result;
        }
        public int SetAdminUserPassword(int userId, string password)
        {
            var props = new DynamicParameters();
            props.Add("UserId", userId);
            props.Add("Userpwd", tools.MD5Encrypt(password));
            return  _mainDbClient.Execute("Admin_UserPassword_Set", props);
        }
        public int SetAdminUserStatus(int userId, int status)
        {
            var props = new DynamicParameters();
            props.Add("UserId", userId);
            props.Add("Status", status);
            return _mainDbClient.Execute("Admin_UserStatus_Set", props);
        }

        public IEnumerable<AdminBrandRole> GetAdminBrandRole(int role)
        {
            return _reportDbClient.Query<AdminBrandRole>("Admin_AdminBrandRole_Get",
                new DynamicParameters(new { Role = role }));
        }

        public IEnumerable<AdminBrand> GetAssignBrand(IEnumerable<int> selectSite, string userName)
        {
            return _reportDbClient.Query<AdminBrand>("Admin_AdminBrand_Get",
                new DynamicParameters(
                    new { SelectSite = JsonConvert.SerializeObject(selectSite), UserName = string.IsNullOrWhiteSpace(userName) ? null : userName }));
        }

        public IEnumerable<UserBrand> GetUserBrand(int userId)
        {
            return _reportDbClient.Query<UserBrand>("Admin_UserBrand_Get",
                new DynamicParameters(
                    new { UserId = userId }));
        }

        public int SetAdminBrand(int userId, int siteId, AdminBrandTypeEnum status)
        {
            var props = new DynamicParameters();
            props.Add("UserId", userId);
            props.Add("SiteId", siteId);
            props.Add("Status", status);
            return _mainDbClient.Execute("Admin_AdminBrand_Set", props);
        }
    }
}
