using BackendSite.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class BankFinancialController : Controller
    {
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferAccountManage" })]
        public IActionResult TransferAccountManage()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferManage" })]
        public IActionResult TransferManage()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancialReport/TransferInfoReport" })]
        public IActionResult TransferInfoReport()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancialReport/BankFinancialReport" })]
        public IActionResult BankFinancialReport()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/ReasonManagement" })]
        public IActionResult ReasonManagement()
        {
            return View();
        }
    }
}
