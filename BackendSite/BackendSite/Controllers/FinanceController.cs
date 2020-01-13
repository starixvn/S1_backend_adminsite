using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    [Authorize(Roles = "Admin, Finance")]
    public class FinanceController : Controller
    {
        private readonly DepositBLL depositBLL;
        private readonly PendingFundBLL pendingFundBLL;

        public FinanceController(DepositBLL depositBLL, PendingFundBLL pendingFundBLL)
        {
            this.depositBLL = depositBLL;
            this.pendingFundBLL = pendingFundBLL;
        }
        public IActionResult WithdrawalList()
        {
            return View();
        }
        public IActionResult BankInfoList()
        {
            return View();
        }
        public IActionResult DepositList()
        {
            return View();
        }
        public IActionResult PaymentAdjustment()
        {
            return View();
        }
        public IActionResult PendingFund()
        {
            IEnumerable<PendingFund> pendingFund = pendingFundBLL.PendingFundList();
            return View(pendingFund.ToList());
        }
        [HttpGet]
        public IActionResult GetDepositReceiptPic(string transId)
        {
            var depositReceipt = depositBLL.GetDepositReceipt(transId);
            if (depositReceipt == null)
            {
                return StatusCode(404);
            }
            var image = depositReceipt.DepositPic;
            return File(image, "image/jpeg");
        }
    }
}