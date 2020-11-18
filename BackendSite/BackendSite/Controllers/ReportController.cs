using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class ReportController : Controller
    {
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "LicWinLost/Report" })]
        public IActionResult LicWinLost()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/MainFinancialReport" })]
        public IActionResult MainFinancialReport()
        {
            return View();
        }
    }
}