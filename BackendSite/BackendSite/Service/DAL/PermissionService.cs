using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

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
