using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class CustManageController : Controller
    {
        public IActionResult CustInfo()
        {
            return View();
        }
    }
}