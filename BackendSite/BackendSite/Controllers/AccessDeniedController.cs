using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class AccessDeniedController : Controller
    {
        public IActionResult RefreshPage()
        {
            return View();
        }
    }
}