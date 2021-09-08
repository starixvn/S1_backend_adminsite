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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitAdjust" })]
        public IActionResult WithdrawalLimitAdjust()
        {
            return View();
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
        public IActionResult PaymentManage()
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
            var pendingFund = pendingFundBLL.PendingFundList();
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
            public string SelectSite { get; set; }
            public string DepositId { get; set; }
            public string UserName { get; set; }
            public string DepositType { get; set; }
            public string CurrencyId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
            public string status { get; set; }
            public string LevelStatus { get; set; }
        }

        [HttpPost]
        public IActionResult ExportDepositList(DepositListReq depositListReq)
        {
            var ExportDeposit = depositBLL.GetDepositExportList(depositListReq.SelectSite, depositListReq.DepositType, depositListReq.startDate, depositListReq.endDate, depositListReq.DepositId, depositListReq.UserName, depositListReq.CurrencyId, depositListReq.status, depositListReq.LevelStatus);

            var stream = excelBLL.Export(ExportDeposit);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nameof(ExportDeposit)}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
        public class WithdrawalListReq
        {
            public string SelectSite { get; set; }
            public string WithdrawalId { get; set; }
            public string UserName { get; set; }
            public string withdrawalType { get; set; }
            public string CurrencyId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
            public string status { get; set; }
            public string LevelStatus { get; set; }
        }

        [HttpPost]
        public IActionResult ExportWithdrawalList(WithdrawalListReq withdrawalListReq)
        {
            var ExportWithdrawal = withdrawalBLL.GetWithdrawalExportList(withdrawalListReq.SelectSite, withdrawalListReq.withdrawalType, withdrawalListReq.startDate, withdrawalListReq.endDate, withdrawalListReq.WithdrawalId, withdrawalListReq.UserName, withdrawalListReq.CurrencyId, withdrawalListReq.status, withdrawalListReq.LevelStatus);

            var stream = excelBLL.Export(ExportWithdrawal);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nameof(ExportWithdrawal)}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositLimitManage" })]
        public IActionResult DepositLimitManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManagement" })]
        public IActionResult WithdrawalLimitManagement()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/BalanceAdjustmentBatch" })]
        public IActionResult BalanceAdjustmentBatch()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/ThirdPartyAdjustment" })]
        public IActionResult ThirdPartyAdjustment()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositSubmission" })]
        public IActionResult DepositSubmission()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalLimitManage" })]
        public IActionResult WithdrawalLimitManage()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositTimeReport" })]
        public IActionResult DepositTimeReport()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalTimeReport" })]
        public IActionResult WithdrawalTimeReport()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/AdjustmentLimitManagement" })]
        public IActionResult AdjustmentLimitManagement()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/AdjustmentLogReport" })]
        public IActionResult AdjustmentLogReport()
        {
            return View();
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalAccountingList" })]
        public IActionResult WithdrawalAccountingList()
        {
            return View();
        }
    }
}