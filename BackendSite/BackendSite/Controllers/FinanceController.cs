using System;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
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
            public string Status { get; set; }
            public string LevelStatus { get; set; }
            public int AmountType { get; set; }
            public string Amount { get; set; }
        }

        [HttpPost]
        public IActionResult ExportDepositList(DepositListReq depositListReq)
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(depositListReq.startDate);
            var endTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(depositListReq.endDate);
            depositListReq.SelectSite = $"[{depositListReq.SelectSite}]";
            depositListReq.DepositType = $"[{depositListReq.DepositType}]";
            depositListReq.CurrencyId = $"[{depositListReq.CurrencyId}]";
            depositListReq.Status = $"[{depositListReq.Status}]";
            depositListReq.LevelStatus = $"[{depositListReq.LevelStatus}]";

            var exportDeposit = depositBLL.GetDepositExportList(depositListReq.SelectSite, depositListReq.DepositType, startTime, endTime, depositListReq.DepositId, depositListReq.UserName, depositListReq.CurrencyId, depositListReq.Status, depositListReq.LevelStatus, depositListReq.AmountType, depositListReq.Amount);

            foreach (var data in exportDeposit)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                if (data.DepositTime != null)
                    data.DepositTime = new DateTimeOffset(data.DepositTime.Value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                if (data.UpdateTime != null)
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.Value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }

            var stream = excelBLL.Export(exportDeposit);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nameof(exportDeposit)}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
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
            public int AmountType { get; set; }
            public string Amount { get; set; }
        }

        [HttpPost]
        public IActionResult ExportWithdrawalList(WithdrawalListReq withdrawalListReq)
        {

            var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(withdrawalListReq.startDate);
            var endTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(withdrawalListReq.endDate);

            var exportWithdrawal = withdrawalBLL.GetWithdrawalExportList(withdrawalListReq.SelectSite, withdrawalListReq.withdrawalType, startTime, endTime, withdrawalListReq.WithdrawalId, withdrawalListReq.UserName, withdrawalListReq.CurrencyId, withdrawalListReq.status, withdrawalListReq.LevelStatus, withdrawalListReq.AmountType, withdrawalListReq.Amount);

            foreach (var data in exportWithdrawal)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                if (data.UpdateTime != null)
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.Value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                if (data.FlowEndTime != null)
                    data.FlowEndTime = new DateTimeOffset(data.FlowEndTime.Value.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }

            var stream = excelBLL.Export(exportWithdrawal);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nameof(exportWithdrawal)}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/USDTWithdrawalLimitManagement" })]
        public IActionResult USDTWithdrawalLimitManagement()
        {
            return View();
        }
    }
}