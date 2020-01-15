using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class MarketingController : Controller
    {
        private readonly SelLangBLL selLangBLL;
        private readonly ServerInfoBLL serverInfoBLL;

        public MarketingController(SelLangBLL selLangBLL, ServerInfoBLL serverInfoBLL)
        {
            this.selLangBLL = selLangBLL;
            this.serverInfoBLL = serverInfoBLL;
        }

        public IActionResult PointLevelManagement()
        {
            return View();
        }
    }
}