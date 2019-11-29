using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;

namespace BackendSite.Controllers
{
    public class WithdrawController : Controller
    {
        private readonly WithdrawBLL withdrawBLL;

        public WithdrawController(WithdrawBLL withdrawBLL)
        {
            this.withdrawBLL = withdrawBLL;
        }

        public IActionResult WithdrawList()
        {
            IEnumerable<WithdrawRequest> withdrawRequests = withdrawBLL.GetWithdrawRequestList();

            return View(withdrawRequests.ToList());
        }
    }
}