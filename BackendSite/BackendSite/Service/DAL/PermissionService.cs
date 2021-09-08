using BackendSite.Service.Model;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class PermissionService
    {
        private IConfiguration configuration;
        private readonly IWebHostEnvironment env;

        public PermissionService(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.env = env;
            this.configuration = configuration;
        }

        public string GetPermissionJSONData()
        {
            var contentRootPath = env.ContentRootPath;
            var initPermissionData = File.ReadAllText($"{contentRootPath}/App_Data/FunctionPermission.json");

            return initPermissionData;
        }
    }
}
