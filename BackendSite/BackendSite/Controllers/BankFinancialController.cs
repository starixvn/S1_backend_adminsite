using BackendSite.Attributes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers
{
    public class BankFinancialController : Controller
    {
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManage" })]
        public IActionResult TransferAccountManage()
        {
            return View();
        }
    }
}
