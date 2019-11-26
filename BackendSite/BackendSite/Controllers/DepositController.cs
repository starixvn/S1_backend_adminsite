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

namespace BackendSite.Controllers
{ 
    public class DepositController : Controller
    {
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration configuration;
        private readonly DepositRequestService depositRequestService;

        public DepositController( DepositRequestService depositrequest)
        {
            this.depositRequestService = depositrequest;
        }

        public IActionResult DepositRequest()
        {
            IEnumerable<DepositRequest> depositRequests = depositRequestService.GetDepositRequest();

            return View(depositRequests.ToList());
        }

        public IActionResult DepositApprove(string transID)
        {
            DepositRequest depositRequest = depositRequestService.GetDepositRequest(transID);

            return View(depositRequest);
        }
    }
}