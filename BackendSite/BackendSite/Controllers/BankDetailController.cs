using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
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

        public IActionResult AddBankInfo(BankDetail bankDetail)
        {
            IEnumerable<BankDetail> result = bankDetailBLL.AddBankInfo(bankDetail);
            return View(result.ToList());
        }

        public IActionResult EditBankInfo(BankDetail bankDetail)
        {
            IEnumerable<BankDetail> bankInfoList = bankDetailBLL.EditBankInfo(bankDetail);
            return View(bankInfoList.ToList());
        }

        public IActionResult DisableBankInfo(BankDetail bankDetail, int direction)
        {
            IEnumerable<BankDetail> bankInfoList = bankDetailBLL.DisableBankInfo(bankDetail, direction);
            return View(bankInfoList.ToList());
        }
    }
}