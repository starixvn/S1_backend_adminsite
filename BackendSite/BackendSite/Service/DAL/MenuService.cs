using Microsoft.AspNetCore.Hosting;
using System.IO;

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
            var contentRootPath = env.ContentRootPath;
            var MenuData = File.ReadAllText($"{contentRootPath}/App_Data/Menu.json");

            return MenuData;
        }
    }
}
