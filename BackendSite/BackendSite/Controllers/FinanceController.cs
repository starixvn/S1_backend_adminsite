using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class FinanceController : Controller
    {
        private readonly DepositBLL depositBLL;
        private readonly PendingFundBLL pendingFundBLL;
        private readonly ExcelBLL excelBLL;
        private readonly WithdrawalBLL withdrawalBLL;

        public FinanceController(DepositBLL depositBLL, PendingFundBLL pendingFundBLL, ExcelBLL excelBLL, WithdrawalBLL withdrawalBLL)
        {
            this.depositBLL = depositBLL;
            this.pendingFundBLL = pendingFundBLL;
            this.excelBLL = excelBLL;
            this.withdrawalBLL = withdrawalBLL;
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


        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/PaymentManage" })]
        public IActionResult PaymentManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/AdjustmentReport" })]
        public IActionResult PaymentAdjustmentReport()
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

        public class DepositListReq
        {
            public string DepositId { get; set; }
            public string UserName { get; set; }
            public int? DepositType { get; set; }
            public int? CurrencyId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
            public int? status { get; set; }
        }

        [HttpPost]
        public IActionResult ExportDepositList(DepositListReq depositListReq)
        {
            var ExportDeposit = depositBLL.GetDepositExportList(depositListReq.DepositType, depositListReq.startDate, depositListReq.endDate, depositListReq.DepositId, depositListReq.UserName, depositListReq.CurrencyId, depositListReq.status);

            var stream = excelBLL.Export(ExportDeposit);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nameof(ExportDeposit)}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
        public class WithdrawalListReq
        {
            public string WithdrawalId { get; set; }
            public string UserName { get; set; }
            public int? withdrawalType { get; set; }
            public int? CurrencyId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
            public int? status { get; set; }
        }

        [HttpPost]
        public IActionResult ExportWithdrawalList(WithdrawalListReq withdrawalListReq)
        {
            var ExportWithdrawal = withdrawalBLL.GetWithdrawalExportList(withdrawalListReq.withdrawalType, withdrawalListReq.startDate, withdrawalListReq.endDate, withdrawalListReq.WithdrawalId, withdrawalListReq.UserName, withdrawalListReq.CurrencyId, withdrawalListReq.status);

            var stream = excelBLL.Export(ExportWithdrawal);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nameof(ExportWithdrawal)}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
    }
}