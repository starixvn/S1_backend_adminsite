using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BackendSite.Service.DAL
{
    public class PermissionService
    {
        private IConfiguration configuration;
        private readonly IWebHostEnvironment env;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public PermissionService(IWebHostEnvironment env, IConfiguration configuration, IServiceProvider provider)
        {
            this.env = env;
            this.configuration = configuration;
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
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
    }
}
