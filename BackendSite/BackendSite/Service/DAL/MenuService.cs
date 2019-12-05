using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class MenuService
    {
        private readonly IWebHostEnvironment env;

        public MenuService(IWebHostEnvironment env)
        {
            this.env = env;
        }

        public string GetMenuData()
        {
            string contentRootPath = env.ContentRootPath;
            var MenuData = File.ReadAllText($"{contentRootPath}/App_Data/Menu.json");

            return MenuData;
        }
    }
}
