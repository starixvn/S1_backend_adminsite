using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
//using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using BackendSite.Service.BLL;

namespace BackendSite.Controllers
{ 
    public class DepositController : Controller
    {
        private readonly DepositBLL depositBLL;

        public DepositController(DepositBLL depositBLL)
        {
            this.depositBLL = depositBLL;
        }

        public IActionResult DepositRequest()
        {
            IEnumerable<DepositRequest> depositRequests = depositBLL.GetDepositRequestList();

            return View(depositRequests.ToList());
        }

        public IActionResult DepositApprove(string transID)
        {
            DepositRequest depositRequest = depositBLL.GetDepositRequest(transID);

            return View(depositRequest);
        }
    }
}