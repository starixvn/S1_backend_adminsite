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

        public IActionResult AddBankInfo(int sysId, string bankAccountName, string bankAccountNo, decimal minAmount, decimal maxAmount, int pointLevel)
        {
            BankDetail bankDetail = new BankDetail();
            bankDetail.BankDid = sysId;
            bankDetail.AccountName = bankAccountName;
            bankDetail.AccountNo = bankAccountNo;
            bankDetail.MinAmount = minAmount;
            bankDetail.MaxAmount = maxAmount;
            bankDetail.PointLevel = pointLevel;

            IEnumerable<BankDetail> result = bankDetailBLL.AddBankInfo(bankDetail);
            return View(result.ToList());
        }

        public IActionResult EditBankInfo(int sysId, string bankAccountName, string bankAccountNo, decimal minAmount, decimal maxAmount, int pointLevel)
        {
            BankDetail bankDetail = new BankDetail();
            bankDetail.BankDid = sysId;
            bankDetail.AccountName = bankAccountName;
            bankDetail.AccountNo = bankAccountNo;
            bankDetail.MinAmount = minAmount;
            bankDetail.MaxAmount = maxAmount;
            bankDetail.PointLevel = pointLevel;

            IEnumerable<BankDetail> bankInfoList = bankDetailBLL.EditBankInfo(bankDetail);
            return View(bankInfoList.ToList());
        }

        public IActionResult BankInfoStatus(BankDetail bankDetail, int bankId, int status)
        {
            IEnumerable<BankDetail> bankInfoList = bankDetailBLL.BankInfoStatus(bankDetail, bankId, status);
            return View(bankInfoList.ToList());
        }
    }
}