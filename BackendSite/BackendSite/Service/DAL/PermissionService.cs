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

        public PermissionService(IWebHostEnvironment env, IConfiguration configuration, IServiceProvider provider)
        {
            this.env = env;
            this.configuration = configuration;
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _connectionString = configuration.GetConnectionString("ConnectionString");
        }

        public string GetPermissionJSONData()
        {
            var contentRootPath = env.ContentRootPath;
            var initPermissionData = File.ReadAllText($"{contentRootPath}/App_Data/FunctionPermission.json");

            return initPermissionData;
        }

        public IEnumerable<AdminMenuDtoV2> GetAdminMenuDtoV2()
        {
            return _reportDbClient.Query<AdminMenuDtoV2>("Admin_AdminMenu_GetAll");
        }
        public IEnumerable<AdminPermissionDtoV2> GetAdminPermissionDtoV2()
        {
            return _reportDbClient.Query<AdminPermissionDtoV2>("Admin_AdminPermission_GetAll");
        }

        public IEnumerable<RolePermission> GetRolePermission(int role)
        {
            return _reportDbClient.Query<RolePermission>("Admin_RolePermission_Get", new DynamicParameters(new { Role = role }));
        }
        public IEnumerable<UserPermissionData> GetUserPermissionData(int userId)
        {
            return _reportDbClient.Query<UserPermissionData>("Admin_UserPermission_Get", new DynamicParameters(new { UserId = userId }));
        }

        public IEnumerable<AdminRole> GetAdminRole()
        {
            return _reportDbClient.Query<AdminRole>("Admin_AdminRole_Get");
        }
        public int AddAdminRole(string roleName, string remark, IEnumerable<int> currencyList, List<SetRolePermission> permission,int userId)
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

        public int UpdateAdminRole(int role, string roleName, string remark, IEnumerable<int> currencyList, List<SetRolePermission> permission)
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

        public void DeleteAdminRole(int role, List<UpdataAdminUserV2Role> updateRoleList)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            _mainDbClient.Execute("Admin_AdminRole_Del", new DynamicParameters(new { Id = role }), conn, trans);
            _mainDbClient.Execute("Admin_RolePermission_Del", new DynamicParameters(new { Role = role }), conn, trans);

            foreach (var updateRole in updateRoleList)
            {
                _mainDbClient.Execute("Admin_UserRole_Set", new DynamicParameters(new { Role = JsonConvert.SerializeObject(updateRole.Role), UserId = updateRole.UserId }), conn, trans);
            }
            trans.Commit();
        }

        public IEnumerable<RoleList> GetRoleList(string roleName)
        {
            return _reportDbClient.Query<RoleList>("Admin_RoleList_Get", 
                new DynamicParameters(new { RoleName = string.IsNullOrWhiteSpace(roleName) ? null: roleName }));
        }
    }
}
