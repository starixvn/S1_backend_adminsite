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
    public class PendingFundController : Controller
    {
        private readonly PendingFundBLL pendingFundBLL;
        private readonly SelLangBLL selLangBLL;

        public PendingFundController(PendingFundBLL pendingFundBLL, SelLangBLL selLangBLL)
        {
            this.pendingFundBLL = pendingFundBLL;
            this.selLangBLL = selLangBLL;
        }

        public IActionResult Index()
        {
            IEnumerable<PendingFund> pendingFund = pendingFundBLL.PendingFundList();
            return View(pendingFund.ToList());
        }
    }
}