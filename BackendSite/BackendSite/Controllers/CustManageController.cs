using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class CustManageController : Controller
    {
        private readonly CustInfoBLL custInfoBLL;
        private readonly ExcelBLL excelBLL;

        public CustManageController(CustInfoBLL custInfoBLL, ExcelBLL excelBLL)
        {
            this.custInfoBLL = custInfoBLL;
            this.excelBLL = excelBLL;
        }

        [HttpGet]
        public IActionResult CustInfoList()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CustInfo(int CustId, string TransId)
        {
            return View();
        }

        #region Old Code

        // 這隻For現行後台使用，有另一支API For Frontend Team，
        // 如有調整邏輯，請兩邊同步調整+測試。(api/CustManage/GetCustPhotoId)
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult GetCustPhotoId(int CustId)
        {
            var custPhotoId = custInfoBLL.GetCustPhotoId(CustId);
            if (custPhotoId == null)
            {
                return StatusCode(404);
            }
            var image = custPhotoId.PhotoIdPic;
            return File(image, "image/jpeg");
        }

        public class CustInfoReq
        {
            public string SelectSite { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public int? BirDate { get; set; }
            public int? BirMonth { get; set; }
            public int? BirYear { get; set; }
            public string UserName { get; set; }
            public string AccountName { get; set; }
            public string LoginIp { get; set; }
            public string TransId { get; set; }
            public string TpUserName { get; set; }
            public string Email { get; set; }
            public string PhoneNo { get; set; }
            public int? AffId { get; set; }
            public string SelectCurrency { get; set; }
            public string BankAccountNumber { get; set; }
            public string SelectStatus { get; set; }
        }

        // 這隻For現行後台使用，有另一支API For Frontend Team，
        // 如有調整邏輯，請兩邊同步調整+測試。(api/CustManage/ExportCustInfoList)
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [RequestSizeLimit(40000000)]
        public IActionResult ExportCustInfoList(CustInfoReq custInfoReq)
        {
            var selectSite = new List<int>();
            var selectCurrency = new List<int>();
            var selectStatus = new List<int>();
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (custInfoReq.SelectSite != null)
            {
                selectSite = Array.ConvertAll<string, int>(custInfoReq.SelectSite.Split(','), s => int.Parse(s)).ToList();
            }
            if (custInfoReq.SelectCurrency != null)
            {
                selectCurrency = Array.ConvertAll<string, int>(custInfoReq.SelectCurrency.Split(','), s => int.Parse(s)).ToList();
            }
            if (custInfoReq.SelectStatus != null)
            {
                selectStatus = Array.ConvertAll<string, int>(custInfoReq.SelectStatus.Split(','), s => int.Parse(s)).ToList();
            }

            if (custInfoReq.StartDate != null)
            {
                if (custInfoReq.StartDate > 2147483647)
                    custInfoReq.StartDate = 2147483647;
                startDate = DateTimeExtensions.UnixToDateTime((long)custInfoReq.StartDate);
            }

            if (custInfoReq.EndDate != null)
            {
                if (custInfoReq.EndDate > 2147483647)
                    custInfoReq.EndDate = 2147483647;
                endDate = DateTimeExtensions.UnixToDateTime((long)custInfoReq.EndDate);
            }

       
            var data = custInfoBLL.GetCustInfoExportList(selectSite,
                                                                    custInfoReq.BirDate,
                                                                    custInfoReq.BirMonth,
                                                                    custInfoReq.BirYear,
                                                                    custInfoReq.UserName?.Trim(),
                                                                    custInfoReq.AccountName?.Trim(),
                                                                    custInfoReq.LoginIp?.Trim(),
                                                                    custInfoReq.TransId?.Trim(),
                                                                    custInfoReq.TpUserName?.Trim(),
                                                                    custInfoReq.Email?.Trim(),
                                                                    custInfoReq.PhoneNo?.Trim(),
                                                                    startDate,
                                                                    endDate,
                                                                    custInfoReq.AffId,
                                                                    selectCurrency,
                                                                    custInfoReq.BankAccountNumber?.Trim(),
                                                                    selectStatus);

            data.ToList().ForEach(s => s.RegTime = new DateTimeOffset(s.RegTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));

            var stream = excelBLL.Export(data);
            return excelBLL.ExcelFile(stream, "CustInfoList");
        }

        #endregion

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TeleCSD" })]
        public IActionResult TeleCSD()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TagManagement" })]
        public IActionResult TagManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement" })]
        public IActionResult RegistrationManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
        public IActionResult DisableUserForGame()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/UserBetLimit" })]
        public IActionResult UserBetLimit()
        {
            return View();
        }



        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TpFailRecordReport" })]
        public IActionResult TpFailRecordReport()
        {
            return View();
        }
    }
}