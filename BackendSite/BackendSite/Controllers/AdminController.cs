using BackendSite.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class AdminController : Controller
    {
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/GroupAuthoritySetting" })]
        public IActionResult GroupAuthoritySetting()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/UserAuthoritySetting" })]
        public IActionResult UserAuthoritySetting()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/SpecialPasswordManagement" })]
        public IActionResult SpecialPasswordManagement()
        {
            return View();
        }
    }
}