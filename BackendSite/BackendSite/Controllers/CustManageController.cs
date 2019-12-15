using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class CustManageController : Controller
    {
        [HttpGet]
        public IActionResult CustInfoList()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CustInfo(int CustId, string TransId)
        {
            return View();
        }
    }
}