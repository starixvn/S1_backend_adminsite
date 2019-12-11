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
    public class BankInfoController : Controller
    {
        private readonly BankDetailBLL bankDetailBLL;
        private readonly SelLangBLL selLangBLL;

        public BankInfoController(BankDetailBLL bankDetailBLL, SelLangBLL selLangBLL)
        {
            this.bankDetailBLL = bankDetailBLL;
            this.selLangBLL = selLangBLL;
        }


        public IActionResult BankInfoList()
        {
            IEnumerable<BankDetail> bankInfoList = bankDetailBLL.BankInfoList();
            return View(bankInfoList.ToList());
        }

        public IActionResult AddBankInfo()
        {
            IEnumerable<BankDetail> bankDetail = bankDetailBLL.BankList();
            return View(bankDetail);
        }
    }
}